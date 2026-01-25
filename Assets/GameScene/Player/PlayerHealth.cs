using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//UI를 사용할려면 이 네임스페이스 추가

public class PlayerHealth : MonoBehaviour
{
    [Header("플레이어 체력 설정")]
    public float MaxHealth = 20f;//최대 체력(인스펙터에서 조절가능)
    [HideInInspector]public float CurrentHealth;//현재 체력

    //인스펙터에서 드래그하지 않고 코드가 스스로 찾을 오브젝트, UI들
    private Slider playerhealthbar;
    private Player playerScript;


    void Awake()
    {
        playerScript = GetComponent<Player>();//같은 오브젝트 내의 Player 스크립트 자동 연결

        //"PlayerHealthBar"라는 이름의 UI오브젝트를 찾아서 Slider 컴포넌트를 가져옴
        GameObject hpBarObj = GameObject.Find("PlayerHealthBar");//씬 내의 체력바 UI(Slider) 자동 연결
        if (hpBarObj != null)
            playerhealthbar = hpBarObj.GetComponent<Slider>();

        //초기 체력 설정
        CurrentHealth = MaxHealth;

        //슬라이더 초기화
        if (playerhealthbar != null)
        {
            playerhealthbar.maxValue = MaxHealth;
            UpdateHealthUI();
        }

        CheckInitialization();//[방어적 프로그래밍] 검증 로직(Awake 함수의 가독성 문제로 로그 알림 함수로 분리)
    }

    private void CheckInitialization()
    {
        if (playerScript == null)
            Debug.LogError($"{gameObject.name}: Player 컴포넌트를 찾을 수 없어!");

        if (playerhealthbar == null)
            Debug.LogWarning($"{gameObject.name}: 'PlayerHealthBar' 오브젝트를 찾을 수 없어 UI가 연동되지 않아!");
    }

    public void TakeDamage( float damageAmount)//체력이 변경될때마다 호출할 함수(PlayerShield로 호출)
    {
        if(playerScript != null && playerScript.IsDead) return;//Player 스크립트의 isDead 확인

        //이미 죽은 상태라면 더 이상 데미지 받거나 사망 처리하지 않음
        //Die함수가 발동되면 isDead가 true 상태가 된다.

        CurrentHealth -= damageAmount;//체력 감소 
        CurrentHealth = Mathf.Max(CurrentHealth, 0);//체력이 0보다 작아지지 않도록 (최소 0)

        UpdateHealthUI();//체력바 UI 업데이트

        if(CurrentHealth <= 0)//체력이 0 이하고 아직 죽은 상태가 아닐 때만 사망 처리
            if (playerScript != null) playerScript.PlayerDie();//Player스크립트의 PlayerDie함수 호출 
    }
    public void Heal( float healAmount)//체력 회복 함수
    {
        if (playerScript != null && playerScript.IsDead) return;//Player 스크립트의 isDead 확인

        CurrentHealth += healAmount;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        UpdateHealthUI();
    }
    
    void UpdateHealthUI()//체력바 UI를 업데이트하는 함수
    {
        if (playerhealthbar != null) playerhealthbar.value = CurrentHealth;  
    }
}
