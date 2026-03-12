using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;//Tilemap을 사용하려면 이 using 문이 필요

public class ItemChestSpawner : MonoBehaviour
{
    //---------옵저버 [아이템 스폰 방송 채널 추가]----
    public static event System.Action<string> OnItemSpawned;
    //------------------------------------------------

    public static ItemChestSpawner Instance { get; private set; }//싱글톤 선언

    [Header("아이템 상자 프리팹 설정")]
    public GameObject ItemChestPrefab;
    public float ItemChestSpawnTime = 10f;//아이템 상자가 스폰되는 주기 (초)

    [Header("스폰 범위 설정(코드 내에서 자동 연결)")]
    public BoxCollider2D ItemChestSpawnCollider;

    [Header("자동 할당 변수 (인스펙터 확인용)")]
    [SerializeField] private Tilemap TargetTilemap;
    [SerializeField] private LayerMask SpawnableLayer;//아이템 스폰할때 "Ground"라는 레이어가 설정된 곳만 안전하게 골라내기 위한 역할
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

        //ItemChestSpawner_OBJ 오브젝트 자식 중에서 아이템 스폰 구역 콜라이더 찾기
        if (ItemChestSpawnCollider == null)
        {
            ItemChestSpawnCollider = GetComponentInChildren<BoxCollider2D>();
            if (ItemChestSpawnCollider != null)
                Debug.Log($"{ItemChestSpawnCollider.gameObject.name}을 아이템 스폰 범위로 연결했어!");
        }

        //레이어마스크 자동 설정
        //인스펙터에서 Nothing(0)으로 되어 있다면 "Ground" 레이어를 자동으로 가져옴
        if (SpawnableLayer == 0) SpawnableLayer = LayerMask.GetMask("Ground");

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
        if (spawnPosition == Vector3.zero)//만약 결과가 (0,0,0)이라면? -> "실패했구나!" 하고 그냥 종료(return)
        {
            Debug.LogWarning("아이템 상자 스폰 실패: 자리가 없어!");
            return;
        }
        Instantiate(ItemChestPrefab, spawnPosition, Quaternion.identity);//(0,0,0)이 아닐 때만 진짜로 상자를 만듦

        //SoundManager 싱글톤을 활용해 오디오 소스 없이 바로 재생
        if (spawnSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(spawnSound);

        //직접 호출 대신 방송을 쏴 (TextAlimManager가 이걸 받고 화면에 출력하지)
        OnItemSpawned?.Invoke("<color=yellow>아이템 상자 발견!</color>");
    }

    Vector3 GetValidSpawnPosition()//유효한 스폰 위치를 찾는 함수 (EnemySpawn 스크립트와 동일)
    {
        //이 코드 덕분에, 설령 0,0,0이 맵의 중앙이 아니라도 그 위치에 상자가 생성되는 일은 없어. 그냥 "이번 스폰은 건너뛰기"가 될 뿐이야
        if (ItemChestSpawnCollider == null) return Vector3.zero;

        int maxAttempts = 100;//무한 루프 방지를 위한 최대 시도 횟수 (100번 안에 못 찾으면 이번 스폰은 포기)
        Bounds bounds = ItemChestSpawnCollider.bounds;//자식 콜라이더의 범위를 가져옴

        for (int i = 0; i < maxAttempts; i++)
        {
            //콜라이더 범위 내 랜덤 좌표
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector3 randomPos = new Vector3(randomX, randomY, 0);

            //해당 지점이 콜라이더 안쪽인지 확인
            if (ItemChestSpawnCollider.OverlapPoint(randomPos))
            {
                //주변에 장애물(Ground, Wall 등)이 없는지 체크
                Collider2D hit = Physics2D.OverlapCircle(randomPos, 0.4f, SpawnableLayer);

                if (hit == null) return randomPos;//모든 검사를 통과하면 이 좌표를 스폰 위치로 확정
            }
        }
        return Vector3.zero;//100번 시도해도 적절한 위치를 못 찾았을 때 안전하게 0 반환
    }
}

