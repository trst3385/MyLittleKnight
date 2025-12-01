using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }//싱글톤 패턴으로 어디서든 접근 가능하게
    private AudioSource sfxAudioSource;//SFX 믹서에 연결된 AudioSource

    void Awake()
    {
        DontDestroyOnLoad(gameObject);// 씬이 변경되어도 파괴되지 않도록 설정

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
    public void PlaySFX(AudioClip clip)
    {
        if (sfxAudioSource != null && clip != null) sfxAudioSource.PlayOneShot(clip);
        //SoundManager의 AudioSource를 사용하므로 믹서 조절이 가능함
    }
}
