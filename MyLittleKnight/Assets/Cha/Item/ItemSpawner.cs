using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;//타일맵을 사용하려면 필요

public class ItemSpawner : MonoBehaviour
{

    //인스펙터에 할당할 변수들
    public GameObject[] ItemPrefabs;//스폰할 아이템 프리팹 목록
    [Header("오브젝트, 레이어 연결")]//위에 itemPrefabs 변수는 []로 드롭다운 메뉴가 생겨서 헤더 안에 포함하지 않았어
    public Tilemap TargetTilemap;//아이템을 스폰할 타일맵
    public LayerMask SpawnableLayer;//아이템이 스폰될 수 있는 레이어 (바닥 등)
    [Header("아이템이 스폰되는 시간")]
    public float ItemSpawnTime = 10f;//아이템이 스폰되는 주기 (초)


    //내부에서 사용할 변수들
    private float spawnTimer;
    private int currentItemCount;
    private TextAlimManager textalimManager;//TextAlimManager 스크립트 참조


    void Start()
    {
        //!= null은 "null이 아닐 때" 즉, "무언가가 존재할 때"를 의미하고,
        //== null은 "null일 때" 즉, "아무것도 없거나 비어있을 때"를 의미해.
        spawnTimer = ItemSpawnTime;
        if (ItemPrefabs == null || ItemPrefabs.Length == 0)
            Debug.LogError("ItemSpawner: 아이템 프리팹이 할당되지 않았어!");

        if (TargetTilemap == null)
            Debug.LogError("ItemSpawner: 타일맵이 할당되지 않았어!");

        textalimManager = FindObjectOfType<TextAlimManager>();
        if (textalimManager == null)
            Debug.LogError("TextAlimManager 씬에서 찾을 수 없어!");
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnRandomItem();
            spawnTimer = ItemSpawnTime;
        }
    }

    void SpawnRandomItem()
    {
        if (ItemPrefabs == null || ItemPrefabs.Length == 0) return;

        Vector3 spawnPosition = GetValidSpawnPosition();//GetValidSpawnPosition함수 호출
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("아이템 스폰: 타일맵 내에서 유효한 스폰 위치를 찾을 수 없었어!");
            return;
        }

        int randomIndex = Random.Range(0, ItemPrefabs.Length);
        GameObject itemToSpawn = ItemPrefabs[randomIndex];

        Instantiate(itemToSpawn, spawnPosition, Quaternion.identity);
        currentItemCount++;

        // TextAlimManager 스크립트로 알림 보내기, UI에 뜰 텍스트
        if (textalimManager != null)
            textalimManager.ShowNotification("<color=yellow>아이템 등장!</color>");
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

    public void ItemDestroyed()//아이템을 먹고 사라질때 호출될 함수
    {   //이건 item 스크립트의 Destroy(gameObject);랑 달라, currentItemCount변수의 값을 1 줄이는거야.
        currentItemCount--;
    }
}
