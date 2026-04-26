using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InvincibilitySkill : MonoBehaviour
{
    //--------- 옵저버 패턴 ----------//
    public static event Action<float, float> OnInvincibleCooldownChanged;//(남은 시간, 총 시간)
    //--------------------------------//

    [Header("스킬 설정")]
    [Tooltip("무적 효과가 지속되는 시간 (초 단위)")]
    public float skillDuration = 3f;//무적 지속 시간

    [Tooltip("스킬을 다시 사용하기까지 필요한 대기 시간 (초 단위)")]
    public float skillCooldown = 20f;//무적 스킬 쿨타임

    [Header("무적 상태 시 색상 변화")]
    [Tooltip("무적 상태일 때 플레이어 캐릭터의 색상")]
    public Color invincibilityColor = new Color(1f, 1f, 0.5f, 0.8f);//무적 시 색상

    [Header("무적 시 사운드")]
    public AudioClip invincibilitySound;//발동 사운드

    //--- 내부 참조 (자동 연결) ---
    private AudioSource audioSource;
    private Player player;
    private SpriteRenderer spriteRenderer;
    private float lastUsedTime = -100f;//마지막 사용 시간 (처음에 바로 쓸 수 있게 넉넉히 과거로 설정)
    private bool isEffectActive = false;//현재 무적 효과가 사용 중인지
    

   

    void Awake()
    {
        player = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //사운드 소스 연결 (자식 오브젝트 중 "SND_Invincibility" 찾기)
        Transform sndTransform = transform.Find("SND_Invincibility");
        if (sndTransform != null)
        {
            audioSource = sndTransform.GetComponent<AudioSource>();
        }
            

        //UI 연결 (하이어라키에서 이름으로 찾기)
        //부모 아이콘 오브젝트를 먼저 찾고, 그 자식들을 찾는 게 가장 안전!
        GameObject skillIconObj = GameObject.Find("InvincibleSkillIcon");

        CheckInitialization();//[방어적 프로그래밍] 검증 로직 호출
    }

    private void CheckInitialization()
    {
        if (player == null)
        {
            Debug.LogError($"{gameObject.name}: Player 스크립트가 없어!");
        }
        if (audioSource == null)
        {
            Debug.LogWarning($"{gameObject.name}: SND_Invincibility 오브젝트나 AudioSource가 없어!");
        }
    }

    void Update()
    {
        //입력 확인 로직 삭제(AttackController에서 관리할 거니까)

        //[옵저버] 매 프레임 UI 매니저에게 쿨타임 정보 방송
        float timeRemaining = lastUsedTime + skillCooldown - Time.time;
        OnInvincibleCooldownChanged?.Invoke(Mathf.Max(0, timeRemaining), skillCooldown);
    }

    public bool CanUse() => Time.time >= lastUsedTime + skillCooldown && !isEffectActive;//지금 이 스킬을 써도 되는 상태인가? 확인.
    //`쿨타임도 다 찼고(&&), 현재 무적 상태도 아닐 때` 에만 스킬을 발동시킴

    public void ActivateSkill()//무적 아이템 사용시
    {
        lastUsedTime = Time.time;

        //사운드 재생
        if (audioSource != null && invincibilitySound != null)
        {
            audioSource.PlayOneShot(invincibilitySound);
        }
            

        StartCoroutine(InvincibilityRoutine());//무적 코루틴 시작
    }

    private IEnumerator InvincibilityRoutine()
    {
        isEffectActive = true;
        if (player != null)
        {
            player.isInvincible = true;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = invincibilityColor;
        }

        Debug.Log("무적 발동!");

        yield return new WaitForSeconds(skillDuration);

        if (player != null)
        {
            player.isInvincible = false;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        isEffectActive = false;

        Debug.Log("무적 종료");
    }
}
