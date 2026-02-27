using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


/// <summary>
/// 플레이어의 실제 능력치(수치)와 강화 로직을 관리하는 핵심 스크립트.
/// UI 연출은 직접 하지 않고 이벤트를 통해 '방송'만 한다.
/// </summary>


public class PlayerStatsEffects : MonoBehaviour
{
    //---------옵저버 패턴: UI 및 무기를 위한 방송국-----//
    public static event Action<int, int> OnArrowLevelChanged;
    public static event Action<int, int> OnSwordLevelChanged;
    public static event Action<int, int> OnMoveSpeedLevelChanged;

    //활 강화 연출을 위한 전용 이벤트
    public static event Action OnArrowEnhancedEffect;//소리 내고 커져라!
    public static event Action<bool> OnArrowColorStateChanged;//색깔 바꿔라!
    //---------------------------------------------------//

    [Header("공격 아이템 최대치 레벨 설정")]
    public int AttackItemMaxLevel = 10;//모든 이속,활,검 레벨에 적용할 최대치


    //내부 데이터 변수
    private int currentArrowLevel = 0;//활, 검, 이동속도의 강화 횟수를 저장할 변수
    private int currentSwordLevel = 0;
    private int currentMoveSpeedLevel = 0;

    //참조 컴포넌트
    private Player player;
    private PlayerHealth playerHealth;
    private BowWeapon bowWeapon;
    private PlayerShield playerShield;
    private SwordWeapon swordWeapon;

    
    void Awake()
    {
        //플레이어 내부 컴포넌트 자동 연결
        player = GetComponent<Player>();
        playerHealth = GetComponent<PlayerHealth>();
        bowWeapon = GetComponent<BowWeapon>();
        playerShield = GetComponent<PlayerShield>();
        swordWeapon = GetComponent<SwordWeapon>();

        CheckInitialization();//[방어적 프로그래밍] 검증 로직(Awake 함수의 가독성 문제로 로그 알림 함수로 분리)
    }

    private void CheckInitialization()
    {
        //필수 컴포넌트 체크
        if (player == null) Debug.LogWarning("Stats: Player 미연결!");
        if (bowWeapon == null) Debug.LogWarning("Stats: BowWeapon 미연결!");
        if (playerShield == null) Debug.LogWarning("Stats: playerShield 미연결!");
        if (swordWeapon == null) Debug.LogWarning("Stats: swordWeapon 미연결!");
    }

    void Start()
    {
        BroadcastAllLevels();//게임 시작 시 초기 UI 상태 방송
    }

    private void BroadcastAllLevels()//게임이 시작되자마자 UI 텍스트들을 0으로 초기화해주는 첫 방송
    {
        OnArrowLevelChanged?.Invoke(currentArrowLevel, AttackItemMaxLevel);
        OnSwordLevelChanged?.Invoke(currentSwordLevel, AttackItemMaxLevel);
        OnMoveSpeedLevelChanged?.Invoke(currentMoveSpeedLevel, AttackItemMaxLevel);
    }


    public void ArrowDamageUp(float ItemCSdamage, float coolDown)//아이템 효과 함수들
    {
        if (bowWeapon != null)//활 레벨과 상관없이 아이템을 먹으면 강화 스택은 무조건 쌓여야 함
        {
            bowWeapon.AcquireBowEnhanceItem();//활 강화 스택 쌓기 로직
            int currentStacks = bowWeapon.GetCurrentStacks();

            if (currentStacks > 0 && currentStacks % 3 == 0)//로직 판단. 3스택마다 UI에게 연출하라고 방송
                OnArrowEnhancedEffect?.Invoke();

            OnArrowColorStateChanged?.Invoke(currentStacks >= 3);//로직 판단. 3스택 이상 여부에 따라 색상 변경 방송
        }

        if (currentArrowLevel < AttackItemMaxLevel)//실제 레벨 및 수치 증가 로직
        {
            if (bowWeapon != null)
            {
                bowWeapon.ArrowDamage += ItemCSdamage;
                bowWeapon.DecreaseAttackCooldown(coolDown, WeaponType.Bow);
                currentArrowLevel++;

                //레벨 변경 방송
                OnArrowLevelChanged?.Invoke(currentArrowLevel, AttackItemMaxLevel);
                Debug.Log($"[Stats] 활 공격력 증가: {bowWeapon.ArrowDamage}");
            }
        }
        else Debug.Log("PlayerStatsEffects: 화살 레벨이 이미 최대치야!");//최대 레벨에 도달했을 때의 메시지
    }
    public void RefreshArrowStackStatus()
    {
        if (bowWeapon != null)//현재 스택이 3으로 강화가 되면 true(금색), 아니면 false(흰색)를 보냄
            OnArrowColorStateChanged?.Invoke(bowWeapon.GetCurrentStacks() >= 3);
    }


    public void SwordDamageUp(float ItemCSdamage)//검 강화
    {
        if (currentSwordLevel < AttackItemMaxLevel)//현재 레벨이 MaxLevel보다 작을 때만 실행
        {
            if(swordWeapon != null)
            {
                swordWeapon.SwordDamage += ItemCSdamage;
                swordWeapon.SwordEnergyDamage += ItemCSdamage;

                swordWeapon.AcquireSwordEnhanceItem();//쿨타임 감소 및 누적 스택 처리는 여기서 한 번만 호출!

                currentSwordLevel++;//검 강화 숫자 증가
                OnSwordLevelChanged?.Invoke(currentSwordLevel, AttackItemMaxLevel);
                Debug.Log("PlayerStatsEffects: 검+검기 공격력이 " + ItemCSdamage + " 증가했다! 현재 공격력: " + swordWeapon.SwordDamage);
            }
        }
        else Debug.Log("PlayerStatsEffects: 검 레벨이 이미 최대치야!");//최대 레벨에 도달했을 때의 로그
    }

    public void MoveSpeedUp(float amount)//이동속도 강화
    {
        if (currentMoveSpeedLevel < AttackItemMaxLevel)//현재 레벨이 MaxLevel보다 작을 때만 실행
        {
            if(player != null)
            {
                player.MoveSpeed += amount;//Player 스크립트의 이동 속도 증가
                currentMoveSpeedLevel++;//이동 속도 횟수 증가
                OnMoveSpeedLevelChanged?.Invoke(currentMoveSpeedLevel, AttackItemMaxLevel);
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
}

   
