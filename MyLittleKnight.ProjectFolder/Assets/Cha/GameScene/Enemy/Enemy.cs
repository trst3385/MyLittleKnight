using System;
using System.Collections;
using UnityEngine;
using static EnemyDifficulty;//Enemy 스크립트에서 EnemyDifficulty 클래스의 static 멤버를 더 편하게 사용하기 위한 문법이야.

public class Enemy : MonoBehaviour
{
    //[Serializable]의 역할:
    //유니티 엔진에게 이 클래스(EnemyStats)가 MonoBehaviour를 상속받지 않은 '일반 C# 클래스'임에도 불구하고,
    //인스펙터 창에 필드들을 표시하고, 그 데이터를 씬 또는 프리팹 파일에 영구적으로 저장(직렬화)할 수 있도록
    //유니티 엔진에게 공식적으로 허가하는 태그. (이 태그가 없으면 인스펙터에 나타나지 않음)
    [Serializable]
    public class EnemyStats//EnemyStats라는 이름으로 MoveSpeed, AttackDamage 등의 항목이 포함된 스탯 템플릿(설계도)
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
    public EnemyStats NormalStats = new EnemyStats();//템플릿(EnemyStats)을 기반으로 실제 데이터 덩어리(인스턴스)를 세 개 만들어내는 단계
    public EnemyStats StrongStats = new EnemyStats();//Strong과 Elite도 동일
    public EnemyStats EliteStats = new EnemyStats();
    //[Serializable] 덕분에 이 세 개의 덩어리가 유니티 인스펙터에 나타나,
    //인스펙터에서 이 덩어리들(NormalStats 등)을 펼쳐서 MoveSpeed 등을 수정하고 저장할 수 있어


    [Header("스크립트 연결")]
    public EnemySpawn EnemySpawner;//EnemySpawn 스크립트 참조, EnemySpawn 스크립트가 이 몬스터 프리팹을 가져오기에 연결해준거야


    [Header("Targeting Offset(플레이어 Y축 중심)")]
    [SerializeField] private float playerTargetOffsetY = -2.7f;
    //플레이어 목표 Y축 오프셋 (인스펙터나 스크립트에서 조절해. 지금은 -2.7이 제일 적당해)


    [Header("사운드")]//사운드 관련
    [SerializeField] private AudioClip deathSound;


    //-----내부변수-----
    //내부 컴포넌트(Awake에서 초기화)
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    //외부 스크립트(Start에서 초기화)
    private Player playerScript;//런타임에 Player 태그를 이용해 찾아 연결할 변수
    private PlayerShield playerShield;//선언(보관함)은 저장 공간이고, Start()의 GetComponent는 그 공간에 값을 넣어주는 역할
    //런타임 스탯
    private float currentMoveSpeed;//이동속도
    private float currentStopDistance;//플레이어와 이 거리에 닿으면 멈춤
    private float currentAttackCooldown;//공격 쿨타임
    private float currentAttackDamage;//데미지
    private float currentDetectionRange;//몬스터가 플레이어를 감지하는 거리
    private int currentScoreValue;//몬스터 처치 시 플레이어가 받을 점수
    //상태변수
    private float lastAttackTime;//마지막으로 공격한 시간
    private bool playerWasDead = false;//플레이어가 이전에 죽었었는지 추적하는 변수
    private bool isDead = false;//사망 변수(기본값 false)
    private bool isKnockedBack = false;//넉백 중인지 여부를 나타내는 플래그
    private bool isPendingDeath = false;//처리를 시간 정지 해제 시까지 대기시키는 플래그


    void Awake()//내부 컴포넌트는 Awake에
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()//외부 스크립트는 Start에
    {
        //몬스터가 씬에 생성될 때, "Player" 태그를 가진 오브젝트를 찾아서 연결
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        if (playerGameObject != null)
        {
            playerScript = playerGameObject.GetComponent<Player>();

            //PlayerShield 스크립트 내부에서 방어력이 0이면 playerHealth. 플레이어 체력으로 넘겨주기에,
            //playerHealth 스크립트를 선언할 필요는 없어
            if (playerScript != null) playerShield = playerGameObject.GetComponent<PlayerShield>();
              
        }
        else { playerScript = null; Debug.LogWarning("Enemy: Player 오브젝트를 찾을 수 없어! 'Player' 태그를 확인해!"); }

        SetEnmeyStats();//몬스터 시작 시 능력치와 외형을 설정하는 함수 호출
        lastAttackTime = Time.time - currentAttackCooldown;//시작하자마자 공격가능, 실행되고 쿨타임 기다리지 않고 바로 공격
    }
    
