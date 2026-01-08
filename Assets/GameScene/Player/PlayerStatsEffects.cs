using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


///<summary>
///플레이어의 능력치 강화 및 UI 업데이트를 관리하는 스크립트야.
///</summary> 
//이렇게 중간에 PlayerStatsEffects 스크립트를 두면, Item 스크립트는 "나는 이런 아이템이야"라고만 알려주고,
//실제로 능력치를 바꾸는 복잡한 일은 PlayerStatsEffects가 담당하게 되니 코드가 훨씬 깔끔해지는 거지. ///주석이 이런 용도야


public class PlayerStatsEffects : MonoBehaviour
{
    [Header("해당 텍스트 UI 연결")]
    //UI 텍스트 오브젝트 참조 변수(인스펙터 연결해)
    public TextMeshProUGUI ArrowLevelText;
    public TextMeshProUGUI SwordLevelText;
    public TextMeshProUGUI MoveSpeedLevelText;

    [Header("아이템 최대치 레벨 관련 변수")]
    public int MaxLevel = 10;//모든 이속,활,검 레벨에 적용할 최대치

    [Header("활 강화 시 사운드 설정")]
    public AudioSource effectsAudioSource;//사운드를 재생할 오디오 소스
    public AudioClip enhancedArrowReadySound;//재생할 'EnhancedArrowReady' 파일

    //활과 검의 강화 횟수를 저장할 변수
    //[HideInInspector]를 써도 되지만 이 변수를 직접 만질 일이 없으니 private으로 했어. 또 이 변수를 외부에서도 쓰지 않으니까
    private int currentArrowLevel = 0;
    private int currentSwordLevel = 0;
    private int currentMoveSpeedLevel = 0;

    private Player player;//Player 스크립트 참조 (이동 속도 증가를 위해)
    private PlayerHealth playerHealth;//PlayerHealth 스크립트 참조 (체력 회복을 위해)
    private BowWeapon bowWeapon;//PlayerAttack 스크립트 참조 (공격력 증가 적용을 위해)
    private PlayerShield playerShield;//PlayerShield 스크립트 참조 (방어력 회복을 위해)
    private SwordWeapon swordWeapon;//SwordWeapon 스크립트 참조

    void Awake()
    {
        player = GetComponent<Player>();
        playerHealth = GetComponent<PlayerHealth>();
        bowWeapon = GetComponent<BowWeapon>();
        playerShield = GetComponent<PlayerShield>();
        swordWeapon = GetComponent<SwordWeapon>();

        if (player == null) Debug.LogWarning("PlayerStatsEffects: Player 스크립트를 찾을 수 없습니다!");
        if (playerHealth == null) Debug.LogWarning("PlayerStatsEffects: PlayerHealth 스크립트를 찾을 수 없습니다!");
        if (bowWeapon == null) Debug.LogWarning("PlayerStatsEffects: PlayerAttack 스크립트를 찾을 수 없습니다!");
        if (playerShield == null) Debug.LogWarning("PlayerStatsEffects: PlayerShield 스크립트를 찾을 수 없습니다!");
    }
    void Start()
    {
        UpdateWeaponLevelUI();//게임 시작 시 현재 레벨(0)을 UI 텍스트에 반영 (UI 초기 설정)
        //UI의 초기 상태(Lv 0)를 설정. 이후 업데이트는 해당 함수(ArrowDamageUp 등)에서 직접 호출해 성능 낭비 방지
    }


    public void ArrowDamageUp(float ItemCSdamage, float coolDown)//아이템 효과 함수들
    {
        //활 레벨과 상관없이 아이템을 먹으면 강화 스택은 무조건 쌓여야 함
        if (bowWeapon != null)
        {
            bowWeapon.AcquireBowEnhanceItem();//스택 증가 및 UI 업데이트 호출

            int currentStacks = bowWeapon.GetCurrentStacks();
            if (currentStacks > 0 && currentStacks % 3 == 0)//활 아이템을 3번 획득할때 레벨 UI와 강화 사운드게 들리게
            {
                // 텍스트 커지는 연출
                StartCoroutine(PunchScale(ArrowLevelText.GetComponent<RectTransform>()));

                // 강화 사운드 재생
                if (effectsAudioSource != null && enhancedArrowReadySound != null)
                    effectsAudioSource.PlayOneShot(enhancedArrowReadySound);
                else//사운드가 안 나올 때 이유를 알려주는 디버그 로그
                {
                    if (effectsAudioSource == null)
                        Debug.LogWarning("PlayerStatsEffects: effectsAudioSource가 인스펙터에 연결되지 않았어!");
                    if (enhancedArrowReadySound == null)
                        Debug.LogWarning("PlayerStatsEffects: enhancedArrowReadySound(클립)이 할당되지 않았어!");
                }
            }
        }

        if (currentArrowLevel < MaxLevel)//레벨 상승 및 공격력 증가는 MaxLevel 미만일 때만 실행
        {
            if (bowWeapon != null)
            {
                bowWeapon.ArrowDamage += ItemCSdamage;
                bowWeapon.DecreaseAttackCooldown(coolDown, WeaponType.Bow);
                currentArrowLevel++;
                Debug.Log("PlayerStatsEffects: 화살 공격력이 " + ItemCSdamage + " 증가! 현재 공격력: " + bowWeapon.ArrowDamage);
            }
        }
        else Debug.Log("PlayerStatsEffects: 화살 레벨이 이미 최대치야!");//최대 레벨에 도달했을 때의 메시지

        UpdateWeaponLevelUI();//UI 업데이트
    }

