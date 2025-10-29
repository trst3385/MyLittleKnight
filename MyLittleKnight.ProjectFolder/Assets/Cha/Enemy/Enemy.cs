using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.Android.Gradle;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static EnemyDifficulty;//Enemy 스크립트에서 EnemyDifficulty 클래스의 static 멤버를 더 편하게 사용하기 위한 문법이야.

public class Enemy : MonoBehaviour
{
    [Serializable]
    public class EnemyStats
    {
        public float MoveSpeed = 4f;
        public float StopDistance = 0.5f;
        public float AttackCooldown = 1f;
        public float AttackDamage = 2f;
        public float DetectionRange = 100f;
        public Color SpriteColor = Color.white;
        public int ScoreValue = 10;//몬스터 처치 시 얻을 점수 (기본값 10점, 인스펙터에서 수정 가능)
    }


    public enum EnemyType { Normal, Strong, Elite }
    //인스펙터창에 드롭다운으로 Normal, Strong, Elite표시

    public EnemyType enemyType = EnemyType.Normal;//인스펙터에서 설정할 몬스터 기본 타입

    [Header("몬스터 타입별 스텟")]
    public EnemyStats NormalStats = new EnemyStats();
    public EnemyStats StrongStats = new EnemyStats();
    public EnemyStats EliteStats = new EnemyStats();

    [Header("EnemySpawner 연결")]
    public EnemySpawn EnemySpawner;//EnemySpawn 스크립트 참조

   
    [Header("Targeting Offset")]
    [SerializeField] private float playerTargetOffsetY = -2.7f;
    //플레이어 목표 Y축 오프셋 (인스펙터나 스크립트에서 조절해. 지금은 -2.7이 제일 적당해)

   
    [Header("사운드")]//사운드 관련
    [SerializeField] private AudioSource deathAudioSource;
    [SerializeField] private AudioClip deathSound;


    //내부변수
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Player playerScript;

    private float currentMoveSpeed;//이동속도
    private float currentStopDistance;//플레이어와 이 거리에 닿으면 멈춤
    private float currentAttackCooldown;//공격 쿨타임
    private float lastAttackTime;//마지막으로 공격한 시간

    private float currentAttackDamage;//데미지
    private float currentDetectionRange;//몬스터가 플레이어를 감지하는 거리
    private int currentScoreValue;//몬스터 처치 시 플레이어가 받을 점수

    private bool playerWasDead = false;//플레이어가 이전에 죽었었는지 추적하는 변수
    private bool isDead = false;//사망 변수(기본값 false)
    private bool isKnockedBack = false;//넉백 중인지 여부를 나타내는 플래그


