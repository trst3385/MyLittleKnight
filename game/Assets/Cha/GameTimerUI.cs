using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;//TextMeshPro를 사용하기 위해 필요


public class GameTimerUI : MonoBehaviour
{
    [Header("GameTimer UI 연결")]
    [SerializeField] private TextMeshProUGUI gameTimer;//유니티 인스펙터에서 UI 텍스트 컴포넌트를 연결할 변수

    private float gameStartTime;//게임이 시작된 시간
    private bool timerRunning = true;//타이머가 작동 중인지 여부

    
    void Awake()
    {
        //TextMeshProUGUI 컴포넌트가 연결되어 있는지 확인
        if (gameTimer == null)
        {
            gameTimer = GetComponent<TextMeshProUGUI>();
            if (gameTimer == null)
            {
                Debug.LogError("GameTimerUI: TextMeshProUGUI 컴포넌트를 찾을 수 없어!. 인스펙터에 연결하거나 같은 오브젝트에 추가해!");
                enabled = false;//스크립트 비활성화하여 추가 오류 방지
                return;
            }
        }
        gameStartTime = Time.time;//게임 시작 시점의 시간 기록
        UpdateTimerUI(0);//게임 시작 시 0초로 표시
    }

    void Update()
    {
        if (!timerRunning) return;//타이머가 작동 중이 아니면 업데이트하지 않음

        float elapsedTime = Time.time - gameStartTime;//현재 시간에서 시작 시간을 빼서 경과 시간 계산
        UpdateTimerUI(elapsedTime);//UI 업데이트
    }

    private void UpdateTimerUI(float time)
    {
        //Mathf.FloorToInt() 이 함수는 소수점 아래를 버리고 가장 가까운 정수로 내림하는 역할

        //time % 3600: 전체 시간을 3600(1시간)으로 나눈 나머지 값을 구해. 이렇게 하면 1시간이 넘는 부분은 버리고, 남은 '분'과 '초'만 남게 돼.
        //(...) / 60: 위에서 구한 값을 60으로 나눠. 이렇게 하면 '분'에 해당하는 값이 나와.
        int minutes = Mathf.FloorToInt((time % 3600) / 60);//분 계산
        int seconds = Mathf.FloorToInt(time % 60);//초 계산
                                                  //전체 시간(time)을 60으로 나눈 나머지 값을 구해.이 나머지 값은 0부터 59 사이의 숫자가 되겠지.이게 바로 '초'에 해당하는 값이야.

        //string.Format을 사용하여 "00:00:00" 형식으로 포맷팅
        //"D2"는 두 자리 숫자로 표시하고, 한 자리일 경우 앞에 0을 붙여줘 (예: 5 -> 05)
        //D2는 문자열 포맷팅에서 사용되는 숫자 포맷 지정자야. D는 Decimal (십진수)를 의미하고, 2는 최소 자릿수를 나타내.
        //따라서 D2는 "두 자리 십진수로 숫자를 표현하라"는 뜻이야.
        //만약 숫자가 한 자리(예: 5)면, 앞에 0을 채워서 두 자리(05)로 만들어줘. 이 기능을 **패딩(Padding)**이라고 해.
        //string.Format("{0:D2}", 5)의 결과는 "05"가 돼.
        //string.Format("{0:D2}", 12)의 결과는 "12"가 돼.
        gameTimer.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

    //필요하다면 외부에서 타이머를 멈추거나 재개하는 함수를 추가할 수 있어.
    //StopTimer, ResumeTimer, ResetTimer 함수는 미리 만들어뒀어
    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResumeTimer()
    {
        timerRunning = true;
    }

    public void ResetTimer()//특정 시점부터 다시 시작하고 싶을 때 (예: 게임 재시작)
    {
        gameStartTime = Time.time;
        timerRunning = true;
        UpdateTimerUI(0);
    }
}
