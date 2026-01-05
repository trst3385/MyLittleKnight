using System.Collections;//코루틴을 위해 추가해주지 UwU
using UnityEngine;
using UnityEngine.SceneManagement;//씬 전환을 위해 추가


public class NextStagePortal : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    public string nextGameScene;//씬의 이름을 적어!(string이니까 씬마다 다른 씬 이름을 적자)

    [Header("포탈 등장 사운드")]
    public AudioClip appearanceSound;//포탈이 등장할때 사운드

    [Header("포탈에 닿았을때 사운드")]
    public AudioClip portalSound;//포탈에 닿으면 들릴 사운드(인스펙터에 연결)

    private bool isTransitioning = false;//콜라이더 중복 상호작용 방지용


    private void OnEnable()//포탈이 활성화(SetActive(true))될 때 자동으로 실행되는 함수야
    {                      //OnEnable은 활성화가 될때, OnDisable은 반대로 비활성화가 될때의 함수
        //SoundManager를 통해 등장 소리를 재생해
        if (SoundManager.Instance != null && appearanceSound != null)
            SoundManager.Instance.PlaySFX(appearanceSound);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //1. 이미 이동 중(true)이면 함수를 즉시 종료(중복 실행 방지)
        //2. 플레이어 태그가 아니면 무시해 상호작용 불가
        if (isTransitioning || !other.CompareTag("Player")) return;
        isTransitioning = true;//포탈에 닿으면 true로 바꿔서 한번만 닿지, 중복으로 닿지 않게

        StartCoroutine(MoveToNextScene());
    }

    IEnumerator MoveToNextScene()//게임을 일시정지시키고 포탈에 닿았을때의 사운드를 들려줘
    {
        Time.timeScale = 0f;//포탈에 닿았을때 게임 일시정지

        //SoundManager를 통해 사운드 재생
        if (SoundManager.Instance != null && portalSound != null)
        {
            SoundManager.Instance.PlaySFX(portalSound);
            yield return new WaitForSecondsRealtime(portalSound.length);//사운드 길이만큼 현실 시간 기준으로 대기
        }

        //이동 직전에 시간을 다시 1로 돌려놓는게 안전
        Time.timeScale = 1f;
        Debug.Log($"{nextGameScene}으로 이동!");
        SceneManager.LoadScene(nextGameScene);
    }
}
