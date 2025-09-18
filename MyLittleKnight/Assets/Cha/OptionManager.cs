using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;//TextMeshPro�� ����Ϸ��� �߰�

public class OptionsManager : MonoBehaviour
{    
    public GameObject optionsPanel;//�ɼ�â �г� ������ ����
    public TextMeshProUGUI warningText;//��� �޽��� �ؽ�Ʈ UI ������ ����


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))//�ɼ� �г��� �������� ���� Ű�� ������ �г��� ��
        {
            ToggleOptionsPanel();
        }
        
    }

    public void ToggleOptionsPanel()//�ɼ�â�� Ȱ��ȭ�ɶ� ������ �Ͻ����� ����� �Ѱ� ���� ����
    {
        //�ɼ� �г��� ���� �ְ�, ī��Ʈ�ٿ��� ���� �� �����ٸ�
        //CountdownManager ��ũ��Ʈ�� isCountdownFinished������ static�̶� �̰����� public ������ CountdownManager ��ũ��Ʈ��,
        //���� ���� �ʾƵ� static �����ϱ� **Ŭ�����̸�.����**  �� ������ �� �־�.
        if (!optionsPanel.activeSelf && !CountdownManager.isCountdownFinished)
        {
            if(warningText != null)//��� �޽����� Ȱ��ȭ
            {
                warningText.gameObject.SetActive(true);
                warningText.text = "ī��Ʈ ���̾�! ��ø� ��ٷ�";

                StartCoroutine(HideWarningText());
            }
            return;//���⼭ �Լ��� ������ �ɼ� â�� ������ �ʰ� ����
        }
        else//���� ���ǿ� �ش����� ������ (�������� ��Ȳ)
        {
            //���� ���ǿ� �ش����� ������ ���������� �ɼ� â�� �����
            if (warningText != null)
            {
                warningText.gameObject.SetActive(false);//�ɼ� â�� �Ѱų� �� �� ��� �޽����� ����
            }

            //�� �� ���� �ڵ�� �ɼ� �г��� �Ѱų� ���� ������ ��, �������� isPanelActive�� ����� �ɼ�â UI�� ���� ������ ���⼭ �����
            bool isPanelActive = !optionsPanel.activeSelf;//!�� �ݴ��� ������. �ɼ�â�� false���¸� isPanelActive�� true,
            //�г� Ȱ��ȭ ���� Ȯ��                       //�ݴ�� �г��� Ȱ��ȭ(true) ���¶�� isPanelActive�� false�� ��
                                                          //activeSelf�� ���� ������Ʈ�� ���� Ȱ��ȭ�Ǿ� �ִ����� �˷��ִ� �Ӽ�
                                                          //�г��� �����ִٸ�(activeSelf�� false), !false�� true�� �Ǵϱ� �г��� �Ѱ�,
                                                          //�г��� �����ִٸ�(activeSelf�� true), !true�� false�� �Ǵϱ� �г��� ���� ����
            optionsPanel.SetActive(isPanelActive);//������Ʈ�� �Ѱų� ���� ������ ��.
            //�г� Ȱ��ȭ/��Ȱ��ȭ
                                                  //isPanelActive ���� ���� ���ڷ� �޾Ƽ�, �г��� ���� ������ �Ѱ�, ���� ������ ������
                                                 
            if (isPanelActive)
            {
                Time.timeScale = 0f;//�ɼ�â�� ���� ������ �Ͻ����� (�ð� �帧�� 0���� ����)
            }
            else
            {
                //�ɼ� â�� ���� ��, ī��Ʈ�ٿ��� �̹� �������� Ȯ���Ѵ�.
                if (CountdownManager.isCountdownFinished)
                {
                    Time.timeScale = 1f;//ī��Ʈ�ٿ��� �������� ������ �ٽ� �����Ѵ�.
                                        //���� ī��Ʈ�ٿ��� ���� ������ �ʾ�����, �ƹ��͵� ���� �ʱ�
                                        //�׷��� ������ ��� �����ִٰ�, ī��Ʈ�ٿ��� �����鼭 Time.timeScale�� 1�� �Ǿ� �ٽ� ����.
                }
            }
        }    
    }
    
    IEnumerator HideWarningText()//warningText UI ��� �޽����� ����� �ڷ�ƾ
    {
        //warningText UI �޼����� ������� n�� ���� ��ٷ�
        yield return new WaitForSecondsRealtime(1f);

        //warningText UI �޽����� ��Ȱ��ȭ
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
    }

    public void RestartGame()//���� ����� �Լ� (����� ��ư�� ����)
    {
        //������ ������� ���� �ð��� �ٽ� �������� ����������.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //LoadScene() Ư�� ���� �ҷ����� �Լ���. 
        //GetActiveScene() ���� Ȱ��ȭ�Ǿ� �ִ�(�÷��� ����) ���� �������� �Լ���. �� �Լ��� ���� ���� ���� ������ ��ȯ��.
        //.name �� GetActiveScene()���� ������ �� ���� �߿��� �� ���� �̸��� ���ڿ�(string) ���·� �����ִ� ������ ��.
        //���� �� �ڵ�� "���� ���� �̸��� �����ͼ�, �� �̸����� ���� �ٽ� �ε���" ��� ���̾�.

    }

    public void ExitGame()//�� ������ ���� ����ȭ������ ���� �Լ�
    {
        Time.timeScale = 1f;//���� ����۰� ����������, ����ȭ������ ���ư� ���� �ð��� �������� �������� �� ����.
        //������ �Ͻ������� ���¿��� ���� ��ȯ�ϸ� ������ �߻��� �� �����Ƿ�, ���� �ε��ϱ� ���� �ð��� �������� �ǵ����� �ž�.

        SceneManager.LoadScene("MainMenuScene");//�� ���θ޴� �� �̸�
        //MainMenuScene �̸��� ���� �ε��ؼ� �ٽ� ����ȭ�� ������ ���ư��� ��.
        Debug.Log("����ȭ������ �̵�");
    }

    public void  QuitGame()//������ ������ ���� �Լ�
    {
        Time.timeScale = 1f;
        Debug.Log("���� ����. ��Ծ���?");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}


