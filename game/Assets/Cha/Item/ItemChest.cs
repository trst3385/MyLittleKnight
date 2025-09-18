using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
//유니티와 C# 기본 라이브러리에도 Random이란 이름이 있어 모호성때문에 오류가 남,
//어떤 Random인지 명확하게 지정하기 위해 추가. 다른 방법으로 메서드 Random 옆에 UnityEngine.사용

public class ItemChest : MonoBehaviour
{
    //여기 변수엔 [Header("")]를 쓰지않아. 변수가 []을 써서 각 변수를 따로 구별이 가능해
    public Sprite OpenChestSprite;//상자가 열렸을 때 보여줄 '이미지 파일' 변수
    public GameObject[] ItemPrefabs;//이 상자에서 나올 아이템 프리팹들. 인스펙터의 itemPrefabs에 넣어둔 아이템 오브젝트들이야
    public Transform[] ItemSpawnPoints;//여러 스폰 포인트를 담을 배열 변수
    

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;
    private bool isOpen = false;//상자가 열렸는지를 추적하는 변수, 플레이어가 닿기 전에는 false

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();


        //Inspector에 필요한 public 변수들이 제대로 연결되었는지 확인하는 디버그 로그
        if (OpenChestSprite == null )
            Debug.Log("OpenChest 스프라이트가 할당되지 않음! 인스팩터를 확인하라고!");

        //아이템 프리팹 배열이 비어있는지 확인
        if (ItemPrefabs == null || ItemPrefabs.Length == 0)
            Debug.LogWarning("ItemPrefabs 배열에 아이템이 할당되지 않았어! 상자에서 아이템이 나오지 않아!.");

        //아이템 스폰 포인트를 따로 지정하지 않았으면 상자 자신의 위치를 사용
        if (ItemSpawnPoints == null || ItemSpawnPoints.Length == 0 )
            Debug.Log("ItemChest: ItemSpawnPoints가 할당되지 않았거나 비어있어! 상자 위치에 스폰할게!");
        //&& 논리연산 = AND 연산은 모든 조건이 true 인 경우 해당 연산이 true가 되는 연산.
        //|| 논리연산 = OR 연산은 한 조건이라도 true이면 true를 반환하는 조건.



    }

    private void OnTriggerEnter2D(Collider2D other)
    {   //상자가 아직 열리지 않았고, 닿은 오브젝트의 태그가 "Player"인지 확인
        //OnTriggerEnter2D 함수가 작동 될려면 Box Collider 2D가 Is Trigger로 켜져 있어야 해
        if (!isOpen && other.CompareTag("Player"))
            OpenChest();//조건을 만족하면 상자를 여는 함수 호출
    }

    void OpenChest()
    {
        isOpen = true;//상자가 열린 상태로 변경

        //1.OpenChest함수가 발동되면 openedChestSprite(열린 상자)로 교체
        if (OpenChestSprite != null)
            spriteRenderer.sprite = OpenChestSprite;

        //1-2.콜라이더 비활성화. 열린 상자는 더 이상 플레이어와 충돌(트리거)할 필요 없으므로 콜라이더 비활성화
        if (boxCollider2D != null)
            boxCollider2D.enabled = false;

        //2.여러 아이템 스폰 함수 호출
        SpawnItems();
      
        Destroy(gameObject, 5f);//아이템 획득 후 5초 후 아이템상자 오브젝트 파괴

    }

    void SpawnItems()
    {
        if(ItemPrefabs == null || ItemPrefabs.Length == 0)
        {
            Debug.LogWarning("ItemChest: 스폰할 아이템 프리팹이 없어!");
            return;        
        }
        //ItemSpawnPoints가 할당되지 않았거나 비어있으면 경고
        if (ItemSpawnPoints == null || ItemSpawnPoints.Length == 0)
        {
            Debug.LogError("ItemChest: 아이템 스폰 포인트가 할당되지 않았어! 아이템을 스폰할 수 없어!");
            return;
        }

        //ItemSpawnPoints 배열을 복사해서 '사용 가능한' 스폰 포인트 리스트를 만들어,
        //리스트는 배열과 달리 요소를 자유롭게 추가하고 제거할 수 있어서,
        //한 번 사용한 스폰 포인트를 리스트에서 제거하기에 편리해.
        List<Transform> availableSpawnPoints = new List<Transform>(ItemSpawnPoints);

        //2개 또는 3개의 아이템을 랜덤으로 스폰
        int randomItems = Random.Range(2, 4);//Random.Range(min, max)에서 max는 포함되지 않아 (2, 3이 나옴)

        //스폰할 아이템의 개수가 스폰 포인트 개수보다 많으면 오류 방지
        //스폰 포인트가 2개인데 3개를 스폰하려고 하면 문제가 되므로, 개수를 제한
        if (randomItems > availableSpawnPoints.Count)
            randomItems = availableSpawnPoints.Count;

        //Random 옆에UnityEngine. 적거나 using으로 using Random = UnityEngine.Random; 선언해야 오류안뜸!
        for (int i = 0; i < randomItems; i++)//랜덤으로 아이템 2~3개가 등장
        {
            //남아있는 스폰 포인트 리스트의 범위 안에서 무작위 인덱스 선택,
            //이렇게 하면 이미 사용된 스폰 포인트는 다시 선택되지 않아.
            int randomSpawnPointIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform selectedSpawnPoint = availableSpawnPoints[randomSpawnPointIndex];

            //스폰할 아이템 종류를 itemPrefabs 배열의 길이만큼 랜덤으로 선택
            //이렇게 해야 모든 종류의 아이템이 나올 확률이 생겨
            int randomItemIndex = Random.Range(0, ItemPrefabs.Length);
            GameObject itemToSpawn = ItemPrefabs[randomItemIndex];

            //선택된 스폰 포인트 위치에 아이템 생성
            Instantiate(itemToSpawn, selectedSpawnPoint.position, Quaternion.identity);

            //아이템을 스폰한 위치는 리스트에서 제거해서 다시 선택되지 않게 하기
            availableSpawnPoints.RemoveAt(randomSpawnPointIndex);
        }
    }
        
}








