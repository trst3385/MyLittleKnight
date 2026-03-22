using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;//Tilemap 관련 기능을 사용하기 위해

public class EnemySpawn : MonoBehaviour//public 필드는 대문자로 시작하는 것이 C#의 표준 코딩 컨벤션이야.
{   //이렇게 통일하면 코드가 훨씬 깔끔하고 다른 개발자들이 봤을 때도 이해하기 쉬워져.

    public static EnemySpawn Instance { get; private set; }//싱글톤 선언

    [Header("스크립트, 오브젝트 연결(코드에서 자동으로 연결된 상태,배열만 제외)")]
    public GameObject[] EnemyPrefabs;//인스펙터에서 스폰할 몬스터 프리팹을 할당 (0번 인덱스에 Normal 몬스터)
    public Tilemap TargetTilemap;//몬스터를 스폰할 타일맵을 할당 (플레이 가능한 영역)
    public LayerMask SpawnableLayer;//몬스터가 스폰될 수 있는 영역 (바닥, 벽 등)의 레이어 마스크
    public Player PlayerScript;//플레이어 스크립트 참조

    [Header("스폰 범위 설정(코드 내에서 자동 연결")]
    public BoxCollider2D EnemySpawnCollider;

    [Header("Strong 몬스터 스폰 설정:몇명을 잡아야 스폰,몇번째 프리팹의 몬스터를")]//Inspector에서 시각적으로 구분
    public int NormalKillsForStrongEnemy = 3;//Strong 몬스터 스폰을 위해 잡아야 할 Normal 몬스터 수
    public int StrongEnemyPrefabIndex = 1;//Strong 몬스터 프리팹의 EnemyPrefabs 배열 인덱스 (예: EnemyPrefabs[1])


    [Header("Elite 몬스터 스폰 설정: 특정 점수 도달 시")]//헤더는 이건 순전히 유니티 인스펙터 창을 정리하고 보기 좋게 만들기 위한 기능이야
    public int EliteSpawnScoreThreshold = 150;//Elite 몬스터가 처음 스폰되는 점수
    public int EliteSpawnScoreInterval = 150;//첫 스폰 이후에 Elite 몬스터가 스폰되는 점수 간격 (예: ~~점마다)
    public int EliteEnemyPrefabIndex = 2;//Elite 몬스터 프리팹의 EnemyPrefabs 배열 인덱스 (예: EnemyPrefabs[2])
    private int nextEliteSpawnScore;//다음 Elite 몬스터가 스폰될 점수 임계값

    [Header("Boss 몬스터 스폰 설정")]
    public int BossEnemyPrefabIndex = 3;//배열에서 보스 프리팹 위치
    private Transform bossSpawnPoint;//자동으로 찾을 자식 오브젝트


    //내부에서 사용할 변수들
    private float spawnTimer;//스폰 주기 계산용 타이머
    private float currentEnemyCount;//현재 생성된 몬스터 수를 담을 변수
    private int normalEnemyKilledSinceLastStrong = 0;//마지막 Strong 몬스터 스폰 후 잡은 Normal 몬스터 수
    private TextAlimManager textalimManager;//TextAlimManager 스크립트를 참조할 변수 추가
    private float normalSpawnTime;//Normal 몬스터의 스폰 주기 (EnemyDifficulty에서 가져온 고정값)
    private int normalSpawnCount = 1;//EnemyDifficulty 스크립트에서 받아올 동시 스폰 개수

    void Awake()
    {
        if (Instance == null)//싱글톤 초기화 (중복 방지)
            Instance = this;
        else { Destroy(gameObject); return; }

        InitializeReferences();//자동 참조 연결
    }

    private void InitializeReferences()
    {
        //타일맵 찾기
        if (TargetTilemap == null) TargetTilemap = FindFirstObjectByType<Tilemap>();

        //코드 내에서 자식 콜라이더 자동 연결 (오늘의 핵심!)
        if (EnemySpawnCollider == null)
        {
            EnemySpawnCollider = GetComponentInChildren<BoxCollider2D>();
            if (EnemySpawnCollider != null) Debug.Log($"{EnemySpawnCollider.gameObject.name}을 스폰 범위로 자동 연결했어!");
            else Debug.LogError("자식 오브젝트에서 BoxCollider2D를 찾을 수 없어!");
        }

        //플레이어 찾기
        if (PlayerScript == null)
        {
            GameObject playerscript = GameObject.FindWithTag("Player");
            if (playerscript != null) PlayerScript = playerscript.GetComponent<Player>();
        }

        //자식 중에 "BossSpawnPoint"라는 이름을 가진 오브젝트를 찾음
        if (bossSpawnPoint == null)
        {
            //transform.Find는 자식 오브젝트만 뒤지기 때문에 효율적이야
            Transform foundPoint = transform.Find("BossSpawnPoint");
            if (foundPoint != null)
            {
                bossSpawnPoint = foundPoint;
            }
            else Debug.LogWarning("자식 오브젝트 중 'BossSpawnPoint'를 찾을 수 없어!");

        }

        //텍스트 알림 매니저 찾기
        if (textalimManager == null) textalimManager = FindFirstObjectByType<TextAlimManager>();
    }


