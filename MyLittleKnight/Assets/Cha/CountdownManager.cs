using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;//TextMeshPro�� ����ϱ� ���� �ʿ�


public class CountdownManager : MonoBehaviour
{
    //�ٸ� ��ũ��Ʈ�� ī��Ʈ�ٿ��� �������� �� �� �ְ� �ϴ� ���� (�ٸ� ��ũ��Ʈ���� ���� �����ϵ��� static���� ����)
    //public static ������ �ٸ� t��ũ��Ʈ���� �� ������ ���� public ������ �� ��ũ��Ʈ�� �������� �ʾƵ� ��.
    //**Ŭ�����̸�.����** �� ������ ������!
    public static bool isCountdownFinished = false;//�⺻���� false ����.
    //Ȥ�ö� ������ ������ϰų� �ٸ� ������ �Ѿ�� ��,
    //���� ������ true ���� ���� ���� �� �ֱ� ������, Ȯ���ϰ� false ���·� ������ִ� �ž�


    [Header("CountdownText UI����")]//�ν����Ϳ� �Ҵ��� ī��Ʈ�ٿ� �ؽ�ƮUI ������Ʈ
    public TextMeshProUGUI CountdownText;

    [Header("���� ����")]//ī��Ʈ�ٿ ���� ����
    public AudioSource countdownAudioSource;
    public AudioClip[] countdownSounds;//�迭�� ����°ž�. AudioClip�� ���� countdownSounds �������� �迭�� ������ ����°ž�

    void Start()
    {
        //���� ���� ���� ����. ī��Ʈ�� ������ ���� ����
        //Time.timeScale: ���� ��ü�� �ð� �帧 �ӵ��� �����մϴ�. 0f�� �����ϸ� ������ �Ͻ����� ����
        //ī��Ʈ�� ������ �����ؾ� �ϴ� �ؿ��� 1f;�� �Ѳ���
        Time.timeScale = 0f;

        isCountdownFinished = false;//Ȥ�� �� ��Ȳ�� ����� �ʱ�ȭ
        //���� ���۵��ڸ��� �ڷ�ƾ�� �����մϴ�.
        StartCoroutine(CountdownToStart());
    }

    IEnumerator CountdownToStart()//���� ���� ī��Ʈ�ٿ��� ó���ϴ� �ڷ�ƾ
    {
        isCountdownFinished = false;
        //false�� �� ���̳� ���� �� ���ʿ��� ���� ���� ������, ����ġ ���� ���׸� ���� �ڵ��� �帧�� �� ��Ȯ�ϰ� �����ִ� ���� �����̾�!
        //ù ��° ���𿡼� �ʱ�ȭ�� ��ũ��Ʈ�� �⺻���� �����ϴ� ���̰�,
        //�� ��° �ڷ�ƾ���� �ʱ�ȭ�� Ư�� ���(ī��Ʈ�ٿ�)�� ���۵� ������ ���¸� �缳���ϴ� ������ ��


        //�ؽ�Ʈ ������Ʈ�� ������� �ʴٸ� Ȱ��ȭ
        if (CountdownText != null)
            CountdownText.gameObject.SetActive(true);
            //countdownText.gameObject.SetActive(true): SetActive��?
            //�� ���� ������Ʈ�� Ȱ��ȭ(true) �Ǵ� ��Ȱ��ȭ(false)�Ͽ� ȭ�鿡 ���̰ų� ����� �����̾�

        //3�� ī��Ʈ�ٿ�
        for (int i = 3; i > 0; i--)
        {
            //�ؽ�Ʈ�� 3, 2, 1�� ǥ��
            CountdownText.text = i.ToString();

            //ī��Ʈ�ٿ� ���� ���
            //�迭�� �ε����� ����� ���� ���� ���
            if (countdownAudioSource != null && countdownSounds.Length >= i)
            {
                countdownAudioSource.PlayOneShot(countdownSounds[3 - i]);
            }

            yield return new WaitForSecondsRealtime(1f);//1�� ���
            //WaitForSeconds(): �� �Լ��� ���� �ð�(Time.timeScale)�� ������ �޾�. Time.timeScale�� 0�� �Ǹ� �� �Լ��� ����.
            //WaitForSecondsRealtime(): �� �Լ��� ���� �ð��� �����,
        }

        //������ �޽��� ��� �� 1�� ���
        CountdownText.text = "Start!";
        if (countdownAudioSource != null && countdownSounds.Length > 3)
        {
            countdownAudioSource.PlayOneShot(countdownSounds[3]);//4��° �Ҹ����� ���
        }

        yield return new WaitForSecondsRealtime(1f);
        //WaitForSecondsRealtime�� ������. �׷��� Time.timeScale�� 0�̾ ī��Ʈ�ٿ��� �����

        //ī��Ʈ�ٿ� �ؽ�Ʈ ��Ȱ��ȭ
        if (CountdownText != null)
        {
            CountdownText.gameObject.SetActive(false);
        }

        //ī��Ʈ�ٿ��� �����ٴ� ���� �˷���
        isCountdownFinished = true;

        //ī��Ʈ�ٿ��� ������ ���� ������, 0f���� 1f�� �ٲ��ݾ�
        Time.timeScale = 1f;
    }
}
