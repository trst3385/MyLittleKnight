using TMPro;
using UnityEngine;
using UnityEngine.UI;//체력, 방어력 슬라이더 조작을 위해 필요
using System.Collections;

public class PlayerStatusUIManager : MonoBehaviour
{
    [Header("연결될 UI 요소들")]
    private Slider hpBar;
    private Slider shieldBar;

    [Header("레벨 UI (TextMeshPro)")]
    private TextMeshProUGUI arrowLevelText;
    private TextMeshProUGUI swordLevelText;
    private TextMeshProUGUI moveSpeedLevelText;

    [Header("활 강화 연출 설정")]
    [SerializeField] private Color levelUpColor = new Color(1f, 0.84f, 0f);//활 강화 시 텍스트 금색
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private AudioSource effectsAudioSource;
    [SerializeField] private AudioClip enhancedArrowReadySound;


    void Awake()
    {
        //체력/방패 슬라이더 자동 연결
        hpBar = GameObject.Find("PlayerHealthBar")?.GetComponent<Slider>();
        shieldBar = GameObject.Find("PlayerShieldBar")?.GetComponent<Slider>();

        //무기 강화 레벨 텍스트 자동 연결
        arrowLevelText = GameObject.Find("ArrowLevelText")?.GetComponent<TextMeshProUGUI>();
        swordLevelText = GameObject.Find("SwordLevelText")?.GetComponent<TextMeshProUGUI>();
        moveSpeedLevelText = GameObject.Find("MoveSpeedLevelText")?.GetComponent<TextMeshProUGUI>();
    }
 

    private void OnEnable()
    {
        //2. [Observer.이벤트 구독]  (체력, 방어력 상태 변화 모니터링 시작)
        //Action<float, float> 형태이므로 받는 함수도 인자가 두 개(current, max)여야 해.
        PlayerHealth.OnHealthChanged += UpdateHealthUI;
        PlayerShield.OnShieldChanged += UpdateShieldUI;

        Player.OnMoveSpeedLevelChanged += UpdateMoveSpeedUI;

        //검 강화 연출 구독
        SwordWeapon.OnSwordLevelChanged += UpdateSwordUI;
        //활 강화 연출 구독
        BowWeapon.OnArrowLevelChanged += UpdateArrowUI;
        BowWeapon.OnArrowEnhancedEffect += PlayArrowEnhancedFX;
        BowWeapon.OnArrowColorStateChanged += SetArrowTextColor;
    }

    private void OnDisable()
    {
        //3. [Observer.구독 해지] 오브젝트가 꺼질 때 이벤트 구독 해제 (메모리 누수 및 Null 에러 방지)
        //연결이 끊어졌을 때(정확히는 오브젝트가 비활성화되거나 파괴될 때)
        PlayerHealth.OnHealthChanged -= UpdateHealthUI;
        PlayerShield.OnShieldChanged -= UpdateShieldUI;

        //이속 강화 연출 구독 해지
        Player.OnMoveSpeedLevelChanged -= UpdateMoveSpeedUI;
        //검 강화 연출 구독 해지
        SwordWeapon.OnSwordLevelChanged -= UpdateSwordUI;

        //활 강화 연출 구독 해지
        BowWeapon.OnArrowLevelChanged -= UpdateArrowUI;
        BowWeapon.OnArrowEnhancedEffect -= PlayArrowEnhancedFX;
        BowWeapon.OnArrowColorStateChanged -= SetArrowTextColor;
    }

    //콜백 함수: 체력/방패
    private void UpdateHealthUI(float current, float max) { if (hpBar) { hpBar.maxValue = max; hpBar.value = current; } }
    private void UpdateShieldUI(float current, float max) { if (shieldBar) { shieldBar.maxValue = max; shieldBar.value = current; } }

    //콜백 함수: 능력치 레벨
    private void UpdateArrowUI(int current, int max) => arrowLevelText.text = (current >= max) ? "B Level: Max" : $"B Level: {current}";
    private void UpdateSwordUI(int current, int max) => swordLevelText.text = (current >= max) ? "S Level: Max" : $"S Level: {current}";
    private void UpdateMoveSpeedUI(int current, int max) => moveSpeedLevelText.text = (current >= max) ? "M Level: Max" : $"M Level: {current}";

    private void PlayArrowEnhancedFX()//활 강화시 텍스트 연출 함수
    {
        if (effectsAudioSource && enhancedArrowReadySound)
        {
            effectsAudioSource.PlayOneShot(enhancedArrowReadySound);
        }

        if (arrowLevelText)
        {
            StartCoroutine(PunchScale(arrowLevelText.rectTransform));
        }
    }
    private void SetArrowTextColor(bool isEnhanced)//활 강화시 텍스트 색 변경
    {
        if (arrowLevelText)
        {
            arrowLevelText.color = isEnhanced ? levelUpColor : defaultColor;
        }
    }


    private IEnumerator PunchScale(RectTransform rect)
    {
        Vector3 originalScale = Vector3.one;
        rect.localScale = originalScale * 1.5f;
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            rect.localScale = Vector3.Lerp(originalScale * 1.5f, originalScale, elapsed / 0.2f);
            yield return null;
        }
        rect.localScale = originalScale;
    }
}
