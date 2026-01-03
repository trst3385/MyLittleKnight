using UnityEngine;
using UnityEngine.Tilemaps;//타일맵을 쓰기 위해 필요!

public class SpikeSpawn : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject SpikePrefab;//가시 프리팹 (인스펙터 연결)

    //!참고! 가시 생성 주기(Spawn Interval)와 수명(Duration) 모두 ObstacleDifficultyManager가 중앙 관리함.
    //SpikeSpawn 스크립트는 이제 타일맵 내에 가시를 스폰하는 역할만 담당해

    [Header("타일맵 참조")]
    public Tilemap TargetTilemap;
    public LayerMask SpawnableLayer;

    [Header("사운드 설정")]//가시 생성 사운드
    [SerializeField] private AudioSource audioSource;//오디오 소스 컴포넌트
    [SerializeField] private AudioClip spikeSpawnSound;//재생할 사운드 클립

    private float spawnTimer;//관리자 스크립트의 CurrentSpikeSpawnInterval 변수를 받음

    void Awake()//내부 컴포넌트는 Awake에
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) Debug.LogWarning("SpikeSpawn: AudioSource 컴포넌트가 없어! 사운드를 재생할 수 없어.");
    }

    void Start()//외부 스크립트는 Start에
    {
        if (TargetTilemap == null)//EnemySpawn 스크립트와 동일: 게임 시작 시 Tilemap 참조 필수 확인
        {
            Debug.LogError("SpikeSpawn: TargetTilemap이 할당되지 않았어! 인스펙터 확인해봐!");
            enabled = false;
            return;//TargetTilemap이 없으면 즉시 함수 종료
        }

        //초기 스폰 주기를 ObstacleDifficultyManager 스크립트에서 가져옴(currentSpikeSpawnInterval 변수)
        float initialInterval = 5f;//기본값
        if (ObstacleDifficultyManager.Instance != null)//ObstacleDifficultyManager 스크립트의 초기값으로 덮어쓰기
            initialInterval = ObstacleDifficultyManager.Instance.GetCurrentSpikeSpawnInterval();

        spawnTimer = initialInterval;
    }

    void Update()
    {
        //TimeFreeze로 의해 시간이 멈췄다면, 난이도 타이머 증가 로직을 건너뛰고 바로 함수 종료
        if (TimeFreeze.Instance != null && TimeFreeze.Instance.IsTimeFrozen) return;


        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnSpike();

            //다음 스폰 주기는 ObstacleDifficultyManager 스크립트에서 최신값을 받아옴
            if (ObstacleDifficultyManager.Instance != null)
                spawnTimer = ObstacleDifficultyManager.Instance.GetCurrentSpikeSpawnInterval();
            else spawnTimer = 5f;//난이도 관리자가 없을 경우를 대비해 기본값 사용  
        }
    }

    void SpawnSpike()
    {
        //EnemySpawn 스크립트의  GetValidSpawnPosition() 로직을 복사/붙여넣기 하거나
        //EnemySpawn 스크립트를 싱글톤으로 만들었다면 거기서 함수를 직접 호출해야 해
        Vector3 spawnPosition = GetValidSpawnPosition();

        if (spawnPosition != Vector3.zero)
        {   
            //PlayOneShot을 사용하여 여러 가시가 빠르게 생성되어도 소리가 겹치지 않고 재생
            if (audioSource != null && spikeSpawnSound != null) audioSource.PlayOneShot(spikeSpawnSound);

            //가시 프리팹 생성
            GameObject newSpike = Instantiate(SpikePrefab, spawnPosition, Quaternion.identity);

            //핵심: 생성된 가시 오브젝트에게 '언제 사라질지' 정보를 전달
            Spike spikeScript = newSpike.GetComponent<Spike>();
            if (spikeScript != null)
            {
                float duration = 5f;//가시가 사라지는 시간 기본값 
                if (ObstacleDifficultyManager.Instance != null)
                {
                    duration = ObstacleDifficultyManager.Instance.GetCurrentSpikeDuration();//난이도 매니저의 현재 수명 사용
                }
                spikeScript.InitializeSpike(duration);//가시 스크립트에게 수명 전달(시간 후 가시 삭제)
            }
        }
        else Debug.LogWarning("SpikeManager: 유효한 스폰 위치를 찾을 수 없었어!");
    }

    Vector3 GetValidSpawnPosition()//유효한 스폰 위치를 찾는 공통 함수,EnemySpawn 스크립트의 함수와 같아.
    {
        int maxAttempts = 100;//최대 100번
        for (int a = 0; a < maxAttempts; a++)
        {
            if (TargetTilemap == null)
            {
                Debug.LogError("TargetTilemap이 할당되지 않았어!");
                return Vector3.zero;//유효한 위치 못 찾으면 Vector3.zero 반환
            }

            BoundsInt bounds = TargetTilemap.cellBounds;
            int randomX = Random.Range(bounds.xMin, bounds.xMax);
            int randomY = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int randomCell = new Vector3Int(randomX, randomY, 0);

            if (TargetTilemap.HasTile(randomCell))
            {
                Vector3 cellCenterWorld = TargetTilemap.GetCellCenterWorld(randomCell);
                Collider2D[] colliders = Physics2D.OverlapCircleAll(cellCenterWorld, 0.5f, SpawnableLayer);
                if (colliders.Length == 0)
                    return cellCenterWorld;//유효한 위치 찾으면 반환
            }
        }
        return Vector3.zero;//100번 시도해도 못 찾으면 Vector3.zero 반환,void로 된 클래스가 아니니 return사용
    }
}
