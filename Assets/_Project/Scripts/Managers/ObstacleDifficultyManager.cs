using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;//TextMeshPro UI랑 같이 써야 하니까 추가하자구!
using UnityEngine;

public class ObstacleDifficultyManager : MonoBehaviour
{
   
    public static ObstacleDifficultyManager Instance { get; private set; }//싱글톤 패턴 선언

    [Header("통합 장애물 레벨업 시간")]//발사체, 가시의 통합 강화 시간
    public float LevelUpTime = 20f;//전체 장애물의 통합 강화 주기
    private float timeSinceLastLevelUp = 0f;//통합 타이머

    //--- ObstacleFireBall을 설정할 난이도 변수들 ---
    [Header("발사체 파괴 시간 조절")]
    public float FireBallDestroyTime = 10f;//발사체가 사라지는 시간

    [Header("발사체 데미지 조절")]
    public int FireBallDamage = 3;//초기 발사체 데미지
    public int FireBallDamageUp = 2;//데미지 증가량. 발사체의 데미지가 한 번 강해질 때마다 몇씩 증가할지 정하는 값

    [Header("발사체 속도 조절")]//헤더는 이건 순전히 유니티 인스펙터 창을 정리하고 보기 좋게 만들기 위한 기능이야
    public float FireBallSpeed = 5f;//초기 발사체 속도. 게임이 시작될 때 발사체가 움직이는 기본 속도
    public float MaxFireBallSpeed = 20f;//최대 발사체 속도
    public float FireBallSpeedUp = 1f;//속도 증가량. 발사체의 속도가 한 번 빨라질 때마다 몇씩 증가할지 정하는 값
                                          
    [Header("발사체 생성 주기 조절")]
    public float FireBallSpawnTime = 6f;//초기 생성 주기. 게임이 시작될 때 발사체가 생성되는 기본 시간 간격.
    public float MinSpawnTime = 2f;//최대 생성 주기. 난이도가 계속 올라가도 이 시간보다 더 짧아지지는 않아. 최대 생성되는 시간 간격.
    public float FireBallSpawnTimeDown = 0.5f;//주기 감소량. 발사체 생성 주기가 한 번 줄어들 때마다 몇 초씩 줄일지 정하는 값이야.

    //--- Spike(가시)를 설정할 난이도 변수들 ---
    [Header("가시 틱 데미지 간격 조절")]
    public float SpikeDamageTickTime = 1f;//(초)f 마다 가시 데미지를 받기

    [Header("가시 데미지 조절")]
    public float SpikeDamage = 2f;//초기 밟았을 때 데미지, 디버프 데미지는 이 값의 50%
    public float SpikeDamageUp = 0.5f;//가시 데미지 증가량
    public float MaxSpikeDamage = 5f;//최대 밟았을 때 데미지

    [Header("가시 디버프 데미지")]
    public float DebuffDamage = 1f;//디버프 데미지 
    public float DebuffDamageUp = 0.5f;//디버프 데미지 증가량
    public float MaxDebuffDamage = 5f;//최대 디버프 데미지

    [Header("가시 디버프 조절")]
    public float SpikeDebuffTime = 3f;//초기 디버프 지속 시간
    public float SpikeDebuffTimeUP = 1.0f;//디버프 시간 증가량
    public float MaxDebuffTime = 10f;//최대 디버프 지속 시간

    [Header("가시 생성 주기 조절")]
    public float SpikeSpawnTime = 6f;//초기 가시 생성 주기 (SpikeSpawn 스크립트에서 가져옴)
    public float MinSpikeSpawnTime = 2f;//최대 1초마다 가시 생성 (난이도가 높아져도 이보다 더 자주 나오지는 않음)
    public float SpikeSpawnTimeDown = 0.5f;//가시 생성 주기 감소량 (0.5초씩 감소)

    [Header("가시 수명 조절")]
    public float SpikeLifeTime = 10f;//초기 가시 수명 (초)
    public float MaxSpikeLifeTime = 30f;//최대 가시 수명 (난이도에 따라 늘어날 수 있음)
    public float SpikeLifeTimeUp = 3f;//난이도 레벨당 수명 증가량


    [Header("UI 알림")]
    public TextMeshProUGUI ObstacleLevelText;//UI 텍스트를 담을 변수


    //--- 내부에서 사용할 변수들, 현재의 변수 상태들을 담을 변수들 ---
    private bool isGameStarted = false;//장애물 생성 시작
    private float startDelayCounter = 0f;//첫 등장 시간을 셀 카운터

    //FireBall 발사체
    private float currentFireballSpeed;//현재 게임에 적용되고 있는 발사체의 속도 (Start에서 초기화 후 Update에서 누적 증가
    private float currentFireballSpawnInterval;//현재 게임에 적용되고 있는 발사체의 생성 주기(Start에서 초기화 후 Update에서 누적 감소)
    private int currentFireballDamage;//현재 게임에 적용되고 있는 발사체의 데미지 (Start에서 초기화 후 Update에서 누적 증가)
    //가시함정
    private float currentSpikeDamage;//현재 가시 데미지
    private float currentDebuffDamageValue;//현재 디버프 데미지
    private float currentDebuffDuration;//현재 디버프 지속 시간
    private float currentSpikeSpawnInterval;//현재 가시 생성 주기
    private float currentSpikeDuration;//현재 가시 수명

    //모든 장애물 강화레벨 통합
    private int currentLevel = 0;//현재 난이도 레벨을 저장할 변수(난이도 강화 시점마다 1씩 증가)


    void Awake()
    {
        //싱글톤 패턴이 스크립트의 유일한 인스턴스 존재를 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        InitializeReferences();//참조 자동 연결
    }
    
