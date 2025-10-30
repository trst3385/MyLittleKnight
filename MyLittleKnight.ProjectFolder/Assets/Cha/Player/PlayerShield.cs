using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//UI 사용을 위해 추가

public class PlayerShield : MonoBehaviour
{
    [Header("방어력 관련 변수")]
    public float MaxShield = 20f;//최대 방어력(인스펙터로 변경 가능)
    public float CurrentShield;//현재 방어력
    public Slider ShieldBar;//방어력 UI 슬라이더

    [Header("스크립트 참조")]
    [SerializeField] private PlayerHealth playerHealth;//]SerializeField]는 private 변수도 인스펙터에 노출되게 하는거야

    // Awake()
    // Start()보다 먼저, 스크립트가 로드될 때 한 번 실행돼.
    // 주로 다른 스크립트 참조(GetComponent)나 초기 상태 설정을 여기에 적는 게 일반적이야.

    //Start()
    //첫 번째 프레임이 업데이트되기 직전에 한 번 실행돼.
    //Awake()가 실행된 이후에 호출돼.
    //모든 스크립트의 Awake() 가 이미 호출된 상태라, 다른 스크립트를 참조할 때 안전해.

    //enabled는 우리가 따로 선언하지 않았는데도 사용할 수 있는, 유니티의 MonoBehaviour 클래스가 기본으로 가지고 있는 내장 변수
    void Awake()
    {
        CurrentShield = 0;//시작 시 방어력 0으로 초기화

        if (playerHealth == null)//드래그앤 드롭으로 연결된 playerHealth 스크립트
        {
            Debug.LogWarning("PlayerShield: playerHealth 스크립트가 인스펙터에 연결되지 않았어!");
            //enabled = false; : MonoBehaviour가 기본으로 가지고 있는 내장 변수야
            //스크립트를 강제로 '비활성화(정지)' 시켜서
            //이후 Update()나 다른 함수에서 NullReferenceException 에러가 나는 것을 막아줘
            enabled = false;
            return;//여기서 함수를 끝내서, 밑의 UpdateShieldUI()가 호출되지 않게 함
        }    
        UpdateShieldUI();
    }

    public void TakeShieldDamage(float damage)//피해를 받아 방어력 감소를 요청할 때
    {
        if(CurrentShield > 0)//방어력이 0 이상이면 피해는 방어력을 감소
        {
            CurrentShield -= damage;
            if(CurrentShield < 0)//방어력이 0이하면
            {
                float remainingDamage = -CurrentShield;//남은 데미지 계산
                CurrentShield = 0;//방어력 0으로 설정
                playerHealth.TakeDamage(remainingDamage);//남은 데미지는 체력에 적용
                Debug.Log("방어력 0! 남은 데미지 " + remainingDamage + "가 체력에 적용돼!");
            }
            else Debug.Log("방어력 " + damage + " 감소! 현재 방어력: " + CurrentShield);                
        }
        else//방어력이 이미 0이면 바로 체력에 데미지 적용
        {
            playerHealth.TakeDamage(damage);
            Debug.Log("체력 " + damage + " 감소!");
        }
        UpdateShieldUI();
    }

    public void HealShield(float amount)//아이템 등으로 방어력 회복 시 호출
    {
        CurrentShield = Mathf.Min(CurrentShield + amount, MaxShield); // 최대 방어력 초과 방지
        UpdateShieldUI();
    }

    void UpdateShieldUI()//방어력 UI 업데이트
    {
        if(ShieldBar != null)//shieldBar 변수에 슬라이더 UI가 연결되어 있는지 확인
        {
            ShieldBar.maxValue = MaxShield;//슬라이더의 최대 값을 플레이어의 최대 방어력으로 설정
            ShieldBar.value = CurrentShield;//슬라이더의 현재 값을 현재 방어력으로 설정
        }

    }
}
