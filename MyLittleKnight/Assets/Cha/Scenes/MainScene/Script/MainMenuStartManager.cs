using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//씬 전환을 위해 반드시 필요해

public class MainMenuStartManager : MonoBehaviour
{
    public void LoadGameScene()//GameStartButton버튼을 누르면 이 함수가 호출되어 게임 씬으로 넘어가
    {
        SceneManager.LoadScene("GameScene");
    }
}