    void FixedUpdate()//FixedUpdate에선 Time.deltaTime보단 Time.fixedDeltaTime(정확한 물리 계산과 일관된 이동 속도를 보장)
    {
        if (TimeFreeze.Instance != null && TimeFreeze.Instance.IsTimeFrozen)//TimeFreeze로 시간이 멈췄는지 체크
        {
            if (animator.enabled) animator.enabled = false;//애니메이터가 활성화되어 있다면
                                                           //애니메이터 비활성화: 몬스터가 그 상태 그대로 얼어붙음

            if (!isKnockedBack)//몬스터가 넉백 중이 아니라면 정지
            {
                rb.linearVelocity = Vector2.zero;//물리적인 움직임 정지
                animator.SetBool("Move", false);//애니메이션 정지
            }
            return;//시간 정지 상태이니 FixedUpdate의 나머지 모든 추적/공격 로직을 건너뛰고 함수 종료
        }
        if (animator != null && !animator.enabled) animator.enabled = true;//시간이 풀렸을 때 몬스터의 애니메이터를 다시 활성화

        
        //시간이 풀렸을 때, 대기 중인 사망이 있다면 실행, 사망 모션이 아니여도 여기서 몬스터 사망시 행동 발동
        if (isPendingDeath) ExecuteDeathSequence();//지연된 사망 시퀀스 실행 (사망 사운드/모션 발동)


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

    public void SetEnmeyStats()//몬스터 시작 시 능력치와 외형을 설정하는 함수
    {
        if (spriteRenderer == null)//SpriteRenderer가 없으면 진행 불가
        {
            Debug.LogError($"Enemy: {gameObject.name}에 SpriteRenderer 컴포넌트가 없어! 스탯 설정에 실패했어!");
            return;
        }

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
            default://기본값
                selectedStats = NormalStats;
                break;
        }

        if (EnemyDifficulty.Instance != null)
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
    
    void DealDamageToPlayer()//몬스터가 플레이어에게 피해를 주는 핵심 로직을 통합
    {   //플레이어의 방패와 체력 스크립트를 찾아 데미지를 계산하고 적용하는 로직의 최종 목표 지점

        if (playerScript == null || playerScript.IsDead)//플레이어 생존/연결 체크
        {
            Debug.Log("플레이어가 없거나 사망하여 데미지를 줄 수 없어!");
            return;
        }

        if (playerShield != null)//플레이어에게 방어력이 있다면, 방어력에 먼저 데미지 적용
        {                        //방어력이 0이면 체력으로 데미지 이전
            playerShield.TakeShieldDamage(currentAttackDamage);
            Debug.Log("몬스터가 플레이어의 방어력에 " + currentAttackDamage + " 데미지를 줬어!");
        }
        else Debug.LogError("PlayerShield 스크립트가 플레이어 오브젝트에 없어! 방어력 시스템에 연결되지 않았어!");
    }

    public void Attack()//몬스터가 플레이어에게 공격
    {
        //이 함수는 호출되면 다시 한번 플레이어와의 거리(+ 1.5f의 추가 공격 범위)를 체크한 후, DealDamageToPlayer()를 호출
        //Attack 애니메이션 이벤트가 호출될 때, 플레이어와의 거리를 다시 확인
        float distanceToPlayer = Vector2.Distance(transform.position, playerScript.transform.position);

        //1.5f는 몬스터 멈춤 지점 및 추가 공격 범위. 콜라이더 밖 거리에서도 데미지 가능
        if (distanceToPlayer <= currentStopDistance + 1.5f) DealDamageToPlayer();//통합 함수 호출
        else Debug.Log("공격 범위 밖이라 데미지를 줄 수 없어!");

    }

    //<접촉 데미지 실행 경로>
    //몬스터가 플레이어에게 붙어있는 상태에서 (StopDistance 이내)
    //AttackCooldown마다 주기적으로 데미지를 적용하는 함수.
    //이 함수는 DealDamageToPlayer()를 호출하여 데미지를 최종적으로 적용하는 전달자 역할 수행.
    //(주로 ProcessMovementAndAttack AI 로직에서 호출됨)
    void ApplyTouchDamage() => DealDamageToPlayer();//통합 함수 호출
  

    //플레이어가 몬스터의 탐지/공격 범위 안에 들어왔을 때 행동을 결정하는 핵심 함수 (AI 분기점)
    private void ProcessMovementAndAttack(bool isInDetectionRange, bool isInStopDistance, bool canAttack, Vector3 playerCenterPosition)
    {
        if (!isInDetectionRange)//탐지 범위 밖이라면 (추격 포기)
        {
            StopMovement();
            return;
        }

        //공격 로직(공격 가능 거리/ 쿨타임 체크)
        if (isInStopDistance)
        {
            rb.linearVelocity = Vector2.zero;//몬스터를 멈추는 대신(StopMovement함수 사용 대신),Rigidbody의 속도를 0으로 강제 설정하여 떨림 방지
            animator.SetBool("Move", false); //함수 호출 대신 로직을 바로 실행하는게 더 효율적

            //쿨타임 체크 및 공격 애니메이션 미활성화 상태 확인
            if (canAttack && !animator.GetBool("Attack"))
            {
                ApplyTouchDamage();//닿는 데미지 로직, 닿는 데미지가 일반 공격 쿨타임과 공유
                lastAttackTime = Time.time;//쿨타임 시작 시간 기록
            }

            if (!animator.GetBool("Attack"))
                animator.SetBool("Attack", true);//닿았을때 데미지와 별개로아직 공격 애니메이션이 시작되지 않았다면 시작
        }
        else//공격 범위 밖에 있을 때
        {   //공격 범위 밖이라면 공격 애니메이션을 끄고 무조건 플레이어를 추적
            if (animator.GetBool("Attack")) animator.SetBool("Attack", false);

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

    public void OnAttackFinished() => animator.SetBool("Attack", false);//코드를 엄청 간결하게 만들어주는 표현식 본문 멤버
    //공격 애니메이션이 끝난 후에 Attack Bool을 다시 false로 바꿔주는 함수, 몬스터의 Animation에서 이 함수를 호출해 공격 종료.


    private void StopMovement()//움직임 정지 로직
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("Move", false);
    }

    private void FlipSprite(float directionX)//몬스터 스프라이트 좌우 반전
    {
        //방향이 0이 아닐 때만 처리 (0일 땐 flipX가 유지됨)
        if (directionX != 0) spriteRenderer.flipX = directionX < 0;//directionX가 음수일 때 (왼쪽) true, 양수일 때 (오른쪽) false
    }

    private bool HandlePlayerDeath()//플레이어가 죽었을때 몬스터의 행동을 정지
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
            return true;//플레이어가 죽었으니 더 이상 추적,공격 로직을 진행하지 않음
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

        if (playerScript != null) playerScript.AddScore(currentScoreValue);//몬스터가 죽으면 플레이어가 몬스터의 점수 획득
        if (EnemySpawner != null)
        {   //현재 몬스터의 타입이 Strong 또는 Elite인지 확인
            bool isThisStrongOrElite = (enemyType == EnemyType.Strong || enemyType == EnemyType.Elite);
            EnemySpawner.EnemyDied(isThisStrongOrElite);//EnemySpawn 스크립트의 EnemyDied 함수를 호출하여 몬스터가 죽었음을 알림
        }

        //시간 정지 상태 체크 및 분기
        bool isFrozen = TimeFreeze.Instance != null && TimeFreeze.Instance.IsTimeFrozen;
        if (isFrozen)
        {
            isPendingDeath = true;
            Debug.Log("사망 모션이 시간이 풀릴 때까지 지연됩니다.");
            return;
        }

        ExecuteDeathSequence();//시간이 정상일 때 지연 없이 즉시 사망 시퀀스 실행
    }
    public void ExecuteDeathSequence()//시간 정지로 지연되었던 사망 시퀀스(사운드, 모션, 파괴)를 실행하는 함수
    {                                 //시간 정지가 아닐때에도 여기서 몬스터의 사망 시퀀스를 발동해
        //1. 사운드 재생 (시간이 풀렸으니 사운드 재생)
        if (deathSound != null && SoundManager.Instance != null) SoundManager.Instance.PlaySFX(deathSound);

        //2. 애니메이션 및 오브젝트 파괴
        if (animator != null)
        {
            animator.SetTrigger("Die");
            float DieTime = 1.5f;
            //시간이 풀리면 1.5초 뒤에 사라지도록 예약
            //Destroy(gameObject, DieTime)는 TimeScale의 영향을 받지 않는 RealTime을 사용하므로, 
            //여기서 호출해야 1.5초 뒤에 사라져.
            Destroy(gameObject, DieTime);
        }
        //3. 플래그 초기화
        isPendingDeath = false;//이제 대기 상태가 아님
    }

    /// <summary>
    /// 강화 화살에 피격 시 호출되어 몬스터에게 슬로우 효과를 적용합니다.
    /// </summary>
    /// <param name="factor">이동 속도 감소 비율 (예: 0.5f = 50% 느려짐)</param>
    public void ApplySlowEffect(float factor)
    {
        if (isDead) return;

        //강화 화살에 여러 번 맞아도 이동 속도가 계속 줄어들지 않도록 방지
        if (currentMoveSpeed < NormalStats.MoveSpeed)
        {
            Debug.Log($"{gameObject.name} 이미 슬로우 상태이므로 중첩하지 않습니다.");
            return;
        }
        currentMoveSpeed *= factor;//현재 이동 속도에 슬로우 비율을 적용하고, 이 값이 영구적으로 유지됨
        Debug.Log($"{gameObject.name} 영구 슬로우 적용! 최종 속도: {currentMoveSpeed}");
    }
}