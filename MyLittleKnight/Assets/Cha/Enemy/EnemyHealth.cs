using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EnemyDifficulty;

//Animation Eventó�� Ư�� ������Ʈ�� ���� ��ũ��Ʈ�� �پ����� ��,
//�׸��� �Լ� �̸��� ���ų� �����ε� �Ǿ� �ִٸ� Unity �ý����� � ��ũ��Ʈ�� � �Լ��� ȣ���ؾ� ����
//��ȣ������ ������ �߻��� �� �־�.


public class EnemyHealth : MonoBehaviour
{
    [Header("������ HP")]
    //���� HP ���� (���� �⺻�� ������ ��)
    [SerializeField] private float baseMaxHP = 5f;//������ �⺻ ü�� (���̵� ���� ��)

    [Header("������ ���ϸ����� ������Ʈ ����")]
    public Animator Animator;//������ ���ϸ����͸� ������ ����

    private float currentMaxHP;//���̵� ���� ���� �ִ� ü��
    private float currentHP;//���� ü��
    private Enemy enemyScript;//Enemy ��ũ��Ʈ ���� (EnemyDie ȣ���)


    void Awake()//Start()���� ���� ȣ��Ǿ�� Enemy.cs���� �����ϱ� �� ü�� �ʱ�ȭ�� ��.
    {
        enemyScript = GetComponent<Enemy>();//Enemy ��ũ��Ʈ ���� (EnemyDie ȣ���)

        if (EnemyDifficulty.Instance != null)//EnemyDifficulty ��ũ��Ʈ�� ���� ���� ���̵��� ���� ������ �ִ� ü���� ����
            currentMaxHP = EnemyDifficulty.Instance.GetAdjustedMonsterStat(baseMaxHP, StatType.Health);
        else
        {   //EnemyDifficulty��ũ��Ʈ�� ������ �⺻ ü�� ���
            Debug.LogWarning("EnemyDifficulty.Instance�� ã�� �� ����!. ���� ü���� �⺻������ �����Ҳ�!.");
            currentMaxHP = baseMaxHP;
        }
        currentHP = currentMaxHP;//������ �ִ� ü������ ���� ü�� �ʱ�ȭ
    }

    public void TakeDamage(float damageAmount)
    {
        //���� ��������ŭ ���� ü���� ���ҽ�Ų��.
        currentHP -= damageAmount;

        //���� ü���� 0���� �۰ų� ������ ���Ͱ� �׾����� Ȯ���Ѵ�.
        if (currentHP <= 0)
        {
            Die();//���Ͱ� �׾��� �� ȣ���ϴ� �Լ�
        }
    }
    

    
    void Die()
    {
        //���Ͱ� ������ �޼��� ���
        Debug.Log(gameObject.name + "����!");

        //Enemy ��ũ��Ʈ�� EnemyDie �Լ� ȣ��
        if (enemyScript != null)
            enemyScript.EnemyDie();//Enemy��ũ��Ʈ�� PlayerDie �Լ� ȣ��
        else Debug.LogError("EnemyHealth: Enemy ��ũ��Ʈ ������Ʈ�� ã�� �� ����!");
    }

}