    public void SetEnmeyStats()//몬스터 시작 시 능력치와 외형을 설정하는 함수
    {
        if (spriteRenderer == null)//SpriteRenderer가 없으면 진행 불가
        {
            Debug.LogError($"Enemy: {gameObject.name}에 SpriteRenderer 컴포넌트가 없어! 스탯 설정에 실패했어!");
            return;
        }
                                                          
        EnemyStats selectedStats;//현재 타입에 맞는 능력치 세트를 저장할 변수�
        switch (enemyType)
        {
            case EnemyType.Normal:
                selectedStats = NormalStats;
                break;
            case EnemyType.Strong:
                selectedStats = StrongStats;
                break;
            case EnemyType.Elite:
                selectedStats = EliteStats;
                break;
            default://기본값
                selectedStats = NormalStats;
                break;
        }

        if(EnemyDifficulty.Instance != null)
        {   
            currentMoveSpeed = EnemyDifficulty.Instance.GetAdjustedMonsterStat(selectedStats.MoveSpeed, StatType.MoveSpeed);
            //난이도 영향 없으면 그대로
            currentStopDistance = selectedStats.StopDistance;
            currentAttackCooldown = selectedStats.AttackCooldown;
            currentAttackDamage = EnemyDifficulty.Instance.GetAdjustedMonsterStat(selectedStats.AttackDamage, StatType.AttackDamage);
            currentDetectionRange = selectedStats.DetectionRange;
            currentScoreValue = selectedStats.ScoreValue;
        }
        else//EnemyDifficulty 인스턴스가 없으면 기본 스탯 사용
        {
            Debug.LogWarning("EnemyDifficulty.Instance를 찾을 수 없어, 몬스터가 기본 스탯으로 생성할게.");
            currentMoveSpeed = selectedStats.MoveSpeed;
            currentStopDistance = selectedStats.StopDistance;
            currentAttackCooldown = selectedStats.AttackCooldown;
            currentAttackDamage = selectedStats.AttackDamage;
            currentDetectionRange = selectedStats.DetectionRange;
            currentScoreValue = selectedStats.ScoreValue;
        }       
        spriteRenderer.color = selectedStats.SpriteColor;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        //"Player" 태그를 가진 오브젝트로 이동(Transform을 할당)
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        if(playerGameObject != null)
            playerScript = playerGameObject.GetComponent<Player>();
        else
        {         
            playerScript = null;
            Debug.LogWarning("Enemy: Player 오브젝트를 찾을 수 없어! 'Player' 태그를 확인해!");
        }
        SetEnmeyStats();//몬스터 시작 시 능력치와 외형을 설정하는 함수 호출

        lastAttackTime = Time.time - currentAttackCooldown;//시작하자마자 공격가능, 실행되고 쿨타임 기다리지 않고 바로 공격

        if (deathAudioSource == null)//오디오 컴포넌트 가져오기
            Debug.LogError("Enemy: AudioSource 컴포넌트를 찾을 수 없어!");     
    }
    

    void FixedUpdate()//FixedUpdate에선 Time.deltaTime보단 Time.fixedDeltaTime(정확한 물리 계산과 일관된 이동 속도를 보장)
    {
        if (isDead)//몬스터 사망시
        {
            rb.linearVelocity = Vector2.zero;//즉시 멈춤
            return;
        }

        if (isKnockedBack) return;//넉백 중일 때는 이동 로직을 건너뜀

        if (HandlePlayerDeath()) return;//플레이어 사망 시 멈춤
        if (playerScript == null) return;//Player 스크립트가 없으면 이동/공격 로직 진행하지 않음

        //플레이어와의 거리 계산
        Vector3 playerCenterPosition = playerScript.GetCenterPosition();//플레이어의 중앙 위치 가져오기
        playerCenterPosition.y += playerTargetOffsetY;//Y축 오프셋 적용


        float distanceToPlayer = Vector2.Distance(transform.position, playerCenterPosition);

        //긴 조건문을 대체하여 가독성을 높이는 지역 변수
        bool isInDetectionRange = distanceToPlayer <= currentDetectionRange;
        bool isInStopDistance = distanceToPlayer <= currentStopDistance;
        bool canAttack = Time.time >= lastAttackTime + currentAttackCooldown;


        //주 로직 분리 (세분화된 함수 호출)
        ProcessMovementAndAttack(isInDetectionRange, isInStopDistance, canAttack, playerCenterPosition);
    }

