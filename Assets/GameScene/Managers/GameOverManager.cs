using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameOverManager : MonoBehaviour
{//플레이어가 죽으면 실행될 UI 스크립트.

    public static GameOverManager Instance { get; private set; }


    [Header("자동 연결될 UI 요소들")]
    [SerializeField] private GameObject GameOverPanel;//게임 오버 패널
    [SerializeField] private TextMeshProUGUI FinalScoreText;//게임 오버 패널에 표시할 점수 텍스트
    [SerializeField] private TextMeshProUGUI NormalKillText;
    [SerializeField] private TextMeshProUGUI StrongKillText;
    [SerializeField] private TextMeshProUGUI EliteKillText;


    [Header("자동 연결될 외부 참조")]
    [SerializeField] private Player PlayerScript;//Player스크립트


    void Awake()
    {
        //싱글톤 초기화(굳이 public GameOverManager gameOverManager; 변수를 만들어서 연결하기 번거롭잖아?)
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        InitializeReferences();//수동 드래그 대신 자동 참조 실행
    }
    
    private void InitializeReferences()
    {
        if (GameOverPanel == null)//게임 오버 패널 찾기 (이름으로 찾기)
            GameOverPanel = GameObject.Find("GameOverPanel");

        if (GameOverPanel != null)//텍스트 UI들 찾기 (패널이 있다면 그 자식들 중에서 찾기)
        {
            //GetComponentInChildren를 쓰면 자식 오브젝트들 중에서 해당 컴포넌트를 가진 애를 찾아줘
            //다만, 이름이 겹칠 수 있으니 '오브젝트 이름'으로 찾는 게 더 정확할 수 있어!
            if (FinalScoreText == null) FinalScoreText = GameObject.Find("FinalScoreText")?.GetComponent<TextMeshProUGUI>();
            if (NormalKillText == null) NormalKillText = GameObject.Find("NormalKillText")?.GetComponent<TextMeshProUGUI>();
            if (StrongKillText == null) StrongKillText = GameObject.Find("StrongKillText")?.GetComponent<TextMeshProUGUI>();
            if (EliteKillText == null) EliteKillText = GameObject.Find("EliteKillText")?.GetComponent<TextMeshProUGUI>();
        }

        if (PlayerScript == null)//플레이어 찾기 (태그 활용)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) PlayerScript = playerObj.GetComponent<Player>();
        }
    }

    void Start()//Start 함수는 오브젝트가 활성화될 때 한 번 실행
    {
        //게임 시작 시 게임 오버 패널은 비활성화, 케릭터가 죽어야만 생성
        if (GameOverPanel != null) GameOverPanel.SetActive(false);
    }

    public void OnGameOver()//게임 오버 상태가 되면 호출될 함수
    {
        //게임 오버 패널 활성화 (화면에 보이게 함)
        if (GameOverPanel != null)
            GameOverPanel.SetActive(true);
        //SetActive는 유니티에서 게임 오브젝트를 활성화하거나 비활성화하는 기능이야
        //true를 넣으면 체크박스를 켜서 오브젝트를 보이게(활성화) 하고,false는 반대로 숨기게(비활성화) 해
        //괄호 안에 들어오는 값을 그대로 받아서 오브젝트의 상태를 true나 false로 바꿔주는 역할만 해.

        //Time.timeScale은 게임 내 시간의 흐름 속도를 조절하는 변수야.
        //Time.timeScale = 1f는 게임이 보통 속도로 돌아가고 있는 상태고,
        //Time.timeScale = 0f는 게임의 시간이 완전히 멈춘 상태를 의미해.

        //게임 시간 멈추기
        Time.timeScale = 0f;
        //최종 점수를 UI에 표시
        DisplayFinalScore();//이제 이 스크립트 안에 있는 DisplayFinalScore 호출
    } 

    void DisplayFinalScore()//게임 오버 시 최종 점수를 UI에 표시하는 함수
    {
        //1. 최종 점수는 모든 씬에서 공통으로 표시
        if (FinalScoreText != null && PlayerScript != null)
            FinalScoreText.text = "최종 점수: " + PlayerScript.CurrentScore.ToString();


        // 2. 처치 수 표시 (MonsterCountManager가 있다면 씬 상관없이 실행)
        if (MonsterCountManager.Instance != null)
        {
            if (NormalKillText != null)// Normal 몬스터 처치 수
            {
                NormalKillText.gameObject.SetActive(true);
                NormalKillText.text = "Normal: " + MonsterCountManager.Instance.normalKills;
            }
            if (StrongKillText != null)// Strong 몬스터 처치 수
            {
                StrongKillText.gameObject.SetActive(true);
                StrongKillText.text = "Strong: " + MonsterCountManager.Instance.strongKills;
            }
            if (EliteKillText != null)// Elite 몬스터 처치 수
            {
                EliteKillText.gameObject.SetActive(true);
                EliteKillText.text = "Elite: " + MonsterCountManager.Instance.eliteKills;
            }
        }
        else Debug.LogWarning("GameOverManager: MonsterCountManager.Instance를 찾을 수 없어!");
    }
    

    public void RestartGame()//게임 다시 시작 함수
    {
        Time.timeScale = 1f;//게임 시간 다시 시작

        //플레이어 스크립트의 점수 초기화 함수 호출
        if (PlayerScript != null) PlayerScript.ResetScore();

        //현재 씬의 이름을 가져와서 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void GoToMainMenu()//메인 화면 이동
    {
        //게임 오버 시 멈췄던 시간을 다시 시작, 이걸 안 하면 메인 화면에 갔을 때 게임이 멈춰있을 수 있어!
        Time.timeScale = 1f;

        //플레이어 점수 초기화 (필요하다면!)
        if (PlayerScript != null) PlayerScript.ResetScore();

        SceneManager.LoadScene("MainMenuScene");//메인 메뉴 씬 로드
    }
}
