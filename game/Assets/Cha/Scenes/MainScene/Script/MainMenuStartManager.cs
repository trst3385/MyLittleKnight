using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//�� ��ȯ�� ���� �ݵ�� �ʿ���

public class MainMenuStartManager : MonoBehaviour
{
    public void LoadGameScene()//GameStartButton��ư�� ������ �� �Լ��� ȣ��Ǿ� ���� ������ �Ѿ
    {
        SceneManager.LoadScene("GameScene");
    }
}
