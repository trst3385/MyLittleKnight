using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using TMPro;//TextMeshPro를 사용하기 위해 추가


public class EnemyDifficulty : MonoBehaviour
{
    //외부에서 currentNormalSpawnTime 값을 읽을 수 있게 해주는 읽기 전용 속성
    public float CurrentNormalSpawnTime => currentNormalSpawnTime;
    //외부에서 currentNormalSpawnCount 값을 읽을 수 있게 해주는 읽기 전용 속성
    public int CurrentNormalSpawnCount => currentNormalSpawnCount;
    /* * => (람다 연산자): '식 본문 멤버(Expression-bodied Member)' 문법으로, 
     * 값을 계산 없이 단순히 반환할 때 get { return ... } 구문을 생략하고 간결하게 처리함.
     * - 역할: Get 접근자를 대체하며, 뒤의 식(currentNormalSpawnTime) 값을 즉시 반환함.
     * - 장점: 읽기 전용(Read-Only)임을 명확히 하고, 코드 간결성을 높임. (set 접근자 가질 수 없음)
    */

    //스탯 타입을 구분하기 위한 Enum
    public enum StatType
    {
        AttackDamage,
        Health,
        MoveSpeed
    }

    //[SerializeField]가 붙은 private라서 인스펙터에 보이니 헤더의 변수는 Pascal Case로 적용했어
    //인스펙터에서 설정할 변수들
    [Header("Normal 몬스터 스폰 초기 스폰 시간")]
    public float NormalSpawnTime = 4f;//게임 시작 시 Normal 몬스터의 스폰 주기 (예: 4초)

    [Header("Normal 몬스터 동시 스폰 개수 조절")]
    public int NormalSpawnCount = 1;//게임 시작 시 Normal 몬스터 동시 스폰 개수
    public int NormalSpawnCountUp = 1;//난이도 레벨마다 동시 스폰 개수 증가량
    public int MaxNormalSpawnCount = 5;//동시 스폰 개수 최대치 (너무 많아지는 것 방지)

    //몬스터 스탯 난이도 조절 변수들
    [Header("몬스터 스탯 난이도 조절")]
    [SerializeField] private float StatLevelUpTime = 20f;//몬스터 스탯이 강해지는 시간 간격 (초)
    [SerializeField] private float AtkIncreaseRatio = 0.2f;//난이도 레벨마다 몬스터 공격력 증가 비율 (20% = 0.2) -> 20%씩
    [SerializeField] private float HPIncreaseRatio = 0.2f;//난이도 레벨마다 몬스터 체력 증가 비율 (20% = 0.2) -> 20%씩
    [SerializeField] private float SpeedIncreaseRatio = 0.15f;//난이도 레벨마다 몬스터 이동 속도 증가 비율 (1% = 0.01) -> 1%씩

    
    [Header("UI, 오브젝트 연결")]
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private TextMeshProUGUI enemyLevelText;
    [SerializeField] private TextAlimManager textalimManager;
    [SerializeField] private EnemySpawn enemySpawn;
    

    public static EnemyDifficulty Instance { get; private set; }

    //내부에서 사용할 변수들
    private float timeSinceLastLevelUp = 0f;//게임 시작 후 총 경과 시간(타이머 리셋 방식)
    private int currentDifficultyLevel = 0;//현재 몬스터 스탯 난이도 레벨
    private float currentNormalSpawnTime;//현재 Normal 몬스터가 스폰되는 실제 주기 (게임 시작 시 고정값)
    private int currentNormalSpawnCount;//현재 동시 스폰 몬스터 개수를 저장할 변수
                                              

