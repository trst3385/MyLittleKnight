using UnityEngine;

public class SoundManager : MonoBehaviour
{


    //12.3 지금은 아이템 생성시에 들릴 목적으로 사용하고 있어. 나중에는 이 스크립트를 이용해서 다른 오브젝트,
    //프리팹에 필요할 사운드를 조절해보자
    public static SoundManager Instance { get; private set; }//싱글톤 패턴으로 어디서든 접근 가능하게
    private AudioSource sfxAudioSource;//SFX 믹서에 연결된 AudioSource

    void Awake()
    {
        DontDestroyOnLoad(gameObject);//씬이 변경되어도 파괴되지 않도록 설정

        //SoundManager가 씬에 하나만 있도록 보장
        if (Instance == null)
        {
            Instance = this;
            sfxAudioSource = GetComponent<AudioSource>();
        }
        else Destroy(gameObject);
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
