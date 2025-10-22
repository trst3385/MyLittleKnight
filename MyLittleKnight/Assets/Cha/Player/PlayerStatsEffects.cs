using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//PlayerStatsEffects 스크립트?  플레이어의 능력치 관련 로직을 한데 모아 관리하는 역할을 해.
//아이템 획득 시 플레이어의 공격력, 이동 속도, 체력 등을 변경하고, 그 변경 사항을 UI에 반영하는 모든 일을 담당하는 역할이야.

//중앙 집중 관리: Item 스크립트에서 직접 PlayerAttack이나 PlayerHealth 스크립트에 접근하는 대신,
//PlayerStatsEffects 스크립트의 함수를 호출해서 모든 능력치 관련 로직을 처리해.

//유지보수 및 확장성: 새로운 능력치 아이템이 추가되거나 UI가 바뀌더라도,
//PlayerStatsEffects 스크립트만 수정하면 되기 때문에 코드가 훨씬 깔끔하고 관리가 쉬워져.

//단일 책임 원칙: 이 스크립트가 능력치 관리라는 한 가지 책임만 지기 때문에,
//다른 스크립트(예: Player, PlayerAttack)는 자기 본연의 역할(이동, 공격)에만 집중할 수 있게 돼.


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
    public TextMeshProUGUI ArrowSpeedText;

    [Header("최대치 레벨 관련 변수")]
    public int MaxLevel = 10;//모든 이속,활,검 레벨에 적용할 최대치


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

    void Start()
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

        //게임 시작 시 UI 텍스트 초기화
        UpdateWeaponLevelUI();
    }


    //아이템 효과 함수들
    public void ArrowDamageUp(float ItemCSdamage, float coolDown)
    {
        if (currentArrowLevel < MaxLevel)//현재 레벨이 MaxLevel보다 작을 때만 실행
        {
            if(bowWeapon != null)
            {
                bowWeapon.ArrowDamage += ItemCSdamage;//PlayerAttack 스크립트의 활 공격력 증가
                bowWeapon.DecreaseAttackCooldown(coolDown, WeaponType.Bow);//PlayerAttack 스크립트의 공격속도 증가
                currentArrowLevel++;//활 강화 숫자 증가
                UpdateWeaponLevelUI();//UI 업데이트
                Debug.Log("PlayerStatsEffects: 화살 공격력이 " + ItemCSdamage + " 증가! 현재 공격력: " + bowWeapon.ArrowDamage);
            }
        }
        else//최대 레벨에 도달했을 때의 메시지
            Debug.Log("PlayerStatsEffects: 화살 레벨이 이미 최대치야!");
    }

    public void SwordDamageUp(float ItemCSdamage)
    {
        if (currentSwordLevel < MaxLevel)//현재 레벨이 MaxLevel보다 작을 때만 실행
        {
            if(bowWeapon != null)
            {
                swordWeapon.SwordDamage += ItemCSdamage;//SwordWeapon의 검 공격력 증가
                swordWeapon.SwordEnergyDamage += ItemCSdamage;//SwordWeapon의 검기 공격력 증가
                currentSwordLevel++;//검 강화 숫자 증가
                UpdateWeaponLevelUI();//UI 업데이트
                Debug.Log("PlayerStatsEffects: 검 공격력이 " + ItemCSdamage + " 증가했다! 현재 공격력: " + swordWeapon.SwordDamage);
                Debug.Log("PlayerStatsEffects: 검기 발사체 공격력이 " + ItemCSdamage + " 증가했다! 현재 공격력: " + swordWeapon.SwordEnergyDamage);
            }
        }
        else//최대 레벨에 도달했을 때의 메시지
            Debug.Log("PlayerStatsEffects: 검 레벨이 이미 최대치야!");
    
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
        else// 최대 레벨에 도달하면 경고 메시지 출력
            Debug.Log("PlayerStatsEffects: 이동속도 레벨이 이미 최대치야!");
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

    void UpdateWeaponLevelUI()//무기 강화 횟수 UI를 업데이트하는 함수
    {
        //if문의 != null이란? 변수가 아무것도 참조하고 있지 않을 때(null)가 아닐 때 즉,
        //뭔가 제대로 연결되어 있을 때 라는 의미야.

        //if문 안에 ? 이랑 : 는 뭘까? 이건 삼항 연산자(Ternary Operator) 라고 해.
        //내가 if문을 짧게 할려고 한 줄만 있으면 중괄호 {}를 안쓰잖아. 삼항 연산자도 사용하면 if-else문을 한 줄로 바꿀 수 있어.
        //? 참일 때 실행할 코드
        //: 거짓일 때 실행할 코드;

        //한 줄이라 삼항 연산자를 쓰는게 좋아. if문은 {}를 써야 가독성도 좋고 그 안에 여러 코드를 넣을 수 있으니까.

        //일반 if문이었으면 이런 식이야. 
        //if (currentArrowLevel >= MaxLevel)
        //    ArrowLevelText.text = "B Level: Max";
        //else
        //    ArrowLevelText.text = $"B Level: {currentArrowLevel}";

        if (ArrowLevelText != null)//활 공격이 강화 됐다고 ArrowLevelText UI에 보내
        {
            ArrowLevelText.text = (currentArrowLevel >= MaxLevel)//강화 숫자가 MaxLevel보다 낮으면
            ? "B Level: Max"
            : $"B Level: {currentArrowLevel}";//MaxLevel과 같다면  
        }
        else Debug.LogWarning("PlayerStatsEffects: ArrowLevelText UI가 할당되지 않았어! 인스펙터를 확인해!");

        if (ArrowSpeedText != null)//활 공격 속도 UI 업데이트
        {
            ArrowSpeedText.text = (currentArrowLevel >= 5)
            ? "SPD: Max"
            : $"SPD: {currentArrowLevel}";

            if (currentArrowLevel >= 5) Debug.Log("최고 속도!");//Debug.Log는 따로 if문을...
        }

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
}

   
