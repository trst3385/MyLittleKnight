using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuExitManager : MonoBehaviour
{
    //전처리기 지시문이란? 
    //C# 코드가 최종 프로그램으로 만들어지기 전에 컴파일러가 특정 작업을 수행하도록 지시하는 명령어야.
    //if, else와 비슷하게 작동하지만, 프로그램이 실행될 때가 아니라 코드가 컴파일될 때 동작한다는 점이 달라.

    //#if UNITY_EDITOR: 이 코드 블록은 네가 유니티 에디터에서 게임을 실행할 때만 포함돼.
    //그래서 EditorApplication.isPlaying = false; 코드가 에디터에서만 작동해서 게임을 멈추는 역할을 해.

    //#else: 이 코드 블록은 UNITY_EDITOR 조건이 참이 아닐 때, 즉 실제 게임으로 빌드했을 때만 포함돼.
    //그래서 Application.Quit() 코드가 실제 게임을 종료하는 역할을 해.
    public void QuitGame()
    {
        Debug.Log("게임 종료 요청됨.");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying= false;
        #else
        Application.Quit();
        #endif
    }
}
