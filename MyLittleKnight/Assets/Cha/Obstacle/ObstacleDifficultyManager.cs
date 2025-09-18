using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR//UnityEditor 네임스페이스를 사용하고 있는 스크립트가 빌드에 포함될 때 발생해.
using static UnityEditor.ShaderData;
#endif//UnityEditor는 에디터에서만 작동하는 기능이라, 실제 게임 빌드에는 포함되면 안 되거든.
//#if UNITY_EDITOR 사용해 해당 코드를 다음과 같이 전처리기 지시문으로 감싸줘.
using TMPro;//TextMeshPro UI랑 같이 써야 하니까 추가하자구!

public class ObstacleDifficultyManager : MonoBehaviour
{
    //--- 인스펙터에서 설정할 난이도 변수들 ---
    [Header("발사체 속도 조절")]//헤더는 이건 순전히 유니티 인스펙터 창을 정리하고 보기 좋게 만들기 위한 기능이야
    public float InitialBoltSpeed = 5f;//초기 발사체 속도. 게임이 시작될 때 발사체가 움직이는 기본 속도
    public float MaxBoltSpeed = 20f;//최대 발사체 속도
    public float SpeedIncreaseRate = 0.5f;//속도 증가량. 발사체의 속도가 한 번 빨라질 때마다 몇씩 증가할지 정하는 값
    public float SpeedIncreaseInterval = 20f;//속도 증가 간격. n 초마다 발사체의 속도를 한 단계씩 올릴지 정하는 시간 간격.
    [Header("발사체 생성 주기 조절")]
    public float InitialSpawnInterval = 3f;//초기 생성 주기. 게임이 시작될 때 발사체가 생성되는 기본 시간 간격.
    public float MinSpawnInterval = 1f;//최대 생성 주기. 난이도가 계속 올라가도 이 시간보다 더 짧아지지는 않아. 최대 생성되는 시간 간격.
    public float IntervalDecreaseRate = 0.5f;//주기 감소량. 발사체 생성 주기가 한 번 줄어들 때마다 몇 초씩 줄일지 정하는 값이야.
    public float IntervalDecreaseInterval = 20f;//주기 감소 간격. 몇 초마다 생성 주기를 한 단계씩 줄일지 정하는 시간 간격. 10초로 설정했으니, 10초마다 intervalDecreaseRate만큼 생성 주기가 짧아지겠지.
    [Header("발사체 데미지 조절")]
    public int InitialDamage = 5;//초기 발사체 데미지
    public int DamageIncrease = 2;//데미지 증가량. 발사체의 데미지가 한 번 강해질 때마다 몇씩 증가할지 정하는 값
    public float DamageIncreaseInterval = 20f;//데미지 증가 간격. 몇 초마다 발사체의 데미지를 한 단계씩 올릴지 정하는 시간 간격. 20초로 설정했으니, 20초마다 damageIncrease만큼 데미지가 증가하겠지.
    [Header("UI 알림")]
    public TextMeshProUGUI ObstacleLevelText;//UI 텍스트를 담을 변수

    //--- 내부에서 사용할 변수들, 현재의 변수 상태들을 담을 변수들 ---
    private float timeSinceLastSpeedIncrease = 0f;
    private float timeSinceLastIntervalDecrease = 0f;
    private float timeSinceLastDamageIncrease = 0f;
    private float currentBoltSpeed;
    private float currentSpawnInterval;
    private int currentDamage;
    private int currentLevel = 0;//현재 난이도 레벨을 저장할 변수


    // 어디서든 이 스크립트에 접근할 수 있게 해주는 '싱글톤' 패턴
    public static ObstacleDifficultyManager Instance { get; private set; }

    void Awake()//Start함수보다 먼저 실행
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
        //초기값 설정
        //게임이 시작될 때, 인스펙터에 설정해 둔 초기값(initial)을,
        //실제 게임에서 사용할 현재 값(current)에 넣어주는 역할을 해.
        currentBoltSpeed = InitialBoltSpeed;
        currentSpawnInterval = InitialSpawnInterval;
        currentDamage = InitialDamage;

