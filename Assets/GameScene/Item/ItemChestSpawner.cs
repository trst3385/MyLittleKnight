using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;//Tilemap을 사용하려면 이 using 문이 필요

public class ItemChestSpawner : MonoBehaviour
{
    public static ItemChestSpawner Instance { get; private set; }//싱글톤 선언

    [Header("아이템 상자 프리팹 설정")]
    public GameObject ItemChestPrefab;
    public float ItemChestSpawnTime = 10f;//아이템 상자가 스폰되는 주기 (초)


    [Header("자동 할당 변수 (인스펙터 확인용)")]
    [SerializeField] private Tilemap TargetTilemap;
    [SerializeField] private LayerMask SpawnableLayer;
    [SerializeField] private AudioClip spawnSound;


    // == 내부에서 사용할 변수들 ==
    private float spawnTimer;//스폰 주기 계산용 타이머
    private TextAlimManager textalimManager;//TextAlimManager 스크립트를 참조할 변수

    void Awake()
    {
        if (Instance == null) Instance = this;//싱글톤 설정
        else { Destroy(gameObject); return; }

        InitializeReferences();//참조 자동화
    }

    private void InitializeReferences()
    {
        //타일맵 자동 찾기
        if (TargetTilemap == null)
        {
            TargetTilemap = FindFirstObjectByType<Tilemap>();
            if (TargetTilemap == null) Debug.LogError("ItemChestSpawner: 타일맵을 찾을 수 없어!");
        }

        //레이어마스크 자동 설정
        //인스펙터에서 Nothing(0)으로 되어 있다면 "Ground" 레이어를 자동으로 가져옴
        if (SpawnableLayer == 0)
            SpawnableLayer = LayerMask.GetMask("Ground");

        textalimManager = FindFirstObjectByType<TextAlimManager>();//알림 매니저 싱글톤/찾기
    }

    void Start()
    {
        spawnTimer = ItemChestSpawnTime;//실행 후 정해신 시간 후에 스폰 되도록 초기화

        //ItemBoxPrefab이 할당되어 있는지 확인 (에러 방지)
        if (ItemChestPrefab == null) Debug.LogError("ItemChestSpawner: ItemChestPrefab이 없어!");
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;//남은 시간 감소

        if (spawnTimer <= 0f)//타이머가 0이하가 되면 스폰
        {
            SpawnItemChest();//아이템 상자 생성 함수 호출
            spawnTimer = ItemChestSpawnTime;//다음 스폰을 위해 타이머 초기화
        }

    }

    void SpawnItemChest()
    {
        if (ItemChestPrefab == null) return;

        Vector3 spawnPosition = GetValidSpawnPosition();//유효한 스폰 위치 찾기
        if (spawnPosition == Vector3.zero) return;//유효한 자리를 차지 못하면 종료

        Instantiate(ItemChestPrefab, spawnPosition, Quaternion.identity);

        //SoundManager 싱글톤을 활용해 오디오 소스 없이 바로 재생
        if (spawnSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(spawnSound);


        if (TextAlimManager.Instance != null)//TextAlimManager에게 아이템 상자가 등장했다는 텍스트를 보내
            TextAlimManager.Instance.ShowNotification("<color=yellow>아이템 상자 발견!</color>");


    }

    Vector3 GetValidSpawnPosition()//유효한 스폰 위치를 찾는 공통 함수 (EnemySpawn 스크립트와 동일)
    {
        int maxAttempts = 100;//유효한 스폰 위치를 찾기 위한 최대 시도 횟수
        if (TargetTilemap == null) return Vector3.zero;//유효한 위치 못 찾으면 Vector3.zero 반환
        for (int i = 0; i < maxAttempts; i++)
        {
            BoundsInt bounds = TargetTilemap.cellBounds;//타일맵의 유효한 셀 범위 가져오기
            int randomX = Random.Range(bounds.xMin, bounds.xMax);
            int randomY = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int randomCell = new Vector3Int(randomX, randomY, 0);//랜덤 셀 위치
            
            if(TargetTilemap.HasTile(randomCell))//선택된 셀에 타일이 있는지 확인
            {
                Vector3 cellCenterWorld = TargetTilemap.GetCellCenterWorld(randomCell);//셀 위치를 월드 좌표로 변환

                //상자가 겹치지 않게 체크 (상자끼리 혹은 다른 장애물과)
                Collider2D[] colliders = Physics2D.OverlapCircleAll(cellCenterWorld, 0.5f, SpawnableLayer);

                if (colliders.Length == 0) return cellCenterWorld;//주변에 콜라이더가 없다면 유효한 위치
            }
        }
        return Vector3.zero;//100번 시도해도 못 찾으면 Vector3.zero 반환
    }
}

