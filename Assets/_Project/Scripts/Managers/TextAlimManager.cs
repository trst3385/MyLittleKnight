using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;//TextMeshPro를 사용하려면 이 using 문이 필요


public class TextAlimManager : MonoBehaviour
{
    public static TextAlimManager Instance { get; private set; }//싱글톤 선언

    [Header("UI 연결(자동으로 찾음)")]
    [SerializeField] private TextMeshProUGUI monsterAlimText;//인스펙터에서 UI 텍스트 컴포넌트를 할당할 변수
    [SerializeField] private TextMeshProUGUI levelText;//몬스터 강화 레벨 표시
    [SerializeField] private TextMeshProUGUI itemAlimText;//아이템 상자 스폰 알림
    [SerializeField] private TextMeshProUGUI bossAlimText;//보스 등장 알림

    [Header("알림이 표시, 사라질 시간")]
    public float DisplayDuration = 2f;//알림이 화면에 표시될 시간 (초)
    public float FadeDuration = 0.5f;//알림이 사라질 때 페이드아웃 되는 시간 (초)

    private Coroutine currentNotificationCoroutine;//현재 실행 중인 코루틴을 추적


    void Awake()
    {
        if (Instance == null) Instance = this;//싱글톤 설정
        else { Destroy(gameObject); return; }

        //자동 연결
        if (monsterAlimText == null)
            monsterAlimText = GameObject.Find("EnemyDifficultyStatsText")?.GetComponent<TextMeshProUGUI>();
        if (levelText == null)
            levelText = GameObject.Find("EnemyDifficultyLevelText")?.GetComponent<TextMeshProUGUI>();
        if (itemAlimText == null)
            itemAlimText = GameObject.Find("ItemTextAlim")?.GetComponent<TextMeshProUGUI>();
        if (bossAlimText == null)
            bossAlimText = GameObject.Find("BossAlimText")?.GetComponent<TextMeshProUGUI>();

        //초기화 (처음에는 모두 투명하게)
        InitText(monsterAlimText);
        InitText(itemAlimText);
        InitText(bossAlimText);
    }

    private void InitText(TextMeshProUGUI txt)
    {
        if (txt != null)
        {
            txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, 0);
            txt.text = "";
        }
    }

    //여기서 직접 방송을 구독
    private void OnEnable()//몬스터 강화 알림, 레벨 알림, 아이템 상자 스폰 알림 방송 구독
    {
        EnemyDifficulty.OnDifficultyNotification += ShowMonsterNotification;
        EnemyDifficulty.OnMonsterLevelUp += UpdateLevelUI;
        ItemChestSpawner.OnItemSpawned += ShowItemNotification;//아이템 상자 스폰 방송 구독
    }
    private void OnDisable()//안전하게 해제
    {
        EnemyDifficulty.OnDifficultyNotification -= ShowMonsterNotification;
        EnemyDifficulty.OnMonsterLevelUp -= UpdateLevelUI;
        ItemChestSpawner.OnItemSpawned -= ShowItemNotification;
    }



    //---몬스터 강화 알림 (전용 코루틴)---
    public void ShowMonsterNotification(string message)
    {
        if (monsterAlimText != null) 
        {
            StartCoroutine(DisplayFade(monsterAlimText, message));
        } 
    }

    //---아이템 등장 알림 (전용 코루틴)---
    public void ShowItemNotification(string message)//ItemChestSpawner에서 이걸 호출
    {
        if (itemAlimText != null)
        {
            StartCoroutine(DisplayFade(itemAlimText, message));
        }
    }

    private void UpdateLevelUI(int level)//레벨 숫자를 바꿔주는 함수
    {
        if (levelText != null)
        {
            levelText.text = $"몬스터 Lv.{level}";
        }
    }

    public void ShowBossNotification(string message)//보스 등장 알림 전용 함수 (강렬한 연출을 위해 DisplayFade 재사용)
    {
        if (bossAlimText != null)
        {
            // 웅아, 보스는 특별하니까 코루틴을 돌리기 전에 기존 연출을 멈추거나 
            // 텍스트 크기를 살짝 키우는 코드를 넣어도 좋아.
            StartCoroutine(DisplayFade(bossAlimText, message));
        }
    }

    //공용 페이드 코루틴 (어떤 텍스트든 받아서 연출해줌)
    IEnumerator DisplayFade(TextMeshProUGUI target, string message)
    {
        target.text = message;

        //페이드 인
        float timer = 0f;
        while (timer < FadeDuration)
        {
            timer += Time.deltaTime;
            target.color = new Color(target.color.r, target.color.g, target.color.b, timer / FadeDuration);
            yield return null;
        }
        target.color = new Color(target.color.r, target.color.g, target.color.b, 1);

        yield return new WaitForSeconds(DisplayDuration);

        //페이드 아웃
        timer = 0f;
        while (timer < FadeDuration)
        {
            timer += Time.deltaTime;
            target.color = new Color(target.color.r, target.color.g, target.color.b, 1 - (timer / FadeDuration));
            yield return null;
        }
        target.color = new Color(target.color.r, target.color.g, target.color.b, 0);
        target.text = "";
    }
}
