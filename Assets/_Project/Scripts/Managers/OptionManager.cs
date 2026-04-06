using System.Collections;
using TMPro;//TextMeshPro를 사용하려면 추가
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Audio;//AudioMixer를 사용하기 위한 네임스페이스
using UnityEngine.SceneManagement;
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
        DontDestroyOnLoad(gameObject);//씬이 바껴도, 다음 씬으로 넘어가도 설정된 상태(BGM,SFX)는 유지   

        InitializeReferences();//씬 시작하자마자 드래그 없이 모든 UI 자동 연결
    }

    void OnEnable()//씬이 로드될 때마다 실행되도록 유니티 시스템에 등록하는 거야
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()//오브젝트가 사라질 때 등록했던 걸 해제해주는 안전장치
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        optionsPanel = null;//이전 씬의 UI 참조를 비워줌으로써 새 씬의 UI를 다시 찾게 함
        InitializeReferences();//새로운 씬이 열릴 때마다 이 함수가 자동으로 실행
    }

    private void InitializeReferences()
    {
        if (optionsPanel == null)//최상위 패널 찾기 (비활성화 상태여도 찾아야 함)
        {
            Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas.gameObject.scene.name == null)
                {
                    continue;//프리팹 제외
                }
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
            //WarningText도 이제 패널 안에 있으니 FindChildEx함수 호출로 한 번만 찾으면 돼
            warningText = FindChildEx(optionsPanel.transform, "WarningText")?.GetComponent<TextMeshProUGUI>();

            //하위 그룹 오브젝트 찾기
            Options = FindChildEx(optionsPanel.transform, "Options");
            soundControlPanel = FindChildEx(optionsPanel.transform, "SoundControlPanel");

            //슬라이더 및 텍스트 찾기
            bgmSlider = FindChildEx(optionsPanel.transform, "BGMSlider")?.GetComponent<Slider>();
            sfxSlider = FindChildEx(optionsPanel.transform, "SFXSlider")?.GetComponent<Slider>();

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

            //---메인화면 씬에서의 옵션창(사운드조정 창만 사용)---
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == "MainMenuScene")
            {
                //메인 씬: 'Options' 버튼 그룹은 끄고, '사운드 조절창'만 켜진 상태로 세팅
                if (Options != null) Options.SetActive(false);
                if (soundControlPanel != null) soundControlPanel.SetActive(true);
            }
            else
            {
                //게임 씬: 원래대로 'Options' 버튼 그룹을 먼저 보여줌
                if (Options != null) Options.SetActive(true);
                if (soundControlPanel != null) soundControlPanel.SetActive(false);
            }
            //---.........................................---

            optionsPanel.SetActive(false);//시작할 때 옵션창 꺼두기
            if (warningText != null)
            {
                warningText.gameObject.SetActive(false);
            } 
        }
    }

    private GameObject FindChildEx(Transform parent, string name)//이름만으로 자식을 뒤져서 찾아주는 헬퍼 함수
    {
        //1. 부모 아래에 있는 모든 자식/손자들의 Transform 정보를 배열로 가져옴 (비활성화 오브젝트도 포함)
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        //GetComponentsInChildren<T>: 나를 포함해서 내 밑에 매달린 모든 세대의 'T' 부품을 싹 다 수집하라는 명령
        //모든 오브젝트 (하이어라키에 담긴 모든건)는 Transform을 가지고 있으므로, 사실상 모든 자식 오브젝트를 다 리스트업하겠다는 뜻

        foreach (Transform child in children)//2. 배열에 담긴 모든 자식을 하나하나 순회하며 확인
            if (child.name == name) return child.gameObject;//3. 현재 확인 중인 자식의 이름이 내가 찾는 이름(name)과 일치하면 즉시 반환
        return null;//4. 반복문이 끝날 때까지 오브젝트의 이름을 못 찾았으면 null(없음)을 반환
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
        //카운트다운 중일 때 처리
        if (!CountdownManager.isCountdownFinished)
        {
            if (!optionsPanel.activeSelf)
            {
                StopAllCoroutines();//중복 방지
                StartCoroutine(ShowWarningOnly());
                return;
            }
        }

        //카운트다운 끝난 후 정상 토글 로직
        bool isPanelActive = !optionsPanel.activeSelf;
        optionsPanel.SetActive(isPanelActive);

        //카운트다운이 끝났을 때만 버튼 사운드 재생
        if (CountdownManager.isCountdownFinished)
        {
            if (SoundManager.Instance != null && buttonClickSound != null)
                SoundManager.Instance.PlaySFX(buttonClickSound);
        }

        if (isPanelActive)//옵션창이 활성화(켜짐) 상태라면 게임 일시정지
        {
            Time.timeScale = 0f;//게임의 물리적 시간을 0으로 만들어 일시정지
            if (soundControlPanel != null) soundControlPanel.SetActive(false);
            if (Options != null) Options.SetActive(true);
            if (warningText != null) warningText.gameObject.SetActive(false);
        }
        else//옵션창이 비활성화(꺼짐) 상태라면
        {
            //카운트다운이 이미 끝난 상태에서만 시간을 다시 흐르게 해(중요한 안전장치!)
            if (CountdownManager.isCountdownFinished) Time.timeScale = 1f;
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

        if (optionsPanel != null)//1. 전체 패널이 꺼져있을 수도 있으니 먼저 켜줘
        {
            optionsPanel.SetActive(true);
        }
        //2. 메인 버튼들(Restart, Quit 등) 숨기기
        //게임 씬에서는 Options가 찾아졌을 테니 정상적으로 작동하고,
        //메인 씬에서는 null이라도 에러 없이 그냥 통과해
        if (Options != null)
        {
            Options.SetActive(false);
        }

        if (soundControlPanel != null)//3. 사운드 조절 패널 보여주기
        {
            soundControlPanel.SetActive(true);
        }
    }
    public void CloseSoundControls()//SoundControlPanel의 BackButton 클릭 시 호출
    {
        PlayClickSound();//Sound창의 Done 버튼을 누를때 사운드

        string sceneName = SceneManager.GetActiveScene().name;//현재 씬 이름을 확인
        if (sceneName == "MainMenuScene")
        {
            //1. 메인 씬이라면? 그냥 전체 옵션 패널을 아예 꺼버려!
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }
        else
        {
            //2. 게임 씬이라면? 원래대로 사운드 창만 끄고 메인 버튼들(Restart 등)을 보여줘
            if (soundControlPanel != null) soundControlPanel.SetActive(false);
            if (Options != null) Options.SetActive(true);
        }
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
    
    public void QuitGame()//게임을 완전히 끄는 함수
    {
        PlayClickSound();//Quit Game 버튼 누를때 사운드
        Time.timeScale = 1f;//옵션창을 켜서 멈춘 시간을 정상으로 돌려놓음
        StartCoroutine(QuitWithTinyDelay(0.15f));//바로 종료하지 않고 코루틴 호출(사운드가 발동되고 0.1초 후 종료)
    }
    //재시작, 메인화면 이동 버튼 사운드는 SoundManager와 OptionsManager는 DontDestroyOnLoad 덕분에,
    //씬이 바뀌는 와중에도 AudioSource가 소리를 끝까지 낼 수 있었던 거야. Quit은 아예 프로세스 자체를 종료시키니 예외였던 거지
    private IEnumerator QuitWithTinyDelay(float delay)//Quit 버튼을 누를때 사운드가 들리게 한 후 게임 종료 코루틴.
    {                                                 //delay 매개변수로 쓰는 이유는 나중에 시간을 QuitGame 함수에서,
                                                      //숫자 하나만 고치면 되니까 관리가 훨씬 편해지지. 

        yield return new WaitForSecondsRealtime(delay);//WaitForSecondsRealtime을 써야 Time.timeScale 영향 없이 정확히 기다려

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