    void Awake()
    {
        //게임 시작 시 이 스크립트의 유일한 인스턴스를 설정해.
        if(Instance != null && Instance != this) Destroy(gameObject);//이미 인스턴스가 있으면 자신을 파괴(중복 방지)

        else
        {
            Instance = this;//자신이 유일한 인스턴스가 됨
            DontDestroyOnLoad(gameObject);//씬이 바뀌어도 파괴되지 않게 (게임 전체 난이도 관리용)
        }
    }
    void Start()
    {
        currentNormalSpawnTime = NormalSpawnTime;//게임 시작 시 초기 스폰 주기로 설정
        currentNormalSpawnCount = NormalSpawnCount;//시작 시 동시 스폰 개수 초기화
        timeSinceLastLevelUp = 0f;//게임 타이머 초기화
        currentDifficultyLevel = 0;//난이도 레벨 초기화
        UpdateMonsterLevelText();
         

        //EnemySpawn 인스턴스 찾아서 저장
        if (enemySpawn == null) Debug.LogError("EnemyDifficulty: EnemySpawn 스크립트를 씬에서 찾을 수 없어!");
        if (textalimManager == null) Debug.LogError("EnemyDifficulty: TextAlimManager 스크립트를 씬에서 찾을 수 없어!");

        if (notificationText != null) notificationText.text = "";//게임 시작 시 UI 텍스트를 비움
    }
    
    void Update()
    {
        //게임 시간 경과 및 스탯 난이도 레벨 증가 로직
        timeSinceLastLevelUp += Time.deltaTime;
        if(timeSinceLastLevelUp >= StatLevelUpTime)
        {
            currentDifficultyLevel++;
            Debug.Log($"몬스터 스탯 난이도 레벨 증가! 현재 레벨: {currentDifficultyLevel}");


            UpdateMonsterLevelText();//몬스터 레벨이 증가한 직후, EnemyDifficultyLevelText UI를 업데이트할 함수를 호출해


            //스탯 난이도 레벨 증가 시 동시 스폰 개수 업데이트
            currentNormalSpawnCount = Mathf.Min(MaxNormalSpawnCount, NormalSpawnCount + (currentDifficultyLevel * NormalSpawnCountUp));
            if (enemySpawn != null) enemySpawn.SetNormalSpawnCount(currentNormalSpawnCount);//EnemySpawn에 변경된 개수 전달

            if (notificationText != null)//EnemyDifficultyStatsText UI로 전달
            {
                notificationText.text = $"<color=red>몬스터가 더 강해졌습니다! (레벨 {currentDifficultyLevel})</color>";
                Invoke("ClearNotification", 3f);
            }
            timeSinceLastLevelUp = 0f;//타이머 리셋
        } 
    }

    private void UpdateMonsterLevelText()//EnemyDifficultyLevelText UI로 보낼 함수
    {                                    //notificationText UI랑 다르게 Lv.0 ~ 1 ~ 2 증가하게 할거야
        if (enemyLevelText != null) enemyLevelText.text = $"몬스터 Lv.{currentDifficultyLevel}";
    }
    private void ClearNotification()//이 함수는 notificationText UI 알림을 화면에서 지워주는 역할
    {                               //이 함수는 보통 Invoke("ClearNotification", 3f);처럼 일정 시간 뒤에 자동으로 호출되도록 해서,
                                    //"몬스터가 강해졌습니다!" 같은 알림이 3초 후에 사라지게 만드는 용도로 쓰여.
        if (notificationText != null) notificationText.text = "";
    }


    public float GetAdjustedMonsterStat(float baseStat, StatType statType)//몬스터 스탯을 현재 난이도에 맞춰 조정하여 반환하는 함수
    {       //Enemy 스크립트가 이 함수를 호출해
        float increaseRatio = 0f;

        switch (statType)
        {
            case StatType.AttackDamage:
                increaseRatio = AtkIncreaseRatio;
                break;
            case StatType.Health:
                increaseRatio = HPIncreaseRatio;
                break;
            case StatType.MoveSpeed:
                increaseRatio = SpeedIncreaseRatio;
                break;
            default:
                Debug.LogWarning($"EnemyDifficulty: 알 수 없는 스탯 타입 요청됨 - {statType}");
                break;
        }
        float adjustedStat = baseStat * (1f + increaseRatio * currentDifficultyLevel);
        return adjustedStat;
    }
} 
