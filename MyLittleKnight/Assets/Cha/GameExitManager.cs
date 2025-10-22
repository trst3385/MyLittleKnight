using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameExitManager : MonoBehaviour
{
  
    void Update()
    {
        //ESC 키가 눌렸는지 확인
        if (Input.GetKeyDown(KeyCode.Escape))//ESC 버튼을 눌러 게임 종료
            QuitGame();//게임 종료 함수 호출
    }

    void QuitGame()
    {
        Debug.Log("게임 종료 요청됨.");

        //유니티 에디터에서 실행 중일 때
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        //실제 빌드된 게임에서 실행 중일 때
#else
            Application.Quit();//애플리케이션 종료
#endif

        //전처리기 지시문이란?
        //C# 코드가 컴파일(Compile)되기 전에 컴파일러에게 특정 작업을 수행하도록 지시하는 명령어
        //C# 코드처럼 프로그램이 실행될 때 동작하는 것이 아니라,
        //코드가 최종 실행 가능한 프로그램으로 만들어지는 빌드 과정에 개입한다고 생각하면 된다.

        //조건부 컴파일 (Conditional Compilation): 특정 조건에 따라 코드의 일부를 컴파일에 포함시키거나 제외할 때 사용.
        //이게 가장 흔한 사용 사례이고, 지금 사용한 #if가 여기에 해당.

        //오류 및 경고 메시지: 컴파일 시점에 특정 메시지를 출력하도록 할 수 있다.

        //영역 정의: 코드의 특정 부분을 접거나 펼칠 수 있는 영역으로 정의할 수 있다.
    }
}
