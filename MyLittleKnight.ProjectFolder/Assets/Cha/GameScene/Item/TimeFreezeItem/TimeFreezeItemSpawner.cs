using UnityEngine;
using UnityEngine.Tilemaps;

public class TimeFreezeItemSpawner : MonoBehaviour
{
    [Header("프리팹 및 설정")]
    [SerializeField] private GameObject timeFreezeItemPrefab;//시간 정지 아이템 프리팹
    [SerializeField] private float spawnInterval = 20f;//20초 마다 아이템 생성
    [SerializeField] private int maxSimultaneousItems = 1;//맵에 존재 가능한 최대 개수 (1개로 고정)

    [Header("오브젝트, 레이어 연결")]
    [SerializeField] private Tilemap targetTilemap;
    [SerializeField] private LayerMask spawnableLayer;

    //내부에서 사용할 변수들
    private float spawnTimer;
    private int currentItemCount = 0;//현재 맵에 존재하는 시간 정지 아이템 수
    private TextAlimManager textalimManager;

    void Start()//내부 컴포넌트는 Awake에, 외부 스크립트는 Start에
    {
        spawnTimer = spawnInterval;

        //필수 컴포넌트 체크
        if (timeFreezeItemPrefab == null) Debug.LogError("TimeFreezeItemSpawner: TimeFreezeItemPrefab이 할당되지 않았어!");
        if (targetTilemap == null) Debug.LogError("TimeFreezeItemSpawner: TargetTilemap이 할당되지 않았어!");

        textalimManager = FindFirstObjectByType<TextAlimManager>();
        if (textalimManager == null) Debug.LogError("TextAlimManager 씬에서 찾을 수 없어!");
    }

    void Update()
    {
        //시간이 멈췄을 때는 스폰 타이머도 멈춤
        if (TimeFreeze.Instance != null && TimeFreeze.Instance.IsTimeFrozen) return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            TrySpawnTimeFreezeItem();
            spawnTimer = spawnInterval;//타이머 리셋
        }
    }

    void TrySpawnTimeFreezeItem()
    {
        if (timeFreezeItemPrefab == null) return;//시간정지 아이템이 없으면 함수종료

        //최대 개수 체크 (1개만 존재 가능)
        if (currentItemCount >= maxSimultaneousItems)
        {
            //Debug.Log("TimeFreezeItemSpawner: 이미 최대 개수가 존재하여 스폰하지 않아!");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("TimeFreezeItemSpawner: 유효한 스폰 위치를 찾을 수 없었어!");
            return;
        }

        Instantiate(timeFreezeItemPrefab, spawnPosition, Quaternion.identity);//아이템 생성
        currentItemCount++;//카운트 증가

        //알림 메시지
        if (textalimManager != null) textalimManager.ShowNotification("<color=yellow>시간 증폭기 등장!</color>");
    }

    Vector3 GetValidSpawnPosition()//타일맵 내 랜덤 위치에 생성
    {                              //몬스터, 아이템, 가시의 스폰 스크립트와 같은 로직이야
        int maxAttempts = 100;

        for (int i = 0; i < maxAttempts; i++)
        {
            BoundsInt bounds = targetTilemap.cellBounds;
            int randomX = Random.Range(bounds.xMin, bounds.xMax);
            int randomY = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int randomCell = new Vector3Int(randomX, randomY, 0);

            if (targetTilemap.HasTile(randomCell))
            {
                Vector3 cellCenterTile = targetTilemap.GetCellCenterWorld(randomCell);

                //주변에 다른 콜라이더(오브젝트)가 없는지 확인
                Collider2D[] colliders = Physics2D.OverlapCircleAll(cellCenterTile, 0.5f, spawnableLayer);

                if (colliders.Length == 0) return cellCenterTile;
            }
        }
        return Vector3.zero;
    }

    //ItemFreezeItem 스크립트에서 호출될 함수
    //아이템이 획득,파괴될 때 이 카운트를 줄여서 다음 스폰이 가능하게 해
    public void ItemDestroyed()
    {
        currentItemCount = Mathf.Max(0, currentItemCount - 1);
        Debug.Log($"시간 정지 아이템이 파괴되었어. 남은 아이템 수: {currentItemCount}");
    }
}
