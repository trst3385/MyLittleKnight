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
        if (Input.GetKeyDown(KeyCode.O))//옵션 패널이 꺼져있을 때만 키를 누르면 패널을 켜
        {
            ToggleOptionsPanel();
        }
        
    }

    public void ToggleOptionsPanel()//옵션창과 활성화될때 게임의 일시정지 기능을 켜고 끄는 역할
    {
        //옵션 패널이 꺼져 있고, 카운트다운이 아직 안 끝났다면
        //CountdownManager 스크립트의 isCountdownFinished변수는 static이라 이곳에서 public 변수로 CountdownManager 스크립트를,
        //연결 하지 않아도 static 변수니까 **클래스이름.변수**  로 가져올 수 있어.
        if (!optionsPanel.activeSelf && !CountdownManager.isCountdownFinished)
        {
            if(warningText != null)//경고 메시지를 활성화
            {
                warningText.gameObject.SetActive(true);
                warningText.text = "카운트 중이야! 잠시만 기다려";

                StartCoroutine(HideWarningText());
            }
            return;//여기서 함수를 끝내서 옵션 창이 켜지지 않게 막아
        }
        else//위의 조건에 해당하지 않으면 (정상적인 상황)
        {
            //위의 조건에 해당하지 않으면 정상적으로 옵션 창을 토글해
            if (warningText != null)
            {
                warningText.gameObject.SetActive(false);//옵션 창을 켜거나 끌 때 경고 메시지는 숨겨
            }

            //이 두 줄의 코드는 옵션 패널을 켜거나 끄는 역할을 해, 지역변수 isPanelActive가 연결된 옵션창 UI를 담은 변수를 여기서 사용해
            bool isPanelActive = !optionsPanel.activeSelf;//!는 반대의 연산자. 옵션창이 false상태면 isPanelActive는 true,
            //패널 활성화 상태 확인                       //반대로 패널이 활성화(true) 상태라면 isPanelActive는 false가 돼
                                                          //activeSelf는 게임 오브젝트가 현재 활성화되어 있는지를 알려주는 속성
                                                          //패널이 꺼져있다면(activeSelf가 false), !false는 true가 되니까 패널을 켜고,
                                                          //패널이 켜져있다면(activeSelf가 true), !true는 false가 되니까 패널을 끄게 되지
            optionsPanel.SetActive(isPanelActive);//오브젝트를 켜거나 끄는 역할을 해.
            //패널 활성화/비활성화
                                                  //isPanelActive 변수 값을 인자로 받아서, 패널이 꺼져 있으면 켜고, 켜져 있으면 꺼버려
                                                 
            if (isPanelActive)
            {
                Time.timeScale = 0f;//옵션창을 열면 게임을 일시정지 (시간 흐름을 0으로 설정)
            }
            else
            {
                //옵션 창이 꺼질 때, 카운트다운이 이미 끝났는지 확인한다.
                if (CountdownManager.isCountdownFinished)
                {
                    Time.timeScale = 1f;//카운트다운이 끝났으면 게임을 다시 시작한다.
                                        //만약 카운트다운이 아직 끝나지 않았으면, 아무것도 하지 않기
                                        //그러면 게임은 계속 멈춰있다가, 카운트다운이 끝나면서 Time.timeScale이 1이 되어 다시 시작.
                }
            }
        }    
    }
    
    IEnumerator HideWarningText()//warningText UI 경고 메시지를 숨기는 코루틴
    {
        //warningText UI 메세지가 띄워지면 n초 동안 기다려
        yield return new WaitForSecondsRealtime(1f);

        //warningText UI 메시지를 비활성화
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
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
        Time.timeScale = 1f;//게임 재시작과 마찬가지로, 메인화면으로 돌아갈 때도 시간을 정상으로 돌려놓는 게 좋아.
        //게임이 일시정지된 상태에서 씬을 전환하면 오류가 발생할 수 있으므로, 씬을 로드하기 전에 시간을 정상으로 되돌리는 거야.

        SceneManager.LoadScene("MainMenuScene");//내 메인메뉴 씬 이름
        //MainMenuScene 이름의 씬을 로드해서 다시 메인화면 씬으로 돌아가게 해.
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


