using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;//TextMeshPro UI랑 같이 써야 하니까 추가하자구!
using UnityEngine;

public class ObstacleDifficultyManager : MonoBehaviour
{ 
    //--- ObstacleFireBall을 설정할 난이도 변수들 ---
    [Header("발사체 속도 조절")]//헤더는 이건 순전히 유니티 인스펙터 창을 정리하고 보기 좋게 만들기 위한 기능이야
    public float InitialFireBallSpeed = 5f;//초기 발사체 속도. 게임이 시작될 때 발사체가 움직이는 기본 속도
    public float MaxFireBallSpeed = 30f;//최대 발사체 속도
    public float SpeedIncreaseRate = 0.5f;//속도 증가량. 발사체의 속도가 한 번 빨라질 때마다 몇씩 증가할지 정하는 값
    public float SpeedIncreaseInterval = 20f;//속도 증가 간격. n 초마다 발사체의 속도를 한 단계씩 올릴지 정하는 시간 간격.
   
    [Header("발사체 데미지 조절")]
    public int InitialDamage = 5;//초기 발사체 데미지
    public int DamageIncrease = 2;//데미지 증가량. 발사체의 데미지가 한 번 강해질 때마다 몇씩 증가할지 정하는 값
    public float DamageIncreaseInterval = 20f;//데미지 증가 간격. 몇 초마다 발사체의 데미지를 한 단계씩 올릴지 정하는 시간 간격. 20초로 설정했으니, 20초마다 damageIncrease만큼 데미지가 증가하겠지.

    [Header("발사체 생성 주기 조절")]
    public float InitialSpawnInterval = 3f;//초기 생성 주기. 게임이 시작될 때 발사체가 생성되는 기본 시간 간격.
    public float MinSpawnInterval = 2f;//최대 생성 주기. 난이도가 계속 올라가도 이 시간보다 더 짧아지지는 않아. 최대 생성되는 시간 간격.
    public float IntervalDecreaseRate = 0.5f;//주기 감소량. 발사체 생성 주기가 한 번 줄어들 때마다 몇 초씩 줄일지 정하는 값이야.
    public float IntervalDecreaseInterval = 20f;//주기 감소 간격. 몇 초마다 생성 주기를 한 단계씩 줄일지 정하는 시간 간격. 

    [Header("가시 함정 데미지 조절")]
    public float InitialSpikeDamage = 1f;//초기 밟았을 때 데미지
    public float DamageIncreaseSpike = 0.5f;//가시 데미지 증가량
    public float MaxSpikeDamage = 10f;//최대 밟았을 때 데미지

    [Header("가시 디버프 조절")]
    public float InitialDebuffDuration = 3f;//초기 디버프 지속 시간
    public float DurationIncreaseRate = 1.0f;//디버프 시간 감소량
    public float MaxDebuffDuration = 10f;//최대 디버프 지속 시간
    public float SpikeIncreaseInterval = 20f;//가시 난이도 증가 간격 (FireBall과 같은 20초로 통일)



    [Header("UI 알림")]
    public TextMeshProUGUI ObstacleLevelText;//UI 텍스트를 담을 변수

    //--- 내부에서 사용할 변수들, 현재의 변수 상태들을 담을 변수들 ---

    //FireBall 발사체
    private float timeSinceLastSpeedIncrease = 0f;//마지막으로 발사체 속도를 올린 후 경과 시간 (Interval과 비교용)
    private float timeSinceLastIntervalDecrease = 0f;//마지막으로 생성 주기를 줄인 후 경과 시간(Interval과 비교용
    private float timeSinceLastDamageIncrease = 0f;//마지막으로 데미지를 올린 후 경과 시간 (Interval과 비교용)
    private float currentFireballSpeed;//현재 게임에 적용되고 있는 발사체의 속도 (Start에서 초기화 후 Update에서 누적 증가
    private float currentFireballSpawnInterval;//현재 게임에 적용되고 있는 발사체의 생성 주기(Start에서 초기화 후 Update에서 누적 감소)
    private int currentFireballDamage;//현재 게임에 적용되고 있는 발사체의 데미지 (Start에서 초기화 후 Update에서 누적 증가)
    //가시함정
    private float timeSinceLastSpikeIncrease = 0f;//가시 증가 경과 시간
    private float currentSpikeDamage;//현재 가시 데미지
    private float currentDebuffDuration;//현재 디버프 지속 시간
    
    private int currentLevel = 0;//현재 난이도 레벨을 저장할 변수(난이도 강화 시점마다 1씩 증가)

    // 어디서든 이 스크립트에 접근할 수 있게 해주는 '싱글톤' 패턴
    public static ObstacleDifficultyManager Instance { get; private set; }