    void Start()
    {
        spawnTimer = 2f;//게임 시작 시 첫 몬스터 스폰을 2초 뒤로 늦춤
        nextEliteSpawnScore = EliteSpawnScoreThreshold;//첫 Elite 스폰 점수 초기화
        

        //EnemyPrefabs 배열이 비어있는지 확인 (에러 방지)
        if (EnemyPrefabs == null || EnemyPrefabs.Length == 0)
            Debug.LogError("EnemySpawn: EnemyPrefabs 배열이 비어있어!");


        //Strong 몬스터 프리팹 인덱스 유효성 검사 (EnemyPrefabs 배열의 크기보다 크거나 0보다 작으면 안됨)
        if (StrongEnemyPrefabIndex >= EnemyPrefabs.Length || StrongEnemyPrefabIndex < 0)
            Debug.LogWarning("Strong Enemy Prefab Index가 EnemyPrefabs 배열 범위를 벗어났어! Strong 몬스터 스폰이 안될 수 있어!");


        //Elite 몬스터 프리팹 인덱스 유효성 검사
        if (EliteEnemyPrefabIndex >= EnemyPrefabs.Length || EliteEnemyPrefabIndex < 0)
            Debug.LogWarning("Elite Enemy Prefab Index가 EnemyPrefabs 배열 범위를 벗어났어! 엘리트 몬스터 스폰이 안될 수 있어!");


        if (EnemyDifficulty.Instance != null)
        {
            normalSpawnTime = EnemyDifficulty.Instance.CurrentNormalSpawnTime;
            normalSpawnCount = EnemyDifficulty.Instance.CurrentNormalSpawnCount;
        }
        else
        {
            Debug.LogError("EnemySpawn: EnemyDifficulty.Instance를 찾을 수 없어! EnemyDifficulty 스크립트가 씬에 있는지 확인해!");
            normalSpawnTime = 4f;//기본값으로 설정 (오류 방지)
        } 
    }

    void Update()
    {
        //TimeFreeze로 시간이 멈췄는지 체크하고, 멈췄다면 타이머 감소 및 스폰 로직을 건너뜀
        if (TimeFreeze.Instance != null && TimeFreeze.Instance.IsTimeFrozen) return;


        spawnTimer -= Time.deltaTime;//남은 시간 감소

        if (spawnTimer <= 0f)//타이머가 0이하가 되면 스폰
        {
            //normalSpawnCount 만큼 반복해서 스폰
            for (int i = 0; i < normalSpawnCount; i++) SpawnNormalEnemy();//일반 몬스터 생성 함수 호출

            spawnTimer = normalSpawnTime;//다음 몬스터 스폰을 위해 타이머 초기화
            //08.23 여기 if문은 중괄호를 없애지 않았어. if밑에는 for문이 있어. 그래서 더 밑의 spawnTimer = normalSpawnTime가
            //if문의 영향을 받지 않아서야. 괄호가 없으면 if밑에 있는 하나만 작동하고 더 밑에 있는건 if문과 연결되지 않아서야
        }
    }   
    public void SetNormalSpawnCount(int newCount)//동시 스폰 개수를 받음 함수
    {
        normalSpawnCount = newCount;
        Debug.Log($"EnemySpawn: Normal 몬스터 동시 스폰 개수 업데이트됨: {normalSpawnCount}마리");
    }