    void DealDamageToPlayer()//몬스터가 플레이어에게 피해를 주는 핵심 로직을 통합
    {   //플레이어의 방패와 체력 스크립트를 찾아 데미지를 계산하고 적용하는 로직의 최종 목표 지점
        if (playerScript == null || playerScript.IsDead)//플레이어 생존/연결 체크
        {
            Debug.Log("플레이어가 없거나 사망하여 데미지를 줄 수 없어!");
            return;
        }

        PlayerShield playerShield = playerScript.GetComponent<PlayerShield>();//플레이어 방어력 (PlayerShield) 컴포넌트 확인
        if (playerShield != null)//플레이어에게 방어력이 있다면, 방어력에 먼저 데미지 적용
        {
            playerShield.TakeShieldDamage(currentAttackDamage);
            Debug.Log("몬스터가 플레이어의 방어력에 " + currentAttackDamage + " 데미지를 줬어!");
        }
        else//방어력이 없으면 체력(PlayerHealth) 컴포넌트 확인 후 데미지 적용
        {
            PlayerHealth playerhealth = playerScript.GetComponent<PlayerHealth>();
            if (playerhealth != null)
            {
                playerhealth.TakeDamage(currentAttackDamage);
                Debug.Log("플레이어가 " + currentAttackDamage + " 데미지를 받았다! 현재 체력: " + playerhealth.CurrentHealth);
            }
            else Debug.LogError("플레이어에게 PlayerHealth 스크립트가 없어!");
        }
    }


    public void Attack()//몬스터가 플레이어에게 공격
    {
        //이 함수는 호출되면 다시 한번 플레이어와의 거리(+ 1.5f의 추가 공격 범위)를 체크한 후, DealDamageToPlayer()를 호출
        //Attack 애니메이션 이벤트가 호출될 때, 플레이어와의 거리를 다시 확인
        float distanceToPlayer = Vector2.Distance(transform.position, playerScript.transform.position);

        if (distanceToPlayer <= currentStopDistance + 1.5f)//1.5f는 몬스터 멈춤 지점 외 추가 공격 범위. 콜라이더 밖 거리에서도 데미지 가능
            DealDamageToPlayer();//통합 함수 호출
        else Debug.Log("공격 범위 밖이라 데미지를 줄 수 없어!");

    }

    void ApplyTouchDamage()//플레이어가 몬스터에게 닿으면 받은 데미지 
    {
        DealDamageToPlayer();//통합 함수 호출
    }

    //플레이어가 몬스터의 탐지/공격 범위 안에 들어왔을 때 행동을 결정하는 핵심 함수 (AI 분기점)
    private void ProcessMovementAndAttack(bool isInDetectionRange, bool isInStopDistance, bool canAttack, Vector3 playerCenterPosition)
    {
        if (!isInDetectionRange)//탐지 범위 밖이라면 (추격 포기)
        {
            StopMovement();
            return;
        }

        //플레이어와의 거리가 StopDistance 이내일 경우 (Attack 준비)
        if (isInStopDistance)
        {
            StopMovement();//몬스터 멈춤

            //쿨타임 체크 및 공격 애니메이션 미활성화 상태 확인
            if (canAttack && !animator.GetBool("Attack"))
            {
                ApplyTouchDamage();//닿는 데미지 로직, 닿는 데미지가 일반 공격 쿨타임과 공유
                lastAttackTime = Time.time;//쿨타임 시작 시간 기록

            }

            if (!animator.GetBool("Attack"))//닿았을때 데미지와 별개로아직 공격 애니메이션이 시작되지 않았다면 시작              
                animator.SetBool("Attack", true);
        }
        else//플레이어와의 거리가 StopDistance보다 멀 경우
        {
            if (animator.GetBool("Attack"))// 공격 애니메이션이 켜져 있었다면 끄고 이동
            {
                animator.SetBool("Attack", false);
            }
            MoveTowardsPlayer(playerCenterPosition);
        }
    }

    
    public void TakeKnockback(Vector2 knockbackDirection, float knockbackForce, float duration)//SwordWeapon 스크립트에서 호출될 넉백 함수
    {
        if (isDead) return;//죽은 몬스터는 넉백되지 않음

        if (rb == null)
        {
            Debug.LogWarning(gameObject.name + "에는 Rigidbody2D가 없어서 넉백을 받을 수 없어!");
            return;
        }

        StopAllCoroutines();//이미 진행 중인 넉백 코루틴이 있다면 중지 (중복 넉백 방지)

        isKnockedBack = true;//넉백 활성화
        rb.linearVelocity = Vector2.zero;//현재 속도 초기화 (이전 움직임 영향 제거)

        //넉백 힘 적용
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        
        StartCoroutine(KnockbackRoutine(duration));//넉백 지속 시간만큼 기다린 후 넉백 상태 해제
    }
    private IEnumerator KnockbackRoutine(float duration)//넉백 코루틴
    {
        yield return new WaitForSeconds(duration);//SwordWeapon 스크립트의 KnockbackDuration = 0.2f(초) 동안 밀려남

        
        rb.linearVelocity = Vector2.zero;//넉백 시간 종료 후 속도 초기화 (밀려나던 것을 멈추게 해)
        isKnockedBack = false;//넉백 상태 해제
        Debug.Log(gameObject.name + " 넉백 종료.");
    }

