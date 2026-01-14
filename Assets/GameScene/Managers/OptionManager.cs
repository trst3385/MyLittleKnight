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

        InitializeReferences();//씬 시작하자마자 드래그 없이 모든 UI 자동 연결
    }

    private void InitializeReferences()
    {
        //최상위 패널 찾기 (비활성화 상태여도 찾아야 함)
        if (optionsPanel == null)
        {
            Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.gameObject.scene.name == null) continue; // 프리팹 제외
                Transform found = canvas.transform.Find("OptionsPanel");
                if (found != null)
                {
                    optionsPanel = found.gameObject;
                    break;
                }
            }
        }

        if (optionsPanel != null)//패널 안의 자식들 이름으로 자동 찾기 및 버튼 함수 연결
        {
            //WarningText도 이제 패널 안에 있으니 FindChildEx로 한 번만 찾으면 돼
            warningText = FindChildEx(optionsPanel.transform, "WarningText")?.GetComponent<TextMeshProUGUI>();

            //하위 그룹 오브젝트 찾기
            Options = FindChildEx(optionsPanel.transform, "Options");
            soundControlPanel = FindChildEx(optionsPanel.transform, "SoundControlPanel");

            //슬라이더 및 텍스트 찾기
            bgmSlider = FindChildEx(optionsPanel.transform, "BGMSlider")?.GetComponent<Slider>();
            sfxSlider = FindChildEx(optionsPanel.transform, "SFXSlider")?.GetComponent<Slider>();
            warningText = FindChildEx(optionsPanel.transform, "WarningText")?.GetComponent<TextMeshProUGUI>();

            //버튼 자동 연결. 인스펙터 OnClick()에 수동 연결할 필요 없음!
            SetupButton("ReStartButton", RestartGame);
            SetupButton("Main MenuButton", GoToMainMenu);
            SetupButton("SoundButton", OpenSoundControls);
            SetupButton("BackButton", CloseSoundControls);
            SetupButton("QuitGameButton", QuitGame);

            //슬라이더 자동 연결
            if (bgmSlider != null)
            {
                bgmSlider.onValueChanged.RemoveAllListeners();
                bgmSlider.onValueChanged.AddListener(SetBGMVolume);
            }
            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            }

            optionsPanel.SetActive(false);//시작할 때 옵션창 꺼두기
            if (warningText != null) warningText.gameObject.SetActive(false);
        }
    }

    private GameObject FindChildEx(Transform parent, string name)//이름만으로 자식을 뒤져서 찾아주는 헬퍼 함수
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == name) return child.gameObject;
        }
        return null;
    }

    private void SetupButton(string btnName, UnityEngine.Events.UnityAction action)//버튼을 찾아 함수(리스너)를 직접 달아주는 함수
    {
        GameObject btnObj = FindChildEx(optionsPanel.transform, btnName);
        if (btnObj != null)
        {
            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(action);
            }
        }
    }


    void Start()//디스크에 저장된 값을 불러와 믹서와 슬라이더에 적용하도록 수정
    {
        //저장된 볼륨 값 로드 (저장된 값이 없으면 기본값 1.0f 사용)
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        //슬라이더의 위치를 로드된 값으로 설정
        if (bgmSlider != null) bgmSlider.value = bgmVolume;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;

        //SetVolume 함수를 호출하여 Audio Mixer에 최종 볼륨 적용
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
        //1. 카운트다운 중일 때 처리
        if (!CountdownManager.isCountdownFinished)
        {
            if (!optionsPanel.activeSelf)
            {
                StopAllCoroutines(); // 중복 방지
                StartCoroutine(ShowWarningOnly());
                return;
            }
        }

        //2. 카운트다운 끝난 후 정상 토글 로직
        bool isPanelActive = !optionsPanel.activeSelf;
        optionsPanel.SetActive(isPanelActive);

        if (SoundManager.Instance != null && buttonClickSound != null)
            SoundManager.Instance.PlaySFX(buttonClickSound);

        if (isPanelActive)
        {
            Time.timeScale = 0f;
            if (soundControlPanel != null) soundControlPanel.SetActive(false);
            if (Options != null) Options.SetActive(true);
            if (warningText != null) warningText.gameObject.SetActive(false);
        }
        else
        {
            if (CountdownManager.isCountdownFinished) Time.timeScale = 1f;//닫을 때 카운트가 끝난 상태면 시간 재개
        }
    }

    IEnumerator ShowWarningOnly()
    {
        optionsPanel.SetActive(true);//warningText가 OptionsPanel의 자식이므로 부모를 먼저 켜야 함

        //버튼들은 안 보이게 숨기기
        if (Options != null) Options.SetActive(false);
        if (soundControlPanel != null) soundControlPanel.SetActive(false);

        if (warningText != null)
        {
            warningText.text = "카운트 중이야! 잠시만 기다려!";
            warningText.gameObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(1f);//실제 시간 기준으로 1초 대기 (Time.timeScale이 0이어도 작동)

        if (warningText != null) warningText.gameObject.SetActive(false);

        optionsPanel.SetActive(false);//마지막에 부모 패널을 다시 꺼줘야 원래 화면으로 돌아옴
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


