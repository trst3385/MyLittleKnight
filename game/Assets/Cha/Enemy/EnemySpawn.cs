using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;//Tilemap ���� ����� ����ϱ� ����

public class EnemySpawn : MonoBehaviour//public �ʵ�� �빮�ڷ� �����ϴ� ���� C#�� ǥ�� �ڵ� �������̾�.
{   //�̷��� �����ϸ� �ڵ尡 �ξ� ����ϰ� �ٸ� �����ڵ��� ���� ���� �����ϱ� ������.

    [Header("��ũ��Ʈ, ������Ʈ ����")]
    //�ν����Ϳ� �Ҵ��� ������
    public GameObject[] EnemyPrefabs;//�ν����Ϳ��� ������ ���� �������� �Ҵ� (0�� �ε����� Normal ����)
    public Tilemap TargetTilemap;//���͸� ������ Ÿ�ϸ��� �Ҵ� (�÷��� ������ ����)
    public LayerMask SpawnableLayer;//���Ͱ� ������ �� �ִ� ���� (�ٴ�, �� ��)�� ���̾� ����ũ
    public Player PlayerScript;//�÷��̾� ��ũ��Ʈ ����

    [Header("Strong ���� ���� ����:����� ��ƾ� ����,���° �������� ���͸�")]//Inspector���� �ð������� ����
    //Header�ؿ� �ִ� ���� ������
    public int NormalKillsForStrongEnemy = 3;//���� ���� ������ ���� ��ƾ� �� Normal ���� ��
    public int StrongEnemyPrefabIndex = 1;//���� ���� �������� EnemyPrefabs �迭 �ε��� (��: EnemyPrefabs[1])
    
    [Header("Elite ���� ���� ����: Ư�� ���� ���� ��")]//����� �̰� ������ ����Ƽ �ν����� â�� �����ϰ� ���� ���� ����� ���� ����̾�
    public int EliteSpawnScoreThreshold = 200;//Elite ���Ͱ� �����Ǳ� �����ϴ� ����
    public int EliteSpawnScoreInterval = 200;//Elite ���Ͱ� �߰��� �����Ǵ� ���� ���� (��: ~~������)
    public int EliteEnemyPrefabIndex = 2;//Elite ���� �������� EnemyPrefabs �迭 �ε��� (��: EnemyPrefabs[2])
    private int nextEliteSpawnScore;//���� Elite ���Ͱ� ������ ���� �Ӱ谪
     

    // == ���ο��� ����� ������ ==
    private float spawnTimer;//���� �ֱ� ���� Ÿ�̸�
    private float currentEnemyCount;//���� ������ ���� ���� ���� ����
    private int normalEnemyKilledSinceLastStrong = 0;//������ ���� ���� ���� �� ���� Normal ���� ��
    private TextAlimManager textalimManager;//TextAlimManager ��ũ��Ʈ�� ������ ���� �߰�

    private float normalSpawnTime;//EnemyDifficulty ��ũ��Ʈ���� �޾ƿ� ���� Normal ���� ���� �ֱ� ����
    private int normalSpawnCount = 1;//EnemyDifficulty ��ũ��Ʈ���� �޾ƿ� ���� ���� ����

