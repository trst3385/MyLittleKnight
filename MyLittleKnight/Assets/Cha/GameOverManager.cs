using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameOverManager : MonoBehaviour
{//�÷��̾ ������ ����� UI ��ũ��Ʈ.

    [Header("�̱��� �ν��Ͻ�")]
    //�ٸ� ��ũ��Ʈ���� ���� ������ �� �ֵ��Ͻ̱��� ������ ���� �ν��Ͻ�
    public static GameOverManager Instance;

    [Header("GameOverPanel UI ����")]
    public GameObject GameOverPanel;//�ν����Ϳ��� ���� ���� �г��� �Ҵ��� ����

    [Header("FinalScoreTextUI ����")]
    public TextMeshProUGUI FinalScoreText;//���� ���� �гο� ǥ���� ���� �ؽ�Ʈ (Inspector���� ����)
                                          //GameOverPanel UI�� �ڽĿ� �־�!
    [Header("Player ��ũ��Ʈ ����")]
    public Player PlayerScript;//Player��ũ��Ʈ ����


    void Start()//Start �Լ��� ������Ʈ�� Ȱ��ȭ�� �� �� �� ����
    {
        //���� ���� �� ���� ���� �г��� ��Ȱ��ȭ, �ɸ��Ͱ� �׾�߸� ����
        if (GameOverPanel != null)
            GameOverPanel.SetActive(false);

        //�÷��̾� ��ũ��Ʈ�� �Ҵ���� �ʾҴٸ� ��� �α� ���
        if (PlayerScript == null)
            Debug.LogError("GameOverManager�� Player Script�� �Ҵ���� �ʾҾ�!");
    }

    public void OnGameOver()//���� ���� ���°� �Ǹ� ȣ��� �Լ�
    {
        //���� ���� �г� Ȱ��ȭ (ȭ�鿡 ���̰� ��)
        if (GameOverPanel != null)
            GameOverPanel.SetActive(true);
        //SetActive�� ����Ƽ���� ���� ������Ʈ�� Ȱ��ȭ�ϰų� ��Ȱ��ȭ�ϴ� ����̾�
        //true�� ������ üũ�ڽ��� �Ѽ� ������Ʈ�� ���̰�(Ȱ��ȭ) �ϰ�,
        //false�� ������ üũ�ڽ��� ���� ������Ʈ�� �����(��Ȱ��ȭ) ��.

        //SetActive() �Լ��� � bool ������ ���� �о �۵��ϴ� �� �ƴ϶�,
        //��ȣ �ȿ� ������ ���� �״�� �޾Ƽ� ������Ʈ�� ���¸� true�� false�� �ٲ��ִ� ���Ҹ� ��.


        //Time.timeScale�� ���� �� �ð��� �帧 �ӵ��� �����ϴ� ������.
        //Time.timeScale = 1f�� ������ ���� �ӵ��� ���ư��� �ִ� ���°�,
        //Time.timeScale = 0f�� ������ �ð��� ������ ���� ���¸� �ǹ���.
        //�װ� �� Time.timeScale = 0f; �ڵ�� ������ ������ ������Ű�� ������ ��.
        //���� �ð� ���߱�
        Time.timeScale = 0f;

        //���� ������ UI�� ǥ��
        DisplayFinalScore();//���� �� ��ũ��Ʈ �ȿ� �ִ� DisplayFinalScore ȣ��
    } 

    void DisplayFinalScore()//���� ���� �� ���� ������ UI�� ǥ���ϴ� �Լ�
    {
        if (FinalScoreText != null && PlayerScript != null)
        {
            FinalScoreText.text = "���� ����: " + PlayerScript.CurrentScore.ToString();
        }
        else if (FinalScoreText == null)
        {
            Debug.LogError("FinalScoreText�� GameOverManager�� �Ҵ���� �ʾҾ�!");
        }
        else if (PlayerScript == null)
        {
            Debug.LogError("Player Script�� GameOverManager�� �Ҵ���� �ʾ� ������ ������ �� ����!");
        }
        //8.23 if���� �߰�ȣ�� ���ٷ��� �ߴµ� ���� if��ó�� else if���� �ΰ��� �־�. �׷��� ��ȣ�� �����.
        //��ȣ���� if���̸� �ٷ� �ؿ� �ִ� �� '�ϳ���' ������ �Ǵϱ�
    }
    

    public void RestartGame()//���� �ٽ� ���� �Լ�
    {
        Time.timeScale = 1f;//���� �ð� �ٽ� ����

        //�÷��̾� ��ũ��Ʈ�� ���� �ʱ�ȭ �Լ� ȣ��
        if (PlayerScript != null)
            PlayerScript.ResetScore();

        //���� ���� �̸��� �����ͼ� �ٽ� �ε�
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
