using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;//Ÿ�ϸ��� ����Ϸ��� �ʿ�

public class ItemSpawner : MonoBehaviour
{

    //�ν����Ϳ� �Ҵ��� ������
    public GameObject[] ItemPrefabs;//������ ������ ������ ���
    [Header("������Ʈ, ���̾� ����")]//���� itemPrefabs ������ []�� ��Ӵٿ� �޴��� ���ܼ� ��� �ȿ� �������� �ʾҾ�
    public Tilemap TargetTilemap;//�������� ������ Ÿ�ϸ�
    public LayerMask SpawnableLayer;//�������� ������ �� �ִ� ���̾� (�ٴ� ��)
    [Header("�������� �����Ǵ� �ð�")]
    public float ItemSpawnTime = 10f;//�������� �����Ǵ� �ֱ� (��)


    //���ο��� ����� ������
    private float spawnTimer;
    private int currentItemCount;
    private TextAlimManager textalimManager;//TextAlimManager ��ũ��Ʈ ����


    void Start()
    {
        //!= null�� "null�� �ƴ� ��" ��, "���𰡰� ������ ��"�� �ǹ��ϰ�,
        //== null�� "null�� ��" ��, "�ƹ��͵� ���ų� ������� ��"�� �ǹ���.
        spawnTimer = ItemSpawnTime;
        if (ItemPrefabs == null || ItemPrefabs.Length == 0)
            Debug.LogError("ItemSpawner: ������ �������� �Ҵ���� �ʾҾ�!");

        if (TargetTilemap == null)
            Debug.LogError("ItemSpawner: Ÿ�ϸ��� �Ҵ���� �ʾҾ�!");

        textalimManager = FindObjectOfType<TextAlimManager>();
        if (textalimManager == null)
            Debug.LogError("TextAlimManager ������ ã�� �� ����!");
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

        Vector3 spawnPosition = GetValidSpawnPosition();//GetValidSpawnPosition�Լ� ȣ��
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning("������ ����: Ÿ�ϸ� ������ ��ȿ�� ���� ��ġ�� ã�� �� ������!");
            return;
        }

        int randomIndex = Random.Range(0, ItemPrefabs.Length);
        GameObject itemToSpawn = ItemPrefabs[randomIndex];

        Instantiate(itemToSpawn, spawnPosition, Quaternion.identity);
        currentItemCount++;

        // TextAlimManager ��ũ��Ʈ�� �˸� ������, UI�� �� �ؽ�Ʈ
        if (textalimManager != null)
            textalimManager.ShowNotification("<color=yellow>������ ����!</color>");
    }

    Vector3 GetValidSpawnPosition()//�������� ���� ��ġ�� ���ϴ� �Լ�, �������� �� ��ġ�߿��� ����
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

                //�ֺ��� �ٸ� �ݶ��̴�(������Ʈ)�� ������ Ȯ��
                Collider2D[] colliders = Physics2D.OverlapCircleAll(cellCenterTile, 0.5f, SpawnableLayer);

                //�ٸ� �ݶ��̴��� ������ ��ȿ�� ��ġ
                if (colliders.Length == 0) return cellCenterTile;
            }
        }
        return Vector3.zero;
    } 

    public void ItemDestroyed()//�������� �԰� ������� ȣ��� �Լ�
    {   //�̰� item ��ũ��Ʈ�� Destroy(gameObject);�� �޶�, currentItemCount������ ���� 1 ���̴°ž�.
        currentItemCount--;
    }
}
