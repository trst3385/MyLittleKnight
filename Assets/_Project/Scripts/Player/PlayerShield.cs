using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    //---------옵저버 패턴----------//
    public static event Action<float, float> OnShieldChanged;//방어력이 변경될때 신호를 보내
    //------------------------------//


    [Header("방어력 설정")]
    public float MaxShield = 20f;//최대 방어력(인스펙터로 조절 가능)
    public float CurrentShield;//현재 방어력

    //인스펙터에서 드래그하지 않고 코드가 스스로 찾을 스크립트
    private PlayerHealth playerHealth;
    private Player playerScript;


    void Awake()    
    {
        playerHealth = GetComponent<PlayerHealth>();//같은 오브젝트에 붙어있는 PlayerHealth스크립트 자동 연결
        playerScript = GetComponent<Player>();//플레이어 스크립트도 같이(무적 아이템 효과로 인해 데미지를 받지 않게)

        CurrentShield = 0;//시작 시 방어력 0
        CheckInitialization();
    }
    //이제 방어력UI는 옵저버 패턴으로 'PlayerStatusUIManager' 스크립트가 맡아서 방어력UI를 업데이트 하기에 강한 결합 방식을 사용하지 않아,
    //직접적인 UI와의 연결을 사용하지 않아!
    void Start()
    {
        OnShieldChanged?.Invoke(CurrentShield, MaxShield);//시작하자마자 현재 방어력 상태를 한 번 방송해서 UI가 초기화되게 함
    }
    private void CheckInitialization()
    {
        if (playerScript == null)
        {
            Debug.LogError($"{gameObject.name}: Player 컴포넌트를 찾을 수 없어!");
        }

        if (playerHealth == null)
        {
            Debug.LogError($"{gameObject.name}: PlayerHealth 컴포넌트를 찾을 수 없어!");
        }
    }


    public void TakeShieldDamage(float damage)//피해를 받아 방어력 감소를 요청할 때
    {
        //무적 상태 체크. 데미지는 입구 컷(무적 아이템을 획득하면 방어력, 체력 상관없이 무적 상태가 돼)
        if (playerScript != null && playerScript.isInvincible)
        {
            Debug.Log("무적 상태: 모든 데미지를 무시합니다.");
            return;//여기서 함수를 끝내버림. 아래의 방어력/체력 감소 로직은 실행 안 됨!
        }

        if (CurrentShield > 0)//방어력이 0 이상이면 받는 데미지는 방어력을 감소시켜
        {
            CurrentShield -= damage;

            if(CurrentShield < 0)//방어력이 0이하면
            {
                float remainingDamage = -CurrentShield;//남은 데미지 계산
                CurrentShield = 0;//방어력 0으로 설정
                playerHealth.TakeDamage(remainingDamage);//남은 데미지는 플레이어 체력에 적용
            }
        }
        else//방어력이 이미 0이면 바로 체력에 데미지 적용
        {
            playerHealth.TakeDamage(damage);
        }

        OnShieldChanged?.Invoke(CurrentShield, MaxShield);//[방송]데미지를 받았다는 신호를 보내 (옵저버 패턴)
    }

    public void HealShield(float amount)//아이템 등으로 방어력 회복 시 호출
    {
        CurrentShield = Mathf.Min(CurrentShield + amount, MaxShield);//최대 방어력 초과 방지
        OnShieldChanged?.Invoke(CurrentShield, MaxShield);
    }
}
