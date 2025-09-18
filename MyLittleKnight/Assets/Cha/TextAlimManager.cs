using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;//TextMeshPro�� ����Ϸ��� �� using ���� �ʿ�


//�� ��ũ��Ʈ�� ���� �� �پ��� �˸� (������ ȹ��, ���� ���� ��)�� ���� ����.
//����: ��� ������ �˸��� �ϰ��� �ð��� ȿ�� (���̵� ��/�ƿ�)�� �����ϰ�,
//���뼺�� ���� ���� ��ũ��Ʈ���� ���� �˸��� ��� �� �ֽ��ϴ�.
//����: ������ �˸����� �ڷ�ƾ�� ���̵� ������ �ʿ��Ͽ� ��������� ������ �� �־�!.
//(EnemyDifficulty ��ũ��Ʈ�� ���� ���� ���� ��İ� ���غ�!)


public class TextAlimManager : MonoBehaviour
{
    [Header("UI ����")]
    public TextMeshProUGUI NotificationText;//�ν����Ϳ��� UI �ؽ�Ʈ ������Ʈ�� �Ҵ��� ����

    [Header("�˸��� ǥ��, ����� �ð�")]
    public float DisplayDuration = 2f;//�˸��� ȭ�鿡 ǥ�õ� �ð� (��)
    public float FadeDuration = 0.5f;//�˸��� ����� �� ���̵�ƿ� �Ǵ� �ð� (��)

    private Coroutine currentNotificationCoroutine;//���� ���� ���� �ڷ�ƾ�� ����


    void Awake()//Start ��� Awake���� �ʱ�ȭ�ϴ� ���� ����.
    {
        if (NotificationText == null)
        {
            Debug.LogError("TextAlimManager: notificationText�� �Ҵ���� �ʾҾ�! �ν����͸� Ȯ����!");
            return;
        }
        //���� �� �ؽ�Ʈ�� ����
        NotificationText.color = new Color(NotificationText.color.r,
        NotificationText.color.g, NotificationText.color.b, 0);
    }
    

    public void ShowNotification(string message)//�ܺο��� �˸��� ��û�� �� ȣ��Ǵ� �Լ�
    {//�ܺο��� ���� �ؽ�Ʈ�� string ������ message �Ű������� ��Ƶΰ� DisplayNotification�� ����

        //������ ���� ���� �ڷ�ƾ(�ؽ�Ʈ �˶�) �� �ִٸ� ����
        //������ �˸��� �ߴ� ���߿� �ٸ� �˸��� �� �߸�, ���ο� �˸��� �ٷ� ǥ�õǵ��� ���� �˸��� ����
        if (currentNotificationCoroutine != null)
            StopCoroutine(currentNotificationCoroutine);


        //�������� message ���� DisplayNotification �ڷ�ƾ�� ����
        currentNotificationCoroutine = StartCoroutine(DisplayNotification(message));
        //ShowNotification�� �ڽ��� message ����(���纻)�� �ִ� ���ڿ� ���� �ٽ� �����ؼ� DisplayNotification�� �Ѱ���.
    }

    
    IEnumerator DisplayNotification(string message)//ShowNotification�� �Ѱ��� �޼����� UI�� �߰� �ϴ� ����
    {
        NotificationText.text = message;//UI�� ���� �ؽ�Ʈ�� ����

        //���̵��� (���� 0 -> 1)
        float timer = 0f;
        Color startColor = new Color(NotificationText.color.r, NotificationText.color.g, NotificationText.color.b, 0);
        Color targetColor = new Color(NotificationText.color.r, NotificationText.color.g, NotificationText.color.b, 1);
        while (timer < FadeDuration)
        {
            NotificationText.color = Color.Lerp(startColor, targetColor, timer / FadeDuration);
            timer += Time.deltaTime;
            yield return null;//�� ������ ���
        }
        NotificationText.color = targetColor;//��Ȯ�� �������ϰ� ����

        yield return new WaitForSeconds(DisplayDuration);
        //�˸��� displayDuration ������ ������ �ð�(��)��ŭ ȭ�鿡 �����ǵ��� ���
        //�� �ڷ�ƾ�� ��� �����ִٰ�, �� �� �Ʒ��� �ڵ带 �����Ͽ� �˸��� ������� �մϴ�.
        
        //���̵�ƿ� (���� 1 -> 0)
        timer = 0f;
        startColor = targetColor;//���� ���� (������)
        targetColor = new Color(NotificationText.color.r, NotificationText.color.g, NotificationText.color.b, 0);

        while (timer < FadeDuration)//�ؽ�Ʈ�� ���̵���/���̵�ƿ� �ִϸ��̼�
        {
            //�ð��� ���� �ؽ�Ʈ ������ ����(startColor)���� ��ǥ(targetColor)���� �ε巴�� ��ȭ��Ŵ (���̵���/�ƿ� �ִϸ��̼�)
            NotificationText.color = Color.Lerp(startColor, targetColor, timer / FadeDuration);
            timer += Time.deltaTime;//�����Ӵ� ��� �ð���ŭ Ÿ�̸� ����
            yield return null;//���� �����ӱ��� ����Ͽ� �ִϸ��̼� ������Ʈ
        }
        NotificationText.color = targetColor;//������ �����ϰ� ����
            NotificationText.text = "";//�ؽ�Ʈ ���뵵 ����ֱ�
    }
}
