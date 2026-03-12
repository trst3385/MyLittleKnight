using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using System;//Action(옵저버)을 쓰기 위해 추가


public class EnemyDifficulty : MonoBehaviour
{
    //---[옵저버 패턴: 방송국 설정]------
    public static event Action<int> OnMonsterLevelUp;//현재 난이도 레벨 (레벨 텍스트 갱신용)
    public static event Action<string> OnDifficultyNotification;//강화 알림 메시지 (3초 알림용)
    //-----------------------------------


    public float CurrentNormalSpawnTime => currentNormalSpawnTime;
    //외부에서 값을 읽을 수 있게 해주는 읽기 전용 속성
    public int CurrentNormalSpawnCount => currentNormalSpawnCount;


    public enum StatType { AttackDamage, Health, MoveSpeed }//스탯 타입을 구분하기 위한 Enum

    [Header("Normal 몬스터 스폰 초기 스폰 시간")]
    public float NormalSpawnTime = 4f;//게임 시작 시 Normal 몬스터의 스폰 주기 (예: 4초)
    [Header("Normal 몬스터 동시 스폰 개수 조절")]
    public int NormalSpawnCount = 1;//게임 시작 시 Normal 몬스터 동시 스폰 개수
    public int NormalSpawnCountUp = 1;//난이도 레벨마다 동시 스폰 개수 증가량
    public int MaxNormalSpawnCount = 5;//동시 스폰 개수 최대치 (너무 많아지는 것 방지)


    [Header("몬스터 스탯 난이도 조절")]//몬스터 스탯 난이도 조절 변수들
    [SerializeField] private float StatLevelUpTime = 20f;//몬스터 스탯이 강해지는 시간 간격 (초)
    [SerializeField] private float AtkIncreaseRatio = 0.2f;//난이도 레벨마다 몬스터 공격력 증가 비율 (20% = 0.2) -> 20%씩
    [SerializeField] private float HPIncreaseRatio = 0.2f;//난이도 레벨마다 몬스터 체력 증가 비율 (20% = 0.2) -> 20%씩
    [SerializeField] private float SpeedIncreaseRatio = 0.15f;//난이도 레벨마다 몬스터 이동 속도 증가 비율 (1% = 0.01) -> 1%씩


    public static EnemyDifficulty Instance { get; private set; }//싱글톤 설정

    //내부에서 사용할 변수들
    private float timeSinceLastLevelUp = 0f;//게임 시작 후 총 경과 시간(타이머 리셋 방식)
    private int currentDifficultyLevel = 0;//현재 몬스터 스탯 난이도 레벨
    private float currentNormalSpawnTime;//현재 Normal 몬스터가 스폰되는 실제 주기 (게임 시작 시 고정값)
    private int currentNormalSpawnCount;//현재 동시 스폰 몬스터 개수를 저장할 변수
                                              

    void Awake()
    {
        //싱글톤 설정 (중복 방지 및 유지)
        if (Instance == null)
            Instance = this;
        else { Destroy(gameObject); return; }

        //시작 시 초기화
        currentNormalSpawnTime = NormalSpawnTime;//게임 시작 시 초기 스폰 주기로 설정
        currentNormalSpawnCount = NormalSpawnCount;//시작 시 동시 스폰 개수 초기화
        timeSinceLastLevelUp = 0f;//게임 타이머 초기화
        currentDifficultyLevel = 0;//난이도 레벨 초기화
    }

    void Start()
    {
        OnMonsterLevelUp?.Invoke(currentDifficultyLevel);//시작하자마자 레벨 0이라고 방송 한 번 쏴주기
    }
    
    void Update()
    {
        //TimeFreeze로 시간이 멈췄는지 체크하고, 멈췄다면 타이머 증가 로직을 건너뜀
        if (TimeFreeze.Instance != null && TimeFreeze.Instance.IsTimeFrozen) return;


        //게임 시간 경과 및 스탯 난이도 레벨 증가 로직
        timeSinceLastLevelUp += Time.deltaTime;//프레임에 맞게 deltaTime으로 Update
        if (timeSinceLastLevelUp >= StatLevelUpTime)
        {
            currentDifficultyLevel++;

            //[옵저버] 방송만 보낸다. 누가 듣는지는 상관 안해!(이 신호를 받을려는 스크립트만 받기)
            OnMonsterLevelUp?.Invoke(currentDifficultyLevel);
            OnDifficultyNotification?.Invoke($"<color=red>몬스터가 더 강해졌다! (레벨 {currentDifficultyLevel})</color>");

            //스탯 난이도 레벨 증가 시 동시 스폰 개수 업데이트
            currentNormalSpawnCount = Mathf.Min(MaxNormalSpawnCount, NormalSpawnCount + (currentDifficultyLevel * NormalSpawnCountUp));
            if (EnemySpawn.Instance != null)//EnemySpawn에 변경된 개수 전달
                EnemySpawn.Instance.SetNormalSpawnCount(currentNormalSpawnCount);

            timeSinceLastLevelUp = 0f;//타이머 리셋
        } 
    }

    public float GetAdjustedMonsterStat(float baseStat, StatType statType)//몬스터 스탯을 현재 난이도에 맞춰 조정하여 반환하는 함수
    {                                                                     //Enemy 스크립트가 이 함수를 호출해
        float increaseRatio = 0f;

        switch (statType)
        {
            case StatType.AttackDamage: increaseRatio = AtkIncreaseRatio; break;
            case StatType.Health: increaseRatio = HPIncreaseRatio; break;
            case StatType.MoveSpeed: increaseRatio = SpeedIncreaseRatio; break;
        }
        return baseStat * (1f + increaseRatio * currentDifficultyLevel);
    }
} 
