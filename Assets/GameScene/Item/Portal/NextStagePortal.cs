using System.Collections;//코루틴을 위해 추가해주지 UwU
using UnityEngine;
using UnityEngine.SceneManagement;//씬 전환을 위해 추가


public class NextStagePortal : MonoBehaviour
{
    public string nextGameScene;
    public AudioClip portalSpawnSound;
    public AudioClip portalTouchSound;


    private void OnEnable()//매니저가 이 오브젝트를 SetActive(true) 하는 순간 실행됨!
    {
        if (SoundManager.Instance != null && portalSpawnSound != null)
            SoundManager.Instance.PlaySFX(portalSpawnSound);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) StartCoroutine(MoveToNextScene());
    }

    IEnumerator MoveToNextScene()
    {
        Time.timeScale = 0f;
        if (SoundManager.Instance != null && portalTouchSound != null)
        {
            SoundManager.Instance.PlaySFX(portalTouchSound);
            yield return new WaitForSecondsRealtime(portalTouchSound.length);
        }
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextGameScene);
    }
}