    void Start()
    {
        spawnTimer = 2f;//���� ���� �� ù ���� ������ 2�� �ڷ� ����

        nextEliteSpawnScore = EliteSpawnScoreThreshold;//ù Elite ���� ���� �ʱ�ȭ


        //EnemyPrefabs �迭�� ����ִ��� Ȯ��w (���� ����)
        if (EnemyPrefabs == null || EnemyPrefabs.Length == 0)
            Debug.LogError("EnemySpawn: EnemyPrefabs �迭�� ���� �������� �Ҵ���� �ʾҾ�! �ν����͸� Ȯ����!");


        //���� ���� ������ �ε��� ��ȿ�� �˻� (EnemyPrefabs �迭�� ũ�⺸�� ũ�ų� 0���� ������ �ȵ�)
        if (StrongEnemyPrefabIndex >= EnemyPrefabs.Length || StrongEnemyPrefabIndex < 0)
            Debug.LogWarning("Strong Enemy Prefab Index�� EnemyPrefabs �迭 ������ �����! ���� ���� ������ �ȵ� �� �־�!");



        //Elite ���� ������ �ε��� ��ȿ�� �˻�
        if (EliteEnemyPrefabIndex >= EnemyPrefabs.Length || EliteEnemyPrefabIndex < 0)
            Debug.LogWarning("Elite Enemy Prefab Index�� EnemyPrefabs �迭 ������ �����! ����Ʈ ���� ������ �ȵ� �� �־�!");


        //���� �� TextAlimManager ��ũ��Ʈ�� ã�Ƽ� �Ҵ�
        textalimManager = FindObjectOfType<TextAlimManager>();
        if (textalimManager == null)
            Debug.LogWarning("EnemySpawn: TextAlimManager�� ã�� �� �����ϴ�. ���� �˸��� ǥ�õ��� �ʽ��ϴ�.");


        //EnemyDifficulty ��ũ��Ʈ���� �ʱ� ���� �ֱ⸦ ������
        //�̱��� ��ũ��Ʈ�� ���� ���� ����, �ν����Ϳ��� ���� ������ �ʿ� ���� �ڵ� �ȿ���
        //EnemyDifficulty.Instance �� ���� �ٷ� ������ �� �־�. �̱��� ������ ���� ū Ư¡�̾�!
        if (EnemyDifficulty.Instance != null)
        {
            normalSpawnTime = EnemyDifficulty.Instance.GetSpawnTime();
            Debug.Log($"EnemySpawn: EnemyDifficulty�κ��� �ʱ� ���� �ֱ� {normalSpawnTime}s ������");
        }
        else
        {
            Debug.LogError("EnemySpawn: EnemyDifficulty.Instance�� ã�� �� ����! EnemyDifficulty ��ũ��Ʈ�� ���� �ִ��� Ȯ����!");
            normalSpawnTime = 4f;//�⺻������ ���� (���� ����)
        }
            
        //Player ��ũ��Ʈ ���� ��������
        GameObject playerGameObject = GameObject.FindWithTag("Player");
        if (playerGameObject != null)
        {
            PlayerScript = playerGameObject.GetComponent<Player>();
            if (PlayerScript == null)
                Debug.LogError("EnemySpawn: Player ������Ʈ�� Player ��ũ��Ʈ�� �����ϴ�!");
        }
        else
            Debug.LogWarning("EnemySpawn: 'Player' �±׸� ���� ������Ʈ�� ã�� �� ����!");
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;//���� �ð� ����

        if (spawnTimer <= 0f)//Ÿ�̸Ӱ� 0���ϰ� �Ǹ� ����
        {
            for (int i = 0; i < normalSpawnCount; i++)//normalSpawnCount ��ŭ �ݺ��ؼ� ����
            {
                SpawnNormalEnemy();//�Ϲ� ���� ���� �Լ� ȣ��
            }  
            spawnTimer = normalSpawnTime;//���� ���� ������ ���� Ÿ�̸� �ʱ�ȭ
            //08.23 ���� if���� �߰�ȣ�� ������ �ʾҾ�. if�ؿ��� for���� �־�. �׷��� �� ���� spawnTimer = normalSpawnTime��
            //if���� ������ ���� �ʾƼ���. ��ȣ�� ������ if�ؿ� �ִ� �ϳ��� �۵��ϰ� �� �ؿ� �ִ°� if���� ������� �ʾƼ���
        }
    }
    public void SetNormalSpawnTime(float newTime)
    {
        normalSpawnTime = newTime;
        Debug.Log($"EnemySpawn: Normal ���� ���� �ֱ� ������Ʈ��: {normalSpawnTime}s");
    }
    
    public void SetNormalSpawnCount(int newCount)//���� ���� ������ ���� �Լ�
    {
        normalSpawnCount = newCount;
        Debug.Log($"EnemySpawn: Normal ���� ���� ���� ���� ������Ʈ��: {normalSpawnCount}����");
    }

