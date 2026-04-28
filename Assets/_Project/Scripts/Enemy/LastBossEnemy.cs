using UnityEngine;

//키워드	누가 써?     역할	            의미
//virtual   부모         함수 수정 허용     이 함수는 자식이 고칠 수 있게 허락한다
//override  자식         함수 내용 수정     부모가 허락한 함수를 내가 직접 고쳐 쓰겠다
//base      자식         부모 함수 호출     내가 고쳐 쓰더라도, 부모의 원래 기능은 일단 실행하겠다
public class LastBossEnemy : Enemy//Enemy 스크립트 상속
{   //부모 클래스(Enemy)를 상속받아 override한 이유가 뭐야?
    //모든 몬스터가 공통으로 가지는 추격, 이동 로직은 부모인 Enemy에 두고 재사용하기 위해서야.
    //보스만 가지는 특수한 패턴(대쉬, 탄막)만 자식 클래스에서 override하여 확장함으로써 코드 중복을 줄이고 유지보수를 쉽게 만들었지.

    //자식 클래스에서 override 키워드 없이 부모와 똑같은 이름의 함수를 쓰면,
    //이건 '재정의'가 아니라 '별개의 새 함수'를 만든 걸로 간주해. 이걸 전문 용어로 **메서드 숨기기(Method Hiding)**라고 불러.


    [Header("보스 발사체 설정")]
    [SerializeField] private GameObject energyPrefab;//발사할 에너지 프리팹
    [SerializeField] private Transform energySpawnPoint;//보스 자식 오브젝트인 EnergySpawnPoint 연결
    [SerializeField] private float energySpeed = 8f;//발사 속도

    [Header("360도 탄막 설정")]
    [SerializeField] private int numberOfEnergies = 8;//발사체 개수(360 방향)
    [SerializeField] private float energyCoolTime = 3f;//발사 간격 (초)
    [SerializeField] private float minDistanceToShoot = 5f;//이 거리보다 멀어야 발사

    [Header("보스 전용 사운드")]
    [SerializeField] private AudioClip dashReadySound;//대쉬 기 모을 때 소리
    [SerializeField] private AudioClip energyShootSound;//탄막 발사 소리


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
        if (playerScript == null || isDead)//플레이어가 없거나 죽으면 아무것도 안 함
        {
            return;
        }

        if (isDashing || isPreparing)//보스가 돌진 준비 중이라면 부모의 일반 이동/공격 로직(base)을 아예 실행하지 않기
        {
            HandleDashCollision();//돌진 중 플레이어와 닿았는지 체크
            return;//돌진 모션 중엔 아래의 일반 추격 / 패턴 로직은 실행하지 않음
        }

        base.FixedUpdate();//부모(Enemy)가 가진 모든 이동, 추격, 거리 체크 로직을 실행,
                           //중요!: 부모의 사망 지연 로직(TimeFreeze 대응)을 먼저 실행하기 위해 최상단에 배치

        if (isDead)//중요!: 체력이 0이 되어 부모로직의 사망(isDead)이 확정되었다면,    
        {          //보스의 개별 패턴(이동,대쉬, 탄막 등)은 실행하지 않고 종료
            return;
        }


        //돌진 쿨타임 체크
        if (Time.time >= lastDashTime + dashCoolTime)
        {
            StartCoroutine(DashRoutine());
            lastDashTime = Time.time;
            return;//대쉬 시작했으면 이번 프레임은 여기서 끝
        }

