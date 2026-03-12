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
    [Header("상자 설정")]
    public Sprite OpenChestSprite;//상자가 열렸을 때 보여줄 '이미지 파일' 변수
    public GameObject[] ItemPrefabs;//이 상자에서 나올 아이템 프리팹들. 인스펙터의 itemPrefabs에 넣어둔 아이템 오브젝트들이야
    public Transform[] ItemSpawnPoints;//여러 스폰 포인트를 담을 배열 변수
    [SerializeField] private AudioClip openSound;//상자가 열리는 사운드

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider2D;
    private bool isOpen = false;//상자가 열렸는지를 추적하는 변수, 플레이어가 닿기 전에는 false

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider2D = GetComponent<BoxCollider2D>();

        //상자에 닿을때 열린 상자 이미지가 제대로 들어있는지 확인(디버깅용)
        if (OpenChestSprite == null ) Debug.Log("OpenChest 스프라이트가 할당되지 않음! 인스팩터를 확인하라고!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {   //상자가 아직 열리지 않았고, 닿은 오브젝트의 태그가 "Player"인지 확인
        //OnTriggerEnter2D 함수가 작동 될려면 Box Collider 2D가 Is Trigger로 켜져 있어야 해
        if (!isOpen && other.CompareTag("Player")) OpenChest();//조건을 만족하면 상자를 여는 함수 호출
    }

    void OpenChest()
    {
        isOpen = true;//상자가 열린 상태로 변경

        //1.OpenChest함수가 발동되면 openedChestSprite(열린 상자)로 교체
        if (OpenChestSprite != null) spriteRenderer.sprite = OpenChestSprite;
        //1-2.콜라이더 비활성화. 열린 상자는 더 이상 플레이어와 충돌(트리거)할 필요 없으므로 콜라이더 비활성화
        if (boxCollider2D != null) boxCollider2D.enabled = false;

        if (openSound != null && SoundManager.Instance != null)//사운드 매니저 싱글톤 활용
            SoundManager.Instance.PlaySFX(openSound);//상자에 닿을때의 사운드 재생

        SpawnItems();//2.여러 아이템 스폰 함수 호출
        Destroy(gameObject, 5f);//아이템 획득 후 5초 후 아이템상자 오브젝트 파괴

    }

    void SpawnItems()
    {   //넷 중 하나라도 비어있으면 원인과 함께 로그를 남기고 종료
        if (ItemPrefabs == null || ItemPrefabs.Length == 0)
        {
            Debug.LogError($"{gameObject.name}: ItemPrefabs가 비어있어!");
            return;
        }

        if (ItemSpawnPoints == null || ItemSpawnPoints.Length == 0)
        {
            Debug.LogError($"{gameObject.name}: ItemSpawnPoints가 설정되지 않았어!");
            return;
        }

        List<Transform> availablePoints = new List<Transform>(ItemSpawnPoints);
        int spawnCount = UnityEngine.Random.Range(2, 4);//2개 또는 3개 아이템 생성

        spawnCount = Mathf.Min(spawnCount, availablePoints.Count);//포인트 개수보다 많이 스폰할 수 없게 방어
        for (int i = 0; i < spawnCount; i++)
        {
            int pointIdx = UnityEngine.Random.Range(0, availablePoints.Count);
            int itemIdx = UnityEngine.Random.Range(0, ItemPrefabs.Length);

            Instantiate(ItemPrefabs[itemIdx], availablePoints[pointIdx].position, Quaternion.identity);
            availablePoints.RemoveAt(pointIdx);//중복 위치 방지
        }
    }       
}








