using UnityEngine;

//키워드	누가 써?     역할	            의미
//virtual   부모         함수 수정 허용     이 함수는 자식이 고칠 수 있게 허락한다
//override  자식         함수 내용 수정     부모가 허락한 함수를 내가 직접 고쳐 쓰겠다
//base      자식         부모 함수 호출     내가 고쳐 쓰더라도, 부모의 원래 기능은 일단 실행하겠다
public class LastBossEnemy : Enemy//Enemy 스크립트 상속
{   //3.21 이제부턴 한줄만 있는 if문이라도 {}를 사용하자. 가독성을 위해 한줄로 썻다지만 이것도 어찌보면 가독성을 저해 하는것처럼 느껴져

    [Header("보스 발사체 설정")]
    [SerializeField] private GameObject energyPrefab;//발사할 에너지 프리팹
    [SerializeField] private Transform energySpawnPoint;//보스 자식 오브젝트인 EnergySpawnPoint 연결
    [SerializeField] private float energySpeed = 8f;//발사 속도

    [Header("360도 탄막 설정")]
    [SerializeField] private int numberOfEnergies = 8;//발사체 개수(360 방향)
    [SerializeField] private float energyCoolTime = 3f;//발사 간격 (초)
    [SerializeField] private float minDistanceToShoot = 5f;//이 거리보다 멀어야 발사


    //---SO에서 받아올 보스 몬스터의 대쉬 관련 변수---
    private float dashSpeed;
    private float dashDuration;
    private float dashCoolTime;
    private float dashReadyTime;
    //-------------------------------------------------

    private float lastDashTime;//마지막으로 대쉬 공격을 한 시간
    private bool isDashing = false;//지금 대쉬 중인가?
    private bool isPreparing = false;//지금 기 모으는 중인가?

    private float lastEnergyTime;//마지막으로 360도 공격을 한 시간
    private float currentEnergyDamage;//SO에서 가져온 보스 전용 발사게 데미지 저장 변수


    protected override void FixedUpdate()
    {   
        if (playerScript == null || isDead)
        {
            return;
        }

        //대쉬(isDashing) 중일 때만 별도로 거리 체크해서 닿음데미지 주기
        if (isDashing)
        {
            //1. 플레이어 중앙 위치 계산 (오프셋 포함)
            Vector3 targetPos = playerScript.GetCenterPosition();
            targetPos.y += statsData.PlayerTargetOffsetY;

            //2. 보스와 플레이어 사이의 거리 계산
            float dist = Vector2.Distance(transform.position, targetPos);

            //3. 멈춤 거리(StopDistance)보다 가까워지면 데미지!
            //부모 로직처럼 쿨타임(lastAttackTime)을 체크
            if (dist <= statsData.StopDistance && Time.time >= lastAttackTime + statsData.AttackCooldown)
            {
                ApplyTouchDamage();//부모의 닿을때의 데미지 함수
                lastAttackTime = Time.time;
                Debug.Log("대쉬 중 몸통 박치기 성공!");
            }
        }

        if (isDashing || isPreparing) return;

        base.FixedUpdate();//부모(Enemy)가 가진 모든 이동, 추격, 거리 체크 로직을 실행, 대쉬 공격 if문 밑에 둬서 끊김 방지.

        //대쉬 쿨타임 체크
        if (Time.time >= lastDashTime + dashCoolTime)
        {
            StartCoroutine(DashRoutine());
            lastDashTime = Time.time;
            return;//대쉬 시작했으면 이번 프레임은 여기서 끝
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerScript.transform.position);//플레이어와의 거리 계산
        //1. 쿨타임이 지났는지 + 2. 플레이어와 충분히 멀리 있는지 체크
        if (Time.time >= lastEnergyTime + energyCoolTime && distanceToPlayer >= minDistanceToShoot)
        {
            ShootEnergyCircle();
            lastEnergyTime = Time.time;
        }
    }
    
    public override void SetEnmeyStats()
    {
        base.SetEnmeyStats();//부모의 기본 스탯 설정(속도, 데미지 등)을 먼저 받아와

        if (statsData != null)
        {
            currentEnergyDamage = statsData.energyDamage;//보스 전용 탄막 데미지 가져오기

            //SO에 적은 대쉬 관련 스탯들을 보스 변수에 연결
            dashSpeed = statsData.DashSpeed;
            dashDuration = statsData.DashDuration;
            dashCoolTime = statsData.DashCoolTime;
            dashReadyTime = statsData.DashReadyTime;
        }
    }

