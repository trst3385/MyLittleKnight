using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;//포스트 프로세싱 사용 시 필요

public class TimeFreeze : MonoBehaviour
{
    public static TimeFreeze Instance { get; private set; }//어디서든 접근할 수 있는 유일한 인스턴스(싱글톤)

    [Header("상태")]
    public bool IsTimeFrozen { get; private set; } = false;//모든 몬스터, 장애물, 게임 타이머는 이 플래그를 체크해야 해
                                                           //현재 시간이 정지 상태인지 확인하는 전역 플래그
    [Header("시각 효과")]
    [SerializeField] private PostProcessVolume freezeVolume;//TimeFreeze_Profile이 연결된 볼륨

    private AudioSource bgmAudioSource;
    private GameTimerManager gameTimerUI;
    private Coroutine freezeCoroutine;
    private float timeWhenFrozen;

    void Awake()
    {
        if (Instance == null) Instance = this;//싱글톤 설정
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        //외부 참조는 Start에서!
        InitializeReferences();
    }

    private void InitializeReferences()
    {
        //GameTimerUI 찾기
        if (gameTimerUI == null)
            gameTimerUI = Object.FindAnyObjectByType<GameTimerManager>();

        if (bgmAudioSource == null)//BGM 오디오 소스 찾기 (BGM_Manager 태그나 이름을 활용)
        {
            GameObject bgmObj = GameObject.Find("BGM_Manager");//이름으로 찾거나
            if (bgmObj != null) bgmAudioSource = bgmObj.GetComponent<AudioSource>();
        }

        if (freezeVolume != null) freezeVolume.enabled = false;//볼륨 초기화
    }

    ///<summary>
    ///아이템 획득 시 호출하여 시간 정지 기능을 활성화.
    ///</summary>
    ///<param name="duration">시간 정지 지속 시간 (초)</param>
    public void ActivateTimeFreeze(float duration)
    {
        if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);

        if (!IsTimeFrozen) timeWhenFrozen = Time.time;

        Debug.Log($"[TimeFreeze] 시간 정지 시작! {duration}초 동안 지속.");
        freezeCoroutine = StartCoroutine(FreezeTimeCoroutine(duration));
    }

    IEnumerator FreezeTimeCoroutine(float duration)
    {
        IsTimeFrozen = true;

        if (bgmAudioSource != null) bgmAudioSource.Pause();//BGM 일시정지

        if (freezeVolume != null) freezeVolume.enabled = true;//화면 효과 켜기

        float timer = duration;
        while (timer > 0)
        {
            timer -= Time.deltaTime;//Time.timeScale을 건드리지 않기 때문에 deltaTime은 정상 작동함
            yield return null;
        }
        ResumeTime();
        freezeCoroutine = null;
    }

    public void ResumeTime()//시간정지가 끝날때
    {
        if (!IsTimeFrozen) return;

        //1. 정지 지속 시간 계산
        float frozenDuration = Time.time - timeWhenFrozen;

        //2. UI 타이머 보정
        if (gameTimerUI != null) gameTimerUI.AdjustStartTime(frozenDuration);//시간이 멈춰있던 시간만큼 타이머 보정

        //3. 시간 재개 로직
        IsTimeFrozen = false;

        if (freezeVolume != null) freezeVolume.enabled = false;//시간 정지 해제 시 볼륨 비활성화 (화면 컬러 복구)

        //BGM 다시 재생 추가 (UnPause 사용)!
        if (bgmAudioSource != null) bgmAudioSource.UnPause();//BGM 다시 재생 추가 (UnPause 사용)!

        Debug.Log("시간 재개: " + frozenDuration.ToString("F2") + "초 동안 멈춤");
    }
}
