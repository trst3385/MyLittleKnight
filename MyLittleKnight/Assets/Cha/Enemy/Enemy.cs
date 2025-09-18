using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static EnemyDifficulty;//Enemy 스크립트에서 EnemyDifficulty 클래스의 static 멤버를 더 편하게 사용하기 위한 문법이야.
                             //원래 Enemy 스크립트에서 EnemyDifficulty의 StatType을 사용하려면 EnemyDifficulty.StatType이라고 써야 해.
                             //하지만 using static EnemyDifficulty;를 선언하면, EnemyDifficulty를 생략하고 그냥 StatType이라고만 써도 돼.

public class Enemy : MonoBehaviour
{ 
    public enum EnemyType { Normal, Strong, Elite }//몬스터 타입 열거형,Normal은 기본타입, Strong은 빨간색, Elite는 노랑색 
    //인스펙터창에 드롭다운으로 Normal, Strong, Elite표시
    public EnemyType enemyType = EnemyType.Normal;//인스펙터에서 설정할 몬스터 기본 타입
     
    [Header("몬스터 타입별 스텟")]//인스펙터에서 구분하기 위한 헤더,
    //시각적으로 "Enemy Stats by Type"이라는 제목을 달아주는 역할만 하는 거야. 
    //[Serializable] 속성이 EnemyStats 클래스 자체에 붙어있어서 public EnemyStats 타입 변수들을
    //인스펙터의 EnemyStats 드롭다운에 표시
    //[Serializable]가 없었다면 이들은 인스펙터에 보이지 않아
    public EnemyStats NormalStats = new EnemyStats();//Normal 타입의 능력치 세트
    public EnemyStats StrongStats = new EnemyStats();//Strong 타입의 능력치 세트
    public EnemyStats EliteStats = new EnemyStats();//Elite 타입의 능력치 세트

    [Header("EnemySpawner 연결")]
    public EnemySpawn EnemySpawner;//EnemySpawn 스크립트 참조, [Header("사운드")] 헤더 때문에 위치를 위쪽으로 옮겨놨어


    //[Header]는 '제목'이라서, 이 밑에 제목을 붙여줄 '내용물(변수)'이 꼭 필요해!
    //[Header] 속성은 항상 그 아래에 오는 변수를 꾸며주는 역할이야.
    [Header("Targeting Offset")]//헤더는 이건 순전히 유니티 인스펙터 창을 정리하고 보기 좋게 만들기 위한 기능이야
    [SerializeField] private float playerTargetOffsetY = -2.7f;
    //플레이어 목표 Y축 오프셋 (인스펙터나 스크립트에서 조절해. 지금은 -2.7이 제일 적당해)

    //GetComponent<T>() vs [SerializeField] 차이
    //GetComponent<T>(): 이 함수는 코드를 통해서 게임 오브젝트의 컴포넌트를 가져올 때 사용하는 거야. 주로 Start()나 Awake() 함수에서 실행돼.
    //[SerializeField]: 이 키워드는 private 변수를 인스펙터에 노출시켜서 개발자가 직접 컴포넌트를 연결하게 만드는 역할을 해.
    //GetComponent<T>()를 꼭 써야 하는 건 아니고, [SerializeField]로 선언된 변수는 인스펙터에서 직접 드래그해서 연결하면 돼
    //그래서 GetComponent<AudioSource>()를 쓰지 않고 [SerializeField]를 사용해 인스펙터에서 오디오 컴포넌트를 그래드 해서 연결했어.
    //한 오브젝트에 여러 사운드를 넣을려면 [SerializeField] private를 써서 드래그해서 연결 시켜주자!
    //사운드 관련 변수
    [Header("사운드")]
    [SerializeField] private AudioSource deathAudioSource;
    [SerializeField] private AudioClip deathSound;

   
    [Serializable]//각 타입별 능력치를 담을 구조체. Normal, Strong, Elite 스텟에 담아
    public class EnemyStats
    { 
        public float MoveSpeed = 4f;
        public float StopDistance = 0.5f;
        public float AttackCooldown = 1f;
        public float AttackDamage = 2f;
        public float DetectionRange = 100f;
        public Color SpriteColor = Color.white;
        //[Serializable] 속성이 EnemyStats 클래스에 적용되었으므로 그 클래스 내부에
        //public으로 선언된 변수들은 인스펙터에서 해당 "Stats" 드롭다운을 열었을 때 나타나는 거야.
        public int ScoreValue = 10;//몬스터 처치 시 얻을 점수 (기본값 10점, 인스펙터에서 수정 가능)
    }

