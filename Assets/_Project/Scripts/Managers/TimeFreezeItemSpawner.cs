using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TimeFreezeItemSpawner : MonoBehaviour
{
    public static TimeFreezeItemSpawner Instance { get; private set; }//싱글톤 선언

    [Header("프리팹 및 설정")]
    [SerializeField] private GameObject timeFreezeItem;//씬에 미리 배치된 시간 정지 아이템
    [SerializeField] private float spawnInterval = 20f;//20초 마다 아이템 생성

    [Header("등장 사운드")]
    [SerializeField] private AudioClip itemSpawnSFX;//등장 사운드


    //내부에서 사용할 변수들
    private float spawnTimer;
    private bool isItemActive = false;//현재 아이템이 활성화 중인지

    void Awake()
    {
        if (Instance == null) Instance = this;//싱글톤 설정
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        spawnTimer = spawnInterval;

        if (timeFreezeItem == null)//아이템이 인스펙터에 연결 안 되어 있으면 태그로 찾기
            timeFreezeItem = GameObject.FindWithTag("TimeFreezeItem");

        if (timeFreezeItem != null)//시작할 때는 시간정지 아이템 꺼두기(비활성화)
            timeFreezeItem.SetActive(false);//여기서 비활성화 처리, 하이어라키에선 시간정지 아이템은 활성화 시켜놔야해!
    }

    void Update()
    {
        //시간 정지 중에는 스폰 타이머도 멈춤
        if (TimeFreeze.Instance != null && TimeFreeze.Instance.IsTimeFrozen) return;

        spawnTimer -= Time.deltaTime;//타이머는 아이템 상태와 상관없이 항상 흐름 (20초 주기 유지)

        if (spawnTimer <= 0f)
        {
            //20초가 되었을 때 아이템이 꺼져 있는 상태라면 활성화
            if (!isItemActive) ActivateItem();
            else Debug.Log("TimeFreezeItem: 이미 아이템이 존재해서 이번 주기는 건너뛰어!");

            spawnTimer = spawnInterval;//아이템을 썻든 안 썻든 타이머는 다시 리셋 (다음 20초를 위해)
        }
    }

    private void ActivateItem()//시간 정지 아이템 활성화
    {
        if (timeFreezeItem == null) return;

        isItemActive = true;
        timeFreezeItem.SetActive(true);

        if (itemSpawnSFX != null && SoundManager.Instance != null)//등장 사운드 재생
            SoundManager.Instance.PlaySFX(itemSpawnSFX);

        Debug.Log("★★★ 시간 정지 아이템 등장! ★★★");
    }

    public void OnItemPickedUp()// 아이템을 획득했을 때(TimeFreezeItem 스크립트에서 호출)
    {
        isItemActive = false;
        if (timeFreezeItem != null)
            timeFreezeItem.SetActive(false);

        Debug.Log("시간 정지 아이템 획득 완료.");
    }
}
