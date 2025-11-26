using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;//TextMeshPro를 사용하려면 추가

public class OptionsManager : MonoBehaviour
{    
    public GameObject optionsPanel;//옵션창 패널 연결할 변수
    public TextMeshProUGUI warningText;//경고 메시지 텍스트 UI 연결할 변수


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) ToggleOptionsPanel();//옵션 패널이 꺼져있을 때만 키를 누르면 패널을 켜
    }

    public void ToggleOptionsPanel()//옵션창과 활성화될때 게임의 일시정지 기능을 켜고 끄는 역할
    {
        //카운트다운 중에는 차단하고 경고 메시지 출력
        if (!optionsPanel.activeSelf && !CountdownManager.isCountdownFinished)
        {
            if (warningText != null)//경고 메시지를 활성화
            {
                warningText.gameObject.SetActive(true);
                warningText.text = "카운트 중이야! 잠시만 기다려!";
                StartCoroutine(HideWarningText());
            }
            return;//여기서 함수를 끝내서 옵션 창이 켜지지 않게 막아
        }

        //경고 메시지 숨기기 (토글 전, 혹시 남아있을 수 있는 경고를 정리)
        if (warningText != null) warningText.gameObject.SetActive(false);

        //패널의 다음 상태를 결정하고 활성화/비활성화
        bool isPanelActive = !optionsPanel.activeSelf;//다음 상태 (true: 켬, false: 끔)
        optionsPanel.SetActive(isPanelActive);

        //게임 시간 제어. Time.timeScale로 게임 시간 정지/재개
        if (isPanelActive) Time.timeScale = 0f;
        else if (CountdownManager.isCountdownFinished) Time.timeScale = 1f;//옵션창 닫고, 카운트다운 끝났으면 시간 재개
    }
    
    IEnumerator HideWarningText()//warningText UI 경고 메시지를 숨기는 코루틴
    {
        //warningText UI 메세지가 띄워지면 n초 동안 기다려
        yield return new WaitForSecondsRealtime(1f);
        //warningText UI 메시지를 비활성화
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    public void RestartGame()//게임 재시작 함수 (재시작 버튼에 연결)
    {
        //게임을 재시작할 때는 시간을 다시 정상으로 돌려놓야해.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //LoadScene() 특정 씬을 불러오는 함수야. 
        //GetActiveScene() 현재 활성화되어 있는(플레이 중인) 씬을 가져오는 함수야. 이 함수는 현재 씬에 대한 정보를 반환해.
        //.name 은 GetActiveScene()으로 가져온 씬 정보 중에서 그 씬의 이름을 문자열(string) 형태로 꺼내주는 역할을 해.
        //따라서 이 코드는 "현재 씬의 이름을 가져와서, 그 이름으로 씬을 다시 로드해" 라는 뜻이야.
    }

    public void ExitGame()//현 게임을 끄고 메인화면으로 가는 함수
    {
        Time.timeScale = 1f;//게임 재시작과 마찬가지로, 메인화면으로 돌아갈 때도 시간을 정상으로 돌려놔.
        SceneManager.LoadScene("MainMenuScene");//MainMenuScene 이름의 씬을 로드해서 다시 메인화면 씬으로 돌아가게 해
        Debug.Log("메인화면으로 이동");
    }

    public void  QuitGame()//게임을 완전히 끄는 함수
    {
        Time.timeScale = 1f;
        Debug.Log("게임 종료. 재밋었어?");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}


