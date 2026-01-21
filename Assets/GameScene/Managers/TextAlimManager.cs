using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;//TextMeshPro를 사용하려면 이 using 문이 필요


public class TextAlimManager : MonoBehaviour
{
    public static TextAlimManager Instance { get; private set; }//싱글톤 선언

    [Header("UI 연결")]
    public TextMeshProUGUI NotificationText;//인스펙터에서 UI 텍스트 컴포넌트를 할당할 변수

    [Header("알림이 표시, 사라질 시간")]
    public float DisplayDuration = 2f;//알림이 화면에 표시될 시간 (초)
    public float FadeDuration = 0.5f;//알림이 사라질 때 페이드아웃 되는 시간 (초)

    private Coroutine currentNotificationCoroutine;//현재 실행 중인 코루틴을 추적


    void Awake()
    {
        if (Instance == null) Instance = this;//싱글톤 설정
        else { Destroy(gameObject); return; }

        if (NotificationText == null)
        {
            //만약 깜빡하고 연결 안 했다면 씬 전체에서 딱 하나 있는 알림 텍스트를 찾아봄
            //하지만 가급적 인스펙터에서 직접 드래그해주는 게 성능상 좋아!
            NotificationText = GameObject.Find("NotificationText")?.GetComponent<TextMeshProUGUI>();

            if (NotificationText == null)
                Debug.LogError("TextAlimManager: 연결된 UI 텍스트가 없어!");
        }

        //시작 시 텍스트를 숨김
        NotificationText.color = new Color(NotificationText.color.r,
        NotificationText.color.g, NotificationText.color.b, 0);
    }
    

    public void ShowNotification(string message)//외부에서 알림을 요청할 때 호출되는 함수
    {//외부에서 받은 텍스트를 string 형식의 message 매개변수에 담아두고 DisplayNotification 코루틴으로 보냄

        //이전에 실행 중인 코루틴이 있다면 중지 간결화
        if (currentNotificationCoroutine != null) StopCoroutine(currentNotificationCoroutine);

        //접수받은 message 값을 DisplayNotification 코루틴을 시작
        currentNotificationCoroutine = StartCoroutine(DisplayNotification(message));
        //ShowNotification이 자신의 message 변수(복사본)에 있는 문자열 값을 다시 복사해서 DisplayNotification에 넘겨줌.
    }

    
    IEnumerator DisplayNotification(string message)//ShowNotification이 넘겨준 메세지를 UI에 뜨게 하는 역할
    {
        NotificationText.text = message;//UI에 받은 텍스트를 보냄

        //페이드인 (투명도 0 -> 1)
        float timer = 0f;
        Color startColor = new Color(NotificationText.color.r, NotificationText.color.g, NotificationText.color.b, 0);
        Color targetColor = new Color(NotificationText.color.r, NotificationText.color.g, NotificationText.color.b, 1);
        while (timer < FadeDuration)
        {
            NotificationText.color = Color.Lerp(startColor, targetColor, timer / FadeDuration);
            timer += Time.deltaTime;
            yield return null;//한 프레임 대기
        }
        NotificationText.color = targetColor;//정확히 불투명하게 설정

        yield return new WaitForSeconds(DisplayDuration);
        //알림이 displayDuration 변수에 설정된 시간(초)만큼 화면에 유지되도록 대기
        //이 코루틴이 잠시 멈춰있다가, 이 줄 아래의 코드를 실행하여 알림을 사라지게 합니다.
        
        //페이드아웃 (투명도 1 -> 0)
        timer = 0f;
        startColor = targetColor;//현재 색상 (불투명)
        targetColor = new Color(NotificationText.color.r, NotificationText.color.g, NotificationText.color.b, 0);

        while (timer < FadeDuration)//텍스트의 페이드인/페이드아웃 애니메이션
        {
            //시간에 따라 텍스트 색상을 시작(startColor)부터 목표(targetColor)까지 부드럽게 변화시킴 (페이드인/아웃 애니메이션)
            NotificationText.color = Color.Lerp(startColor, targetColor, timer / FadeDuration);
            timer += Time.deltaTime;//프레임당 경과 시간만큼 타이머 증가
            yield return null;//다음 프레임까지 대기하여 애니메이션 업데이트
        }
        NotificationText.color = targetColor;//완전히 투명하게 설정
        NotificationText.text = "";//텍스트 내용도 비워주기
    }
}