    void SpawnNormalEnemy()//Normal ���͸� �����ϴ� �Լ�
    {
        //���� �������� �Ҵ�Ǿ� �ִ��� Ȯ��
        if (EnemyPrefabs == null || EnemyPrefabs.Length == 0)
        {
            Debug.LogError("���� �������� �Ҵ���� �ʾҾ�! �ν����͸� Ȯ����!.");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();//��ȿ�� ���� ��ġ ã��,GetValidSpawnPosition�Լ��� ȣ��
        if (spawnPosition == Vector3.zero)//��ȿ�� ��ġ�� ã�� ��������
            Debug.LogWarning("Normal ���� ����: Ÿ�ϸ� ������ ��ȿ�� ���� ��ġ�� ã�� �� ����!.");


        //�׻� �ν������� Element 0�� �ִ� Normal ���͸� ����
        GameObject enemyToSpawn = EnemyPrefabs[0];
        Enemy.EnemyType enemyTypeToSpawn = Enemy.EnemyType.Normal;
        //Debug.Log("Normal ���� ����!");

        SpawnEnemy(enemyToSpawn, spawnPosition, enemyTypeToSpawn);//InstantiateAndSetupEnemy�Լ� ȣ��
        
        //TextAlimManager ��ũ��Ʈ�� �ؽ�Ʈ �˸� ǥ��
        if (textalimManager != null)
            textalimManager.ShowNotification("���� ����!");
    }

    void SpawnStrongEnemy()//Strong ���͸� ȣ���ϴ� �Լ�
    {
        //���� ������ ��ȿ�� �˻� �� �ε��� Ȯ��
        if (EnemyPrefabs == null || EnemyPrefabs.Length <= StrongEnemyPrefabIndex || StrongEnemyPrefabIndex < 0)
        {
            Debug.LogError("���� ���� �������� �Ҵ���� �ʾҰų� �ε����� �߸��ƾ�! �ν����͸� Ȯ����!.");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();//��ȿ�� ���� ��ġ ã��, GetValidSpawnPosition ȣ��
        if (spawnPosition == Vector3.zero)//��ȿ�� ��ġ�� ã�� ��������
        {
            Debug.LogWarning("���� ���� ����: Ÿ�ϸ� ������ ��ȿ�� ���� ��ġ�� ã�� �� ������.");
            return;
        }
      
        //���� ���� ������ ���� 
        GameObject enemyToSpawn = EnemyPrefabs[StrongEnemyPrefabIndex];
        Enemy.EnemyType enemyTypeToSpawn =Enemy.EnemyType.Strong;
        Debug.Log("<color=red>���� ���� ����!</color>");

        SpawnEnemy(enemyToSpawn, spawnPosition, enemyTypeToSpawn);

        //���� ���Ͱ� �����Ǿ����� ī��Ʈ �ʱ�ȭ
        normalEnemyKilledSinceLastStrong = 0;

        //TextAlimManager ��ũ��Ʈ�� �ؽ�Ʈ �˸� ǥ��
        if (textalimManager != null)
            textalimManager.ShowNotification("<color=red>���� ���� ����!</color>");

    }

    void SpawnEliteEnemy()//Elite ���͸� ȣ���ϴ� �Լ�
    {
        //���� ������ ��ȿ�� �˻� �� �ε��� Ȯ��
        if (EnemyPrefabs == null || EnemyPrefabs.Length <= EliteEnemyPrefabIndex || EliteEnemyPrefabIndex < 0)
        {
            Debug.LogError("����Ʈ ���� �������� �Ҵ���� �ʾҰų� �ε����� �߸��ƾ�! �ν����͸� Ȯ����!.");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();//��ȿ�� ���� ��ġ ã��
        if(spawnPosition == Vector3.zero)//��ȿ�� ��ġ�� ã�� ��������
        {
            Debug.LogWarning("����Ʈ ���� ����: Ÿ�ϸ� ������ ��ȿ�� ���� ��ġ�� ã�� �� ������!");
            return;
        }

        //����Ʈ ���� ������ ����
        GameObject enemyToSpawn = EnemyPrefabs[EliteEnemyPrefabIndex];
        Enemy.EnemyType enemyTypeToSpawn = Enemy.EnemyType.Elite;
        Debug.Log("<color=purple>����Ʈ ���� ����!</color>");//���� ����

        SpawnEnemy(enemyToSpawn, spawnPosition, enemyTypeToSpawn);

        if (textalimManager != null)//TextAlimManager ��ũ��Ʈ�� �ؽ�Ʈ �˸� ǥ��
            textalimManager.ShowNotification("<color=purple>����Ʈ ���� ����!</color>");//���� ����
    }


    void SpawnEnemy(GameObject prefab, Vector3 position, Enemy.EnemyType type)
    {//���͸� ������ �����ϰ� �����ϴ� ���� �Լ�
     //�� �Լ��� "� ������ ���� ������(GameObject prefab)��, � ��ġ(Vector3 position)��,
     //�׸��� � ���� Ÿ��(Enemy.EnemyType type)���� ��������" ������ �޾Ƽ�
     //������ ���͸� ���� ���� ����� �ʿ��� �ʱ� ������ ���ִ� ������ ��

        //prefab���� ���� ���� �������� position ��ġ��
        //Quaternion.identity (ȸ�� ����) ���·� ���� ���� �����ؼ� newEnemy��� ������ ������.
        GameObject newEnemy = Instantiate(prefab, position, Quaternion.identity);
        Enemy newEnemyScript = newEnemy.GetComponent<Enemy>();
        //Enemy ��ũ��Ʈ�� newEnemyScript ������ ����

        if ((newEnemyScript != null))
        {
            newEnemyScript.EnemySpawner = this;//this�� �� �Լ��� ���� �ִ� ��ũ��Ʈ ��ü(�ν��Ͻ�)�� ������.
            newEnemyScript.enemyType = type;
            newEnemyScript.SetEnmeyStats();
        }
        else//���� �����տ� Enemy ��ũ��Ʈ�� ���ٸ�            
            Debug.LogWarning("������ ���Ϳ� Enemy ��ũ��Ʈ�� ����!");


        newEnemy.name = prefab.name + "_" + currentEnemyCount;
        currentEnemyCount++;//�� ���� �� ����, ++ �����ڴ� ������ ���� 1�� ����
    }
    
    Vector3 GetValidSpawnPosition()//��ȿ�� ���� ��ġ�� ã�� ���� �Լ�(�ڵ尡 ������� �Լ��� �и�)
    {//GetValidSpawnPosition��� �Լ� �ȿ� Vector3�� ������ ���� ����ִ�!
        int maxAttempts = 100;
        for(int a = 0; a < maxAttempts; a++)
        {
            if(TargetTilemap == null)
            {
                Debug.LogError("TargetTilemap�� �Ҵ���� �ʾҾ�!");
                return Vector3.zero;//��ȿ�� ��ġ �� ã���� Vector3.zero ��ȯ
            }
            //Vector3�� 3���� ������ ��ǥ(x, y, z)�� ������ ��Ÿ���� ����ü�ε�,
            //Vector3.zero�� ������ ���� ��ǥ�� �ǹ���:x = 0, y = 0, z = 0
            //��, (0, 0, 0) �̶�� ���� ��ǥ�� ���ϴ� ����.

            BoundsInt bounds = TargetTilemap.cellBounds;
            int randomX = Random.Range(bounds.xMin, bounds.xMax);
            int randomY = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int randomCell = new Vector3Int(randomX, randomY, 0);

            if (TargetTilemap.HasTile(randomCell))
            {
                Vector3 cellCenterWorld = TargetTilemap.GetCellCenterWorld(randomCell);
                Collider2D[] colliders = Physics2D.OverlapCircleAll(cellCenterWorld, 0.5f, SpawnableLayer);
                if (colliders.Length == 0)
                    return cellCenterWorld;//��ȿ�� ��ġ ã���� ��ȯ

            }
        }
        return Vector3.zero;//100�� �õ��ص� �� ã���� Vector3.zero ��ȯ,void�� �� Ŭ������ �ƴϴ� return���
    }
    
    public void EnemyDied(bool isStrongOrEliteEnemyDied)//���Ͱ� �׾��� �� ȣ��� �Լ�(Enemy ��ũ��Ʈ���� ȣ���ؾ� ��)
    {   //-- �����ڴ� ������ ���� 1�� ���� ������ ��
        currentEnemyCount--;

        //Strong ���� ������ ���� ī��Ʈ�� ���� Normal ���Ͱ� �׾��� ���� ����
        if (!isStrongOrEliteEnemyDied)//���� ���Ͱ� Normal ������ ��
        {
            normalEnemyKilledSinceLastStrong++;//Normal ���� ų ī��Ʈ ����, ++ �����ڴ� ������ ���� 1�� ����
            Debug.Log("Normal ���� ���! ���� ���� �������� ���� ų ��: " + (NormalKillsForStrongEnemy - normalEnemyKilledSinceLastStrong) + "����.");

            //Strong ���� ���� ���� ���� ��
            if (normalEnemyKilledSinceLastStrong >= NormalKillsForStrongEnemy)
                SpawnStrongEnemy();//Strong ���� ����! (���� ������)

        }
        else//���� ���ͳ� ����Ʈ ���Ͱ� �׾��� �� (���� ���� ���� ī��Ʈ�� ���� ����)
            Debug.Log("����/����Ʈ ���Ͱ� ����߽��ϴ�. Normal ���� ų ī��Ʈ���� ������ ���� �ʾ�!");


        //Player ��ũ��Ʈ�� ����Ǿ� �ְ�, �÷��̾ ������� ���� ���� ��� ���� üũ
        if (PlayerScript != null && !PlayerScript.IsDead)
        { 
            if (PlayerScript.CurrentScore >= nextEliteSpawnScore)//���� �÷��̾��� ������ ���� ����Ʈ ���� ���� �Ӱ谪�� �����ߴ��� Ȯ��.
            {
                SpawnEliteEnemy();//SpawnEliteEnemy�Լ��� ����Ʈ ���͸� ����

                //���� ����Ʈ ���Ͱ� ������ ���� �Ӱ谪�� ������Ʈ(���� �Ӱ谪 + ������ ����)
                nextEliteSpawnScore += EliteSpawnScoreInterval;//���� Elite ���� ���� ����
                Debug.Log($"���� ����Ʈ ���ʹ� {nextEliteSpawnScore}���� �� ������ �����̾�!");
            }
        }
    }
}