    void Awake()
    {//함수 안에 있는 if문은 딱 한 가지 목적을 위해 존재해. 이 스크립트를 가진 오브젝트가 게임에 딱 하나만 존재하도록 보장하는 것
        if (Instance != null && Instance != this)//만약 이미 '유일한 인스턴스'가 존재하고, 그게 지금 나 자신이 아니라면
            Destroy(gameObject);//이 코드가 "중복되는 오브젝트를 파괴하는 역할"을 해.
        else
        {
            Instance = this;//이건 "오, 아직 아무도 없네? 그럼 내가 바로 그 '유일한 인스턴스'가 되어야겠다!" 라는 뜻이야.
            DontDestroyOnLoad(gameObject);//EnemyDifficulty 스크립트에서도 있는거야. "씬이 바뀌어도 나를 파괴하지 마!" 라는 뜻이야.
        }
    }
    void Start()
    {
        //게임이 시작될 때, 인스펙터에 설정해 둔 초기값(initial)을,
        //실제 게임에서 사용할 현재 값(current)에 넣어주는 역할을 해.
        currentFireballSpeed = InitialFireBallSpeed;
        currentFireballSpawnInterval = InitialSpawnInterval;
        currentFireballDamage = InitialDamage;

        //가시 초기화
        currentSpikeDamage = InitialSpikeDamage;
        currentDebuffDuration = InitialDebuffDuration;

        //게임 시작 시 UI 텍스트에 초기 레벨을 표시
        UpdateLevelText();
    }
    
    void Update()
    {
        //시간이 지남에 따라 난이도 조절
        timeSinceLastSpeedIncrease += Time.deltaTime;
        timeSinceLastIntervalDecrease += Time.deltaTime;
        timeSinceLastDamageIncrease += Time.deltaTime;
        timeSinceLastSpikeIncrease += Time.deltaTime;//가시함정 증가 시간

        //FireBall속도 증가
        if (timeSinceLastSpeedIncrease >= SpeedIncreaseInterval)
        {
            currentFireballSpeed = Mathf.Min(currentFireballSpeed + SpeedIncreaseRate, MaxFireBallSpeed);
            timeSinceLastSpeedIncrease = 0f;
            Debug.Log($"발사체 속도 증가! 현재 속도: {currentFireballSpeed}");

            //난이도 레벨 증가 및 UI 업데이트는 여기에서 한 번만 호출
            currentLevel++;     //속도 증가 if문에 넣어도 다른 if문은 전부 같은 시간에 값을 증가 하잖아? 그래서 셋 중 하나에만 넣어야해.
            UpdateLevelText();  //각 if문 마다 넣으면 각 조건이 참일때마다 레벨1씩을 주니까 레벨 n초마다 레벨 1증가가 아니라 3증가가 되버리지!
                                //if문들이 동시에 작동해서 레벨이 증가해. 걱정마! 대신 셋의 강화 시간을 똑같이 해놔야해!   
        }
        //FireBall생성 주기 감소
        if (timeSinceLastIntervalDecrease >= IntervalDecreaseInterval)
        {
            currentFireballSpawnInterval = Mathf.Max(MinSpawnInterval, currentFireballSpawnInterval - IntervalDecreaseRate);
            timeSinceLastIntervalDecrease = 0f;
            Debug.Log($"발사체 생성 주기 감소! 현재 주기: {currentFireballSpawnInterval}");
        }
        //FireBall데미지 증가
        if (timeSinceLastDamageIncrease >= DamageIncreaseInterval)
        {
            currentFireballDamage += DamageIncrease;
            timeSinceLastDamageIncrease = 0f;
            Debug.Log($"발사체 데미지 증가! 현재 데미지: {currentFireballDamage}");
        }

        //가시 난이도 증가
        if (timeSinceLastSpikeIncrease >= SpikeIncreaseInterval)
        {
            // 데미지 증가(최대값 제한)
            currentSpikeDamage = Mathf.Min(currentSpikeDamage + DamageIncreaseSpike, MaxSpikeDamage);

            // 디버프 지속 시간 감소(최소값 제한)
            currentDebuffDuration = Mathf.Min(MaxDebuffDuration, currentDebuffDuration + DurationIncreaseRate);
            timeSinceLastSpikeIncrease = 0f;
            Debug.Log($"가시 난이도 증가! 데미지: {currentSpikeDamage}, 디버프 시간: {currentDebuffDuration}");
        }
    }

    //FireBall, 발사체가 생성될 때, 현재 적용된 속도, 생성 주기, 데미지를 반환
    public float GetCurrentFireBallSpeed() => currentFireballSpeed;//ObstacleFireBallSpawner 스크립트의 SpawnFireBall() 함수가 호출
    public int GetCurrentDamage() => currentFireballDamage;//SpawnFireBall()함수가 호출
    public float GetCurrentSpawnInterval() => currentFireballSpawnInterval;//Start, SpawnFireBall()함수가 호출
    
    //가시, 실시간으로 가시의 데미지, 디버프 데미지를 반환
    public float GetCurrentSpikeDamage() => currentSpikeDamage;//Spike 스크립의 ApplySpikeDamage()코루틴이 호출
    public float GetCurrentDebuffDuration() => currentDebuffDuration;//ApplyDebuffDamage()코루틴이 호출

    private void UpdateLevelText()//UI 텍스트를 업데이트 함수
    {
        if (ObstacleLevelText != null)
        {
            ObstacleLevelText.text = $"장애물 Lv.{currentLevel}";
        }
    }
}

