using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;//TextMeshPro UI랑 같이 써야 하니까 추가하자구!
using UnityEngine;

public class ObstacleDifficultyManager : MonoBehaviour
{
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
    public float MaxFireBallSpeed = 30f;//최대 발사체 속도
    public float FireBallSpeedUp = 0.5f;//속도 증가량. 발사체의 속도가 한 번 빨라질 때마다 몇씩 증가할지 정하는 값
                                          
    [Header("발사체 생성 주기 조절")]
    public float FireBallSpawnTime = 3f;//초기 생성 주기. 게임이 시작될 때 발사체가 생성되는 기본 시간 간격.
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
    public float SpikeSpawnTime = 5f;//초기 가시 생성 주기 (SpikeSpawn 스크립트에서 가져옴)
    public float MinSpikeSpawnTime = 2f;//최대 1초마다 가시 생성 (난이도가 높아져도 이보다 더 자주 나오지는 않음)
    public float SpikeSpawnTimeDown = 0.5f;//가시 생성 주기 감소량 (0.5초씩 감소)

    [Header("가시 수명 조절")]
    public float SpikeLifeTime = 10f;//초기 가시 수명 (초)
    public float MaxSpikeLifeTime = 30f;//최대 가시 수명 (난이도에 따라 늘어날 수 있음)
    public float SpikeLifeTimeUp = 3f;//난이도 레벨당 수명 증가량


    [Header("UI 알림")]
    public TextMeshProUGUI ObstacleLevelText;//UI 텍스트를 담을 변수


    //--- 내부에서 사용할 변수들, 현재의 변수 상태들을 담을 변수들 ---

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

    // 어디서든 이 스크립트에 접근할 수 있게 해주는 '싱글톤' 패턴
    public static ObstacleDifficultyManager Instance { get; private set; }


    void Awake()
    {   //싱글톤 패턴이 스크립트의 유일한 인스턴스 존재를 보장
        //1.이미 인스턴스가 존재하면(중복이면)
        if (Instance != null && Instance != this) Destroy(gameObject);//이 중복된 오브젝트를 파괴하고 종료
        else
        {
            Instance = this;//2.인스턴스가 없으면, '나'를 유일한 인스턴스로 지정
            DontDestroyOnLoad(gameObject);//씬이 바뀌어도 오브젝트를 파괴하지 않고 유지
        }
    }
    void Start()
    {
        //게임이 시작될 때, 인스펙터에 설정해 둔 초기값(initial)을,
        //실제 게임에서 사용할 현재 값(current)에 넣어주는 역할을 해.
        //current... 변수를 **게임의 변화하는 상태**를 담는 그릇으로 사용하고,
        //initial... 변수를 **변하지 않는 기준점**으로 사용해서 역할을 명확히 분리.
        currentFireballSpeed = FireBallSpeed;
        currentFireballSpawnInterval = FireBallSpawnTime;
        currentFireballDamage = FireBallDamage;

        //가시 초기화
        currentSpikeDamage = SpikeDamage;
        currentDebuffDuration = SpikeDebuffTime;
        currentSpikeSpawnInterval = SpikeSpawnTime  ;
        currentSpikeDuration = SpikeLifeTime;
        currentDebuffDamageValue = DebuffDamage;

        //게임 시작 시 UI 텍스트에 초기 레벨을 표시
        UpdateLevelText();
    }
    
    void Update()
    {
        bool levelUpJustHappened = false;//장애물의 레벨업이 발생했는지 확인하는 플래그

        timeSinceLastLevelUp += Time.deltaTime;//통합 강화 시간 타이머 증가


        if (timeSinceLastLevelUp >= LevelUpTime)//장애물 통합 강화 조건
        {
            //FireBall 강화 로직
            currentFireballSpeed = Mathf.Min(currentFireballSpeed + FireBallSpeedUp, MaxFireBallSpeed);
            currentFireballSpawnInterval = Mathf.Max(MinSpawnTime, currentFireballSpawnInterval - FireBallSpawnTimeDown);
            currentFireballDamage += FireBallDamageUp;
            Debug.Log($"발사체 강화 완료! 속도: {currentFireballSpeed}, 주기: {currentFireballSpawnInterval}, 데미지: {currentFireballDamage}");

            //가시 강화 로직
            currentSpikeDamage = Mathf.Min(currentSpikeDamage + SpikeDamageUp, MaxSpikeDamage);
            currentDebuffDamageValue = Mathf.Min(currentDebuffDamageValue + DebuffDamageUp, MaxDebuffDamage);
            currentDebuffDuration = Mathf.Min(MaxDebuffTime, currentDebuffDuration + SpikeDebuffTimeUP);
            currentSpikeSpawnInterval = Mathf.Max(MinSpikeSpawnTime, currentSpikeSpawnInterval - SpikeSpawnTimeDown);
            currentSpikeDuration = Mathf.Min(currentSpikeDuration + SpikeLifeTimeUp, MaxSpikeLifeTime);
            Debug.Log($"가시 강화 완료! 데미지: {currentSpikeDamage}, 디버프 시간: {currentDebuffDuration}, 디버프 데미지: {currentDebuffDamageValue}");

            //강화가 완료되었으니, 타이머를 리셋하고 레벨업 플래그 설정
            timeSinceLastLevelUp = 0f;//통합 타이머 리셋
            levelUpJustHappened = true;//모든 장애물이 레벨업 되었다고 true
        }
        

        if (levelUpJustHappened)//모든 강화가 끝난 후, 레벨업 플래그를 확인하여 레벨 증가 UI 업데이트
        {                       //이 로직은 모든 개별 강화 if문이 끝난 후, 강화가 발생했음을 확인하는 최종 검증 단계
            currentLevel++;
            UpdateLevelText();

            Debug.Log($"★★★ 전체 장애물 레벨업! (Lv.{currentLevel}) 완벽 동기화 성공! FireBall/가시 동시 강화 완료!");
        }
    }

    //FireBall, 발사체가 생성될 때, 현재 적용된 속도, 생성 주기, 데미지를 반환
    public float GetCurrentFireBallSpeed() => currentFireballSpeed;
    public int GetCurrentFireBallDamage() => currentFireballDamage;
    public float GetCurrentSpawnInterval() => currentFireballSpawnInterval;
    public float GetFireBallDestroyTime() => FireBallDestroyTime;

    //가시, 실시간으로 가시의 데미지, 디버프 데미지를 반환
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

