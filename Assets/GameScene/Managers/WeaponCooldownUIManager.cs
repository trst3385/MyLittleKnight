using UnityEngine;
using UnityEngine.UI;

public class WeaponCooldownUIManager : MonoBehaviour
{
    [Header("활 쿨타임 UI 설정")]//[Header]를 써서 인스펙터에서도 상태를 확인할 수 있게 하자
    private Slider bowSlider;//활 쿨타임 UI 바

    void Awake()
    {
        GameObject uiObj = GameObject.Find("BowCooldownBar");//이름으로 UI 오브젝트를 찾아서 슬라이더 연결

        if (uiObj != null)
        {
            bowSlider = uiObj.GetComponent<Slider>();
            bowSlider.gameObject.SetActive(false);//시작할 때는 UI를 꺼둔다 (데이터(활 공격)가 들어오면 켜질 거야)
        }
        else
            Debug.LogWarning("WeaponCooldownUIManager: 'BowCooldownBar' 오브젝트를 찾을 수 없어!");
    }

    private void OnEnable()
    {
        //[옵저버] BowWeapon 스크립트 방송국 채널 구독
        BowWeapon.OnBowCooldownChanged += UpdateBowSlider;
    }
    private void OnDisable()
    {
        //[옵저버] 구독 해지 (안전한 종료)
        BowWeapon.OnBowCooldownChanged -= UpdateBowSlider;
    }

    private void UpdateBowSlider(float remaining, float total)// 4. 방송 수신(활 공격 감시) 시 실행될 로직
    {
        if (bowSlider == null) return;

        if (remaining > 0)
        {
            if (!bowSlider.gameObject.activeSelf) bowSlider.gameObject.SetActive(true);//쿨타임 중이면 UI바를 활성화하고 값 갱신

            bowSlider.maxValue = total;
            bowSlider.value = remaining;
        }
        else
            bowSlider.gameObject.SetActive(false);//쿨타임이 끝나면 비활성화
    }
}
