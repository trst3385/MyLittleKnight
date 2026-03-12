using System.Collections;//코루틴을 위해 추가해주지 UwU
using UnityEngine;
using UnityEngine.SceneManagement;//씬 전환을 위해 추가


public class NextStagePortal : MonoBehaviour
{
    public string nextGameScene;//인스펙터에 넘어갈 씬 이름을 적자!
    [Header("사운드 설정")]
    [SerializeField] private AudioClip portalSpawnSound;//스폰 됐을때 사운드
    [SerializeField] private AudioClip portalTouchSound;//포탈에 닿았을때 사운드

    private bool isTransitioning = false;//중복 이동 방지 플래그

    private void OnEnable()//매니저가 이 오브젝트를 SetActive(true) 하는 순간 실행됨!
    {
        if (SoundManager.Instance != null && portalSpawnSound != null)
            SoundManager.Instance.PlaySFX(portalSpawnSound);
    }

    private void OnTriggerEnter2D(Collider2D other)//닿은 오브젝트가 플레이어(태그)면 함수 발동
    {
        //1. 닿는 오브젝트의 태그가 플레이어야? 2.이미 이동 중은 아닌가? (중복 실행 방지)
        if (!isTransitioning && other.CompareTag("Player"))
            StartCoroutine(MoveToNextScene());
    }

    IEnumerator MoveToNextScene()
    {
        isTransitioning = true;//포탈 통과 시작, 이제 다른 충돌은 무시

        //게임 정지 (사운드에 집중하고 씬 로딩 중 변수 변화 방지)
        Time.timeScale = 0f;
        if (SoundManager.Instance != null && portalTouchSound != null)
        {
            SoundManager.Instance.PlaySFX(portalTouchSound);
            //TimeScale이 0이라서 Realtime 대기를 사용해야 소리 길이만큼 기다림
            yield return new WaitForSecondsRealtime(portalTouchSound.length);
        }

        Time.timeScale = 1f;//씬 이동 직전에 시간을 반드시 1로 복구(일시정지 해제)
        if (!string.IsNullOrEmpty(nextGameScene))
            SceneManager.LoadScene(nextGameScene);//다음 넘어갈 씬 이름이 확인되면 그 다음 씬으로 넘어가
        else//이름이 없거나 비어있으면 에러로그를 띄우고 재시도
        {
            Debug.LogError($"{gameObject.name}: 다음 씬 이름이 설정 안 됐어!");
            isTransitioning = false;//에러 시 다시 포탈 이동을 시도할 수 있게 풀어줌
        }
    }
}