    //[Serializable]와 [Header("Enemy Stats by Type")] 같은 속성(Attribute)은
    //바로 그 아래에 선언된 클래스나 변수에 적용되는 거야.

    //기존 변수들 (이 변수들에 위에서 선언한 타입별 값들이 할당됨)
    //private이라 인스펙터에서 수정 불가
    private float currentMoveSpeed;//이동 속도
    private float currentStopDistance;//플레이어와 이 거리에 닿으면 멈춤
    private float currentAttackCooldown;//공격 쿨타임
    private float currentAttackDamage;//공격 데미지
    private float currentDetectionRange;//몬스터가 플레이어를 감지하는 거리
    private int currentScoreValue;//몬스터 처치 시 줄 점수


    private bool playerWasDead = false;//플레이어가 이전에 죽었었는지 추적하는 변수
    private float lastAttackTime;//마지막으로 공격한 시간
    private bool isDead = false;//사망 변수(기본값 false)
    private bool isKnockedBack = false;//넉백 중인지 여부를 나타내는 플래그

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Player playerScript;//Player 스크립트 참조
    


    public void SetEnmeyStats()//몬스터 시작 시 능력치와 외형을 설정하는 함수, 임의로 지은 함수 이름
    {
        if (spriteRenderer == null) return;//SpriteRenderer가 없으면 더 이상 진행하지 않음
                                           //spriteRenderer가 없을 경우 로그

        EnemyStats selectedStats;//현재 타입에 맞는 능력치 세트를 저장할 변수
       
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
            default:
                selectedStats = NormalStats;//기본값
                break;
        }

        if(EnemyDifficulty.Instance != null)
        {   //using static EnemyDifficulty;를 선언했기에,
            //원래면public float GetAdjustedMonsterStat(float baseStat, EnemyDifficulty.StatType statType) 처럼 EnemyDifficulty를 적었어야해.
            //하지만 Enemy 스크립트에서는 EnemyDifficulty의 StatType을 가져와서 사용해야 하므로 using static EnemyDifficulty;가 매우 유용하게 쓰여.
            //이 구문을 사용하면 코드가 더 간결해지고 가독성이 좋아져.
            currentMoveSpeed = EnemyDifficulty.Instance.GetAdjustedMonsterStat(selectedStats.MoveSpeed, StatType.MoveSpeed);//selectedStats 사용
            currentStopDistance = selectedStats.StopDistance;//난이도 영향 없으면 그대로
            currentAttackCooldown = selectedStats.AttackCooldown;//난이도 영향 없으면 그대로
            currentAttackDamage = EnemyDifficulty.Instance.GetAdjustedMonsterStat(selectedStats.AttackDamage, StatType.AttackDamage);//selectedStats 사용
            currentDetectionRange = selectedStats.DetectionRange;//난이도 영향 없으면 그대로
            currentScoreValue = selectedStats.ScoreValue;//난이도 영향 없으면 그대로
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
        //spriteRenderer.color는 난이도와 상관없이 몬스터 타입에 따라 고정될 테니, 이 줄은 지우지 않고 그대로 놔둬
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();


        //"Player" 태그를 가진 오브젝트로 이동(Transform을 할당)
        //public Transform playerTransform이 변수로 플레이어의 컴포넌트와 연결됨
        GameObject playerGameObject = GameObject.FindWithTag("Player");//플레이어 오브젝트 연결
        if(playerGameObject != null)
            playerScript = playerGameObject.GetComponent<Player>();//Player 스크립트 참조 가져오기
        else
        {         
            playerScript = null;
            Debug.LogWarning("Enemy: Player 오브젝트를 찾을 수 없다고! 'Player' 태그를 확인해!");
        }
        //몬스터 시작 시 능력치와 외형을 설정하는 함수 호출
        SetEnmeyStats();

        lastAttackTime = Time.time - currentAttackCooldown;//시작하자마자 공격가능, 실행되고 쿨타임 기다리지 않고 바로 공격

        //오디오 컴포넌트 가져오기
        if (deathAudioSource == null)
            Debug.LogError("Enemy: AudioSource 컴포넌트를 찾을 수 없어!");     
    }