    public void SwordDamageUp(float ItemCSdamage)
    {
        if (currentSwordLevel < MaxLevel)//현재 레벨이 MaxLevel보다 작을 때만 실행
        {
            if(swordWeapon != null)
            {
                swordWeapon.SwordDamage += ItemCSdamage;
                swordWeapon.SwordEnergyDamage += ItemCSdamage;

                if (swordWeapon != null) swordWeapon.AcquireSwordEnhanceItem();//스택 증가 함수 호출

                currentSwordLevel++;//검 강화 숫자 증가
                UpdateWeaponLevelUI();//UI 업데이트
                Debug.Log("PlayerStatsEffects: 검 공격력이 " + ItemCSdamage + " 증가했다! 현재 공격력: " + swordWeapon.SwordDamage);
                Debug.Log("PlayerStatsEffects: 검기 발사체 공격력이 " + ItemCSdamage + " 증가했다! 현재 공격력: " + swordWeapon.SwordEnergyDamage);            
            }
        }
        else Debug.Log("PlayerStatsEffects: 검 레벨이 이미 최대치야!");//최대 레벨에 도달했을 때의 로그
    }
    public void MoveSpeedUp(float amount)
    {
        if (currentMoveSpeedLevel < MaxLevel)//현재 레벨이 MaxLevel보다 작을 때만 실행
        {
            if(player != null)
            {
                player.MoveSpeed += amount;//Player 스크립트의 이동 속도 증가
                currentMoveSpeedLevel++;//이동 속도 횟수 증가
                UpdateWeaponLevelUI();//UI 업데이트
                                      //코드는 항상 위에서 아래 순서로 실행되니까 순서를 잘 지켜야해!
                                      //위의 currentMoveSpeedLevel++랑 UpdateWeaponLevelUI()의 순서가 만약 서로 달라졌으면
                                      //결국 UI는 currentMoveSpeedLevel이 0일 때 이미 업데이트를 마쳤기 때문에,
                                      //나중에 currentMoveSpeedLevel이 1로 바뀌더라도 UI에는 반영되지 않는 거야.
                Debug.Log("PlayerStatsEffects: 이동 속도가 " + amount + " 증가했다! 현재 속도: " + player.MoveSpeed);
            }
        }
        else Debug.Log("PlayerStatsEffects: 이동속도 레벨이 이미 최대치야!");// 최대 레벨에 도달하면 경고 메시지 출력
    }
    public void Heal(float amount)
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(amount);//PlayerHealth 스크립트의 Heal 함수를 호출!
            Debug.Log("PlayerStatsEffects: 체력이 " + amount + " 회복되었다! 현재 체력: " + playerHealth.CurrentHealth);
        }
    }

    public void HealShield(float amount)
    {
        if (playerShield != null)
        {
            playerShield.HealShield(amount);//PlayerShield 스크립트의 HealShield 함수 호출
            Debug.Log("PlayerStatsEffects: 방어력이 " + amount + " 회복되었다! 현재 방어력: " + playerShield.CurrentShield);
        }
    }

    //?와 :는 C# 언어의 삼항 연산자, 간단한 if-else 구문을 한 줄로 간결하게 표현할 수 있어
    //?은 참(if문의 내용), :은 거짓(else 내용)
    public void UpdateWeaponLevelUI()//무기 강화 횟수 UI를 업데이트하는 함수
    {
        if (ArrowLevelText != null)//활 공격이 강화 됐다고 ArrowLevelText UI에 보내
        {
            ArrowLevelText.text = (currentArrowLevel >= MaxLevel)//강화 숫자가 MaxLevel보다 낮으면
            ? "B Level: Max"
            : $"B Level: {currentArrowLevel}";//MaxLevel과 같다면  

            //2. ★ 색상 변경 로직 ★ 
            //3회 획득할 때마다 강화된다고 했으니까, % (나머지 연산자)를 쓰면 편해!
            //레벨이 3, 6, 9... 처럼 3의 배수일 때 색을 바꿔주는 거야.
            if (bowWeapon != null && bowWeapon.GetCurrentStacks() >= 3) ArrowLevelText.color = new Color(1f, 0.84f, 0f);//금색으로 변경
            else ArrowLevelText.color = Color.white;//강화 화살을 쐈거나 스택이 부족하면 다시 흰색
        }
        else Debug.LogWarning("PlayerStatsEffects: ArrowLevelText UI가 할당되지 않았어! 인스펙터를 확인해!");

        if (SwordLevelText != null)//검 공격이 강화됐다고 SwordLevelText UI에 보내 
        {
            SwordLevelText.text = (currentSwordLevel >= MaxLevel)
            ? "S Level: Max"
            : $"S Level: {currentSwordLevel}";
        }
        else Debug.LogWarning("PlayerStatsEffects: SwordLevelText UI가 할당되지 않았어! 인스펙터를 확인해!");
        

        if(MoveSpeedLevelText != null)//이동속도가 강화됐다고 MoveSpeedLevelText UI에 보내
        {
            MoveSpeedLevelText.text = (currentMoveSpeedLevel >= MaxLevel)
            ? "M Level: Max"
            : $"M Level: {currentMoveSpeedLevel}";
        }
        else Debug.LogWarning("PlayerStatsEffects: MoveSpeedLevelText UI가 할당되지 않았어! 인스펙터를 확인해!");      
    }


    IEnumerator PunchScale(RectTransform rect)//텍스트를 순간적으로 키웠다 원래대로 돌리는 연출
    {
        Vector3 originalScale = Vector3.one;//기본 크기 (1, 1, 1)
        rect.localScale = originalScale * 1.5f;//1.5배로 커짐

        float duration = 0.2f;//연출 시간
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            //부드럽게 원래 크기로 돌아오게 함
            rect.localScale = Vector3.Lerp(originalScale * 1.5f, originalScale, elapsed / duration);
            yield return null;
        }
        rect.localScale = originalScale;//마지막에 확실히 1로 고정
    }
}

   
