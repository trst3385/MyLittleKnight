using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;//Tilemap을 사용하려면 이 using 문이 필요

public class ItemChestSpawner : MonoBehaviour
{
    [Header("연결할 오브젝트, 레이어 변수")]
    // ==인스펙터에 할당할 변수들==
    public GameObject ItemChestPrefab;//인스펙터에서 스폰할 아이템 상자 프리팹을 할당
    public Tilemap TargetTilemap;//아이템 상자를 스폰할 타일맵을 할당
    public LayerMask SpawnableLayer;//아이템 상자가 스폰될 수 있는 영역 (바닥 등)의 레이어 마스크

    [Header("아이템 상자의 스폰 시간")]
    public float ItemChestSpawnTime = 30f;//아이템 상자가 스폰되는 주기 (초)

    [Header("사운드 변수")]
    //생성 시 들릴 사운드 변수
    public AudioSource AudioSource;
    public AudioClip SpawnSound;

    // == 내부에서 사용할 변수들 ==
    private float spawnTimer;//스폰 주기 계산용 타이머
    private TextAlimManager textalimManager;//TextAlimManager 스크립트를 참조할 변수
    void Start()
    {
        spawnTimer = ItemChestSpawnTime;//실행 후 바로 스폰 되도록 초기화

        //!= null은 "null이 아닐 때" 즉, "무언가가 존재할 때"를 의미하고,
        //== null은 "null일 때" 즉, "아무것도 없거나 비어있을 때"를 의미해.

        //ItemBoxPrefab이 할당되어 있는지 확인 (에러 방지)
        if (ItemChestPrefab == null)
            Debug.LogError("ItemBoxSpawner: ItemBoxPrefab이 할당되지 않았어! 인스펙터를 확인해!");

        //시작 시 TextAlimManager 스크립트를 찾아서 할당
        textalimManager = FindObjectOfType<TextAlimManager>();
        if (textalimManager == null)
            Debug.LogWarning("ItemChestSpawner: TextAlimManager스크립트를 찾을 수 없어!. 상자 스폰 알림이 표시되지 않아!.");
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
        //아이템 상자 프리팹이 할당되어 있는지 다시 한번 확인
        if (ItemChestPrefab == null)
        {
            Debug.LogError("아이템 상자 프리팹이 할당되지 않았어! 인스펙터를 확인해!.");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();//유효한 스폰 위치 찾기
        if(spawnPosition == Vector3.zero)//유효한 위치를 찾지 못했으면
        {
            Debug.LogWarning("아이템 상자 스폰: 타일맵 내에서 유효한 스폰 위치를 찾을 수 없었어!.");
            return;
        }

        GameObject newItemChest = Instantiate(ItemChestPrefab, spawnPosition, Quaternion.identity);

        if (AudioSource != null && SpawnSound != null)//생성시 들릴 사운드
            AudioSource.PlayOneShot(SpawnSound);//한번만 사운드가 들리게 말 그대로 OneShot

        if (textalimManager != null)//TextAlimManager 스크립트에 텍스트 알림 표시
            textalimManager.ShowNotification("<color=yellow>아이템 상자 발견!</color>");
    }

    Vector3 GetValidSpawnPosition()//유효한 스폰 위치를 찾는 공통 함수 (EnemySpawn 스크립트와 동일)
    {
        int maxAttempts = 100;//유효한 스폰 위치를 찾기 위한 최대 시도 횟수
        for(int i = 0; i < maxAttempts; i++)
        {
            if(TargetTilemap == null)
            {
                Debug.LogError("TargetTilemap이 ItemBoxSpawner에 할당되지 않았어!");
                return Vector3.zero;//유효한 위치 못 찾으면 Vector3.zero 반환
            }

            BoundsInt bounds = TargetTilemap.cellBounds;//타일맵의 유효한 셀 범위 가져오기
            int randomX = Random.Range(bounds.xMin, bounds.xMax);
            int randomY = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int randomCell = new Vector3Int(randomX, randomY, 0);//랜덤 셀 위치

            if(TargetTilemap.HasTile(randomCell))//선택된 셀에 타일이 있는지 확인
            {
                Vector3 cellCenterTile = TargetTilemap.GetCellCenterWorld(randomCell);//셀 위치를 월드 좌표로 변환

                Collider2D[] colliders = Physics2D.OverlapCircleAll(cellCenterTile, 0.5f, SpawnableLayer);

                if (colliders.Length == 0) return cellCenterTile;//주변에 콜라이더가 없다면 유효한 위치
            }
        }
        Debug.LogWarning("아이템 상자 스폰: 타일맵 내에서 유효한 스폰 위치를 100번 시도했으나 찾을 수 없었어!.");
        return Vector3.zero;//100번 시도해도 못 찾으면 Vector3.zero 반환
    }
}

