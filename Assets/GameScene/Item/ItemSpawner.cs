using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;//타일맵을 사용하려면 필요

public class ItemSpawner : MonoBehaviour
{
    public static ItemSpawner Instance { get; private set; }//싱글톤 선언, 어디서든 ItemSpawner.Instance로 접근

    [Header("스폰할 아이템 목록 (인스펙터 할당)")]
    public GameObject[] ItemPrefabs;

    [Header("자동 연결될 참조들")]
    [SerializeField] private Tilemap TargetTilemap;
    [SerializeField] private LayerMask SpawnableLayer;

    [Header("아이템이 스폰되는 시간")]
    public float ItemSpawnTime = 5f;


    //내부에서 사용할 변수들
    private float spawnTimer;
    private int currentItemCount;
    private TextAlimManager textalimManager;//TextAlimManager 스크립트 참조

    void Awake()
    {
        //싱글톤 초기화
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        InitializeReferences();
    }

    private void InitializeReferences()
    {
        //타일맵 자동 찾기
        if (TargetTilemap == null)
        {
            TargetTilemap = FindFirstObjectByType<Tilemap>();
            if (TargetTilemap == null) Debug.LogError("ItemSpawner: 타일맵을 찾을 수 없어!");
        }

        //알림 매니저 자동 연결
        if (textalimManager == null) textalimManager = FindFirstObjectByType<TextAlimManager>();

        //레이어 기본값 설정 (Ground 레이어 가정)
        if (SpawnableLayer == 0) SpawnableLayer = LayerMask.GetMask("Ground");
    }

    void Start()
    {
        spawnTimer = ItemSpawnTime;//게임 시작 후 ItemSpawnTime시간에 아이템이 등장

        if (ItemPrefabs == null || ItemPrefabs.Length == 0)
            Debug.LogError("ItemSpawner: 아이템 프리팹이 할당되지 않았어!");
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)//랜덤한 아이템이 spawnTimer 시간에 등장
        {
            SpawnRandomItem();
            spawnTimer = ItemSpawnTime;
        }
    }

    void SpawnRandomItem()
    {
        if (ItemPrefabs == null || ItemPrefabs.Length == 0) return;

        Vector3 spawnPosition = GetValidSpawnPosition();//GetValidSpawnPosition함수 호출
        if (spawnPosition == Vector3.zero) return;//생성할 자리가 없으면 스폰 중단

        int randomIndex = Random.Range(0, ItemPrefabs.Length);
        GameObject itemToSpawn = ItemPrefabs[randomIndex];

        Instantiate(itemToSpawn, spawnPosition, Quaternion.identity);
        currentItemCount++;
    }

    Vector3 GetValidSpawnPosition()//아이템의 생성 위치를 정하는 함수, 랜덤으로 세 위치중에서 선택
    {
        int maxAttempts = 100;

        for (int i = 0; i < maxAttempts; i++)
        {
            BoundsInt bounds = TargetTilemap.cellBounds;
            int randomX = Random.Range(bounds.xMin, bounds.xMax);
            int randomY = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int randomCell = new Vector3Int(randomX, randomY, 0);

            if (TargetTilemap.HasTile(randomCell))
            {
                Vector3 cellCenterTile = TargetTilemap.GetCellCenterWorld(randomCell);

                //주변에 다른 콜라이더(오브젝트)가 없는지 확인
                Collider2D[] colliders = Physics2D.OverlapCircleAll(cellCenterTile, 0.5f, SpawnableLayer);

                //다른 콜라이더가 없으면 유효한 위치
                if (colliders.Length == 0) return cellCenterTile;
            }
        }
        return Vector3.zero;
    }

    //아이템을 먹고 사라질때 호출될 함수
    //이건 item 스크립트의 Destroy(gameObject);랑 달라, currentItemCount변수의 값을 1 줄이는거야.
    public void ItemDestroyed() => currentItemCount--;
}