    void FixedUpdate()//물리관련 로직은 FixedUpdate로 
    {//FixedUpdate에선 Time.deltaTime보단 Time.fixedDeltaTime(정확한 물리 계산과 일관된 이동 속도를 보장)

        if (isDead)//몬스터 사망
        {
            rb.velocity = Vector2.zero;//사망시 즉시 멈춤
            return;//사망 시 이동을 하지 못함
        }

        if (isKnockedBack)//넉백 중일 때는 이동 로직을 건너뜀
            return;


        if (HandlePlayerDeath()) return;//플레이어 사망 시 행동 처리 및 FixedUpdate 종료, 함수 호출
        if (playerScript == null) return;//플레이어 스크립트가 없으면 이동/공격 로직 진행하지 않음

        //플레이어와의 거리 계산
        Vector3 playerCenterPosition = playerScript.GetCenterPosition();//플레이어의 중앙 위치 가져오기
        playerCenterPosition.y += playerTargetOffsetY;//Y축 오프셋 적용


        float distanceToPlayer = Vector2.Distance(transform.position, playerCenterPosition);

        //if문이 많아 가독성이 떨어져 설명전용 변수 추가
        bool isInDetectionRange = distanceToPlayer <= currentDetectionRange;
        bool isInStopDistance = distanceToPlayer <= currentStopDistance;
        bool canAttack = Time.time >= lastAttackTime + currentAttackCooldown;

        //주 로직 분리 (세분화된 함수 호출)
        ProcessMovementAndAttack(isInDetectionRange, isInStopDistance, canAttack, playerCenterPosition);
    }

