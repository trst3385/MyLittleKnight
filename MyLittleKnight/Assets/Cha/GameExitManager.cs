using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameExitManager : MonoBehaviour
{
  
    void Update()
    {
        //ESC Ű�� ���ȴ��� Ȯ��
        if (Input.GetKeyDown(KeyCode.Escape))//ESC ��ư�� ���� ���� ����
            QuitGame();//���� ���� �Լ� ȣ��
    }

    void QuitGame()
    {
        Debug.Log("���� ���� ��û��.");

        //����Ƽ �����Ϳ��� ���� ���� ��
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        //���� ����� ���ӿ��� ���� ���� ��
#else
            Application.Quit();//���ø����̼� ����
#endif

        //��ó���� ���ù��̶�?
        //C# �ڵ尡 ������(Compile)�Ǳ� ���� �����Ϸ����� Ư�� �۾��� �����ϵ��� �����ϴ� ��ɾ�
        //C# �ڵ�ó�� ���α׷��� ����� �� �����ϴ� ���� �ƴ϶�,
        //�ڵ尡 ���� ���� ������ ���α׷����� ��������� ���� ������ �����Ѵٰ� �����ϸ� �ȴ�.

        //���Ǻ� ������ (Conditional Compilation): Ư�� ���ǿ� ���� �ڵ��� �Ϻθ� �����Ͽ� ���Խ�Ű�ų� ������ �� ���.
        //�̰� ���� ���� ��� ����̰�, ���� ����� #if�� ���⿡ �ش�.

        //���� �� ��� �޽���: ������ ������ Ư�� �޽����� ����ϵ��� �� �� �ִ�.

        //���� ����: �ڵ��� Ư�� �κ��� ���ų� ��ĥ �� �ִ� �������� ������ �� �ִ�.
    }
}
