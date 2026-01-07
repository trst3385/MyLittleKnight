using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public float targetTime = 60f;
    public GameObject portalObject;//씬에 비활성화된 포탈을 연결

    private bool isActivated = false;

    void Update()
    {
        if (isActivated) return;

        //시간과 몬스터 체크 (SRP 준수)
        bool timeReached = false;
        GameTimerUI timer = Object.FindAnyObjectByType<GameTimerUI>();
        if (timer != null) timeReached = timer.GetElapsedTime() >= targetTime;

        bool missionOk = (MonsterCountManager.Instance != null) && MonsterCountManager.Instance.IsMissionComplete();

        //조건 충족 시 포탈 오브젝트를 활성화
        if (timeReached && missionOk)
        {
            isActivated = true;
            if (portalObject != null) portalObject.SetActive(true);
            Debug.Log("★★★ 매니저가 포탈을 활성화함! ★★★");
        }
    }
}
