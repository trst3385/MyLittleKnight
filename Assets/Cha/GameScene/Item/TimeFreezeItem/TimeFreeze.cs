using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

public class TimeFreeze : MonoBehaviour
{
    public static TimeFreeze Instance { get; private set; }//어디서든 접근할 수 있는 유일한 인스턴스(싱글톤)
    public bool IsTimeFrozen { get; private set; } = false;//모든 몬스터, 장애물, 게임 타이머는 이 플래그를 체크해야 해
                                                           //현재 시간이 정지 상태인지 확인하는 전역 플래그
    [Header("시각 효과")]
    [SerializeField] private PostProcessVolume freezeVolume;//TimeFreeze_Profile이 연결된 볼륨

    [Header("배경음악 제어")]
    [SerializeField] private AudioSource bgmAudioSource;//BGM_Manager오브젝트의 AudioSource 연결,정지 시 배경음악 멈춤

    private GameTimerUI gameTimerUI;//GameTimerUI 변수
    private Coroutine freezeCoroutine;//시간 정지 코루틴을 제어할 변수
    private float timeWhenFrozen;//시간이 정지되기 시작한 시점을 기록할 변수

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);//싱글톤 패턴 구현으로 이 스크립트의 유일한 인스턴스를 보장
        else Instance = this;

        //GameTimerUI 참조 찾기
        gameTimerUI = FindFirstObjectByType<GameTimerUI>();
        if (gameTimerUI == null) Debug.LogError("TimeFreeze: GameTimerUI 스크립트를 찾을 수 없어!");
    }

    public void ResumeTime()
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

    ///<summary>
    ///아이템 획득 시 호출하여 시간 정지 기능을 활성화.
    ///</summary>
    ///<param name="duration">시간 정지 지속 시간 (초)</param>
    public void ActivateTimeFreeze(float duration)
    {
        //1.이미 정지 중이라면 기존 코루틴을 멈추고 새로운 시간으로 연장
        if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);

        if (!IsTimeFrozen) timeWhenFrozen = Time.time;//새로 정지를 시작할 때만 timeWhenFrozen 기록

        Debug.Log($"[TimeFreeze] 시간 정지 시작! {duration}초 동안 지속.");
        freezeCoroutine = StartCoroutine(FreezeTimeCoroutine(duration));
    }

    IEnumerator FreezeTimeCoroutine(float duration)
    {
        IsTimeFrozen = true;//1. 시간 정지 상태 On
                            
        if (bgmAudioSource != null) bgmAudioSource.Pause();//BGM 일시 정지

        //@@정지 시작 시 사운드, 이펙트 등을 여기에 추가해@@

        if (freezeVolume != null) freezeVolume.enabled = true;//시간 정지 시작 시 볼륨 활성화 (화면 회색)


        float timer = duration;

        while (timer > 0)//2. 시간이 멈춰있는 동안 카운트다운
        {
            timer -= Time.deltaTime;//Time.deltaTime을 사용하여 시간이 멈춰도 플레이어 관점의 시간은 흐르게 함
            yield return null;
        }

        ResumeTime();//코루틴이 끝났을 때 ResumeTime()을 호출

        freezeCoroutine = null;//(정지 해제 시 사운드, 이펙트 등을 여기에 추가할 수 있어)
    }
}
