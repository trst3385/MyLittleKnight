using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;//TextMeshPro�� ����ϱ� ���� �ʿ�


public class GameTimerUI : MonoBehaviour
{
    [Header("GameTimer UI ����")]
    [SerializeField] private TextMeshProUGUI gameTimer;//����Ƽ �ν����Ϳ��� UI �ؽ�Ʈ ������Ʈ�� ������ ����

    private float gameStartTime;//������ ���۵� �ð�
    private bool timerRunning = true;//Ÿ�̸Ӱ� �۵� ������ ����

    
    void Awake()
    {
        //TextMeshProUGUI ������Ʈ�� ����Ǿ� �ִ��� Ȯ��
        if (gameTimer == null)
        {
            gameTimer = GetComponent<TextMeshProUGUI>();
            if (gameTimer == null)
            {
                Debug.LogError("GameTimerUI: TextMeshProUGUI ������Ʈ�� ã�� �� ����!. �ν����Ϳ� �����ϰų� ���� ������Ʈ�� �߰���!");
                enabled = false;//��ũ��Ʈ ��Ȱ��ȭ�Ͽ� �߰� ���� ����
                return;
            }
        }
        gameStartTime = Time.time;//���� ���� ������ �ð� ���
        UpdateTimerUI(0);//���� ���� �� 0�ʷ� ǥ��
    }

    void Update()
    {
        if (!timerRunning) return;//Ÿ�̸Ӱ� �۵� ���� �ƴϸ� ������Ʈ���� ����

        float elapsedTime = Time.time - gameStartTime;//���� �ð����� ���� �ð��� ���� ��� �ð� ���
        UpdateTimerUI(elapsedTime);//UI ������Ʈ
    }

    private void UpdateTimerUI(float time)
    {
        //Mathf.FloorToInt() �� �Լ��� �Ҽ��� �Ʒ��� ������ ���� ����� ������ �����ϴ� ����

        //time % 3600: ��ü �ð��� 3600(1�ð�)���� ���� ������ ���� ����. �̷��� �ϸ� 1�ð��� �Ѵ� �κ��� ������, ���� '��'�� '��'�� ���� ��.
        //(...) / 60: ������ ���� ���� 60���� ����. �̷��� �ϸ� '��'�� �ش��ϴ� ���� ����.
        int minutes = Mathf.FloorToInt((time % 3600) / 60);//�� ���
        int seconds = Mathf.FloorToInt(time % 60);//�� ���
                                                  //��ü �ð�(time)�� 60���� ���� ������ ���� ����.�� ������ ���� 0���� 59 ������ ���ڰ� �ǰ���.�̰� �ٷ� '��'�� �ش��ϴ� ���̾�.

        //string.Format�� ����Ͽ� "00:00:00" �������� ������
        //"D2"�� �� �ڸ� ���ڷ� ǥ���ϰ�, �� �ڸ��� ��� �տ� 0�� �ٿ��� (��: 5 -> 05)
        //D2�� ���ڿ� �����ÿ��� ���Ǵ� ���� ���� �����ھ�. D�� Decimal (������)�� �ǹ��ϰ�, 2�� �ּ� �ڸ����� ��Ÿ��.
        //���� D2�� "�� �ڸ� �������� ���ڸ� ǥ���϶�"�� ���̾�.
        //���� ���ڰ� �� �ڸ�(��: 5)��, �տ� 0�� ä���� �� �ڸ�(05)�� �������. �� ����� **�е�(Padding)**�̶�� ��.
        //string.Format("{0:D2}", 5)�� ����� "05"�� ��.
        //string.Format("{0:D2}", 12)�� ����� "12"�� ��.
        gameTimer.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
    }

    //�ʿ��ϴٸ� �ܺο��� Ÿ�̸Ӹ� ���߰ų� �簳�ϴ� �Լ��� �߰��� �� �־�.
    //StopTimer, ResumeTimer, ResetTimer �Լ��� �̸� �����׾�
    public void StopTimer()
    {
        timerRunning = false;
    }

    public void ResumeTimer()
    {
        timerRunning = true;
    }

    public void ResetTimer()//Ư�� �������� �ٽ� �����ϰ� ���� �� (��: ���� �����)
    {
        gameStartTime = Time.time;
        timerRunning = true;
        UpdateTimerUI(0);
    }
}
