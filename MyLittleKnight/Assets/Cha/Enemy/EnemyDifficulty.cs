using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using TMPro;//TextMeshPro를 사용하기 위해 추가


//이 스크립트(EnemyDifficulty)는 '몬스터 강화 알림' UI (DifficultyStatsText)를 직접 제어.
//장점: 특정 기능(몬스터 강화)에 종속된 간단한 알림을 빠르게 구현하기 용이합니다.
//단점: 페이드 인/아웃과 같은 시각적 효과가 없으며, 여러 종류의 알림을 통합 관리하기 어려워!
//(TextAlimManager와 같은 통합 관리 스크립트와 비교해봐!)



public class EnemyDifficulty : MonoBehaviour
{
    //인스펙터에서 설정할 변수들
    [Header("Normal 몬스터 스폰 주기 조절")]//헤더는 이건 순전히 유니티 인스펙터 창을 정리하고 보기 좋게 만들기 위한 기능이야
    public float InitialNormalSpawnTime = 4f;//게임 시작 시 Normal 몬스터의 스폰 주기 (예: 4초)
    public float MinNormalSpawnTime = 1f;//스폰 주기가 아무리 빨라져도 이 값 이하로는 안 내려감 (예: 1초)
    public float SpawnTimeDecreaseRate = 0.1f;//몇 초마다 스폰 주기를 얼마나 줄일지 (예: 0.1초씩 줄어듦)
    public float DecreaseInterval = 10f;//스폰 주기가 줄어드는 시간 간격 (예: 10초마다 한 번씩)
    //decreaseInterval의 n초 마다 몬스터의 스폰 시간이 spawnTimeDecreaseRate의 n초 만큼 줄어든다.

    [Header("Normal 몬스터 동시 스폰 개수 조절")]
    public int InitialNormalSpawnCount = 1;//게임 시작 시 Normal 몬스터 동시 스폰 개수
    public int SpawnCountIncreasePerLevel = 1;//난이도 레벨마다 동시 스폰 개수 증가량
    public int MaxNormalSpawnCount = 5;//동시 스폰 개수 최대치 (너무 많아지는 것 방지)

    //몬스터 스탯 난이도 조절 변수들
    [Header("몬스터 스탯 난이도 조절")]
    [SerializeField] private float statInterval = 20f;//몬스터 스탯이 강해지는 시간 간격 (초)
    [SerializeField] private float atkIncrease = 0.2f;//난이도 레벨마다 몬스터 공격력 증가 비율 (20% = 0.2) -> 20%씩
    [SerializeField] private float hpIncrease = 0.2f;//난이도 레벨마다 몬스터 체력 증가 비율 (20% = 0.2) -> 20%씩
    [SerializeField] private float speedIncrease = 0.01f;//난이도 레벨마다 몬스터 이동 속도 증가 비율 (1% = 0.01) -> 1%씩

    
    [Header("UI, 오브젝트 연결")]//헤더는 이건 순전히 유니티 인스펙터 창을 정리하고 보기 좋게 만들기 위한 기능이야
    [SerializeField] private TextMeshProUGUI notificationText;//EnemyDifficultyStatsText UI를 연결해!
    [SerializeField] private TextMeshProUGUI enemyLevelText;//EnemyDifficultyLevelText UI를 연결해!!
    [SerializeField] private TextAlimManager textalimManager;//TextAlimManager UI를 연결해!
    [SerializeField] private EnemySpawn enemySpawnRef;//EnemySpawn 오브젝트 연결해!!


    private float gameTimer = 0f;//게임 시작 후 총 경과 시간
    private int currentDifficultyLevel = 0;//현재 몬스터 스탯 난이도 레벨

    //FindObjectOfType
    //내부에서 사용할 변수들
    private float currentNormalSpawnTime;//현재 Normal 몬스터가 스폰되는 실제 주기 (이 값이 계속 변할 거야)
    private float timeSinceLastDecrease;//마지막으로 스폰 주기를 줄인 후 지난 시간
    private int currentNormalSpawnCount;//현재 동시 스폰 몬스터 개수를 저장할 변수
    public static EnemyDifficulty Instance { get; private set; }
    //private set;의 의미:
    //public static EnemyDifficulty Instance : 외부(다른 스크립트)에서 EnemyDifficulty.Instance로 읽을 수 있게 허용.
    //get; : 값을 가져오는 건 누구나 가능.
    //private set; : 값을 설정하는 건 오직 이 EnemyDifficulty 클래스 내부에서만 가능.

