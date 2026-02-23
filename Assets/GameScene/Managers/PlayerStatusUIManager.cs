using UnityEngine;
using UnityEngine.UI;//체력, 방어력 슬라이더 조작을 위해 필요!

public class PlayerStatusUIManager : MonoBehaviour
{
    [Header("연결될 UI 요소들")]
    private Slider hpBar;
    private Slider shieldBar;

    void Awake()
    {
        //1. [자동 연결] 씬에서 이름으로 UI 오브젝트를 찾아 Slider 컴포넌트 연결
        GameObject hpObj = GameObject.Find("PlayerHealthBar");
        if (hpObj != null) hpBar = hpObj.GetComponent<Slider>();

        GameObject sdObj = GameObject.Find("PlayerShieldBar");
        if (sdObj != null) shieldBar = sdObj.GetComponent<Slider>();

        //방어적 프로그래밍: 못 찾았으면 로그로 알려주기
        if (hpBar == null) Debug.LogWarning("PlayerStatusUIManager: 'PlayerHealthBar'를 찾을 수 없어!");
        if (shieldBar == null) Debug.LogWarning("PlayerStatusUIManager: 'PlayerShieldBar'를 찾을 수 없어!");
    }

    private void OnEnable()
    {
        //2. [Observer.이벤트 구독]  (체력, 방어력 상태 변화 모니터링 시작)
        //Action<float, float> 형태이므로 받는 함수도 인자가 두 개(current, max)여야 해.
        PlayerHealth.OnHealthChanged += UpdateHealthUI;
        PlayerShield.OnShieldChanged += UpdateShieldUI;
    }

    private void OnDisable()
    {
        //3. [Observer.구독 해지] 오브젝트가 꺼질 때 이벤트 구독 해제 (메모리 누수 및 Null 에러 방지)
        //연결이 끊어졌을 때(정확히는 오브젝트가 비활성화되거나 파괴될 때)
        PlayerHealth.OnHealthChanged -= UpdateHealthUI;
        PlayerShield.OnShieldChanged -= UpdateShieldUI;
    }

    private void UpdateHealthUI(float currentHealth, float maxHealth)//5. [Callback] 이벤트 발생 시 체력UI 업데이트를 수행하는 콜백 함수
    {
        if (hpBar != null)
        {
            hpBar.maxValue = maxHealth;//최대치 동기화
            hpBar.value = currentHealth;//현재치 반영
        }
    }
    private void UpdateShieldUI(float currentShield, float maxShield)//5. [Callback] 이벤트 발생 시 방어력UI 업데이트를 수행하는 콜백 함수
    {
        if (shieldBar != null)
        {
            shieldBar.maxValue = maxShield;
            shieldBar.value = currentShield;
        }
    }
}