        //게임 시작 시 UI 텍스트에 초기 레벨을 표시
        UpdateLevelText();
    }

    void Update()
    {
        //시간이 지남에 따라 난이도 조절
        timeSinceLastSpeedIncrease += Time.deltaTime;
        timeSinceLastIntervalDecrease += Time.deltaTime;
        timeSinceLastDamageIncrease += Time.deltaTime;

        //if문은 특정 조건이 충족될 때만 안에 있는 코드를 실행하게 하는거잖아?
        //n초가 지났다면 이 조건은 '참(true)' 이 돼.

        //속도 증가
        if (timeSinceLastSpeedIncrease >= SpeedIncreaseInterval)
        {
            currentBoltSpeed = Mathf.Min(currentBoltSpeed + SpeedIncreaseRate, MaxBoltSpeed);
            timeSinceLastSpeedIncrease = 0f;
            Debug.Log($"발사체 속도 증가! 현재 속도: {currentBoltSpeed}");

            //난이도 레벨 증가 및 UI 업데이트는 여기에서 한 번만 호출
            currentLevel++;     //속도 증가 if문에 넣어도 다른 if문은 전부 같은 시간에 값을 증가 하잖아? 그래서 셋 중 하나에만 넣어야해.
            UpdateLevelText();  //각 if문 마다 넣으면 각 조건이 참일때마다 레벨1씩을 주니까 레벨 n초마다 레벨 1증가가 아니라 3증가가 되버리지!
                                //if문들이 동시에 작동해서 레벨이 증가해. 걱정마! 대신 셋의 강화 시간을 똑같이 해놔야해!

            //만약 if문이 아닌 Update 함수에 적으면...레벨이 n초마다 오르는 게 아니라, 매 프레임마다 계속해서 올라가게 될 거야,
            //Update 함수는 게임이 실행되는 내내 매 프레임마다 호출되는 함수니까. 그래서 if문 안에 적는거야

            //currentLevel 변수는 Update 함수 안에 있는 변수가 아니라 ObstacleDifficultyManager라는 클래스 전체에 속해 있는 변수잖아,
            //그래서 이 클래스 안에 있는 어떤 함수든 이 currentLevel 변수에 자유롭게 접근하고 값을 바꿀 수 있어.

            //1. currentLevel++; 이 코드가 실행되면, ObstacleDifficultyManager 스크립트 안에 있는 currentLevel 변수의 값이 1 증가해서 저장돼.
            //2. 그다음 줄에 있는 UpdateLevelText(); 함수가 호출돼.
            //3. UpdateLevelText() 함수 안의 코드가 실행되는데, 이때 {currentLevel} 부분을 만나면,
            //스크립트 안에 저장된 currentLevel 변수의 현재 값을 직접 가져와서 사용하는 거지.
        }

        //생성 주기 감소
        if (timeSinceLastIntervalDecrease >= IntervalDecreaseInterval)
        {
            currentSpawnInterval = Mathf.Max(MinSpawnInterval, currentSpawnInterval - IntervalDecreaseRate);
            timeSinceLastIntervalDecrease = 0f;
            Debug.Log($"발사체 생성 주기 감소! 현재 주기: {currentSpawnInterval}");
        }

        //데미지 증가
        if (timeSinceLastDamageIncrease >= DamageIncreaseInterval)
        {
            currentDamage += DamageIncrease;
            timeSinceLastDamageIncrease = 0f;
            Debug.Log($"발사체 데미지 증가! 현재 데미지: {currentDamage}");
        }    
    }

    
    private void UpdateLevelText()//UI 텍스트를 업데이트 함수
    {
        if (ObstacleLevelText != null)
        {
            ObstacleLevelText.text = $"장애물 Lv.{currentLevel}";
            Debug.Log($"발사체 장애물이 강화됐어!");
        }
    }

    //다른 스크립트에서 현재 값을 가져갈 수 있는 함수들
    public float GetCurrentBoltSpeed()
    {
        return currentBoltSpeed;
    }

    public float GetCurrentSpawnInterval()
    {
        return currentSpawnInterval;
    }

    public int GetCurrentDamage()
    {
        return currentDamage;
    }
}

