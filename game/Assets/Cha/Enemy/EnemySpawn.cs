using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;//Tilemap 관련 기능을 사용하기 위해

public class EnemySpawn : MonoBehaviour//public 필드는 대문자로 시작하는 것이 C#의 표준 코딩 컨벤션이야.
{   //이렇게 통일하면 코드가 훨씬 깔끔하고 다른 개발자들이 봤을 때도 이해하기 쉬워져.

    [Header("스크립트, 오브젝트 연결")]
    //인스펙터에 할당할 변수들
    public GameObject[] EnemyPrefabs;//인스펙터에서 스폰할 몬스터 프리팹을 할당 (0번 인덱스에 Normal 몬스터)
    public Tilemap TargetTilemap;//몬스터를 스폰할 타일맵을 할당 (플레이 가능한 영역)
    public LayerMask SpawnableLayer;//몬스터가 스폰될 수 있는 영역 (바닥, 벽 등)의 레이어 마스크
    public Player PlayerScript;//플레이어 스크립트 참조

    [Header("Strong 몬스터 스폰 설정:몇명을 잡아야 스폰,몇번째 프리팹의 몬스터를")]//Inspector에서 시각적으로 구분
    //Header밑에 있는 것이 설정됨
    public int NormalKillsForStrongEnemy = 3;//강한 몬스터 스폰을 위해 잡아야 할 Normal 몬스터 수
    public int StrongEnemyPrefabIndex = 1;//강한 몬스터 프리팹의 EnemyPrefabs 배열 인덱스 (예: EnemyPrefabs[1])
    
    [Header("Elite 몬스터 스폰 설정: 특정 점수 도달 시")]//헤더는 이건 순전히 유니티 인스펙터 창을 정리하고 보기 좋게 만들기 위한 기능이야
    public int EliteSpawnScoreThreshold = 200;//Elite 몬스터가 스폰되기 시작하는 점수
    public int EliteSpawnScoreInterval = 200;//Elite 몬스터가 추가로 스폰되는 점수 간격 (예: ~~점마다)
    public int EliteEnemyPrefabIndex = 2;//Elite 몬스터 프리팹의 EnemyPrefabs 배열 인덱스 (예: EnemyPrefabs[2])
    private int nextEliteSpawnScore;//다음 Elite 몬스터가 스폰될 점수 임계값
     

    // == 내부에서 사용할 변수들 ==
    private float spawnTimer;//스폰 주기 계산용 타이머
    private float currentEnemyCount;//현재 생성된 몬스터 수를 담을 변수
    private int normalEnemyKilledSinceLastStrong = 0;//마지막 강한 몬스터 스폰 후 잡은 Normal 몬스터 수
    private TextAlimManager textalimManager;//TextAlimManager 스크립트를 참조할 변수 추가

    private float normalSpawnTime;//EnemyDifficulty 스크립트에서 받아올 현재 Normal 몬스터 스폰 주기 변수
    private int normalSpawnCount = 1;//EnemyDifficulty 스크립트에서 받아올 동시 스폰 개수

