using TMPro;
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

    [Header("등장 사운드")]
    [SerializeField] private AudioClip itemSpawnSFX;//인스펙터에 연결할 등장 사운드


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
            SpawnTimeFreezeItem();
            spawnTimer = spawnInterval;//타이머 리셋
        }
    }

    void SpawnTimeFreezeItem()
    {
        if (timeFreezeItemPrefab == null) return;//시간정지 아이템이 없으면 함수종료

        //최대 개수 체크 (1개만 존재 가능)
        if (currentItemCount >= maxSimultaneousItems)
        {
            Debug.Log("TimeFreezeItemSpawner: 이미 최대 개수가 존재하여 스폰되지 않아!");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("TimeFreezeItemSpawner: 유효한 스폰 위치를 찾을 수 없어!");
            return;
        }

        Instantiate(timeFreezeItemPrefab, spawnPosition, Quaternion.identity);//아이템 생성
        currentItemCount++;//카운트 증가

        //알림 메시지(12/12. 지금은 사용 안할거야. 등장시 들릴 사운드만 추가만 할거야.
        //if (textalimManager != null) textalimManager.ShowNotification("<color=yellow>Time Freeze 등장!</color>");
        
        if (itemSpawnSFX != null && SoundManager.Instance != null) SoundManager.Instance.PlaySFX(itemSpawnSFX);//아이템 등장시 사운드
        //SoundManager 스크립트를 이용

    }

    Vector3 GetValidSpawnPosition()//타일맵 특정 위치(중앙)에 생성
    {                              //몬스터, 아이템, 가시의 스폰 스크립트의 로직을 가져왔지만 이 아이템은 오직 중앙에만 오도록 수정

        //1. 타일맵의 경계(Bounds)를 가져옴
        BoundsInt bounds = targetTilemap.cellBounds;

        //2. 중앙 셀 좌표 계산 (타일맵 좌표계를 기준으로 정중앙 셀을 찾음)
        //예를 들어 10x10 타일맵이라면 중심은 (5, 5)가 되도록 계산
        int centerX = bounds.xMin + bounds.size.x / 2;
        int centerY = bounds.yMin + bounds.size.y / 2;
        Vector3Int centerCell = new Vector3Int(centerX, centerY, 0);

        //3. 중앙에 타일이 존재하는지 확인 (타일이 없으면 스폰 불가)
        if (targetTilemap.HasTile(centerCell))
        {
            //4. 중앙 셀의 월드 좌표를 가져옴
            Vector3 centerWorldPosition = targetTilemap.GetCellCenterWorld(centerCell);

            //5. 주변에 다른 오브젝트(콜라이더)가 없는지 확인 (안전성 확보)
            Collider2D[] colliders = Physics2D.OverlapCircleAll(centerWorldPosition, 0.5f, spawnableLayer);

            //다른 콜라이더가 없으면 유효한 중앙 위치를 반환
            if (colliders.Length == 0) return centerWorldPosition;

            Debug.LogWarning("TimeFreezeItemSpawner: 타일맵 중앙에 이미 다른 오브젝트가 있어!");
        }
        return Vector3.zero;//중앙에 타일이 없거나, 다른 오브젝트가 있으면 스폰 실패
    }

    //ItemFreezeItem 스크립트에서 호출될 함수
    //아이템이 획득,파괴될 때 이 카운트를 줄여서 다음 스폰이 가능하게 해
    public void ItemDestroyed()
    {
        currentItemCount = Mathf.Max(0, currentItemCount - 1);
        Debug.Log($"시간 정지 아이템이 파괴되었어. 남은 아이템 수: {currentItemCount}");
    }
}