    private void InitializeReferences()
    {
        if (ObstacleLevelText == null)//UI 텍스트 자동 찾기
        {
            GameObject textObj = GameObject.Find("ObstacleLevelText");
            if (textObj != null) ObstacleLevelText = textObj.GetComponent<TextMeshProUGUI>();
        }
    }

    void Start()
    {
        //게임이 시작될 때, 인스펙터에 설정해 둔 초기값(initial)을,
        //실제 게임에서 사용할 현재 값(current)에 넣어주는 역할을 해.
        //current... 변수를 **게임의 변화하는 상태**를 담는 그릇으로 사용하고,
        //initial... 변수를 **변하지 않는 기준점**으로 사용해서 역할을 명확히 분리.

        //FireBall 초기화
        currentFireballSpeed = FireBallSpeed;
        currentFireballSpawnInterval = FireBallSpawnTime;
        currentFireballDamage = FireBallDamage;

        //가시 초기화
        currentSpikeDamage = SpikeDamage;
        currentDebuffDuration = SpikeDebuffTime;
        currentSpikeSpawnInterval = SpikeSpawnTime  ;
        currentSpikeDuration = SpikeLifeTime;
        currentDebuffDamageValue = DebuffDamage;

        UpdateLevelText();//게임 시작 시 UI 텍스트에 초기 레벨을 표시
    }
    
    void Update()
    {
        //시작 직후 [5초 대기 로직] 여긴 장애물 등장 신호만 담당해
        if (!isGameStarted)
        {
            startDelayCounter += Time.deltaTime;
            if (startDelayCounter >= 5f)
            {
                isGameStarted = true;
                //여기서 UpdateLevelText()를 빼는 이유는, 
                //아래 레벨업 로직에서 어차피 20초마다 갱신해주기 때문.
                Debug.Log("장애물 생성 신호 활성화!");
            }
        }

        //TimeFreeze 스크립트의 시간정지 아이템 획득 후 시간이 멈췄다면, 난이도 타이머 증가 로직을 건너뛰고 바로 함수 종료
        if (TimeFreeze.Instance != null && TimeFreeze.Instance.IsTimeFrozen) return;

        timeSinceLastLevelUp += Time.deltaTime;//timeSinceLastLevelUp의 시간이 0..1...2..초 증가. 20초가 되면 강화 시작
        //누적된 게임 시간이 설정한 레벨업 주기(LevelUpTime(20초))에 도달하면 실제 강화 함수를 호출함
        if (timeSinceLastLevelUp >= LevelUpTime) LevelUpObstacles();
    }
    
    private void LevelUpObstacles()
    {
        //FireBall 강화
        currentFireballSpeed = Mathf.Min(currentFireballSpeed + FireBallSpeedUp, MaxFireBallSpeed);
        currentFireballSpawnInterval = Mathf.Max(MinSpawnTime, currentFireballSpawnInterval - FireBallSpawnTimeDown);
        currentFireballDamage += FireBallDamageUp;

        //가시 강화
        currentSpikeDamage = Mathf.Min(currentSpikeDamage + SpikeDamageUp, MaxSpikeDamage);
        currentDebuffDamageValue = Mathf.Min(currentDebuffDamageValue + DebuffDamageUp, MaxDebuffDamage);
        currentDebuffDuration = Mathf.Min(MaxDebuffTime, currentDebuffDuration + SpikeDebuffTimeUP);
        currentSpikeSpawnInterval = Mathf.Max(MinSpikeSpawnTime, currentSpikeSpawnInterval - SpikeSpawnTimeDown);
        currentSpikeDuration = Mathf.Min(currentSpikeDuration + SpikeLifeTimeUp, MaxSpikeLifeTime);

        //공통 데이터 갱신
        timeSinceLastLevelUp = 0f;
        currentLevel++;
        UpdateLevelText();

        Debug.Log($"★★★ 전체 장애물 레벨업! (Lv.{currentLevel}) ★★★");
    }


    //FireBall, 발사체가 생성될 때, 현재 적용된 속도, 생성 주기, 데미지를 반환
    public float GetCurrentFireBallSpeed() => currentFireballSpeed;
    public int GetCurrentFireBallDamage() => currentFireballDamage;
    public float GetCurrentSpawnInterval() => currentFireballSpawnInterval;
    public float GetFireBallDestroyTime() => FireBallDestroyTime;

    //가시, 실시간으로 가시의 데미지, 디버프 데미지를 반환

    public bool IsObstacleActionReady() => isGameStarted;//스폰 스크립트들이 "이제 장애물 만들어도 돼?"라고 물어볼 때 대답해주는 함수
    public float GetCurrentSpikeDamage() => currentSpikeDamage;//Spike 스크립트의 ApplySpikeDamage()코루틴이 호출
    public float GetCurrentDebuffDamage() => currentDebuffDamageValue;//Spike 스크립트의 ApplyDebuffDamage()으로 전달
    public float GetCurrentDebuffDuration() => currentDebuffDuration;//ApplyDebuffDamage()코루틴에게 전달
    public float GetCurrentSpikeSpawnInterval() => currentSpikeSpawnInterval;//SpikeSpawn 스크립트가 호출받을 함수
    public float GetCurrentSpikeDuration() => currentSpikeDuration;//SpikeSpawn에게 전달
    public float GetSpikeDamageInterval() => SpikeDamageTickTime;//Spike 스크립트로 데미지 틱 간격을 보내
    
    private void UpdateLevelText()//UI 텍스트를 업데이트 함수
    {
        if (ObstacleLevelText != null) ObstacleLevelText.text = $"장애물 Lv.{currentLevel}";
    }
}

