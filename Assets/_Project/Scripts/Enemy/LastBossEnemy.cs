using UnityEngine;

//키워드	누가 써?     역할	            의미
//virtual   부모         함수 수정 허용     이 함수는 자식이 고칠 수 있게 허락한다
//override  자식         함수 내용 수정     부모가 허락한 함수를 내가 직접 고쳐 쓰겠다
//base      자식         부모 함수 호출     내가 고쳐 쓰더라도, 부모의 원래 기능은 일단 실행하겠다
public class LastBossEnemy : Enemy//Enemy 스크립트 상속
{
    [Header("보스 추가 패턴 설정")]
    [SerializeField] private GameObject energyPrefab;//발사할 에너지 프리팹
    [SerializeField] private Transform energySpawnPoint;//보스 자식 오브젝트인 EnergySpawnPoint 연결
    [SerializeField] private float energySpeed = 8f;//발사 속도

    [Header("360도 탄막 설정")]
    [SerializeField] private int numberOfEnergies = 8;//발사체 개수(360 방향)
    [SerializeField] private float energyCoolTime = 3f;//발사 간격 (초)
    [SerializeField] private float minDistanceToShoot = 5f;//이 거리보다 멀어야 발사

    private float lastEnergyTime;//마지막으로 360도 공격을 한 시간
    private float currentEnergyDamage;//SO에서 가져온 보스 전용 발사게 데미지 저장 변수

    protected override void FixedUpdate()
    {
        base.FixedUpdate();// 부모(Enemy)가 가진 모든 이동, 추격, 거리 체크 로직을 실행

        if (playerScript == null || isDead)
        {
            return;
        } 

        float distanceToPlayer = Vector2.Distance(transform.position, playerScript.transform.position);//플레이어와의 거리 계산

        //1. 쿨타임이 지났는지 + 2. 플레이어와 충분히 멀리 있는지 체크
        if (Time.time >= lastEnergyTime + energyCoolTime && distanceToPlayer >= minDistanceToShoot)
        {
            ShootEnergyCircle();
            lastEnergyTime = Time.time;
        }
    }
    //3.21 이제부턴 한줄만 있는 if문이라도 {}를 사용하자. 가독성을 위해 한줄로 썻다지만 이것도 어찌보면 가독성을 저해 하는것처럼 느껴져
    public override void SetEnmeyStats()
    {
        base.SetEnmeyStats();//부모의 기본 스탯 설정(속도, 데미지 등)을 먼저 받아와

        if (statsData != null)//보스만의 추가 스탯인 energyDamage를 따로 챙겨오자
        {
            currentEnergyDamage = statsData.energyDamage;
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

    // 웅이가 가져온 360도 발사 로직을 보스용으로 개조!
    public void ShootEnergyCircle()
    {
        if (energyPrefab == null || energySpawnPoint == null)
        { 
            return;  
        }

        Debug.Log("보스의 360도 전방위 에너지 발사!");

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
}