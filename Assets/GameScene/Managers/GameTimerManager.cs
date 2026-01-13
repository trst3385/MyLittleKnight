using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;//TextMeshPro를 사용하기 위해 필요


public class GameTimerManager : MonoBehaviour
{
    public static GameTimerManager Instance { get; private set; }//어디서든 접근 가능한 싱글톤 설정

    [Header("GameTimer UI 연결")]
    [SerializeField] private TextMeshProUGUI gameTimer;//유니티 인스펙터에서 UI 텍스트 컴포넌트를 연결할 변수


    private float gameStartTime;//게임이 시작된 시간
    private bool timerRunning = true;//타이머가 작동 중인지 여부

    void Awake()
    {
        if (Instance == null) Instance = this;//싱글톤 초기화
        else { Destroy(gameObject); return; }

        if (gameTimer == null)//이름으로 찾기. 인스펙터에 드래그 안 되어 있으면 이름으로 직접 찾기
        {
            GameObject timerObj = GameObject.Find("GameTimer");//UI의 GameTimer 이름
            if (timerObj != null)
                gameTimer = timerObj.GetComponent<TextMeshProUGUI>();
        }

        gameStartTime = Time.time;//게임 시작 시점의 시간 기록
        UpdateTimerUI(0);//게임 시작 시 0초로 표시
    }

    void Update()
    {
        if (!timerRunning) return;//타이머가 작동 중이 아니면 업데이트하지 않음

        //TimeFreeze로 시간이 멈췄다면 타이머 업데이트를 하지 않고, 현재 시간에 머무름
        if (TimeFreeze.Instance != null && TimeFreeze.Instance.IsTimeFrozen) return;
 
        float elapsedTime = Time.time - gameStartTime;//현재 시간에서 시작 시간을 빼서 경과 시간 계산
        UpdateTimerUI(elapsedTime);//UI 업데이트
    }
    

    private void UpdateTimerUI(float time)
    {
        if (gameTimer == null) return;

        // 1. 시간 단위 계산 (내림 처리)
        int minutes = Mathf.FloorToInt((time % 3600) / 60);//전체 초에서 '분' 추출
        int seconds = Mathf.FloorToInt(time % 60);//60으로 나눈 나머지 '초'

        //2. UI 표시(D2: 한 자리 숫자일 때 앞에 0을 채워 00:00 형식 유지)
        gameTimer.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }
    public float GetElapsedTime() => Time.time - gameStartTime;//다른 매니저에서 시간을 물어볼 때 사용

    public void AdjustStartTime(float frozenDuration)//TimeFreeze로 시간 정지가 끝나고 시간을 재개할 때 Time.time과의 차이를 보정하는 함수
    {
        //정지되어 있던 시간(frozenDuration)만큼 gameStartTime을 늘려서 보정.
        //Time.time은 계속 흐르므로, gameStartTime을 늘려야 경과 시간이 '줄어드는' 효과가 생김.
        gameStartTime += frozenDuration;
        Debug.Log($"GameTimerUI: 시작 시간 {frozenDuration:F2}초 보정 완료.");
    }

    //필요하다면 외부에서 타이머를 멈추거나 재개하는 함수를 추가할 수 있어.
    //StopTimer, ResumeTimer, ResetTimer 함수는 미리 만들어뒀어
    //타이머 제어 함수를 표현식 본문(람다식)으로 간결화
    public void StopTimer() => timerRunning = false;
    public void ResumeTimer() => timerRunning = true;
    public void ResetTimer()//특정 시점부터 다시 시작하고 싶을 때 (예: 게임 재시작)
    {
        gameStartTime = Time.time;
        timerRunning = true;
        UpdateTimerUI(0);
    }
}
