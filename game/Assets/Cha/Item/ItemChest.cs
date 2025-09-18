using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
//����Ƽ�� C# �⺻ ���̺귯������ Random�̶� �̸��� �־� ��ȣ�������� ������ ��,
//� Random���� ��Ȯ�ϰ� �����ϱ� ���� �߰�. �ٸ� ������� �޼��� Random ���� UnityEngine.���

public class ItemChest : MonoBehaviour
{
    //���� ������ [Header("")]�� �����ʾ�. ������ []�� �Ἥ �� ������ ���� ������ ������
    public Sprite OpenChestSprite;//���ڰ� ������ �� ������ '�̹��� ����' ����
    public GameObject[] ItemPrefabs;//�� ���ڿ��� ���� ������ �����յ�. �ν������� itemPrefabs�� �־�� ������ ������Ʈ���̾�
    public Transform[] ItemSpawnPoints;//���� ���� ����Ʈ�� ���� �迭 ����
    

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;
    private bool isOpen = false;//���ڰ� ���ȴ����� �����ϴ� ����, �÷��̾ ��� ������ false

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();


        //Inspector�� �ʿ��� public �������� ����� ����Ǿ����� Ȯ���ϴ� ����� �α�
        if (OpenChestSprite == null )
            Debug.Log("OpenChest ��������Ʈ�� �Ҵ���� ����! �ν����͸� Ȯ���϶��!");

        //������ ������ �迭�� ����ִ��� Ȯ��
        if (ItemPrefabs == null || ItemPrefabs.Length == 0)
            Debug.LogWarning("ItemPrefabs �迭�� �������� �Ҵ���� �ʾҾ�! ���ڿ��� �������� ������ �ʾ�!.");

        //������ ���� ����Ʈ�� ���� �������� �ʾ����� ���� �ڽ��� ��ġ�� ���
        if (ItemSpawnPoints == null || ItemSpawnPoints.Length == 0 )
            Debug.Log("ItemChest: ItemSpawnPoints�� �Ҵ���� �ʾҰų� ����־�! ���� ��ġ�� �����Ұ�!");
        //&& ������ = AND ������ ��� ������ true �� ��� �ش� ������ true�� �Ǵ� ����.
        //|| ������ = OR ������ �� �����̶� true�̸� true�� ��ȯ�ϴ� ����.



    }

    private void OnTriggerEnter2D(Collider2D other)
    {   //���ڰ� ���� ������ �ʾҰ�, ���� ������Ʈ�� �±װ� "Player"���� Ȯ��
        //OnTriggerEnter2D �Լ��� �۵� �ɷ��� Box Collider 2D�� Is Trigger�� ���� �־�� ��
        if (!isOpen && other.CompareTag("Player"))
            OpenChest();//������ �����ϸ� ���ڸ� ���� �Լ� ȣ��
    }

    void OpenChest()
    {
        isOpen = true;//���ڰ� ���� ���·� ����

        //1.OpenChest�Լ��� �ߵ��Ǹ� openedChestSprite(���� ����)�� ��ü
        if (OpenChestSprite != null)
            spriteRenderer.sprite = OpenChestSprite;

        //1-2.�ݶ��̴� ��Ȱ��ȭ. ���� ���ڴ� �� �̻� �÷��̾�� �浹(Ʈ����)�� �ʿ� �����Ƿ� �ݶ��̴� ��Ȱ��ȭ
        if (boxCollider2D != null)
            boxCollider2D.enabled = false;

        //2.���� ������ ���� �Լ� ȣ��
        SpawnItems();
      
        Destroy(gameObject, 5f);//������ ȹ�� �� 5�� �� �����ۻ��� ������Ʈ �ı�

    }

    void SpawnItems()
    {
        if(ItemPrefabs == null || ItemPrefabs.Length == 0)
        {
            Debug.LogWarning("ItemChest: ������ ������ �������� ����!");
            return;        
        }
        //ItemSpawnPoints�� �Ҵ���� �ʾҰų� ��������� ���
        if (ItemSpawnPoints == null || ItemSpawnPoints.Length == 0)
        {
            Debug.LogError("ItemChest: ������ ���� ����Ʈ�� �Ҵ���� �ʾҾ�! �������� ������ �� ����!");
            return;
        }

        //ItemSpawnPoints �迭�� �����ؼ� '��� ������' ���� ����Ʈ ����Ʈ�� �����,
        //����Ʈ�� �迭�� �޸� ��Ҹ� �����Ӱ� �߰��ϰ� ������ �� �־,
        //�� �� ����� ���� ����Ʈ�� ����Ʈ���� �����ϱ⿡ ����.
        List<Transform> availableSpawnPoints = new List<Transform>(ItemSpawnPoints);

        //2�� �Ǵ� 3���� �������� �������� ����
        int randomItems = Random.Range(2, 4);//Random.Range(min, max)���� max�� ���Ե��� �ʾ� (2, 3�� ����)

        //������ �������� ������ ���� ����Ʈ �������� ������ ���� ����
        //���� ����Ʈ�� 2���ε� 3���� �����Ϸ��� �ϸ� ������ �ǹǷ�, ������ ����
        if (randomItems > availableSpawnPoints.Count)
            randomItems = availableSpawnPoints.Count;

        //Random ����UnityEngine. ���ų� using���� using Random = UnityEngine.Random; �����ؾ� �����ȶ�!
        for (int i = 0; i < randomItems; i++)//�������� ������ 2~3���� ����
        {
            //�����ִ� ���� ����Ʈ ����Ʈ�� ���� �ȿ��� ������ �ε��� ����,
            //�̷��� �ϸ� �̹� ���� ���� ����Ʈ�� �ٽ� ���õ��� �ʾ�.
            int randomSpawnPointIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform selectedSpawnPoint = availableSpawnPoints[randomSpawnPointIndex];

            //������ ������ ������ itemPrefabs �迭�� ���̸�ŭ �������� ����
            //�̷��� �ؾ� ��� ������ �������� ���� Ȯ���� ����
            int randomItemIndex = Random.Range(0, ItemPrefabs.Length);
            GameObject itemToSpawn = ItemPrefabs[randomItemIndex];

            //���õ� ���� ����Ʈ ��ġ�� ������ ����
            Instantiate(itemToSpawn, selectedSpawnPoint.position, Quaternion.identity);

            //�������� ������ ��ġ�� ����Ʈ���� �����ؼ� �ٽ� ���õ��� �ʰ� �ϱ�
            availableSpawnPoints.RemoveAt(randomSpawnPointIndex);
        }
    }
        
}