    void Start()
    {
        spawnTimer = 2f;//게임 시작 시 첫 몬스터 스폰을 2초 뒤로 늦춤

        nextEliteSpawnScore = EliteSpawnScoreThreshold;//첫 Elite 스폰 점수 초기화


        //EnemyPrefabs 배열이 비어있는지 확인w (에러 방지)
        if (EnemyPrefabs == null || EnemyPrefabs.Length == 0)
            Debug.LogError("EnemySpawn: EnemyPrefabs 배열에 몬스터 프리팹이 할당되지 않았어! 인스펙터를 확인해!");


        //강한 몬스터 프리팹 인덱스 유효성 검사 (EnemyPrefabs 배열의 크기보다 크거나 0보다 작으면 안됨)
        if (StrongEnemyPrefabIndex >= EnemyPrefabs.Length || StrongEnemyPrefabIndex < 0)
            Debug.LogWarning("Strong Enemy Prefab Index가 EnemyPrefabs 배열 범위를 벗어났어! 강한 몬스터 스폰이 안될 수 있어!");



        //Elite 몬스터 프리팹 인덱스 유효성 검사
        if (EliteEnemyPrefabIndex >= EnemyPrefabs.Length || EliteEnemyPrefabIndex < 0)
            Debug.LogWarning("Elite Enemy Prefab Index가 EnemyPrefabs 배열 범위를 벗어났어! 엘리트 몬스터 스폰이 안될 수 있어!");


        //시작 시 TextAlimManager 스크립트를 찾아서 할당
        textalimManager = FindObjectOfType<TextAlimManager>();
        if (textalimManager == null)
            Debug.LogWarning("EnemySpawn: TextAlimManager를 찾을 수 없습니다. 스폰 알림이 표시되지 않습니다.");


        //EnemyDifficulty 스크립트에서 초기 스폰 주기를 가져와
        //싱글톤 스크립트라서 따로 참조 변수, 인스펙터에서 직접 연결할 필요 없이 코드 안에서
        //EnemyDifficulty.Instance 를 통해 바로 접근할 수 있어. 싱글톤 패턴의 가장 큰 특징이야!
        if (EnemyDifficulty.Instance != null)
        {
            normalSpawnTime = EnemyDifficulty.Instance.GetSpawnTime();
            Debug.Log($"EnemySpawn: EnemyDifficulty로부터 초기 스폰 주기 {normalSpawnTime}s 가져옴");
        }
        else
        {
            Debug.LogError("EnemySpawn: EnemyDifficulty.Instance를 찾을 수 없어! EnemyDifficulty 스크립트가 씬에 있는지 확인해!");
            normalSpawnTime = 4f;//기본값으로 설정 (오류 방지)
        }
            
        //Player 스크립트 참조 가져오기
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        if (playerGameObject != null)
        {
            PlayerScript = playerGameObject.GetComponent<Player>();
            if (PlayerScript == null)
                Debug.LogError("EnemySpawn: Player 오브젝트에 Player 스크립트가 없습니다!");
        }
        else
            Debug.LogWarning("EnemySpawn: 'Player' 태그를 가진 오브젝트를 찾을 수 없어!");
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;//남은 시간 감소

        if (spawnTimer <= 0f)//타이머가 0이하가 되면 스폰
        {
            for (int i = 0; i < normalSpawnCount; i++)//normalSpawnCount 만큼 반복해서 스폰
            {
                SpawnNormalEnemy();//일반 몬스터 생성 함수 호출
            }  
            spawnTimer = normalSpawnTime;//다음 몬스터 스폰을 위해 타이머 초기화
            //08.23 여기 if문은 중괄호를 없애지 않았어. if밑에는 for문이 있어. 그래서 더 밑의 spawnTimer = normalSpawnTime가
            //if문의 영향을 받지 않아서야. 괄호가 없으면 if밑에 있는 하나만 작동하고 더 밑에 있는건 if문과 연결되지 않아서야
        }
    }
    public void SetNormalSpawnTime(float newTime)
    {
        normalSpawnTime = newTime;
        Debug.Log($"EnemySpawn: Normal 몬스터 스폰 주기 업데이트됨: {normalSpawnTime}s");
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
            Debug.LogWarning("Normal 몬스터 스폰: 타일맵 내에서 유효한 스폰 위치를 찾을 수 없어!.");


        //항상 인스펙터의 Element 0에 있는 Normal 몬스터만 스폰
        GameObject enemyToSpawn = EnemyPrefabs[0];
        Enemy.EnemyType enemyTypeToSpawn = Enemy.EnemyType.Normal;
        //Debug.Log("Normal 몬스터 스폰!");

        SpawnEnemy(enemyToSpawn, spawnPosition, enemyTypeToSpawn);//InstantiateAndSetupEnemy함수 호출
        
        //TextAlimManager 스크립트에 텍스트 알림 표시
        if (textalimManager != null)
            textalimManager.ShowNotification("몬스터 스폰!");
    }

