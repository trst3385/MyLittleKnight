using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuExitManager : MonoBehaviour
{
    //��ó���� ���ù��̶�? 
    //C# �ڵ尡 ���� ���α׷����� ��������� ���� �����Ϸ��� Ư�� �۾��� �����ϵ��� �����ϴ� ��ɾ��.
    //if, else�� ����ϰ� �۵�������, ���α׷��� ����� ���� �ƴ϶� �ڵ尡 �����ϵ� �� �����Ѵٴ� ���� �޶�.

    //#if UNITY_EDITOR: �� �ڵ� ����� �װ� ����Ƽ �����Ϳ��� ������ ������ ���� ���Ե�.
    //�׷��� EditorApplication.isPlaying = false; �ڵ尡 �����Ϳ����� �۵��ؼ� ������ ���ߴ� ������ ��.

    //#else: �� �ڵ� ����� UNITY_EDITOR ������ ���� �ƴ� ��, �� ���� �������� �������� ���� ���Ե�.
    //�׷��� Application.Quit() �ڵ尡 ���� ������ �����ϴ� ������ ��.
    public void QuitGame()
    {
        Debug.Log("���� ���� ��û��.");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying= false;
        #else
        Application.Quit();
        #endif
    }
}