    //어디서든 이 스크립트에 쉽게 접근할 수 있게 해주는 '싱글톤' 패턴
    //GameManager처럼 게임에 하나만 있고 다른 스크립트들이 이 값을 가져다 써야 할 때 많이 사용
    //Instance란 특별한 키워드가 아닌 변수의 이름일 뿐이야, 개발자들이 싱글톤 패턴을 만들 때 관습적으로 사용하는 이름이야.
    //핵심은 Instance라는 이름이 아니라, 그 앞에 붙은 public static이야.
    //public static이 붙었기 때문에, Instance는 '어떤 오브젝트에도 속하지 않고', 클래스 자체에 딱 하나만 존재하는 특별한 변수가 되는 거지.
    //Awake() 함수에서 Instance = this;라고 코드를 짜서, 이 변수가 게임에서 가장 먼저 만들어진 그 인스턴스를 가리키도록 규칙을 정해준 거야.
    //DifficultyStatsText UI랑 DifficultyManager 오브젝트에서 이 스크립트를 쓰니 싱글톤 패턴으로 만들었어
    //이 EnemyDifficulty 스크립트는 DifficultyManager 오브젝트에 붙어 게임 전체의 난이도를 총괄하며,
    //DifficultyStatsText UI 알림 제어와 같은 기능을 게임 내 여러 다른 스크립트에서 참조하고 사용하기 위해 싱글톤 패턴으로 만들었어.

    //스탯 타입을 구분하기 위한 Enum
    public enum StatType
    {
        AttackDamage,
        Health,
        MoveSpeed
    }

    void Awake()
    {
        //게임 시작 시 이 스크립트의 유일한 인스턴스를 설정해.
        if(Instance != null && Instance != this)
            Destroy(gameObject);//이미 인스턴스가 있으면 자신을 파괴(중복 방지)
        else
        {
            Instance = this as EnemyDifficulty;//자신이 유일한 인스턴스가 됨
            DontDestroyOnLoad(gameObject);//씬이 바뀌어도 파괴되지 않게 (게임 전체 난이도 관리용)
            //DontDestroyOnLoad는 유니티에서 특정 게임 오브젝트를 현재 씬이 아닌
            //다음 씬으로 넘어갈 때도 파괴되지 않고 유지되도록 할 때 사용하는 함수
        }
    }
    void Start()
    {
        currentNormalSpawnTime = InitialNormalSpawnTime;//게임 시작 시 초기 스폰 주기로 설정
        timeSinceLastDecrease = 0f;//시간 초기화
        gameTimer = 0f;//게임 타이머 초기화
        currentDifficultyLevel = 0;//난이도 레벨 초기화
        currentNormalSpawnCount = InitialNormalSpawnCount;//시작 시 동시 스폰 개수 초기화   
        UpdateMonsterLevelText();


        //EnemySpawn 인스턴스 찾아서 저장
        if (enemySpawnRef == null)
            Debug.LogError("EnemyDifficulty: EnemySpawn 스크립트를 씬에서 찾을 수 없어!");
        else
            //EnemySpawn 스크립트에 초기 동시 스폰 개수 설정
            enemySpawnRef.SetNormalSpawnCount(currentNormalSpawnCount);


        if(textalimManager == null)
            Debug.LogError("EnemyDifficulty: TextAlimManager 스크립트를 씬에서 찾을 수 없어!");


        if (notificationText != null)//게임 시작 시 UI 텍스트를 비움
            notificationText.text = "";
    }
    
