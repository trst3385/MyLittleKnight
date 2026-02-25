using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponCooldownUIManager : MonoBehaviour
{
    [Header("활 쿨타임 UI 설정")]//[Header]를 써서 인스펙터에서도 상태를 확인할 수 있게 하자
    private Slider bowSlider;//활 쿨타임 UI 바

    [Header("검 쿨타임 UI 설정")]
    private Image swordCooldownOverlay;//검 아이콘
    private TextMeshProUGUI swordCooldownText;//검 쿨타임 숫자 텍스트

    [Header("무적 스킬 UI 설정")]
    private Image invincibleOverlay;
    private TextMeshProUGUI invincibleText;

    void Awake()
    {
        GameObject uiObj = GameObject.Find("BowCooldownBar");//이름으로 UI 오브젝트를 찾아서 슬라이더 연결
        if (uiObj != null)
        {
            bowSlider = uiObj.GetComponent<Slider>();
            bowSlider.gameObject.SetActive(false);//시작할 때는 UI를 꺼둔다 (데이터(활 공격)가 들어오면 켜질 거야)
        }
        else Debug.LogWarning("WeaponCooldownUIManager: 'BowCooldownBar' 오브젝트를 찾을 수 없어!");

        GameObject swordIconObj = GameObject.Find("SwordSkillIcon");//검 UI 연결 (SwordSkillIcon의 자식들 찾기)
        if (swordIconObj != null)
        {
            Transform overlay = swordIconObj.transform.Find("CooldownOverlay");
            if (overlay != null) swordCooldownOverlay = overlay.GetComponent<Image>();

            Transform text = swordIconObj.transform.Find("CooldownText");
            if (text != null) swordCooldownText = text.GetComponent<TextMeshProUGUI>();

            //초기 상태: 쿨타임 UI 숨김
            if (swordCooldownOverlay != null) swordCooldownOverlay.fillAmount = 0;
            if (swordCooldownText != null) swordCooldownText.gameObject.SetActive(false);
        }

        GameObject invincIconObj = GameObject.Find("InvincibleSkillIcon");//무적 스킬 UI 연결(InvincibleSkillIcon의 자식들 찾기)
        if (invincIconObj != null)
        {
            Transform overlay = invincIconObj.transform.Find("CooldownOverlay");
            if (overlay != null) invincibleOverlay = overlay.GetComponent<Image>();

            Transform text = invincIconObj.transform.Find("CooldownText");
            if (text != null) invincibleText = text.GetComponent<TextMeshProUGUI>();

            if (invincibleOverlay != null) invincibleOverlay.fillAmount = 0;
            if (invincibleText != null) invincibleText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        //[옵저버] BowWeapon 스크립트 방송국 채널 구독
        BowWeapon.OnBowCooldownChanged += UpdateBowSlider;
        SwordWeapon.OnSwordCooldownChanged += UpdateSwordCooldownUI;
        InvincibilitySkill.OnInvincibleCooldownChanged += UpdateInvincibleUI;
    }
    private void OnDisable()
    {
        //[옵저버] 구독 해지 (안전한 종료)
        BowWeapon.OnBowCooldownChanged -= UpdateBowSlider;
        SwordWeapon.OnSwordCooldownChanged -= UpdateSwordCooldownUI;
        InvincibilitySkill.OnInvincibleCooldownChanged -= UpdateInvincibleUI;
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
        else bowSlider.gameObject.SetActive(false);//쿨타임이 끝나면 비활성화
    }

    private void UpdateSwordCooldownUI(float remaining, float total)//검 UI 갱신
    {
        if (swordCooldownOverlay == null || swordCooldownText == null) return;

        if (remaining > 0)
        {
            //쿨타임 중일 때
            swordCooldownText.gameObject.SetActive(true);
            swordCooldownText.text = remaining.ToString("F1");

            swordCooldownOverlay.gameObject.SetActive(true);
            swordCooldownOverlay.fillAmount = remaining / total;
        }
        else
        {
            //쿨타임 완료 시
            swordCooldownText.gameObject.SetActive(false);
            swordCooldownOverlay.fillAmount = 0;
            swordCooldownOverlay.gameObject.SetActive(false);
        }
    }

    private void UpdateInvincibleUI(float remaining, float total)//무적 스킬 UI 갱신 (검 공격과 로직 동일)
    {
        if (invincibleOverlay == null || invincibleText == null) return;

        if (remaining > 0)
        {
            invincibleText.gameObject.SetActive(true);
            //무적 스킬은 쿨타임이 기니까(20초) CeilToInt로 정수 표시해도 무방하지만, 
            //검이랑 통일감을 주기 위해 "F1" 써.
            invincibleText.text = remaining.ToString("F1");

            invincibleOverlay.gameObject.SetActive(true);
            invincibleOverlay.fillAmount = remaining / total;
        }
        else
        {
            invincibleText.gameObject.SetActive(false);
            invincibleOverlay.fillAmount = 0;
            invincibleOverlay.gameObject.SetActive(false);
        }
    }
}
