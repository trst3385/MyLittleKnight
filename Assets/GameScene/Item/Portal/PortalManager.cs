using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }//어디서든 접근 가능한 싱글톤

    [Header("포탈 활성화 조건")]
    public float targetTime = 60f;
    public GameObject portalObject;//씬에 비활성화된 포탈을 연결

    private bool isActivated = false;
    private GameTimerUI gameTimer;//매 프레임 Find를 하지 않기 위해 미리 캐싱해둘 변수

    void Awake()
    {
        if (Instance != null && Instance != this)//싱글톤 설정
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializeReferences();//시작할 때 미리 타이머를 찾아둠 (Update에서 Find를 안 쓰기 위함)

        if (portalObject != null) portalObject.SetActive(false);//게임 시작 시 포탈은 비활성화(조건이 맞아야지만 활성화)
    }

    private void InitializeReferences()
    {
        if (gameTimer == null)//타이머 찾기
            gameTimer = Object.FindAnyObjectByType<GameTimerUI>();

        // 포탈 오브젝트가 비어있다면 태그로 찾기
        if (portalObject == null) portalObject = GameObject.FindWithTag("Portal");
        // 유니티 인스펙터 상단에서 Tag를 'Portal'로 설정해두면 됨!
    }


    void Update()
    {
        if (isActivated) return;//이미 활성화됐다면 더 이상 계산할 필요 없음

        //1. 시간 조건 체크 (캐싱해둔 gameTimer 사용)
        bool timeReached = false;
        if (gameTimer != null) timeReached = gameTimer.GetElapsedTime() >= targetTime;

        //2. 몬스터 미션 조건 체크 (이미 싱글톤인 MonsterCountManager 사용)
        bool missionOk = (MonsterCountManager.Instance != null) && MonsterCountManager.Instance.IsMissionComplete();

        //3. 모든 조건 충족 시 포탈 활성화
        if (timeReached && missionOk) ActivatePortal();
    }
    private void ActivatePortal()
    {
        isActivated = true;
        if (portalObject != null)
        {
            portalObject.SetActive(true);
            Debug.Log($"★★★ 조건 충족! 포탈 활성화 (목표시간: {targetTime}s) ★★★");
        }
    }
}
