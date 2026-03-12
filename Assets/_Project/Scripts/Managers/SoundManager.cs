using UnityEngine;

[RequireComponent(typeof(AudioSource))]// 이 스크립트가 붙은 오브젝트에 AudioSource가 없다면 자동으로 추가해주는 기능
//이 코드를 클래스 위에 적어두면, 네가 실수로 AudioSource를 삭제하려고 해도 유니티가,
//"이 스크립트 쓰려면 이거 필요해!"라며 삭제를 막아주고, 스크립트를 붙일 때 자동으로 컴포넌트도 추가해줘.

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }//싱글톤 패턴으로 어디서든 접근 가능하게
    private AudioSource sfxAudioSource;//SFX 믹서에 연결된 AudioSource

    void Awake()
    {
        //싱글톤 중복 방지 및 유지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        //AudioSource 참조 자동 연결
        InitializeAudioSource();
    }

    private void InitializeAudioSource()
    {
        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
        }

        //기본 설정 (SFX는 보통 루프하지 않고, 게임 시작 시 바로 재생되지 않게 설정)
        sfxAudioSource.playOnAwake = false;
        sfxAudioSource.loop = false;
        //주의: 인스펙터에서 이 AudioSource의 'Output'에 
        //Mixer의 SFX 그룹을 꼭 연결해줘야 나중에 볼륨 조절이 먹혀!
    }

    ///<summary>
    ///SFX 믹서 그룹을 통해 소리를 재생하는 공용 함수
    ///</summary>
    //<summary>, </summary> 기능은 다른 스크립트에서 이 PlaySFX함수가 어떤 역할을 하는지 알려주는거야,
    //여기서나 외부에서나 이 함수 이름을 선택하면 "SFX 믹서 그룹...." 적힌게 보여
    public void PlaySFX(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null) sfxAudioSource.PlayOneShot(clip);
        //SoundManager의 AudioSource를 사용하므로 믹서 조절이 가능함
    }
}