    private bool HandlePlayerDeath()//플레이어 사망 상태 처리
    {
        bool isPlayerDead = (playerScript != null && playerScript.IsDead);

        if(isPlayerDead)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("Move", false);

            if(!playerWasDead)
            {
                animator.SetTrigger("Idle");
                playerWasDead = true;
            }
            return true;//플레이어가 죽었으니 더 이상 추적/공격 로직을 진행하지 않음
        }
        else playerWasDead = false; return false; 
    }


    //플레이어가 몬스터 공격 범위 안에 들어왔을때 분기점 역할 함수, 범위에 따라 각각의 함수 호출
    private void ProcessMovementAndAttack(bool isInDetectionRange, bool isInStopDistance, bool canAttack, Vector3 playerCenterPosition)
    {
        if(!isInDetectionRange)//플레이어가 감지 범위 밖에 있으면 즉시 멈춤
        {
            StopMovement();
            return;
        }

        if(!isInStopDistance)//플레이어가 감지 범위 안에 있지만 공격 거리에 있지 않으면 추적
        {
            //만약 공격 상태였다면, 즉시 공격을 취소하고 움직여
            if (animator.GetBool("Attack"))
            {
                animator.SetBool("Attack", false);
            }  //GetBool이란? 애니메이터에 있는 Attack이라는 이름의 bool 파라미터의 현재 상태를 가져오는(Get) 역할을 해
               //만약 Attack 파라미터가 true로 설정되어 있다면, 이 함수는 true 값을 반환해
               //GetBool을 사용해서 "만약 현재 애니메이터가 'Attack' 상태에 있다면"이라는 조건을 만들 수 있어
               //SetBool은? true라면 애니메이터에게 'Attack' 상태로 전환하라고 명령,
               //false면 애니메이터에게 'Attack' 상태를 해제하라고 명령 하는 거지
            MoveTowardsPlayer(playerCenterPosition);
        }
        else//플레이어가 공격 거리에 있으면 멈추고 공격
        {
            StopAndAttackPlayer(canAttack, playerCenterPosition);
        }
    }


    private void MoveTowardsPlayer(Vector3 targetPosition)//플레이어 추적/이동 로직
    {
        Vector2 direction = (targetPosition - transform.position).normalized;
        rb.velocity = direction * currentMoveSpeed;

        animator.SetBool("Move", true);
        FlipSprite(direction.x);//좌우 반전 함수 호출
    }


    //플레이어에게 도착 후 공격 로직. 공격 모션을 시작하게 해주는 역할
    private void StopAndAttackPlayer(bool canAttack, Vector3 playerCenterPosition)
    {
        rb.velocity = Vector3.zero;
        animator.SetBool("Move", false);

        FlipSprite(playerCenterPosition.x - transform.position.x);//멈춰서 공격할 때도 플레이어를 바라보게

        if(canAttack)
        {
            //몬스터가 공격을 시작하기 전에 플레이어가 여전히 공격 범위 내에 있는지 다시 확인
            float distanceToPlayer = Vector2.Distance(transform.position, playerCenterPosition);

            if (distanceToPlayer <= currentStopDistance + 0.1f)
            {
                animator.SetBool("Attack", true);
                lastAttackTime = Time.time;
            }
            else
            {
                //플레이어가 이미 공격 범위를 벗어났으면 공격을 시작하지 않음
                Debug.Log("플레이어가 너무 멀리 있어 공격을 취소했어!");
            }
        }
    }
    public void OnAttackFinished()//공격 애니메이션이 끝난 후에 Attack Bool을 다시 false로 바꿔주는 함수
    {                             //에니메이션에서 이벤트로 추가해줘
        animator.SetBool("Attack", false);
    }

    private void StopMovement()//움직임 정지 로직
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("Move", false);
    }

    private void FlipSprite(float directionX)//몬스터 스프라이트 좌우 반전
    {
        if(directionX > 0) 
            spriteRenderer.flipX = false;      
        else if (directionX < 0)
            spriteRenderer.flipX = true;

    }

    public void Attack()//이 함수는 공격이 성공했을 때 데미지라는 결과를 만들어내는 역할
    {
        //Animation Event처럼 특정 오브젝트에 여러 스크립트가 붙어있을 때,
        //그리고 함수 이름이 같거나 오버로드 되어 있다면 Unity 시스템이 어떤 스크립트의 어떤 함수를 호출해야 할지
        //모호해져서 문제가 발생할 수 있어.

        //Attack 애니메이션 이벤트가 호출될 때, 플레이어와의 거리를 다시 확인
        float distanceToPlayer = Vector2.Distance(transform.position, playerScript.transform.position);

        if (distanceToPlayer <= currentStopDistance + 1f)//1f는 여유를 주기 위한 값
        {
            if (playerScript == null || playerScript == null || playerScript.IsDead)
            {   //몬스터가 공격 애니메이션 도중 플레이어가 죽거나 없어진 경우를 대비한 로그
                Debug.Log("플레이어가 없거나 사망하여 데미지를 줄 수 없어!");
                return;
            }

            PlayerShield playerShield = playerScript.GetComponent<PlayerShield>();
            if (playerShield != null)//플레이어에게 PlayerShield 스크립트가 있다면, 방어력에 데미지 적용
            {
                playerShield.TakeShieldDamage(currentAttackDamage);
                Debug.Log("몬스터가 플레이어의 방어력에 " + currentAttackDamage + " 데미지를 줬어!");
            }
            else//PlayerShield 스크립트가 없다면, 기존처럼 PlayerHealth에 직접 데미지 적용
            {   //몬스터가 플레이어에게 데미지를 주는 로직
                PlayerHealth playerhealth = playerScript.GetComponent<PlayerHealth>();
                if (playerhealth != null)
                {
                    playerhealth.TakeDamage(currentAttackDamage);
                    Debug.Log("플레이어가 " + currentAttackDamage + " 데미지를 받았다! 현재 체력: " + playerhealth.CurrentHealth);
                }
                else Debug.LogError("플레이어에게 PlayerHealth 스크립트가 없어!");
            }
        }
        else
        {
            Debug.Log("공격 범위 밖이라 데미지를 줄 수 없어!");
        }
    }
    
    public void EnemyDie()//몬스터가 죽을때  
    {       
        if (isDead) return;//이미 죽은 상태라면, 더 이상 아무것도 하지 않고 함수를 종료

        isDead = true;//사망 상태로 변경, 맨 위에 선언에는 아직 사망 모션을 하면 안되니 false로
        Debug.Log("적 사망!");
       
        //몬스터 사망시 사망 사운드 재생
        if (deathAudioSource != null && deathSound != null)
        {
            deathAudioSource.PlayOneShot(deathSound);//PlayOneShot은 이름 그대로 현재 재생 중인 다른 소리를 끊지 않고, 새로운 소리를 한 번만 재생하는 함수야.
            //예를 들어, 몬스터가 여러 마리 동시에 죽을 때 각자 죽는 소리가 모두 들리게 하려면 PlayOneShot을 쓰는 게 좋아.
            //만약 그냥 audioSource.Play()를 썼다면, 다른 소리가 재생될 때마다 이전 소리가 끊기게 돼.
        }

        if (playerScript != null)//!= null은 Player스크립트가 null과 같지 않으면?. 즉 제대로 스크립트와 연결된 상태.
        {
            //Player.AddScore 함수를 호출하여 몬스터의 점수 값을 전달
            //Player.Instance.AddScore(currentScoreValue); 라고 쓸 수도 있지만,
            //currentScore가 static이므로 Player.AddScore가 아니라 Player.currentScore += currentScoreValue;
            //로 직접 접근하는 게 현재 설계에서는 더 간단할 수 있어.
            //하지만 player 스크립트에 AddScore() 함수를 만들었으니 그 함수를 호출하는 게 더 객체지향적이야.
            if (playerScript != null)//playerScript는 Start에서 이미 찾아 뒀으니 이걸 활용
            {
                playerScript.AddScore(currentScoreValue);
            }
        }

        if (EnemySpawner != null)
        {   //현재 몬스터의 타입이 Strong 또는 Elite인지 확인
            bool isThisStrongOrElite = (enemyType == EnemyType.Strong || enemyType == EnemyType.Elite);

            //EnemySpawn 스크립트의 EnemyDied 함수를 호출하여 몬스터가 죽었음을 알림
            //이때, 이 몬스터가 강한/엘리트 몬스터인지 여부를 함께 전달
            EnemySpawner.EnemyDied(isThisStrongOrElite);
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");//Animator의 "Die" 트리거 발동

            float DieTime = 1.5f;//사망 후 ~초 후에 사라짐
    
            Destroy(gameObject, DieTime);//Destroy함수는 게임 오브젝트나 컴포넌트를 게임에서 제거(파괴)할 때 사용
            //즉 Enemy 오브젝트가 사망하면 DieTime에 나온 시간 (초)가 지난 후 제거한다.
        }
    }

    //PlayerAttack 스크립트에서 호출될 넉백 함수
    public void TakeKnockback(Vector2 knockbackDirection, float knockbackForce, float duration)
    {
        if (isDead) return;//죽은 몬스터는 넉백되지 않음

        if (rb == null)
        {
            Debug.LogWarning(gameObject.name + "에는 Rigidbody2D가 없어서 넉백을 받을 수 없어!");
            return;
        }

        //이미 진행 중인 넉백 코루틴이 있다면 중지 (중복 넉백 방지)
        StopAllCoroutines();//이 스크립트의 모든 코루틴 중지 (다른 중요한 코루틴이 있다면 주의!)
                            //특정 코루틴만 중지하고 싶다면 StopCoroutine(코루틴_참조); 사용해야 해.

        isKnockedBack = true;//넉백 상태 플래그 설정
        rb.velocity = Vector2.zero;//현재 속도 초기화 (이전 움직임 영향 제거)

        //넉백 힘 적용
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        Debug.Log(gameObject.name + "에게 넉백 " + knockbackForce + "만큼 적용! 방향: " + knockbackDirection);

        //넉백 지속 시간만큼 기다린 후 넉백 상태 해제
        StartCoroutine(KnockbackRoutine(duration));
    }
    private IEnumerator KnockbackRoutine(float duration)//넉백 코루틴
    {
        yield return new WaitForSeconds(duration);

        //넉백 시간 종료 후 속도 초기화 (밀려나던 것을 멈추게 해)
        rb.velocity = Vector2.zero;
        isKnockedBack = false;//넉백 상태 플래그 해제
        Debug.Log(gameObject.name + " 넉백 종료.");
    }
}


