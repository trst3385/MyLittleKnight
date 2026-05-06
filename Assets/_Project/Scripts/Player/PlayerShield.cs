using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShield : MonoBehaviour
{
    //---------옵저버 패턴----------//
    public static event Action<float, float> OnShieldChanged;//방어력이 변경될때 신호를 보내
    //------------------------------//


    public float CurrentShield { get; private set; }//5.6 하드코딩된 MaxShield 변수 삭제. 현재 방어력만 관리, SO에서 방어력값 관리
                                                    //읽기 전용: UI나 다른 스크립트에서 값을 가져가서 보여줄 순 있지만,
                                                    //수정 금지: 외부에서 값을 직접 바꾸려고 하면 컴파일 에러가 발생해서 코드를 보호해

    //인스펙터에서 드래그하지 않고 코드가 스스로 찾을 스크립트
    private PlayerHealth playerHealth;
    private Player playerScript;


    void Awake()    
    {
        playerHealth = GetComponent<PlayerHealth>();//같은 오브젝트에 붙어있는 PlayerHealth스크립트 자동 연결
        playerScript = GetComponent<Player>();//플레이어 스크립트도 같이(무적 아이템 효과로 인해 데미지를 받지 않게)

        CurrentShield = 0;//시작 시 방어력 0
        CheckInitialization();//[방어적 프로그래밍] 검증 로직
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

    //이제 방어력UI는 옵저버 패턴으로 'PlayerStatusUIManager' 스크립트가 맡아서 방어력UI를 업데이트 하기에 강한 결합 방식을 사용하지 않아,
    //직접적인 UI와의 연결을 사용하지 않아!
    void Start()
    {
        var stats = playerScript.Stats;//시작 시 SO에서 데이터를 가져와 UI 초기화 방송
        if (stats != null)
        {
            OnShieldChanged?.Invoke(CurrentShield, stats.maxShield);
        }
    }


    public void TakeShieldDamage(float damage)//피해를 받아 방어력 감소
    {
        //무적 상태 체크. 무적 아이템을 획득하면 방어력, 체력 상관없이 무적 상태가 돼
        if (playerScript != null && playerScript.isInvincible)
        {
            return;//여기서 함수를 끝내버림. 아래의 방어력/체력 감소 로직은 실행 안 됨!
        }

        var stats = playerScript.Stats;//실행 시점에 최신 SO 데이터 참조 (방어적 설계)
        if (stats == null)
        {
            return;
        }

        if (CurrentShield > 0)//현재 방어력이 남아있는지 확인 (0보다 큰 경우)
        {
            CurrentShield -= damage;

            if(CurrentShield < 0)//방어력을 초과하는 데미지를 입었다면?
            {
                float remainingDamage = -CurrentShield;//방어력을 초과한 들어온 남은 데미지 계산
                CurrentShield = 0;//방어력이 마이너스가 되지 않게 0으로 초기화(방어막 파괴)
                playerHealth.TakeDamage(remainingDamage);//남은 데미지는 플레이어 체력에 적용
            }
        }
        else//방어력이 이미 0이라서 방어력이 없는 상태라면
        {
            playerHealth.TakeDamage(damage);//바로 체력에 데미지 적용
        }

        OnShieldChanged?.Invoke(CurrentShield, stats.maxShield);//SO의 maxShield를 인자로 전달하여 UI 업데이트
    }

    public void HealShield(float amount)//아이템 등으로 방어력 회복 시 호출
    {
        var stats = playerScript.Stats;//함수 실행 시점에 가장 최신화된 능력치(SO)를 안전하게 가져옴(지역 변수로 데이터 오염 방지)
        if (stats == null)
        {
            return;
        }

        //SO의 maxShield를 기준으로 회복량 제한
        CurrentShield = Mathf.Min(CurrentShield + amount, stats.maxShield);//최대 방어력 초과 방지
        OnShieldChanged?.Invoke(CurrentShield, stats.maxShield);
        //Mathf.Min이란? (값 A, 값 B)은 입력받은 두 숫자 중에서 더 작은 값을 결과로 내놓는 함수야.
        //작동 방식: "현재 방어력 + 회복량"과 "최대 방어력"을 비교해서, 둘 중 작은 쪽을 선택해 CurrentShield에 다시 넣어주는 방식이야.
    }
}
