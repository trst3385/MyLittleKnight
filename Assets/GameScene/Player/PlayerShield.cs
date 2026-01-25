using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//UI 사용을 위해 추가

public class PlayerShield : MonoBehaviour
{
    [Header("방어력 설정")]
    public float MaxShield = 20f;//최대 방어력(인스펙터로 조절 가능)
    public float CurrentShield;//현재 방어력

    //인스펙터에서 드래그하지 않고 코드가 스스로 찾을 오브젝트, UI들
    private Slider shieldBar;
    private PlayerHealth playerHealth;


    void Awake()    
    {
        playerHealth = GetComponent<PlayerHealth>();//같은 오브젝트에 붙어있는 PlayerHealth스크립트 자동 연결

        //씬 내의 방어력바 UI 자동 연결 (이름은 PlayerShieldBar라고 가정)
        GameObject sbObj = GameObject.Find("PlayerShieldBar");
        if (sbObj != null)
            shieldBar = sbObj.GetComponent<Slider>();

        CurrentShield = 0;//시작 시 방어력 0

        CheckInitialization();//[방어적 프로그래밍] 검증 로직(Awake 함수의 가독성 문제로 로그 알림 함수로 분리)
        UpdateShieldUI();//현재 방어력체크 및 초기화
    }

    private void CheckInitialization()
    {
        //핵심 참조가 없으면 스크립트를 정지시켜 에러 확산을 방지
        //enabled는 인스펙터 창에서 스크립트 이름 왼쪽에 있는 그 '체크박스'를 코드로 끄고 켜는 거야.
        //enabled = true; : 체크박스 체크 (스크립트 작동 중), enabled = false; : 체크박스 해제(스크립트 정지)
        if (playerHealth == null)
        {
            Debug.LogError($"{gameObject.name}: PlayerHealth 컴포넌트를 찾을 수 없어! 스크립트를 비활성화!");
            enabled = false;//enabled: 이 스크립트(컴포넌트)의 체크박스를 해제해 기능을 정지시켜,
        }                   //Null 에러가 발생하여 게임이 멈추는 것을 방지하는 안전장치야.
        if (shieldBar == null)
            Debug.LogWarning($"{gameObject.name}: 'PlayerShieldBar' UI오브젝트를 찾을 수 없어! UI가 연동되지 않아!");
    }

    public void TakeShieldDamage(float damage)//피해를 받아 방어력 감소를 요청할 때
    {
        if(CurrentShield > 0)//방어력이 0 이상이면 받는 데미지는 방어력을 감소시켜
        {
            CurrentShield -= damage;

            if(CurrentShield < 0)//방어력이 0이하면
            {
                float remainingDamage = -CurrentShield;//남은 데미지 계산
                CurrentShield = 0;//방어력 0으로 설정
                playerHealth.TakeDamage(remainingDamage);//남은 데미지는 플레이어 체력에 적용
            }
        }
        else playerHealth.TakeDamage(damage);//방어력이 이미 0이면 바로 체력에 데미지 적용

        UpdateShieldUI();
    }

    public void HealShield(float amount)//아이템 등으로 방어력 회복 시 호출
    {
        CurrentShield = Mathf.Min(CurrentShield + amount, MaxShield);//최대 방어력 초과 방지
        UpdateShieldUI();
    }

    void UpdateShieldUI()//방어력 UI 업데이트
    {
        if(shieldBar != null)//shieldBar 변수에 슬라이더 UI가 연결되어 있는지 확인
        {
            shieldBar.maxValue = MaxShield;//슬라이더의 최대 값을 플레이어의 최대 방어력으로 설정
            shieldBar.value = CurrentShield;//슬라이더의 현재 값을 현재 방어력으로 설정
        }
    }
}
