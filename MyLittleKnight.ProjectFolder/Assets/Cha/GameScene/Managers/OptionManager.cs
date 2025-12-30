using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;//TextMeshPro를 사용하려면 추가
using UnityEngine.Audio;//AudioMixer를 사용하기 위한 네임스페이스
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public GameObject optionsPanel;//옵션창 패널 연결할 변수
    public TextMeshProUGUI warningText;//경고 메시지 텍스트 UI 연결할 변수

    [Header("사운드 UI 및 믹서 설정")]
    public AudioMixer mainMixer;//MainMixer 에셋 연결 (인스펙터에서)
    public GameObject Options;//메인 버튼들(Restart, SoundButton 등)을 담은 'Options' 오브젝트 연결
    public GameObject soundControlPanel;//슬라이더와 BackButton을 담은 'SoundControlPanel' 오브젝트 연결
    public Slider bgmSlider;//BGM 슬라이더 오브젝트 참조
    public Slider sfxSlider;//SFX 슬라이더 오브젝트 참조


    void Start()//디스크에 저장된 값을 불러와 믹서와 슬라이더에 적용하도록 수정
    {
        //1. 저장된 볼륨 값 로드 (저장된 값이 없으면 기본값 1.0f 사용)
        float bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        //2. 슬라이더의 위치를 로드된 값으로 설정
        if (bgmSlider != null) bgmSlider.value = bgmVolume;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;

        //3.SetVolume 함수를 호출하여 Audio Mixer에 최종 볼륨 적용
        //(SetVolume 함수 내부에 저장 로직을 넣을 예정이므로, 여기서 호출하면 로드와 동시에 저장도 됨)
        if (bgmSlider != null) SetBGMVolume(bgmSlider.value);
        if (sfxSlider != null) SetSFXVolume(sfxSlider.value);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) ToggleOptionsPanel();//옵션 패널이 꺼져있을 때만 키를 누르면 패널을 켜
    }

    public void ToggleOptionsPanel()//옵션창과 활성화될때 게임의 일시정지 기능을 켜고 끄는 역할
    {
        //카운트다운 중에는 차단하고 경고 메시지 출력
        if (!optionsPanel.activeSelf && !CountdownManager.isCountdownFinished)
        {
            if (warningText != null)//경고 메시지를 활성화
            {
                warningText.gameObject.SetActive(true);
                warningText.text = "카운트 중이야! 잠시만 기다려!";
                StartCoroutine(HideWarningText());
            }
            return;//여기서 함수를 끝내서 옵션 창이 켜지지 않게 막아
        }

        if (warningText != null) warningText.gameObject.SetActive(false);//경고 메시지 숨기기 (토글 전, 혹시 남아있을 수 있는 경고를 정리)

        //패널의 다음 상태를 결정하고 활성화/비활성화
        bool isPanelActive = !optionsPanel.activeSelf;//다음 상태 (true: 켬, false: 끔)
        optionsPanel.SetActive(isPanelActive);//최상위 OptionsPanel 켜기/ 끄기

        if (isPanelActive)//게임 시간 제어.옵션창이 켜지면 게임 멈춤
        {
            Time.timeScale = 0f;//게임 시간 정지, Time.timeScale로 게임 시간 정지/재개

            //옵션창을 켤 때 (isPanelActive == true)
            //1. 사운드 조절 창은 무조건 끄고 (숨기고)
            soundControlPanel.SetActive(false);
            //2. 메인 버튼 컨테이너만 무조건 킨다. (O키를 누르면 항상 이 화면으로 시작)
            Options.SetActive(true);
        }
        else if (CountdownManager.isCountdownFinished) Time.timeScale = 1f;//옵션창 닫고, 카운트다운 끝났으면 시간 재개


        //옵션 창 닫을 때 사운드 창도 함께 닫는 로직은 그대로 유지
        if (!isPanelActive && soundControlPanel.activeSelf) CloseSoundControls();
    }
    IEnumerator HideWarningText()//warningText UI 경고 메시지를 숨기는 코루틴
    {
        yield return new WaitForSecondsRealtime(1f);//warningText UI 메세지가 띄워지면 n초 동안 기다려

        if (warningText != null) warningText.gameObject.SetActive(false);//warningText UI 메시지를 비활성화
    }

    public void SetBGMVolume(float volume)//볼륨 조절, BGM 슬라이더 OnValueChanged 이벤트에 연결(다이나믹 플롯의 함수로 연결)
    {   //-80dB가 **'무음(Silence)'**을 나타내는 값
        //슬라이더 값이 0에 가까울 때(Min Value) 완전히 무음(-80dB)으로 설정
        if (volume <= 0.0001f) mainMixer.SetFloat("BGMVolume", -80f);
        else
        {
            float db = 20f * Mathf.Log10(volume);//정상 범위(0.0001 ~ 1.0)에서는 로그 공식 적용
            mainMixer.SetFloat("BGMVolume", db);
        }
        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();//디스크에 저장
    }
    public void SetSFXVolume(float volume)//SFX 슬라이더 OnValueChanged 이벤트에 연결(다이나믹 플롯의 함수로 연결)
    {
        if (volume <= 0.0001f) mainMixer.SetFloat("SFXVolume", -80f);
        else
        {
            float db = 20f * Mathf.Log10(volume);
            mainMixer.SetFloat("SFXVolume", db);
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();//디스크에 저장
    }
    public void OpenSoundControls()//SoundButton 클릭 시 호출
    {
        //1. 메인 버튼 컨테이너를 숨기고
        Options.SetActive(false);
        //2. 사운드 조절 패널을 보여줘
        soundControlPanel.SetActive(true);
    }
    public void CloseSoundControls()//SoundControlPanel의 BackButton 클릭 시 호출
    {
        //1. 사운드 조절 패널을 숨기고
        soundControlPanel.SetActive(false);
        //2. 메인 버튼 컨테이너를 다시 보여줘
        Options.SetActive(true);
    }

    public void RestartGame()//게임 재시작 함수 (재시작 버튼에 연결)
    {
        //게임을 재시작할 때는 시간을 다시 정상으로 돌려놓야해.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //LoadScene() 특정 씬을 불러오는 함수야. 
        //GetActiveScene() 현재 활성화되어 있는(플레이 중인) 씬을 가져오는 함수야. 이 함수는 현재 씬에 대한 정보를 반환해.
        //.name 은 GetActiveScene()으로 가져온 씬 정보 중에서 그 씬의 이름을 문자열(string) 형태로 꺼내주는 역할을 해.
        //따라서 이 코드는 "현재 씬의 이름을 가져와서, 그 이름으로 씬을 다시 로드해" 라는 뜻이야.
    }

    public void ExitGame()//현 게임을 끄고 메인화면으로 가는 함수
    {
        Time.timeScale = 1f;//게임 재시작과 마찬가지로, 메인화면으로 돌아갈 때도 시간을 정상으로 돌려놔.
        SceneManager.LoadScene("MainMenuScene");//MainMenuScene 이름의 씬을 로드해서 다시 메인화면 씬으로 돌아가게 해
        Debug.Log("메인화면으로 이동");
    }

    public void  QuitGame()//게임을 완전히 끄는 함수
    {
        Time.timeScale = 1f;
        Debug.Log("게임 종료. 재밋었어?");

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}


