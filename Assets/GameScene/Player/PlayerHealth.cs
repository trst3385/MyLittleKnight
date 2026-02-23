using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class PlayerHealth : MonoBehaviour
{
    //---------옵저버 패턴----------//
    public static event Action<float, float> OnHealthChanged;//체력이 변경될때 신호를 보내
    //------------------------------//


    [Header("플레이어 체력 설정")]
    public float MaxHealth = 20f;//최대 체력(인스펙터에서 조절가능)
    [HideInInspector]public float CurrentHealth;//현재 체력

    private Player playerScript;


    void Awake()
    {
        playerScript = GetComponent<Player>();//같은 오브젝트 내의 Player 스크립트 자동 연결
        CurrentHealth = MaxHealth;//초기 체력 설정

        CheckInitialization();//[방어적 프로그래밍] 검증 로직
    }
    //이제 체력UI는 옵저버 패턴으로 'PlayerStatusUIManager' 스크립트가 맡아서 체력UI를 업데이트 하기에 강한 결합 방식을 사용하지 않아,
    //직접적인 UI와의 연결을 사용하지 않아!
    void Start()
    {
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);//시작하자마자 현재 체력 상태를 한 번 방송해서 UI가 초기화되게 함
    }
    private void CheckInitialization()
    {
        if (playerScript == null) Debug.LogError($"{gameObject.name}: Player 컴포넌트를 찾을 수 없어!");
    }

    public void TakeDamage( float damageAmount)//체력이 변경될때마다 호출할 함수(PlayerShield로 호출)
    {
        if(playerScript != null && playerScript.IsDead) return;//Player 스크립트의 isDead 확인

        //이미 죽은 상태라면 더 이상 데미지 받거나 사망 처리하지 않음
        //Die함수가 발동되면 isDead가 true 상태가 된다.

        CurrentHealth -= damageAmount;//체력 감소 
        CurrentHealth = Mathf.Max(CurrentHealth, 0);//체력이 0보다 작아지지 않도록 (최소 0)

        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);//[방송]데미지를 받았다는 신호를 보내 (옵저버 패턴)

        if (CurrentHealth <= 0)//체력이 0 이하고 아직 죽은 상태가 아닐 때만 사망 처리
            if (playerScript != null) playerScript.PlayerDie();//Player스크립트의 PlayerDie함수 호출 
    }
    public void Heal( float healAmount)//체력 회복 함수
    {
        if (playerScript != null && playerScript.IsDead) return;//Player 스크립트의 isDead 확인

        CurrentHealth += healAmount;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);//[방송]체력이 회복 되었다는 신호를 보내 (옵저버 패턴)
    }
}