    void SpawnStrongEnemy()//Strong 몬스터를 호출하는 함수
    {
        //몬스터 프리팹 유효성 검사 및 인덱스 확인
        if (EnemyPrefabs == null || EnemyPrefabs.Length <= StrongEnemyPrefabIndex || StrongEnemyPrefabIndex < 0)
        {
            Debug.LogError("강한 몬스터 프리팹이 할당되지 않았거나 인덱스가 잘못됐어! 인스펙터를 확인해!.");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();//유효한 스폰 위치 찾기, GetValidSpawnPosition 호출
        if (spawnPosition == Vector3.zero)//유효한 위치를 찾지 못했으면
        {
            Debug.LogWarning("강한 몬스터 스폰: 타일맵 내에서 유효한 스폰 위치를 찾을 수 없었어.");
            return;
        }
      
        //강한 몬스터 프리팹 선택 
        GameObject enemyToSpawn = EnemyPrefabs[StrongEnemyPrefabIndex];
        Enemy.EnemyType enemyTypeToSpawn =Enemy.EnemyType.Strong;
        Debug.Log("<color=red>강한 몬스터 스폰!</color>");

        SpawnEnemy(enemyToSpawn, spawnPosition, enemyTypeToSpawn);

        //강한 몬스터가 스폰되었으니 카운트 초기화
        normalEnemyKilledSinceLastStrong = 0;

        //TextAlimManager 스크립트에 텍스트 알림 표시
        if (textalimManager != null)
            textalimManager.ShowNotification("<color=red>강한 몬스터 등장!</color>");

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

        if (textalimManager != null)//TextAlimManager 스크립트에 텍스트 알림 표시
            textalimManager.ShowNotification("<color=purple>엘리트 몬스터 등장!</color>");//색깔 변경
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
        else//몬스터 프리팹에 Enemy 스크립트가 없다면            
            Debug.LogWarning("생성된 몬스터에 Enemy 스크립트가 없어!");


        newEnemy.name = prefab.name + "_" + currentEnemyCount;
        currentEnemyCount++;//총 몬스터 수 증가, ++ 연산자는 변수의 값을 1씩 더해
    }
    
    Vector3 GetValidSpawnPosition()//유효한 스폰 위치를 찾는 공통 함수(코드가 길어지니 함수로 분리)
    {//GetValidSpawnPosition라는 함수 안에 Vector3로 설정한 값이 들어있다!
        int maxAttempts = 100;
        for(int a = 0; a < maxAttempts; a++)
        {
            if(TargetTilemap == null)
            {
                Debug.LogError("TargetTilemap이 할당되지 않았어!");
                return Vector3.zero;//유효한 위치 못 찾으면 Vector3.zero 반환
            }
            //Vector3는 3차원 공간의 좌표(x, y, z)나 방향을 나타내는 구조체인데,
            //Vector3.zero는 다음과 같은 좌표를 의미해:x = 0, y = 0, z = 0
            //즉, (0, 0, 0) 이라는 원점 좌표를 뜻하는 거지.

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
    
    public void EnemyDied(bool isStrongOrEliteEnemyDied)//몬스터가 죽었을 때 호출될 함수(Enemy 스크립트에서 호출해야 함)
    {   //-- 연산자는 변수의 값을 1씩 빼는 역할을 해
        currentEnemyCount--;

        //Strong 몬스터 스폰을 위한 카운트는 오직 Normal 몬스터가 죽었을 때만 증가
        if (!isStrongOrEliteEnemyDied)//죽은 몬스터가 Normal 몬스터일 때
        {
            normalEnemyKilledSinceLastStrong++;//Normal 몬스터 킬 카운트 증가, ++ 연산자는 변수의 값을 1씩 더해
            Debug.Log("Normal 몬스터 사망! 강한 몬스터 스폰까지 남은 킬 수: " + (NormalKillsForStrongEnemy - normalEnemyKilledSinceLastStrong) + "마리.");

            //Strong 몬스터 스폰 조건 충족 시
            if (normalEnemyKilledSinceLastStrong >= NormalKillsForStrongEnemy)
                SpawnStrongEnemy();//Strong 몬스터 스폰! (따로 스폰됨)

        }
        else//강한 몬스터나 엘리트 몬스터가 죽었을 때 (강한 몬스터 스폰 카운트에 영향 없음)
            Debug.Log("강한/엘리트 몬스터가 사망했습니다. Normal 몬스터 킬 카운트에는 영향을 주지 않아!");


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