        //원거리 공격 발사 로직
        float distanceToPlayer = Vector2.Distance(transform.position, playerScript.transform.position);//플레이어와의 거리 계산
        //1. 쿨타임이 지났는지 + 2. 플레이어와 충분히 멀리 있는지 체크
        if (Time.time >= lastEnergyTime + energyCoolTime && distanceToPlayer >= minDistanceToShoot)
        {
            ShootEnergyCircle();
            lastEnergyTime = Time.time;
        }
    }

    private void HandleDashCollision()//돌진 상태일 때 닿으면 데미지를 주는 로직
    {
        if (isDashing)//1. 오직 '대쉬 중'일 때만 충돌 데미지 체크를 수행
        {
            //2. 플레이어의 중심 위치(Y축 보정 포함)를 가져와서
            Vector3 targetPos = playerScript.GetCenterPosition();
            targetPos.y += statsData.PlayerTargetOffsetY;

            //3. 보스와 플레이어 사이의 실제 거리를 계산해
            float dist = Vector2.Distance(transform.position, targetPos);

            //4. 거리가 가깝고(StopDistance), 공격 쿨타임이 지났다면?
            if (dist <= statsData.StopDistance && Time.time >= lastAttackTime + statsData.AttackCooldown)
            {
                ApplyTouchDamage();//닿을때 데미지
                lastAttackTime = Time.time;//쿨타임 갱신
            }
        }
    }

    protected override bool HandleTimeFreeze()
    {
        //보스 몬스터는 부모의 '정지 로직'을 실행하지 않아.
        //대신 애니메이터가 꺼져있다면 다시 켜주기만 하고 false를 반환해
        if (!animator.enabled)
        {
            animator.enabled = true;
        }

        return false;//"난 안 멈췄으니까 행동을 계속 진행해"
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
        //보스는 덩치가 크니까 일반 몬스터보다 판정 범위를 넉넉하게(예: + 3f) 잡아줘
        float distanceToPlayer = Vector2.Distance(transform.position, playerScript.transform.position);

        //1.5f 대신 보스에게 맞는 적당한 값(예: 3.5f~4f)으로 보정
        if (distanceToPlayer <= currentStopDistance + 3.5f)
        {
            DealDamageToPlayer();//공격 데미지 입히기
        }
        else 
        {
            Debug.Log("보스 공격: 사거리가 살짝 모자라! 거리: " + distanceToPlayer);
        }
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

        //탄막 발사 사운드 재생
        if (energyShootSound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(energyShootSound);
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

    //왜 코루틴을 사용해 대쉬를 구현했어?
    //대쉬는 '준비 - 실행 - 후딜레이'라는 시간의 흐름이 필요한 동작이기 때문이야.
    //Update에서 복잡한 if문이나 타이머 변수를 여러 개 쓰는 것보다,
    //코루틴의 yield return을 활용해 가독성 높고 직관적인 상태 제어를 하기 위해 선택했어.
    private System.Collections.IEnumerator DashRoutine()//대쉬 공격의 3단계 과정을 관리하는 코루틴
    {
        isPreparing = true;
        rb.linearVelocity = Vector2.zero;//1. 돌진 전에는 무조건 정지

        //1-1. 대쉬 준비 사운드 재생
        if (dashReadySound != null && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(dashReadySound);
        }

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

    public override void EnemyDie()//부모(Enemy)의 사망 함수를 보스 전용으로 재정의(Override)
    {
        StopAllCoroutines();//1. 보스가 돌진(DashRoutine) 중에 죽었다면, 그 코루틴을 즉시 멈춰

        //2.상태 변수들도 확실히 꺼주기
        isDashing = false;
        isPreparing = false;

        if (rb != null)//3. 돌진(Dash 코루틴 발동) 중이더라도 즉시 정지
        {
            rb.linearVelocity = Vector2.zero;
        }

        //4. 이전 돌진 패턴(기 모으기 0배속, 돌진 1.5배속 등)에서 변경된 애니메이션 속도를 
        //1(정상)로 복구하여, 사망 모션이 정상적인 속도로 출력되게 함
        if (animator != null)
        {
            animator.speed = 1f;
        }

        //5.부모(Enemy)가 가진 기본 사망 로직(점수 추가, 콜라이더 끄기 등) 실행
        base.EnemyDie();
    }

    public override void ApplySlowEffect(float factor)//부모(Enemy)의 슬로우 로직을 보스만 무시하도록 재정의
    {
        // [설계 의도]
        //1. base.ApplySlowEffect(factor)를 호출하지 않은 이유: 
        //   부모(Enemy)에 정의된 실제 이동 속도 감소 로직이 실행되는 것을 원천 차단하기 위해서야.
        //2. 함수 내부를 비워둔 이유: 
        //   보스는 상태 이상(슬로우)에 면역이어야 하므로, 호출 시 아무런 연산도 일어나지 않게 '무시' 처리.
        Debug.Log("보스: 슬로우 면역!");
    }
}