    void Update()
    {
        //플레이어가 죽었을 때는 난이도 조절을 멈추도록 여기에 조건문을 추가해야 해.
        //예를 들어, if (Player.Instance != null && !Player.Instance.isDead) { ... }
        //Player 스크립트에도 싱글톤 패턴을 적용했다면 이렇게 접근할 수 있어.

        //게임 시간 경과 및 스탯 난이도 레벨 증가 로직
        gameTimer += Time.deltaTime;
        if(gameTimer >= (currentDifficultyLevel + 1) * statInterval)
        {
            currentDifficultyLevel++;
            Debug.Log($"몬스터 스탯 난이도 레벨 증가! 현재 레벨: {currentDifficultyLevel}, 총 경과 시간: {gameTimer:F2}s");
            //F2는 C# 문자열 포맷팅에서 부동소수점(Float) 숫자를 소수점 둘째 자리까지 표시하라는 의미
            //gameTimer 값이 123.45678f 라고 예시) F0이면 123, F1이면 123.5, F2이면 123.46


            UpdateMonsterLevelText();//몬스터 레벨이 증가한 직후, EnemyDifficultyLevelText UI를 업데이트할 함수를 호출해
                                     //UpdateMonsterLevelText() 함수는 currentDifficultyLevel 값이 변할 때마다 호출되어야 해
                                     //그래서! currentDifficultyLevel++ 코드 바로 아래에 추가하는 게 가장 적절한거야.
                                     //밑의 if문들은 currentDifficultyLevel이 증가하면 그걸 받을 스크립트가 작동이 되는지 확인 후 값을 전달하는거야


            //스탯 난이도 레벨 증가 시 동시 스폰 개수 업데이트
            currentNormalSpawnCount = Mathf.Min(InitialNormalSpawnCount + (currentDifficultyLevel * SpawnCountIncreasePerLevel));
            Debug.Log($"Normal 몬스터 동시 스폰 개수 증가! 현재 개수: {currentNormalSpawnCount}마리");
            if (enemySpawnRef != null)
                enemySpawnRef.SetNormalSpawnCount(currentNormalSpawnCount);//EnemySpawn에 변경된 개수 전달


            if (notificationText != null)//EnemyDifficultyStatsText UI로 전달
            {
                notificationText.text = $"<color=red>몬스터가 더 강해졌습니다! (레벨 {currentDifficultyLevel})</color>";
                Invoke("ClearNotification", 3f);
            }
        }
        //시간이 흐름에 따라 스폰 주기를 줄이는 로직
        timeSinceLastDecrease += Time.deltaTime;//마지막 감소 후 시간을 계속 더해.

        if (timeSinceLastDecrease >= DecreaseInterval)//설정한 감소 간격이 지났으면
        {
            //currentNormalSpawnTime을 감소시키되, minNormalSpawnTime 이하로는 내려가지 않게 해
            currentNormalSpawnTime = Mathf.Max(MinNormalSpawnTime, currentNormalSpawnTime - SpawnTimeDecreaseRate);
            timeSinceLastDecrease = 0f;//시간 초기화 (다음 감소 간격을 위해)
            Debug.Log($"Normal 몬스터 스폰 주기 감소! 현재 주기: {currentNormalSpawnTime}s");

            if (enemySpawnRef != null)//EnemySpawn에게 새로운 스폰 주기를 알려줘!
                enemySpawnRef.SetNormalSpawnTime(currentNormalSpawnTime);
                //EnemySpawn 스크립트의 SetNormalSpawnTime() 함수
        }
    }

    private void UpdateMonsterLevelText()//EnemyDifficultyLevelText UI로 보낼 함수
    {                                    //notificationText UI랑 다르게 Lv.0 ~ 1 ~ 2 증가하게 할거야
        if (enemyLevelText != null)
            enemyLevelText.text = $"몬스터 Lv.{currentDifficultyLevel}";
    }


    private void ClearNotification()//이 함수는 notificationText UI 알림을 화면에서 지워주는 역할
    {                               //이 함수는 보통 Invoke("ClearNotification", 3f);처럼 일정 시간 뒤에 자동으로 호출되도록 해서,
                                    //"몬스터가 강해졌습니다!" 같은 알림이 3초 후에 사라지게 만드는 용도로 쓰여.
        if (notificationText != null)
            notificationText.text = "";
    }


    public float GetSpawnTime()//EnemySpawn 스크립트가 이 값을 가져갈 수 있도록 함수 생성
    {                          //이 함수는 '현재 몬스터가 스폰되는 주기(시간)' 값을 다른 스크립트에게 알려주는 역할
                               //return currentNormalSpawnTime; 이 코드는 EnemyDifficulty 스크립트 내부에서 관리하고 있는,
                               //currentNormalSpawnTime이라는 변수 값을 그대로 반환해 줘.
        return currentNormalSpawnTime;//현재 계산된 스폰 주기를 돌려줘.
    }

    public int GetSpawnCount()
    {//이 함수는 '현재 한 번에 스폰되는 몬스터 마릿수' 값을 다른 스크립트에게 알려주는 역할
     //return currentNormalSpawnCount; 이 코드는 EnemyDifficulty 스크립트 내부의 currentNormalSpawnCount 변수 값을 그대로 반환해 줘.
     //GetCurrentNormalSpawnTime() 함수와 역할이 똑같아. 단지 float 타입의 스폰 주기 대신,
     //int 타입의 스폰 개수를 돌려줄 뿐
        return currentNormalSpawnCount;
    }

    public float GetAdjustedMonsterStat(float baseStat, StatType statType)//몬스터 스탯을 현재 난이도에 맞춰 조정하여 반환하는 함수
    {       
        float increaseRatio = 0f;

        switch (statType)
        {
            case StatType.AttackDamage:
                increaseRatio = atkIncrease;
                break;
            case StatType.Health:
                increaseRatio = hpIncrease;
                break;
            case StatType.MoveSpeed:
                increaseRatio = speedIncrease;
                break;
            default:
                Debug.LogWarning($"EnemyDifficulty: 알 수 없는 스탯 타입 요청됨 - {statType}");
                break;
        }
        float adjustedStat = baseStat * (1f + increaseRatio * currentDifficultyLevel);
        return adjustedStat;
    }
}