    public override void Attack()
    {
        base.Attack();//부모의 기본 공격(데미지 주기, 사운드 등)을 그대로 실행
    }


    //플레이어에게 데미지를 줄 때 화면 흔들기 추가(준비중!)
    public override void DealDamageToPlayer()
    {
        base.DealDamageToPlayer();
        //CameraShake.Instance.Shake(); (나중에 카메라 쉐이크 스크립트가 있다면 추가)
    }

    //보스 몬스터는 특별하게 원거리 공격이 가능(플레이어와 일정 거리 벌어지면 발사)
    public void ShootEnergyCircle()
    {
        if (energyPrefab == null || energySpawnPoint == null)
        { 
            return;  
        }


        float angleStep = 360f / numberOfEnergies;

        for (int i = 0; i < numberOfEnergies; i++)
        {
            float angle = i * angleStep;
            float radianAngle = angle * Mathf.Deg2Rad;

            //발사 방향 계산
            Vector2 direction = new Vector2(Mathf.Cos(radianAngle), Mathf.Sin(radianAngle)).normalized;

            //에너지 생성
            GameObject energy = Instantiate(energyPrefab, energySpawnPoint.position, Quaternion.identity);

            //데미지 설정
            EnemyEnergy enemyEnergy = energy.GetComponent<EnemyEnergy>();
            if (enemyEnergy != null) 
            {
                enemyEnergy.SetDamage(currentAttackDamage);
            }

            //속도 및 회전 설정
            Rigidbody2D rb = energy.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = direction * energySpeed;

                // 이미지가 날아가는 방향을 보게 함
                float lookAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                energy.transform.rotation = Quaternion.AngleAxis(lookAngle, Vector3.forward);
            }
        }
    }

    private System.Collections.IEnumerator DashRoutine()//대쉬 공격의 3단계 과정을 관리하는 코루틴
    {
        isPreparing = true;
        rb.linearVelocity = Vector2.zero;//1. 돌진 전에는 무조건 정지

        if (animator != null)
        {
            animator.SetTrigger("BossDashAttack");
            animator.speed = 1f;
            animator.Play("BossDashAttack", 0, 0f);//강제로 모션을 0:00 프레임으로 보내버려, 즉시 도끼 든 자세로 시작
            animator.speed = 0f;
        }

        spriteRenderer.color = Color.darkGoldenRod;//기 모으는 동안 보스 색상 변경(임시로 금색)
        yield return new WaitForSeconds(dashReadyTime);//dashReadyTime의 시간만큼 준비시간 대기

        //2. 돌진 시작
        isPreparing = false;
        isDashing = true;

        if (animator != null)
        {
            animator.speed = 1.5f;//도끼를 휘두르며 돌진
        }
        spriteRenderer.color = statsData.SpriteColor;//다시 보스몬스터의 원래의 색으로 전환

        //돌진 방향 결정 (준비가 끝난 시점의 플레이어 위치 방향)
        Vector3 targetPos = playerScript.GetCenterPosition(); //플레이어 중앙 좌표
        targetPos.y += statsData.PlayerTargetOffsetY;         //SO에 설정한 오프셋 더하기
        Vector2 dashDirection = (targetPos - transform.position).normalized;
        rb.linearVelocity = dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);//dashDuration 동안 물리 엔진이 밀어주는 힘을 유지하도록 대기

        //3. 대쉬 종료 및 후딜레이
        rb.linearVelocity = Vector2.zero;//대쉬 후 멈춤
        isDashing = false;

        if (animator != null)
        {
            animator.speed = 1f;//애니메이션 속도 정상화
        }
        yield return new WaitForSeconds(0.5f);//돌진 후 잠깐 멍 때리는 시간 (플레이어의 딜 타임)
    }
}
//왜 코루틴을 사용해 대쉬를 구현했어?
//대쉬는 '준비 - 실행 - 후딜레이'라는 시간의 흐름이 필요한 동작이기 때문이야.
//Update에서 복잡한 if문이나 타이머 변수를 여러 개 쓰는 것보다,
//코루틴의 yield return을 활용해 가독성 높고 직관적인 상태 제어를 하기 위해 선택했어.

//부모 클래스(Enemy)를 상속받아 override한 이유가 뭐야?
//모든 몬스터가 공통으로 가지는 추격, 이동 로직은 부모인 Enemy에 두고 재사용하기 위해서야.
//보스만 가지는 특수한 패턴(대쉬, 탄막)만 자식 클래스에서 override하여 확장함으로써 코드 중복을 줄이고 유지보수를 쉽게 만들었지.