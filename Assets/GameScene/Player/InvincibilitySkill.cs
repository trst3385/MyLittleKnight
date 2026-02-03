using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InvincibilitySkill : MonoBehaviour
{
    [Header("스킬 설정")]
    [Tooltip("무적 효과가 지속되는 시간 (초 단위)")]
    public float skillDuration = 3f;//무적 지속 시간

    [Tooltip("스킬을 다시 사용하기까지 필요한 대기 시간 (초 단위)")]
    public float skillCooldown = 30f;//무적 스킬 쿨타임

    [Header("무적 상태 시 색상 변화")]
    [Tooltip("무적 상태일 때 플레이어 캐릭터의 색상")]
    public Color invincibilityColor = new Color(1f, 1f, 0.5f, 0.8f);//무적 시 색상

    [Header("무적 시 사운드")]
    public AudioClip invincibilitySound;//발동 사운드

    //--- 내부 참조 (자동 연결) ---
    private AudioSource audioSource;
    private Player player;
    private SpriteRenderer spriteRenderer;
    //UI관련(외부에서 접근할 필요 없고, 코드에서 자동 할당하므로 private으로 변경)
    private Image cooldownOverlay;
    private TextMeshProUGUI cooldownText;

    private float lastUsedTime = -100f;//마지막 사용 시간 (처음에 바로 쓸 수 있게 넉넉히 과거로 설정)
    private bool isEffectActive = false;//현재 무적 효과가 사용 중인지
    

   

    void Awake()
    {
        player = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //사운드 소스 연결 (자식 오브젝트 중 "SND_Invincibility" 찾기)
        Transform sndTransform = transform.Find("SND_Invincibility");
        if (sndTransform != null)
            audioSource = sndTransform.GetComponent<AudioSource>();

        //UI 연결 (하이어라키에서 이름으로 찾기)
        //부모 아이콘 오브젝트를 먼저 찾고, 그 자식들을 찾는 게 가장 안전!
        GameObject skillIconObj = GameObject.Find("InvincibleSkillIcon");

        //UI 컴포넌트 하이어라키에서 찾기 (SwordWeapon 스크립트의 스킬 아이콘 찾는 방식과 동일)
        GameObject skillIcon = GameObject.Find("InvincibleSkillIcon");
        if (skillIcon != null)
        {
            Transform overlay = skillIcon.transform.Find("CooldownOverlay");
            if (overlay != null) cooldownOverlay = overlay.GetComponent<Image>();

            Transform text = skillIcon.transform.Find("CooldownText");
            if (text != null) cooldownText = text.GetComponent<TextMeshProUGUI>();
        }

        CheckInitialization();//[방어적 프로그래밍] 검증 로직 호출
    }

    private void CheckInitialization()
    {
        if (player == null) Debug.LogError($"{gameObject.name}: Player 스크립트가 없어!");
        if (cooldownOverlay == null) Debug.LogWarning($"{gameObject.name}: Invincible CooldownOverlay를 찾을 수 없어!");
        if (cooldownText == null) Debug.LogWarning($"{gameObject.name}: Invincible CooldownText를 찾을 수 없어!");
        if (audioSource == null) Debug.LogWarning($"{gameObject.name}: SND_Invincibility 오브젝트나 AudioSource가 없어!");
    }

    void Update()
    {
        //카운트다운이 끝났을 때만 입력을 확인하도록 조건 추가
        //CountdownManager.isCountdownFinished 가 true일 때만 실행돼.
        if (CountdownManager.isCountdownFinished && Input.GetKeyDown(KeyCode.E) && CanUse())
        {
            ActivateSkill();
        }

        UpdateSkillUI();//UI 업데이트
    }

    public bool CanUse() => Time.time >= lastUsedTime + skillCooldown && !isEffectActive;//지금 이 스킬을 써도 되는 상태인가? 확인.
    //`쿨타임도 다 찼고(&&), 현재 무적 상태도 아닐 때` 에만 스킬을 발동시킴

    private void ActivateSkill()//무적 아이템 사용시
    {
        lastUsedTime = Time.time;

        //사운드 재생
        if (audioSource != null && invincibilitySound != null)
            audioSource.PlayOneShot(invincibilitySound);

        //무적 코루틴 시작
        StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        isEffectActive = true;
        if (player != null) player.isInvincible = true;
        if (spriteRenderer != null) spriteRenderer.color = invincibilityColor;

        Debug.Log("무적 발동!");

        yield return new WaitForSeconds(skillDuration);

        if (player != null) player.isInvincible = false;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
        isEffectActive = false;

        Debug.Log("무적 종료");
    }

    private void UpdateSkillUI()
    {
        if (cooldownOverlay == null || cooldownText == null) return;

        //SwordWeapon 스크립트와 똑같은 방식의 남은 시간 계산
        float timeRemaining = lastUsedTime + skillCooldown - Time.time;

        if (timeRemaining > 0)
        {
            cooldownText.gameObject.SetActive(true);
            cooldownText.text = Mathf.CeilToInt(timeRemaining).ToString();

            cooldownOverlay.gameObject.SetActive(true);
            cooldownOverlay.fillAmount = timeRemaining / skillCooldown;
        }
        else
        {
            cooldownText.gameObject.SetActive(false);
            cooldownOverlay.gameObject.SetActive(false);
        }
    }
}
