using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;//TextMeshPro를 사용하기 위해 필요


public class CountdownManager : MonoBehaviour
{
    //다른 스크립트가 카운트다운이 끝났는지 알 수 있게 하는 변수 (다른 스크립트에서 접근 가능하도록 static으로 선언)
    //public static 변수라서 다른 t스크립트에서 이 변수를 쓸때 public 변수로 이 스크립트를 연결하지 않아도 돼.
    //**클래스이름.변수** 로 접근이 가능해!
    public static bool isCountdownFinished = false;//기본값은 false 상태.
    //혹시라도 게임을 재시작하거나 다른 씬에서 넘어올 때,
    //이전 상태의 true 값이 남아 있을 수 있기 때문에, 확실하게 false 상태로 만들어주는 거야


    [Header("CountdownText UI연결")]//인스펙터에 할당할 카운트다운 텍스트UI 오브젝트
    public TextMeshProUGUI CountdownText;

    [Header("사운드 연결")]//카운트다운에 나올 사운드
    public AudioSource countdownAudioSource;
    public AudioClip[] countdownSounds;//배열을 만드는거야. AudioClip을 넣을 countdownSounds 변수들을 배열로 여러개 만드는거야

    void Start()
    {
        //게임 시작 전에 멈춤. 카운트가 끝나야 게임 시작
        //Time.timeScale: 게임 전체의 시간 흐름 속도를 조절합니다. 0f로 설정하면 게임이 일시정지 상태
        //카운트가 끝나면 시작해야 하니 밑에는 1f;로 둘꺼야
        Time.timeScale = 0f;

        isCountdownFinished = false;//혹시 모를 상황을 대비해 초기화
        //씬이 시작되자마자 코루틴을 실행합니다.
        StartCoroutine(CountdownToStart());
    }

    IEnumerator CountdownToStart()//게임 시작 카운트다운을 처리하는 코루틴
    {
        isCountdownFinished = false;
        //false를 두 번이나 적는 게 불필요해 보일 수도 있지만, 예상치 못한 버그를 막고 코드의 흐름을 더 명확하게 보여주는 좋은 습관이야!
        //첫 번째 선언에서 초기화는 스크립트의 기본값을 설정하는 것이고,
        //두 번째 코루틴에서 초기화는 특정 기능(카운트다운)이 시작될 때마다 상태를 재설정하는 역할을 해


        //텍스트 오브젝트가 비어있지 않다면 활성화
        if (CountdownText != null)
            CountdownText.gameObject.SetActive(true);
            //countdownText.gameObject.SetActive(true): SetActive란?
            //이 게임 오브젝트를 활성화(true) 또는 비활성화(false)하여 화면에 보이거나 숨기는 역할이야

        //3초 카운트다운
        for (int i = 3; i > 0; i--)
        {
            //텍스트를 3, 2, 1로 표시
            CountdownText.text = i.ToString();

            //카운트다운 사운드 재생
            //배열의 인덱스를 사용해 사운드 파일 재생
            if (countdownAudioSource != null && countdownSounds.Length >= i)
            {
                countdownAudioSource.PlayOneShot(countdownSounds[3 - i]);
            }

            yield return new WaitForSecondsRealtime(1f);//1초 대기
            //WaitForSeconds(): 이 함수는 게임 시간(Time.timeScale)에 영향을 받아. Time.timeScale이 0이 되면 이 함수도 멈춰.
            //WaitForSecondsRealtime(): 이 함수는 실제 시간을 사용해,
        }

        //마지막 메시지 출력 후 1초 대기
        CountdownText.text = "Start!";
        if (countdownAudioSource != null && countdownSounds.Length > 3)
        {
            countdownAudioSource.PlayOneShot(countdownSounds[3]);//4번째 소리파일 재생
        }

        yield return new WaitForSecondsRealtime(1f);
        //WaitForSecondsRealtime로 수정해. 그래야 Time.timeScale이 0이어도 카운트다운이 진행돼

        //카운트다운 텍스트 비활성화
        if (CountdownText != null)
        {
            CountdownText.gameObject.SetActive(false);
        }

        //카운트다운이 끝났다는 것을 알려줘
        isCountdownFinished = true;

        //카운트다운이 끝나면 게임 시작해, 0f에서 1f로 바꿨잖아
        Time.timeScale = 1f;
    }
}
