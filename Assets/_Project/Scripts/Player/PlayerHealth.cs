using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class PlayerHealth : MonoBehaviour
{
    //---------옵저버 패턴----------//
    public static event Action<float, float> OnHealthChanged;//체력이 변경될때 신호를 보내
    //------------------------------//


    public float CurrentHealth { get; private set; }//5.6하드코딩된 MaxHealth 삭제 및 CurrentHealth 데이터 보호 적용 SO에서 체력값 관리
                                                    //읽기 전용: UI나 다른 스크립트에서 값을 가져가서 보여줄 순 있지만,
                                                    //수정 금지: 외부에서 값을 직접 바꾸려고 하면 컴파일 에러가 발생해서 코드를 보호해
    private Player playerScript;


    void Awake()
    {
        playerScript = GetComponent<Player>();//같은 오브젝트 내의 Player 스크립트 자동 연결

        CheckInitialization();//[방어적 프로그래밍] 검증 로직
    }
    private void CheckInitialization()
    {
        if (playerScript == null)
        {
            Debug.LogError($"{gameObject.name}: Player 컴포넌트를 찾을 수 없어!");
        }
    }

    //이제 체력UI는 옵저버 패턴으로 'PlayerStatusUIManager' 스크립트가 맡아서 체력UI를 업데이트 하기에 강한 결합 방식을 사용하지 않아,
    //직접적인 UI와의 연결을 사용하지 않아!
    void Start()
    {
        var stats = playerScript.Stats;//시작 시 SO에서 데이터를 가져와 현재 체력 초기화 및 UI 방송
        if (stats != null)
        {
            CurrentHealth = stats.maxHealth;
            OnHealthChanged?.Invoke(CurrentHealth, stats.maxHealth);
        }
    }


    public void TakeDamage( float damageAmount)//체력이 변경될때마다 호출할 함수(PlayerShield로 호출)
    {
        if(playerScript != null && playerScript.IsDead)// 플레이어 스크립트가 존재하고 이미 죽은 상태라면 즉시 종료                                              
        {                                             
            return;
        }

        //함수 실행 시점에 최신 SO 데이터 참조 (방어적 설계)
        var stats = playerScript.Stats;
        if (stats == null)
        {
            return;
        }

        CurrentHealth -= damageAmount;//체력 감소 
        CurrentHealth = Mathf.Max(CurrentHealth, 0);//체력이 0보다 작아지지 않도록 (최소 0)

        OnHealthChanged?.Invoke(CurrentHealth, stats.maxHealth);//SO의 maxHealth를 인자로 전달하여 UI 업데이트 신호 전송

        if (CurrentHealth <= 0)//체력이 0이거나 0 이하고, 아직 죽은 상태가 아닐 때만 사망 처리
        {
            playerScript?.PlayerDie();//playerScript가 null이 아닐 때만 PlayerDie() 호출
        }
            
    }
    public void Heal( float healAmount)//체력 회복
    {
        if (playerScript != null && playerScript.IsDead)//플레이어가 이미 죽은 상태라면 회복하지 않음
        {
            return;
        }

        var stats = playerScript.Stats;//함수 실행 시점에 최신 능력치(SO)를 안전하게 가져옴
        if (stats == null)
        {
            return;
        }

        CurrentHealth += healAmount;
        CurrentHealth = Mathf.Min(CurrentHealth, stats.maxHealth);//SO의 maxHealth를 기준으로 회복량 제한
        OnHealthChanged?.Invoke(CurrentHealth, stats.maxHealth);//[방송] 체력 회복 신호 전송
        //Mathf.Min이란? (값 A, 값 B)은 입력받은 두 숫자 중에서 더 작은 값을 결과로 내놓는 함수야.
        //작동 방식: "현재 체력 + 회복량"과 "최대 체력"을 비교해서, 둘 중 작은 쪽을 선택해 CurrentHealth에 다시 넣어주는 방식이야.
    }
}
