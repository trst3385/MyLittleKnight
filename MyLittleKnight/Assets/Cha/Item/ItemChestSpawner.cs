using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;//Tilemap�� ����Ϸ��� �� using ���� �ʿ�

public class ItemChestSpawner : MonoBehaviour
{
    [Header("������ ������Ʈ, ���̾� ����")]
    // ==�ν����Ϳ� �Ҵ��� ������==
    public GameObject ItemChestPrefab;//�ν����Ϳ��� ������ ������ ���� �������� �Ҵ�
    public Tilemap TargetTilemap;//������ ���ڸ� ������ Ÿ�ϸ��� �Ҵ�
    public LayerMask SpawnableLayer;//������ ���ڰ� ������ �� �ִ� ���� (�ٴ� ��)�� ���̾� ����ũ

    [Header("������ ������ ���� �ð�")]
    public float ItemChestSpawnTime = 30f;//������ ���ڰ� �����Ǵ� �ֱ� (��)

    [Header("���� ����")]
    //���� �� �鸱 ���� ����
    public AudioSource AudioSource;
    public AudioClip SpawnSound;

    // == ���ο��� ����� ������ ==
    private float spawnTimer;//���� �ֱ� ���� Ÿ�̸�
    private TextAlimManager textalimManager;//TextAlimManager ��ũ��Ʈ�� ������ ����
    void Start()
    {
        spawnTimer = ItemChestSpawnTime;//���� �� �ٷ� ���� �ǵ��� �ʱ�ȭ

        //!= null�� "null�� �ƴ� ��" ��, "���𰡰� ������ ��"�� �ǹ��ϰ�,
        //== null�� "null�� ��" ��, "�ƹ��͵� ���ų� ������� ��"�� �ǹ���.

        //ItemBoxPrefab�� �Ҵ�Ǿ� �ִ��� Ȯ�� (���� ����)
        if (ItemChestPrefab == null)
            Debug.LogError("ItemBoxSpawner: ItemBoxPrefab�� �Ҵ���� �ʾҾ�! �ν����͸� Ȯ����!");

        //���� �� TextAlimManager ��ũ��Ʈ�� ã�Ƽ� �Ҵ�
        textalimManager = FindObjectOfType<TextAlimManager>();
        if (textalimManager == null)
            Debug.LogWarning("ItemChestSpawner: TextAlimManager��ũ��Ʈ�� ã�� �� ����!. ���� ���� �˸��� ǥ�õ��� �ʾ�!.");
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;//���� �ð� ����

        if (spawnTimer <= 0f)//Ÿ�̸Ӱ� 0���ϰ� �Ǹ� ����
        {
            SpawnItemChest();//������ ���� ���� �Լ� ȣ��
            spawnTimer = ItemChestSpawnTime;//���� ������ ���� Ÿ�̸� �ʱ�ȭ
        }

    }

    void SpawnItemChest()
    {
        //������ ���� �������� �Ҵ�Ǿ� �ִ��� �ٽ� �ѹ� Ȯ��
        if (ItemChestPrefab == null)
        {
            Debug.LogError("������ ���� �������� �Ҵ���� �ʾҾ�! �ν����͸� Ȯ����!.");
            return;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();//��ȿ�� ���� ��ġ ã��
        if(spawnPosition == Vector3.zero)//��ȿ�� ��ġ�� ã�� ��������
        {
            Debug.LogWarning("������ ���� ����: Ÿ�ϸ� ������ ��ȿ�� ���� ��ġ�� ã�� �� ������!.");
            return;
        }

        GameObject newItemChest = Instantiate(ItemChestPrefab, spawnPosition, Quaternion.identity);

        if (AudioSource != null && SpawnSound != null)//������ �鸱 ����
            AudioSource.PlayOneShot(SpawnSound);//�ѹ��� ���尡 �鸮�� �� �״�� OneShot

        if (textalimManager != null)//TextAlimManager ��ũ��Ʈ�� �ؽ�Ʈ �˸� ǥ��
            textalimManager.ShowNotification("<color=yellow>������ ���� �߰�!</color>");
    }

    Vector3 GetValidSpawnPosition()//��ȿ�� ���� ��ġ�� ã�� ���� �Լ� (EnemySpawn ��ũ��Ʈ�� ����)
    {
        int maxAttempts = 100;//��ȿ�� ���� ��ġ�� ã�� ���� �ִ� �õ� Ƚ��
        for(int i = 0; i < maxAttempts; i++)
        {
            if(TargetTilemap == null)
            {
                Debug.LogError("TargetTilemap�� ItemBoxSpawner�� �Ҵ���� �ʾҾ�!");
                return Vector3.zero;//��ȿ�� ��ġ �� ã���� Vector3.zero ��ȯ
            }

            BoundsInt bounds = TargetTilemap.cellBounds;//Ÿ�ϸ��� ��ȿ�� �� ���� ��������
            int randomX = Random.Range(bounds.xMin, bounds.xMax);
            int randomY = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int randomCell = new Vector3Int(randomX, randomY, 0);//���� �� ��ġ

            if(TargetTilemap.HasTile(randomCell))//���õ� ���� Ÿ���� �ִ��� Ȯ��
            {
                Vector3 cellCenterTile = TargetTilemap.GetCellCenterWorld(randomCell);//�� ��ġ�� ���� ��ǥ�� ��ȯ

                Collider2D[] colliders = Physics2D.OverlapCircleAll(cellCenterTile, 0.5f, SpawnableLayer);

                if (colliders.Length == 0) return cellCenterTile;//�ֺ��� �ݶ��̴��� ���ٸ� ��ȿ�� ��ġ
            }
        }
        Debug.LogWarning("������ ���� ����: Ÿ�ϸ� ������ ��ȿ�� ���� ��ġ�� 100�� �õ������� ã�� �� ������!.");
        return Vector3.zero;//100�� �õ��ص� �� ã���� Vector3.zero ��ȯ
    }
}

