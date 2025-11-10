using UnityEngine;
using UnityEngine.Tilemaps;//타일맵을 쓰기 위해 필요!

public class SpikeSpawn : MonoBehaviour
{
    [Header("스폰 설정")]
    public GameObject SpikePrefab;//가시 프리팹 (인스펙터 연결)
    public float SpawnInterval = 5f;//가시가 스폰되는 시간(n초마다 등장)
    public float SpikeDuration = 3f;//가시가 사라지는 시간

    [Header("타일맵 참조")]
    public Tilemap TargetTilemap;
    public LayerMask SpawnableLayer;

    private float spawnTimer;

    void Start()
    {
        if (TargetTilemap == null)//EnemySpawn 스크립트와 동일: 게임 시작 시 Tilemap 참조 필수 확인
        {
            Debug.LogError("SpikeSpawn: TargetTilemap이 할당되지 않았어!");
            enabled = false;
        }
        spawnTimer = SpawnInterval;
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnSpike();
            spawnTimer = SpawnInterval;
        }
    }

    void SpawnSpike()
    {
        //EnemySpawn 스크립트의  GetValidSpawnPosition() 로직을 복사/붙여넣기 하거나
        //EnemySpawn 스크립트를 싱글톤으로 만들었다면 거기서 함수를 직접 호출해야 해
        Vector3 spawnPosition = GetValidSpawnPosition();

        if (spawnPosition != Vector3.zero)
        {
            //가시 프리팹 생성
            GameObject newSpike = Instantiate(SpikePrefab, spawnPosition, Quaternion.identity);

            //핵심: 생성된 가시 오브젝트에게 '언제 사라질지' 정보를 전달
            Spike spikeScript = newSpike.GetComponent<Spike>();
            if (spikeScript != null)
            {
                spikeScript.InitializeSpike(SpikeDuration);
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