    private void MoveTowardsPlayer(Vector3 targetPosition)//플레이어 추적/이동 로직
    {
        Vector2 direction = (targetPosition - transform.position).normalized;
        rb.linearVelocity = direction * currentMoveSpeed;

        animator.SetBool("Move", true);
        FlipSprite(direction.x);//좌우 반전 함수 호출
    }
    public void OnAttackFinished()//공격 애니메이션이 끝난 후에 Attack Bool을 다시 false로 바꿔주는 함수
    {                          
        animator.SetBool("Attack", false);
    }

    private void StopMovement()//움직임 정지 로직
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("Move", false);
    }

    private void FlipSprite(float directionX)//몬스터 스프라이트 좌우 반전
    {
        //방향이 0이 아닐 때만 처리 (0일 땐 flipX가 유지됨)
        if (directionX != 0)
        {
            //directionX가 음수일 때 (왼쪽) true, 양수일 때 (오른쪽) false
            spriteRenderer.flipX = directionX < 0;
        }
    }


    private bool HandlePlayerDeath()//플레이어 사망 상태 처리
    {
        bool isPlayerDead = (playerScript != null && playerScript.IsDead);

        if (isPlayerDead)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("Move", false);

            if (!playerWasDead)
            {
                animator.SetTrigger("Idle");
                playerWasDead = true;
            }
            return true;//플레이어가 죽었으니 더 이상 추적/공격 로직을 진행하지 않음
        }
        else
        {
            playerWasDead = false;
            return false;
        }
    }

    public void EnemyDie()//몬스터 사망
    {       
        if (isDead) return;//이미 죽은 상태라면, 더 이상 아무것도 하지 않고 함수를 종료

        isDead = true;
        Debug.Log(enemyType + "몬스터 컷!");
        
        if (deathAudioSource != null && deathSound != null)//몬스터 사망시 사망 사운드 재생
            deathAudioSource.PlayOneShot(deathSound);//PlayOneShot은 현재 재생 중인 다른 소리를 끊지 않고, 새로운 소리를 한 번만 재생
                                                     //예를 들어, 몬스터가 여러 마리 동시에 죽을 때 각자 죽는 소리가 모두 들리게 하려면 PlayOneShot을 쓰는 게 좋아.
                                                     //만약 그냥 audioSource.Play()를 썼다면, 다른 소리가 재생될 때마다 이전 소리가 끊기게 돼

        if (playerScript != null)//!= null은 Player스크립트가 null과 같지 않으면?. 즉 제대로 스크립트와 연결된 상태.
        {
            if (playerScript != null)
                playerScript.AddScore(currentScoreValue);
        }

        if (EnemySpawner != null)
        {   //현재 몬스터의 타입이 Strong 또는 Elite인지 확인
            bool isThisStrongOrElite = (enemyType == EnemyType.Strong || enemyType == EnemyType.Elite);

            EnemySpawner.EnemyDied(isThisStrongOrElite);//EnemySpawn 스크립트의 EnemyDied 함수를 호출하여 몬스터가 죽었음을 알림
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");

            float DieTime = 1.5f;//사망 후 ~초 후에 사라짐

            Destroy(gameObject, DieTime);
        }
    }   
}

