using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;//TextMeshPro를 사용하려면 추가
using UnityEngine.Audio;//AudioMixer를 사용하기 위한 네임스페이스
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager Instance { get; private set; }//어디서든 접근 가능하게 싱글톤 설정

    public GameObject optionsPanel;//최상위 옵션창 패널
    public TextMeshProUGUI warningText;//경고 메시지 텍스트 UI

    [Header("사운드 UI 및 믹서 설정")]
    public AudioClip buttonClickSound;//버튼 클릭 효과음
    public AudioMixer mainMixer;//MainMixer 에셋
    public GameObject Options;//메인 버튼 그룹 (Restart, Sound 등)
    public GameObject soundControlPanel;//사운드 조절 그룹 (Sliders, Back)
    public Slider bgmSlider;//BGM 슬라이더 
    public Slider sfxSlider;//SFX 슬라이더 

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

        InitializeReferences();//드래그 생략을 위한 자동 참조 시도
    }

    private void InitializeReferences()
    {
        //씬에 "OptionsCanvas" 같은 이름으로 프리팹이 있다면 내부에서 컴포넌트들을 자동으로 찾음
        //(직접 드래그가 되어있다면 넘어가고, 비어있을 때만 찾음)
        if (optionsPanel == null) optionsPanel = GameObject.Find("OptionsPanel");
        if (warningText == null) warningText = GameObject.Find("WarningText")?.GetComponent<TextMeshProUGUI>();

        //슬라이더 등도 이름으로 찾기 가능
        if (bgmSlider == null) bgmSlider = GameObject.Find("BGMSlider")?.GetComponent<Slider>();
        if (sfxSlider == null) sfxSlider = GameObject.Find("SFXSlider")?.GetComponent<Slider>();
    }

    void Start()//디스크에 저장된 값을 불러와 믹서와 슬라이더에 적용하도록 수정
    {
        //저장된 볼륨 값 로드 (저장된 값이 없으면 기본값 1.0f 사용)
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        //슬라이더의 위치를 로드된 값으로 설정
        if (bgmSlider != null) bgmSlider.value = bgmVolume;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;

        //3.SetVolume 함수를 호출하여 Audio Mixer에 최종 볼륨 적용
        //(SetVolume 함수 내부에 저장 로직을 넣을 예정이므로, 여기서 호출하면 로드와 동시에 저장도 됨)
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleOptionsPanel();//옵션 패널이 꺼져있을 때만 Esc를 누르면 패널을 켜
    }

    public void ToggleOptionsPanel()//옵션창과 활성화될때 게임의 일시정지 기능을 켜고 끄는 역할
    {
        //카운트다운 중에는 차단
        if (!optionsPanel.activeSelf && !CountdownManager.isCountdownFinished)
        {
            ShowWarning("카운트 중이야! 잠시만 기다려!");
            return;
        }

        bool isPanelActive = !optionsPanel.activeSelf;
        optionsPanel.SetActive(isPanelActive);

        //버튼 사운드 재생
        if (SoundManager.Instance != null && buttonClickSound != null)
            SoundManager.Instance.PlaySFX(buttonClickSound);

        if (isPanelActive)
        {
            Time.timeScale = 0f;//일시정지
            soundControlPanel.SetActive(false);
            Options.SetActive(true);
        }
        else
        {
            if (CountdownManager.isCountdownFinished) Time.timeScale = 1f;//옵션창 닫을 때 카운트가 끝난 상태면 시간 재개
        }
    }

    private void ShowWarning(string message)
    {
        if (warningText != null)
        {
            warningText.text = message;
            warningText.gameObject.SetActive(true);
            StopAllCoroutines();//기존 코루틴 중복 방지
            StartCoroutine(HideWarningText());
        }
    }

    IEnumerator HideWarningText()//warningText UI 경고 메시지를 숨기는 코루틴
    {
        yield return new WaitForSecondsRealtime(1f);//warningText UI 메세지가 띄워지면 n초 동안 기다려

        if (warningText != null) warningText.gameObject.SetActive(false);//warningText UI 메시지를 비활성화
    }

    public void SetBGMVolume(float volume)//볼륨 조절
    {   //-80dB가 **'무음(Silence)'**을 나타내는 값
        //슬라이더 값이 0에 가까울 때(Min Value) 완전히 무음(-80dB)으로 설정
        float db = (volume <= 0.0001f) ? -80f : 20f * Mathf.Log10(volume);

        mainMixer.SetFloat("BGMVolume", db);
        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();//안전하게 즉시 저장
    }
    public void SetSFXVolume(float volume)//효과음 조절
    {
        float db = (volume <= 0.0001f) ? -80f : 20f * Mathf.Log10(volume);

        mainMixer.SetFloat("SFXVolume", db);
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void OpenSoundControls()//SoundButton 클릭 시 호출
    {
        PlayClickSound();//Sound창 버튼을 누를때 사운드
        Options.SetActive(false);//1. 메인 버튼 컨테이너를 숨기고
        soundControlPanel.SetActive(true);//2. 사운드 조절 패널을 보여줘
    }
    public void CloseSoundControls()//SoundControlPanel의 BackButton 클릭 시 호출
    {
        PlayClickSound();//Sound창의 Done 버튼을 누를때 사운드
        soundControlPanel.SetActive(false);//1. 사운드 조절 패널을 숨기고
        Options.SetActive(true);//2. 메인 버튼 컨테이너를 다시 보여줘
    }

    public void RestartGame()//게임 재시작 함수 (현재 씬을 재시작)
    {
        PlayClickSound();//ReStart 버튼을 누를때 사운드
        Time.timeScale = 1f;//재시작시 정지된 시간을 정상(1.0)으로 복구
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);//현재 활성화된 씬의 이름을 가져와서 해당 씬을 다시 로드 (재시작)
    }

    public void GoToMainMenu()//현 게임을 끄고 메인화면으로 가는 함수
    {
        PlayClickSound();//MainMenu 버튼 누를때 사운드
        Time.timeScale = 1f;//게임 재시작과 마찬가지로, 메인화면으로 돌아갈 때도 시간을 정상으로 돌려놔.
        SceneManager.LoadScene("MainMenuScene");//MainMenuScene 이름의 씬을 로드해서 다시 메인화면 씬으로 돌아가게 해
    }

    public void  QuitGame()//게임을 완전히 끄는 함수
    {
        PlayClickSound();//Quit Game 버튼 누를때 사운드
        Time.timeScale = 1f;

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void PlayClickSound()
    {
        if (SoundManager.Instance != null && buttonClickSound != null)
            SoundManager.Instance.PlaySFX(buttonClickSound);
    }
}


