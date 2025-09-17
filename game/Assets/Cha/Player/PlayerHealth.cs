using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//UI를 사용할려면 이 네임스페이스 추가

public class PlayerHealth : MonoBehaviour
{
    [Header("플레이어 체력 관련")]
    public float MaxHealth = 20f;//최대 체력(조절가능)
    public float CurrentHealth;//현재 체력
    public Slider healthSlider;//인스펙터에서 이 슬라이더 연결

    [Header("스크립트 참조")]
    [SerializeField] private Player playerScript;//플레이어 스크립트 참조


    void Awake()//Awake()는 Start()보다 먼저 호출되며, 해당 게임 오브젝트가 활성화될 때 무조건적으로 먼저 실행됨.
    {

        CurrentHealth = MaxHealth;//게임 시작 시 현재 체력을 최대 체력으로 설정(조절가능)

        //슬라이더의 최대 값 설정
        if (healthSlider != null)
        {
            healthSlider.maxValue = MaxHealth;
            UpdateHealthUI();
        }
    }
    public void TakeDamage( float damageAmount)//체력이 변경될때마다 호출할 함수
    {
        if(playerScript != null && playerScript.IsDead) return; //Player 스크립트의 isDead 확인

        //이미 죽은 상태라면 더 이상 데미지 받거나 사망 처리하지 않음
        //Die함수가 발동되면 isDead가 true 상태가 된다.

        CurrentHealth -= damageAmount; //체력 감소 

        //체력이 0보다 작아지지 않도록 (최소 0)
        CurrentHealth = Mathf.Max(CurrentHealth, 0);

        UpdateHealthUI(); //체력바 UI 업데이트

        if(CurrentHealth <= 0) //체력이 0 이하고 아직 죽은 상태가 아닐 때만 사망 처리
        {
            if(playerScript != null) playerScript.PlayerDie();//Player스크립트의 PlayerDie함수 호출         
            else Debug.LogError("PlayerHealth: PlayerScript 컴포넌트를 찾을 수 없습니다.");
        }
    }
    public void Heal( float healAmount)//체력 회복 함수
    {
        if (playerScript != null && playerScript.IsDead) return; //Player 스크립트의 isDead 확인

        CurrentHealth += healAmount;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        UpdateHealthUI();
    }
    
    void UpdateHealthUI()//체력바 UI를 업데이트하는 함수
    {
        if (healthSlider != null) healthSlider.value = CurrentHealth;  
    }
}
