using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        UpdateCounterUI();//시작할 때 초기화 (0/10 이런 식으로)
    }

    public void DeathCount(Enemy.EnemyType type)//타입별 몬스터 사망시 카운트
    {
        //현재 씬이 3번 씬(무한 모드)인지 체크
        bool isInfiniteMode = SceneManager.GetActiveScene().name == "GameScene3";

        switch (type)
        {
            case Enemy.EnemyType.Normal:
                if (isInfiniteMode) normalKills++;//무한 모드면 그냥 증가
                else normalKills = Mathf.Min(normalKills + 1, reqNormal);//아니면 제한
                break;

            case Enemy.EnemyType.Strong:
                if (isInfiniteMode) strongKills++;
                else strongKills = Mathf.Min(strongKills + 1, reqStrong);
                break;

            case Enemy.EnemyType.Elite:
                if (isInfiniteMode) eliteKills++;
                else eliteKills = Mathf.Min(eliteKills + 1, reqElite);
                break;
        }
        UpdateCounterUI();//숫자가 오를 때마다 UI 갱신
    }

    private void UpdateCounterUI()//화면에 처치 현황을 그려주는 함수
    {
        if (counterText == null) return;

        //현재 활성화된 씬의 이름이나 인덱스를 확인
        if (SceneManager.GetActiveScene().name == "GameScene3")
        {
            //챌린지 모드: 목표치 없이 현재 처치 수만 표시
            counterText.text = string.Format(
                "Normal: {0}\nStrong: {1}\nElite: {2}",
                normalKills, strongKills, eliteKills
            );//무한 모드니까 텍스트 색상은 흰색(기본)
        }
        else
        {
            //기존 1, 2번 씬 로직: 목표치와 색상 변경 포함
            string nColor = (normalKills >= reqNormal) ? "<color=#00FF00>" : "<color=#FFFFFF>";
            string sColor = (strongKills >= reqStrong) ? "<color=#00FF00>" : "<color=#FFFFFF>";
            string eColor = (eliteKills >= reqElite) ? "<color=#00FF00>" : "<color=#FFFFFF>";

            counterText.text = string.Format(
                "{0}Normal: {1}/{2}</color>\n{3}Strong: {4}/{5}</color>\n{6}Elite: {7}/{8}</color>",
                nColor, normalKills, reqNormal,
                sColor, strongKills, reqStrong,
                eColor, eliteKills, reqElite
            );
        }
    }
    public bool IsMissionComplete()//조건(몬스터 일정 수 처치)을 만족했는지 알려주는 함수
    {                              //GameTimerUI 스크립트의 Update함수에서 사용
        return normalKills >= reqNormal &&
               strongKills >= reqStrong &&
               eliteKills >= reqElite;
    }
}
