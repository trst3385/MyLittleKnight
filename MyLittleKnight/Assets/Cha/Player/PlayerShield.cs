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
    [SerializeField] private PlayerHealth playerHealth;//PlayerHealth 스크립트 참조
    //[SerializeField]는 private 변수를 유니티 인스펙터에 보이게 만들어주는 기능을 해.
    //원래 private 변수는 인스펙터에 보이지 않잖아? 하지만 가끔은 외부에서는 접근 못하게 하면서, 인스펙터에서만 값을 바꾸고 싶을 때가 있지.
    //[SerializeField]를 변수 위에 붙여주면 변수는 여전히 private이지만 인스펙터에는 나타나
    //지금 이건 UI를 드래그해서나 직접 넣어서 

    void Start()
    {

    }

    //Awake()
    //스크립트가 로드될 때 바로 한 번 실행돼.
    //게임 오브젝트가 비활성화 상태라도 호출돼.
    //주로 자신의 초기화에 사용돼.다른 스크립트가 로드되었는지 보장되지 않아서 다른 스크립트를 참조하려 할 때 문제가 생길 수 있지.

    //Start()
    //첫 번째 프레임이 업데이트되기 직전에 한 번 실행돼.
    //Awake()가 실행된 이후에 호출돼.
    //모든 스크립트의 Awake() 가 이미 호출된 상태라, 다른 스크립트를 참조할 때 안전해.

    //PlayerShield 스크립트에서 PlayerHealth 스크립트를 참조하려 할 때, Start() 함수를 사용하면,
    //PlayerHealth 스크립트가 이미 로드된 상태가 보장되니 에러 발생 확률이 훨씬 줄어들어. 아주 좋은 습관이야!


    void Awake()
    {
        if (playerHealth == null)            
        CurrentShield = 0;//시작 시 방어력 0으로 초기화
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
            ShieldBar.maxValue = MaxShield;//슬라이더의 최대 값을 현재 몬스터의 최대 방어력으로 설정
            ShieldBar.value = CurrentShield;//슬라이더의 현재 값을 현재 방어력으로 설정
        }

    }
}
