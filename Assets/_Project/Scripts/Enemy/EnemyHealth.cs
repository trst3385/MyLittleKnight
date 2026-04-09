using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyDifficulty;
//EnemyDifficulty 스크립트를 using으로 호출


public class EnemyHealth : MonoBehaviour
{
    [Header("Stats 연결")]                          //3.7이제 SO 파일에서 몬스터의 스탯을 관리해
    [SerializeField] private EnemyStatsSO statsData;//SO 파일 연결(몬스터의 체력, 데미지, 색상 등등 들어있으니까!)

    [Header("몬스터의 에니메이터 컴포넌트 연결")]
    public Animator Animator;//몬스터의 에니메이터를 연결할 변수

    private float currentMaxHP;//난이도 적용 후의 최대 체력
    private float currentHP;//현재 체력
    private Enemy enemyScript;//Enemy 스크립트 참조 (EnemyDie 호출용)


    void Awake()//Start()보다 먼저 호출되어야 Enemy.cs에서 참조하기 전 체력 초기화가 됨.
    {
        enemyScript = GetComponent<Enemy>();//Enemy 스크립트 참조 (EnemyDie 호출용)
    }

    void Start()
    {
        if (statsData == null)//만약 내(EnemyHealth) 인스펙터가 비어있다면
        {
            if (enemyScript != null) statsData = enemyScript.statsData;//같은 오브젝트에 붙어있는 Enemy 스크립트에서 statsData를 가져와
        }
        if (statsData == null)//둘 다 뒤졌는데도 없으면 그때 statsData가 없다고 에러를 띄워
        {
            Debug.LogError($"{gameObject.name}: Enemy 스크립트에도 statsData가 없어! SO를 연결해줘!");
            return;
        }

        float baseMaxHP = statsData.MaxHP;//SO의 체력을 기본값으로 사용
        if (EnemyDifficulty.Instance != null)//난이도에 따른 HP 보정 로직
            currentMaxHP = EnemyDifficulty.Instance.GetAdjustedMonsterStat(baseMaxHP, StatType.Health);
        else currentMaxHP = baseMaxHP;

        currentHP = currentMaxHP;
    }

    public void TakeDamage(float damageAmount)
    {
        //받은 데미지만큼 현재 체력을 감소시킨다.
        currentHP -= damageAmount;

        //현재 체력이 0보다 작거나 같으면 몬스터가 죽었는지 확인한다.
        if (currentHP <= 0) Die();//몬스터가 죽었을 때 호출하는 함수
    }
        
    public void Die()//몬스터가 사망했다고 Enemy 스크립트로 보내
    {
        //체력이 0이 되면 Enemy 스크립트의 EnemyDie() 호출, 사망 로직 발동
        if (enemyScript != null)
        {
            enemyScript.EnemyDie();      
        }
        else
        {
            Debug.LogError("EnemyHealth: Enemy 스크립트 컴포넌트를 찾을 수 없어!");
        } 
    }
}