    void SpawnNormalEnemy()//Normal 몬스터만 스폰하는 함수
    {
        //몬스터 프리팹이 할당되어 있는지 확인
        if (EnemyPrefabs == null || EnemyPrefabs.Length == 0)
        {
            Debug.LogError("몬스터 프리팹이 할당되지 않았어! 인스펙터를 확인해!.");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();//유효한 스폰 위치 찾기,GetValidSpawnPosition함수를 호출
        if (spawnPosition == Vector3.zero)//유효한 위치를 찾지 못했으면
        {
            Debug.LogWarning("엘리트 몬스터 스폰: 타일맵 내에서 유효한 스폰 위치를 찾을 수 없었어!");
            return;
        }

        //항상 인스펙터의 Element 0에 있는 Normal 몬스터만 스폰
        GameObject enemyToSpawn = EnemyPrefabs[0];
        Enemy.EnemyType enemyTypeToSpawn = Enemy.EnemyType.Normal;


        SpawnEnemy(enemyToSpawn, spawnPosition, enemyTypeToSpawn);//InstantiateAndSetupEnemy함수 호출
    }

    void SpawnStrongEnemy()//Strong 몬스터를 호출하는 함수
    {
        //몬스터 프리팹 유효성 검사 및 인덱스 확인
        if (EnemyPrefabs == null || EnemyPrefabs.Length <= StrongEnemyPrefabIndex || StrongEnemyPrefabIndex < 0)
        {
            Debug.LogError("Strong 몬스터 프리팹이 할당되지 않았거나 인덱스가 잘못됐어! 인스펙터를 확인해!.");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();//유효한 스폰 위치 찾기, GetValidSpawnPosition 호출
        if (spawnPosition == Vector3.zero)//유효한 위치를 찾지 못했으면
        {
            Debug.LogWarning("Strong 몬스터 스폰: 타일맵 내에서 유효한 스폰 위치를 찾을 수 없었어.");
            return;
        }

        //Strong 몬스터 프리팹 선택 
        GameObject enemyToSpawn = EnemyPrefabs[StrongEnemyPrefabIndex];
        Enemy.EnemyType enemyTypeToSpawn =Enemy.EnemyType.Strong;
        Debug.Log("<color=red>Strong 몬스터 스폰!</color>");

        SpawnEnemy(enemyToSpawn, spawnPosition, enemyTypeToSpawn);

        //Strong 몬스터가 스폰되었으니 카운트 초기화
        normalEnemyKilledSinceLastStrong = 0;//Strong 몬스터가 스폰되었으니 카운트 초기화
    }

    void SpawnEliteEnemy()//Elite 몬스터를 호출하는 함수
    {
        //몬스터 프리팹 유효성 검사 및 인덱스 확인
        if (EnemyPrefabs == null || EnemyPrefabs.Length <= EliteEnemyPrefabIndex || EliteEnemyPrefabIndex < 0)
        {
            Debug.LogError("엘리트 몬스터 프리팹이 할당되지 않았거나 인덱스가 잘못됐어! 인스펙터를 확인해!.");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();//유효한 스폰 위치 찾기
        if(spawnPosition == Vector3.zero)//유효한 위치를 찾지 못했으면
        {
            Debug.LogWarning("엘리트 몬스터 스폰: 타일맵 내에서 유효한 스폰 위치를 찾을 수 없었어!");
            return;
        }

        //엘리트 몬스터 프리팹 선택
        GameObject enemyToSpawn = EnemyPrefabs[EliteEnemyPrefabIndex];
        Enemy.EnemyType enemyTypeToSpawn = Enemy.EnemyType.Elite;
        Debug.Log("<color=purple>엘리트 몬스터 스폰!</color>");//색깔 변경

        SpawnEnemy(enemyToSpawn, spawnPosition, enemyTypeToSpawn);

        //TextAlimManager 스크립트에 텍스트 알림 표시
        if (textalimManager != null) textalimManager.ShowMonsterNotification("<color=purple>엘리트 몬스터 등장!</color>");//색깔 변경
    }

    public void SpawnBossEnemy()//Boss 몬스터를 호출하는 함수
    {
        if (EnemyPrefabs == null || EnemyPrefabs.Length <= BossEnemyPrefabIndex)
        {
            Debug.LogError("보스 프리팹 인덱스가 잘못되었거나 배열이 비어있어!");
            return;
        }

        //bossSpawnPoint가 없으면 그냥 매니저 위치(this.transform)에서 생성하도록 예외 처리
        Vector3 spawnPos = (bossSpawnPoint != null) ? bossSpawnPoint.position : transform.position;

        GameObject bossPrefab = EnemyPrefabs[BossEnemyPrefabIndex];
        Enemy.EnemyType type = Enemy.EnemyType.Boss;

        Debug.Log("<color=red><b>최종 보스 출현!</b></color>");

        SpawnEnemy(bossPrefab, spawnPos, type);//기존에 만들어둔 SpawnEnemy 함수를 그대로 활용 (재사용성)

        //보스 등장 시 알림 메시지 (TextAlimManager 활용)
        if (textalimManager != null)
            textalimManager.ShowMonsterNotification("<color=red>최종 보스가 등장했습니다!</color>");
    }


    void SpawnEnemy(GameObject prefab, Vector3 position, Enemy.EnemyType type)
    {//몬스터를 실제로 생성하고 설정하는 공통 함수
     //이 함수는 "어떤 종류의 몬스터 프리팹(GameObject prefab)을, 어떤 위치(Vector3 position)에,
     //그리고 어떤 몬스터 타입(Enemy.EnemyType type)으로 생성할지" 정보를 받아서
     //실제로 몬스터를 게임 씬에 만들고 필요한 초기 설정을 해주는 역할을 해

        //prefab으로 받은 몬스터 프리팹을 position 위치에
        //Quaternion.identity (회전 없음) 상태로 게임 씬에 복제해서 newEnemy라는 변수에 저장해.
        GameObject newEnemy = Instantiate(prefab, position, Quaternion.identity);
        Enemy newEnemyScript = newEnemy.GetComponent<Enemy>();
        //Enemy 스크립트를 newEnemyScript 변수에 참조

        if ((newEnemyScript != null))
        {
            newEnemyScript.EnemySpawner = this;//this는 그 함수가 속해 있는 스크립트 객체(인스턴스)를 가리켜.
            newEnemyScript.enemyType = type;
            newEnemyScript.SetEnmeyStats();
        }
        else Debug.LogWarning("생성된 몬스터에 Enemy 스크립트가 없어!");//몬스터 프리팹에 Enemy 스크립트가 없다면         


        newEnemy.name = prefab.name + "_" + currentEnemyCount;
        currentEnemyCount++;//총 몬스터 수 증가, ++ 연산자는 변수의 값을 1씩 더해
    }

    Vector3 GetValidSpawnPosition()
    {   
        if (EnemySpawnCollider == null) return Vector3.zero;//스폰 범위(콜라이더)가 없으면 0 좌표 반환 (에러 방지)

        int maxAttempts = 100;//무한 루프 방지를 위한 최대 시도 횟수 (100번 안에 못 찾으면 이번 스폰은 포기)

        Bounds bounds = EnemySpawnCollider.bounds;//콜라이더의 사각형 경계값(최소/최대 좌표) 추출

        for (int a = 0; a < maxAttempts; a++)
        {
            //콜라이더 박스 범위 내의 랜덤한 지점 하나를 뽑음
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector3 randomPos = new Vector3(randomX, randomY, 0);

            //뽑은 곳이 실제로 콜라이더 영역 내부인지 확인
            if (EnemySpawnCollider.OverlapPoint(randomPos))
            {
                //해당 위치에 장애물(벽 등)이 없는지 최종 확인
                Collider2D hit = Physics2D.OverlapCircle(randomPos, 0.3f, SpawnableLayer);

                if (hit == null) return randomPos;//모든 검사를 통과하면 이 좌표를 스폰 위치로 확정
            }
        }
        return Vector3.zero;//100번 시도해도 적절한 위치를 못 찾았을 때 안전하게 0 반환
    }

    public void EnemyDied(bool isStrongOrEliteEnemyDied)//몬스터가 죽었을 때 호출될 함수(Enemy 스크립트에서 호출해야 함)
    {   //-- 연산자는 변수의 값을 1씩 빼는 역할을 해
        currentEnemyCount--;

        //Strong 몬스터 스폰을 위한 카운트는 오직 Normal 몬스터가 죽었을 때만 증가
        if (!isStrongOrEliteEnemyDied)//죽은 몬스터가 Normal 몬스터일 때
        {
            normalEnemyKilledSinceLastStrong++;//Normal 몬스터 킬 카운트 증가, ++ 연산자는 변수의 값을 1씩 더해
            Debug.Log("Normal 몬스터 사망! Strong 몬스터 스폰까지 남은 킬 수: " + (NormalKillsForStrongEnemy - normalEnemyKilledSinceLastStrong) + "마리.");

            //Strong 몬스터 스폰 조건 충족 시
            if (normalEnemyKilledSinceLastStrong >= NormalKillsForStrongEnemy) SpawnStrongEnemy();//Strong 몬스터 스폰! (따로 스폰됨)
        }
        else Debug.Log("Strong/엘리트 몬스터가 사망했습니다. Normal 몬스터 킬 카운트에는 영향을 주지 않아!");

        //Player 스크립트가 연결되어 있고, 플레이어가 살아있을 때만 점수 기반 스폰 체크
        if (PlayerScript != null && !PlayerScript.IsDead)
        { 
            if (PlayerScript.CurrentScore >= nextEliteSpawnScore)//현재 플레이어의 점수가 다음 엘리트 몬스터 스폰 임계값에 도달했는지 확인.
            {
                SpawnEliteEnemy();//SpawnEliteEnemy함수로 엘리트 몬스터를 스폰

                //다음 엘리트 몬스터가 스폰될 점수 임계값을 업데이트(현재 임계값 + 설정된 간격)
                nextEliteSpawnScore += EliteSpawnScoreInterval;//다음 Elite 스폰 점수 갱신
                Debug.Log($"다음 엘리트 몬스터는 {nextEliteSpawnScore}점일 때 스폰될 예정이야!");
            }
        }
    }
}

