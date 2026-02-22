using TMPro;
using UnityEngine;

public class ScoreUIManager : MonoBehaviour//플레이가 몬스터 처치 시 받는 점수를 UI에 띄우는 옵저버 패턴의 스크립트.
{
    private TextMeshProUGUI scoreText;//점수를 표시할 실제 UI 텍스트 (인스펙터에서 드래그 안 해도 돼, 코드 내 연결 방식을 유지하니까)

    void Awake()
    {   //1. 코드 내 연결로 씬 전체에서 "ScoreText"라는 이름을 가진 오브젝트를 찾아서 연결함
        GameObject scoreObj = GameObject.Find("ScoreText");

        if (scoreObj != null) scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        else Debug.LogError("ScoreUIManager: 'ScoreText' 오브젝트를 씬에서 찾을 수 없어! 이름을 확인해봐!");
    }

    void Start()
    {   //2. 게임 시작하자마자 화면에 점수를 0으로 세팅 (처음엔 아무 점수도 안 들어오니까!)
        if (scoreText != null)
            scoreText.text = "Score: 0";
    }

    private void OnEnable()
    {   //3. 플레이어 스크립트에서 "점수가 올랐다"는 신호를 보내면, 아래의 UpdateScoreDisplay 함수를 실행하도록 연결함
        Player.OnScoreChanged += UpdateScoreDisplay;
    }

    private void OnDisable()
    {   //4. 이 UI가 사라지거나 꺼질 때는, 플레이어의 신호를 더 이상 받지 않도록 연결을 끊음 (에러 방지용)
        Player.OnScoreChanged -= UpdateScoreDisplay;
    }


    private void UpdateScoreDisplay(int currentScore)//5. 플레이어로부터 실제 점수(int) 데이터를 전달받아서 화면 글자를 바꿔주는 부분
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore.ToString();
    }
}
