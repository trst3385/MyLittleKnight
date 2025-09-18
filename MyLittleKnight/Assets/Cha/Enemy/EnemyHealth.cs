using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyDifficulty;

//Animation Event처럼 특정 오브젝트에 여러 스크립트가 붙어있을 때,
//그리고 함수 이름이 같거나 오버로드 되어 있다면 Unity 시스템이 어떤 스크립트의 어떤 함수를 호출해야 할지
//모호해져서 문제가 발생할 수 있어.


public class EnemyHealth : MonoBehaviour
{
    [Header("몬스터의 HP")]
    //기존 HP 변수 (이제 기본값 역할을 함)
    [SerializeField] private float baseMaxHP = 5f;//몬스터의 기본 체력 (난이도 적용 전)

    [Header("몬스터의 에니메이터 컴포넌트 연결")]
    public Animator Animator;//몬스터의 에니메이터를 연결할 변수

    private float currentMaxHP;//난이도 적용 후의 최대 체력
    private float currentHP;//현재 체력
    private Enemy enemyScript;//Enemy 스크립트 참조 (EnemyDie 호출용)


    void Awake()//Start()보다 먼저 호출되어야 Enemy.cs에서 참조하기 전 체력 초기화가 됨.
    {
        enemyScript = GetComponent<Enemy>();//Enemy 스크립트 참조 (EnemyDie 호출용)

        if (EnemyDifficulty.Instance != null)//EnemyDifficulty 스크립트를 통해 현재 난이도에 맞춰 몬스터의 최대 체력을 조정
            currentMaxHP = EnemyDifficulty.Instance.GetAdjustedMonsterStat(baseMaxHP, StatType.Health);
        else
        {   //EnemyDifficulty스크립트가 없으면 기본 체력 사용
            Debug.LogWarning("EnemyDifficulty.Instance를 찾을 수 없어!. 몬스터 체력이 기본값으로 설정할께!.");
            currentMaxHP = baseMaxHP;
        }
        currentHP = currentMaxHP;//조정된 최대 체력으로 현재 체력 초기화
    }

    public void TakeDamage(float damageAmount)
    {
        //받은 데미지만큼 현재 체력을 감소시킨다.
        currentHP -= damageAmount;

        //현재 체력이 0보다 작거나 같으면 몬스터가 죽었는지 확인한다.
        if (currentHP <= 0)
        {
            Die();//몬스터가 죽었을 때 호출하는 함수
        }
    }
    

    
    void Die()
    {
        //몬스터가 죽으면 메세지 출력
        Debug.Log(gameObject.name + "따잇!");

        //Enemy 스크립트의 EnemyDie 함수 호출
        if (enemyScript != null)
            enemyScript.EnemyDie();//Enemy스크립트의 PlayerDie 함수 호출
        else Debug.LogError("EnemyHealth: Enemy 스크립트 컴포넌트를 찾을 수 없어!");
    }

}





