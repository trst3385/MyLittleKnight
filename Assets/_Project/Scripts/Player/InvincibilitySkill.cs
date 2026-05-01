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

    //--- 내부 참조 (자동 연결) ---
    private AudioSource audioSource;
    private Player player;
    private SpriteRenderer spriteRenderer;
    private float lastUsedTime = -100f;//처음에 바로 사용할 수 있도록 과거 시간으로 초기화
    private bool isEffectActive = false;//현재 무적 효과가 사용 중인지 체크
    

    void Awake()
    {
        player = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //사운드 소스 연결 (자식 오브젝트 중 "SND_Invincibility" 찾기), 개별 볼륨 및 믹서 제어를 위해 자식 오브젝트로 분리함
        Transform sndTransform = transform.Find("SND_Invincibility");
        if (sndTransform != null)
        {   //이름으로 찾은 자식 오브젝트에서 오디오 컴포넌트를 가져와 캐싱 (인스펙터 드래그 대체)
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
        if (player != null && player.Stats == null)
        {
            Debug.LogError($"{gameObject.name}: Player 스크립트에 PlayerStatsSO가 연결되지 않았어!");
        }
        if (audioSource == null)
        {
            Debug.LogWarning($"{gameObject.name}: SND_Invincibility 오브젝트나 AudioSource가 없어!");
        }
    }

    void Update()
    {
        //Player나 Stats가 없으면 에러 방지를 위해 리턴
        if (player == null || player.Stats == null)
        {
            return;
        }

        //[옵저버] 매 프레임 UI 매니저에게 쿨타임 정보 방송
        float timeRemaining = lastUsedTime + player.Stats.invincibilityCooldown - Time.time;
        OnInvincibleCooldownChanged?.Invoke(Mathf.Max(0, timeRemaining), player.Stats.invincibilityCooldown);
    }

    public bool CanUse() => Time.time >= lastUsedTime + player.Stats.invincibilityCooldown && !isEffectActive;
    //지금 이 스킬을 써도 되는 상태인가? 확인.`쿨타임도 다 찼고(&&), 현재 무적 상태도 아닐 때` 에만 스킬을 발동시킴

    public void ActivateSkill()//무적 아이템 사용시
    {
        if (!CanUse()) return;

        lastUsedTime = Time.time;

        if (audioSource != null && player.Stats.invincibilitySound != null)//SO에 등록된 무적 사운드를 재생
        {
            audioSource.PlayOneShot(player.Stats.invincibilitySound);
        }

        StartCoroutine(InvincibilityRoutine());//무적 코루틴 시작
    }

    private IEnumerator InvincibilityRoutine()
    {
        isEffectActive = true;//무적 사용중이라 체크

        player.isInvincible = true;//무적 상태 돌입

        if (spriteRenderer != null)
        {
            spriteRenderer.color = player.Stats.invincibilityColor;
        }

        Debug.Log("무적 발동!");

        //SO에 설정된 지속 시간만큼 무적 상태
        yield return new WaitForSeconds(player.Stats.invincibilityDuration);

        player.isInvincible = false;//무적 상태 종료 및 색상 복구
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        isEffectActive = false;
        Debug.Log("무적 종료");
    }
}
