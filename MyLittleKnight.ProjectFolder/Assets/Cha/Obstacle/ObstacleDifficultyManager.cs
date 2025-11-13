using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;//TextMeshPro UI랑 같이 써야 하니까 추가하자구!
using UnityEngine;

public class ObstacleDifficultyManager : MonoBehaviour
{
    //--- ObstacleFireBall을 설정할 난이도 변수들 ---
    [Header("발사체 난이도 증가 시간")]
    public float FireBallLevelUpTime = 20f;//FireBall의 속도, 데미지, 생성 주기가 동시에 강화되는 시간 간격(f초)

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
    [Header("가시 난이도 증가 시간")]
    public float SpikeLevelUpTime = 20f;//가시 난이도 증가 간격 (데미지와 디버프 시간을 동시에 조절)

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
    private float timeSinceLastFireBallIncrease = 0f;//마지막으로 발사체 강화를 올린 후 경과 시간(발사체 통합)
    private float currentFireballSpeed;//현재 게임에 적용되고 있는 발사체의 속도 (Start에서 초기화 후 Update에서 누적 증가
    private float currentFireballSpawnInterval;//현재 게임에 적용되고 있는 발사체의 생성 주기(Start에서 초기화 후 Update에서 누적 감소)
    private int currentFireballDamage;//현재 게임에 적용되고 있는 발사체의 데미지 (Start에서 초기화 후 Update에서 누적 증가)
    //가시함정
    private float timeSinceLastSpikeIncrease = 0f;//가시 증가 경과 시간
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
        bool levelUpJustHappened = false;//레벨업이 발생했는지 확인하는 플래그

        //이번 프레임에 어떤 개별 강화가 성공했는지 추적하는 로컬(지역변수) 플래그
        bool fireballSpeedIncreased = false;
        bool intervalDecreased = false;
        bool fireballDamageIncreased = false;
        bool spikeIncreased = false;

        //시간이 지남에 따라 난이도 조절
        timeSinceLastFireBallIncrease += Time.deltaTime;//발사체 통합 타이머 증가
        timeSinceLastSpikeIncrease += Time.deltaTime;//가시함정 증가 시간

        //FireBall속도 증가
        if (timeSinceLastFireBallIncrease >= FireBallLevelUpTime)
        {
            //발사체 속도 증가
            currentFireballSpeed = Mathf.Min(currentFireballSpeed + FireBallSpeedUp, MaxFireBallSpeed);
            Debug.Log($"발사체 속도 증가! 현재 속도: {currentFireballSpeed}");
            fireballSpeedIncreased = true;

            //생성 주기 감소
            currentFireballSpawnInterval = Mathf.Max(MinSpawnTime, currentFireballSpawnInterval - FireBallSpawnTimeDown);
            Debug.Log($"발사체 생성 주기 감소! 현재 주기: {currentFireballSpawnInterval}");
            intervalDecreased = true;

            //데미지 증가
            currentFireballDamage += FireBallDamageUp;
            Debug.Log($"발사체 데미지 증가! 현재 데미지: {currentFireballDamage}");
            fireballDamageIncreased = true;

            //통합 타이머 초기화 및 레벨업 플래그 설정
            timeSinceLastFireBallIncrease = 0f;
            levelUpJustHappened = true;
        }

        //가시 난이도 증가
        if (timeSinceLastSpikeIncrease >= SpikeLevelUpTime)
        {
            //데미지 증가(최대값 제한)
            currentSpikeDamage = Mathf.Min(currentSpikeDamage + SpikeDamageUp, MaxSpikeDamage);

            //디버프 데미지 증가(최대값 제한)
            currentDebuffDamageValue = Mathf.Min(currentDebuffDamageValue + DebuffDamageUp, MaxDebuffDamage);

            //디버프 지속 시간 감소(최소값 제한)
            currentDebuffDuration = Mathf.Min(MaxDebuffTime, currentDebuffDuration + SpikeDebuffTimeUP);

            //생성 주기 감소(최소값 제한)
            currentSpikeSpawnInterval = Mathf.Max(MinSpikeSpawnTime, currentSpikeSpawnInterval - SpikeSpawnTimeDown);

            //가시 수명 증가
            currentSpikeDuration = Mathf.Min(currentSpikeDuration + SpikeLifeTimeUp, MaxSpikeLifeTime);

            //로그 통합:모든 변수가 업데이트된 후 한 번에 로그 출력
            Debug.Log($"가시 난이도 증가! 데미지: {currentSpikeDamage}, 디버프 시간: {currentDebuffDuration}, 디버프 데미지: {currentDebuffDamageValue}");
            timeSinceLastSpikeIncrease = 0f;  
            levelUpJustHappened = true;
            spikeIncreased = true;
        }
       
        if (levelUpJustHappened)//모든 강화가 끝난 후, 레벨업 플래그를 확인하여 레벨 증가 UI 업데이트
        {                       //이 로직은 모든 개별 강화 if문이 끝난 후, 강화가 발생했음을 확인하는 최종 검증 단계
            currentLevel++;
            UpdateLevelText();

            //성공한 항목과 실패한 항목을 구분해서 메시지 작성
            string successful = "성공: ";
            string missed = "지연/미발동(오차): ";
            bool allSucceeded = true;

            //성공 항목 체크, 띄워쓰기로 가독성도 높이자!
            if (fireballSpeedIncreased) successful += " [FireBall 속도] "; else { missed += " [FireBall 속도] "; allSucceeded = false; }
            if (intervalDecreased) successful += " [FireBall 생성 주기] "; else { missed += " [FireBall 생성 주기] "; allSucceeded = false; }
            if (fireballDamageIncreased) successful += " [FireBall 데미지] "; else { missed += " [FireBall 데미지] "; allSucceeded = false; }
            if (spikeIncreased) successful += " [가시 함정] "; else { missed += " [가시 함정] "; allSucceeded = false; }

            //최종 디버그 로그 출력
            if (allSucceeded)
                Debug.Log($"★★★ 전체 장애물 레벨업! (Lv.{currentLevel}) 완벽 동기화 성공! ({successful})");
            else//하나라도 놓쳤을 경우 상세 로그 출력 
                Debug.LogWarning($"★★★ 전체 장애물 레벨업! (Lv.{currentLevel}) 부분 동기화 됨. ★★★\n[성공 항목]: {successful.Trim()}\n[지연 항목]: {missed.Trim()}");
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
        if (ObstacleLevelText != null)
        {
            ObstacleLevelText.text = $"장애물 Lv.{currentLevel}";
        }
    }
}

