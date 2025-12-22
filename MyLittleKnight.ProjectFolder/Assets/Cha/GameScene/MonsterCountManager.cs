using TMPro;
using UnityEngine;

public class MonsterCountManager : MonoBehaviour
{
    public static MonsterCountManager Instance;//어디서든 접근 가능하게 싱글톤 설정

    [Header("현재 처치 수")]//private으로 해도되지만 얼마를 죽였는지 인스펙터로도 확인할 수 있게
    public int normalKills;
    public int strongKills;
    public int eliteKills;

    [Header("클리어 조건 (목표치)")]//인스펙터에서 설정(이 횟수를 채워야 다음 스테이지로 갈 포탈 생성, 1분 이후)
    public int reqNormal;
    public int reqStrong;
    public int reqElite;

    [Header("MonsterCounterText UI 연결")]
    [SerializeField] private TextMeshProUGUI counterText;//화면에 표시될 텍스트

    void Awake()
    {
        if (Instance == null) Instance = this;//싱글톤으로 설정 (어디서든 접근 가능하게)
        else Destroy(gameObject);
    }
    void Start()
    {
        UpdateCounterUI(); // 시작할 때 초기화 (0/10 이런 식으로)
    }

    public void DeathCount(Enemy.EnemyType type)//타입별 몬스터 사망시 카운트
    {
        switch (type)
        {   //Mathf.Min(현재값 + 1, 목표값)을 사용하면 목표값을 절대 넘지 않아  
            case Enemy.EnemyType.Normal:
                normalKills = Mathf.Min(normalKills + 1, reqNormal);
                break;

            case Enemy.EnemyType.Strong:
                strongKills = Mathf.Min(strongKills + 1, reqStrong);
                break;

            case Enemy.EnemyType.Elite:
                eliteKills = Mathf.Min(eliteKills + 1, reqElite);
                break;
        }
        UpdateCounterUI();//숫자가 오를 때마다 UI 갱신
    }

    private void UpdateCounterUI()//화면에 처치 현황을 그려주는 함수
    {
        if (counterText == null) return;

        //각 몬스터별로 조건을 다 채웠는지 확인해서 색상 태그 입히기(조건식 ? 참 일때 값, : 거짓일_때_값 삼항 연산자)
        string nColor = (normalKills >= reqNormal) ? "<color=#00FF00>" : "<color=#FFFFFF>";
        string sColor = (strongKills >= reqStrong) ? "<color=#00FF00>" : "<color=#FFFFFF>";
        string eColor = (eliteKills >= reqElite) ? "<color=#00FF00>" : "<color=#FFFFFF>";

        // 문자열 형식을 만들어줌
        counterText.text = string.Format(
        "{0}Normal: {1}/{2}</color>\n{3}Strong: {4}/{5}</color>\n{6}Elite: {7}/{8}</color>",
        nColor, normalKills, reqNormal,
        sColor, strongKills, reqStrong,
        eColor, eliteKills, reqElite
        );
    }

    public bool IsMissionComplete()//조건(몬스터 일정 수 처치)을 만족했는지 알려주는 함수
    {                              //GameTimerUI 스크립트의 Update함수에서 사용
        return normalKills >= reqNormal &&
               strongKills >= reqStrong &&
               eliteKills >= reqElite;
    }
}
