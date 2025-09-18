using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameOverManager : MonoBehaviour
{//플레이어가 죽으면 실행될 UI 스크립트.

    [Header("싱글톤 인스턴스")]
    //다른 스크립트에서 쉽게 접근할 수 있도록싱글톤 패턴을 위한 인스턴스
    public static GameOverManager Instance;

    [Header("GameOverPanel UI 연결")]
    public GameObject GameOverPanel;//인스펙터에서 게임 오버 패널을 할당할 변수

    [Header("FinalScoreTextUI 연결")]
    public TextMeshProUGUI FinalScoreText;//게임 오버 패널에 표시할 점수 텍스트 (Inspector에서 연결)
                                          //GameOverPanel UI의 자식에 있어!
    [Header("Player 스크립트 참조")]
    public Player PlayerScript;//Player스크립트 참조


    void Start()//Start 함수는 오브젝트가 활성화될 때 한 번 실행
    {
        //게임 시작 시 게임 오버 패널은 비활성화, 케릭터가 죽어야만 생성
        if (GameOverPanel != null)
            GameOverPanel.SetActive(false);

        //플레이어 스크립트가 할당되지 않았다면 경고 로그 출력
        if (PlayerScript == null)
            Debug.LogError("GameOverManager에 Player Script가 할당되지 않았어!");
    }

    public void OnGameOver()//게임 오버 상태가 되면 호출될 함수
    {
        //게임 오버 패널 활성화 (화면에 보이게 함)
        if (GameOverPanel != null)
            GameOverPanel.SetActive(true);
        //SetActive는 유니티에서 게임 오브젝트를 활성화하거나 비활성화하는 기능이야
        //true를 넣으면 체크박스를 켜서 오브젝트를 보이게(활성화) 하고,
        //false를 넣으면 체크박스를 꺼서 오브젝트를 숨기게(비활성화) 해.

        //SetActive() 함수는 어떤 bool 변수의 값을 읽어서 작동하는 게 아니라,
        //괄호 안에 들어오는 값을 그대로 받아서 오브젝트의 상태를 true나 false로 바꿔주는 역할만 해.


        //Time.timeScale은 게임 내 시간의 흐름 속도를 조절하는 변수야.
        //Time.timeScale = 1f는 게임이 보통 속도로 돌아가고 있는 상태고,
        //Time.timeScale = 0f는 게임의 시간이 완전히 멈춘 상태를 의미해.
        //네가 쓴 Time.timeScale = 0f; 코드는 게임을 완전히 정지시키는 역할을 해.
        //게임 시간 멈추기
        Time.timeScale = 0f;

        //최종 점수를 UI에 표시
        DisplayFinalScore();//이제 이 스크립트 안에 있는 DisplayFinalScore 호출
    } 

    void DisplayFinalScore()//게임 오버 시 최종 점수를 UI에 표시하는 함수
    {
        if (FinalScoreText != null && PlayerScript != null)
        {
            FinalScoreText.text = "최종 점수: " + PlayerScript.CurrentScore.ToString();
        }
        else if (FinalScoreText == null)
        {
            Debug.LogError("FinalScoreText가 GameOverManager에 할당되지 않았어!");
        }
        else if (PlayerScript == null)
        {
            Debug.LogError("Player Script가 GameOverManager에 할당되지 않아 점수를 가져올 수 없어!");
        }
        //8.23 if문의 중괄호를 없앨려고 했는데 위의 if문처럼 else if문이 두개가 있어. 그러니 괄호를 써야해.
        //괄호없는 if문이면 바로 밑에 있는 것 '하나만' 연결이 되니까
    }
    

    public void RestartGame()//게임 다시 시작 함수
    {
        Time.timeScale = 1f;//게임 시간 다시 시작

        //플레이어 스크립트의 점수 초기화 함수 호출
        if (PlayerScript != null)
            PlayerScript.ResetScore();

        //현재 씬의 이름을 가져와서 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
