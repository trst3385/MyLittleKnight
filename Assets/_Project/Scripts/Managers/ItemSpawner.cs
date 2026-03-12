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

    [Header("스폰 범위 설정(코드 내에서 자동 연결)")]
    public BoxCollider2D ItemSpawnCollider;

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

        //스폰 위치 ItemSpawnCollider 자식 콜라이더 자동 연결
        if (ItemSpawnCollider == null)
        {
            ItemSpawnCollider = GetComponentInChildren<BoxCollider2D>();
            if (ItemSpawnCollider != null)
                Debug.Log($"{ItemSpawnCollider.gameObject.name}을 일반 아이템 스폰 범위로 연결했어!");
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
        if (spawnPosition == Vector3.zero) return;//생성할 자리가 없으면 스폰 중단, 여기서 "0,0,0"이면 함수를 바로 종료해버림

        int randomIndex = Random.Range(0, ItemPrefabs.Length);
        GameObject itemToSpawn = ItemPrefabs[randomIndex];

        Instantiate(itemToSpawn, spawnPosition, Quaternion.identity);
        currentItemCount++;
    }

    //maxAttempts = 100으로 횟수를 제한했기 때문에, 100번의 for문이 다 돌아갈 동안 return randomPos;를 한 번도 만나지 못하면,
    //결국 가장 밑바닥에 있는 return Vector3.zero;까지 내려오게 되는 거야.
    Vector3 GetValidSpawnPosition()//유효한 스폰 위치를 찾는 함수(아이템상자, 몬스터 스폰 스크립트와 동일)
    {
        if (ItemSpawnCollider == null) return Vector3.zero;

        int maxAttempts = 100;//무한 루프 방지를 위한 최대 시도 횟수 (100번 안에 못 찾으면 이번 스폰은 포기)
        Bounds bounds = ItemSpawnCollider.bounds;//자식 콜라이더의 범위를 사용

        for (int i = 0; i < maxAttempts; i++)
        {   //콜라이더 범위 내 랜덤 좌표
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector3 randomPos = new Vector3(randomX, randomY, 0);

            ////해당 지점이 콜라이더 안쪽인지 확인
            if (ItemSpawnCollider.OverlapPoint(randomPos))
            {
                //주변에 장애물(Ground, Wall 등)이 없는지 체크
                Collider2D hit = Physics2D.OverlapCircle(randomPos, 0.3f, SpawnableLayer);

                if (hit == null) return randomPos;//모든 검사를 통과하면 이 좌표를 스폰 위치로 확정
            }
        }
        return Vector3.zero;//100번 시도해도 적절한 위치를 못 찾았을 때 안전하게 0 반환
    }

    //아이템을 먹고 사라질때 호출될 함수
    //이건 item 스크립트의 Destroy(gameObject);랑 달라, currentItemCount변수의 값을 1 줄이는거야.
    public void ItemDestroyed() => currentItemCount--;
}
