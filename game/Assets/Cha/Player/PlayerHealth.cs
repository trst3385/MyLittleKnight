using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//UI�� ����ҷ��� �� ���ӽ����̽� �߰�

public class PlayerHealth : MonoBehaviour
{
    [Header("�÷��̾� ü�� ����")]
    public float MaxHealth = 20f;//�ִ� ü��(��������)
    public float CurrentHealth;//���� ü��
    public Slider healthSlider;//�ν����Ϳ��� �� �����̴� ����

    [Header("��ũ��Ʈ ����")]
    [SerializeField] private Player playerScript;//�÷��̾� ��ũ��Ʈ ����


    void Awake()//Awake()�� Start()���� ���� ȣ��Ǹ�, �ش� ���� ������Ʈ�� Ȱ��ȭ�� �� ������������ ���� �����.
    {

        CurrentHealth = MaxHealth;//���� ���� �� ���� ü���� �ִ� ü������ ����(��������)

        //�����̴��� �ִ� �� ����
        if (healthSlider != null)
        {
            healthSlider.maxValue = MaxHealth;
            UpdateHealthUI();
        }
    }
    public void TakeDamage( float damageAmount)//ü���� ����ɶ����� ȣ���� �Լ�
    {
        if(playerScript != null && playerScript.IsDead) return; //Player ��ũ��Ʈ�� isDead Ȯ��

        //�̹� ���� ���¶�� �� �̻� ������ �ްų� ��� ó������ ����
        //Die�Լ��� �ߵ��Ǹ� isDead�� true ���°� �ȴ�.

        CurrentHealth -= damageAmount; //ü�� ���� 

        //ü���� 0���� �۾����� �ʵ��� (�ּ� 0)
        CurrentHealth = Mathf.Max(CurrentHealth, 0);

        UpdateHealthUI(); //ü�¹� UI ������Ʈ

        if(CurrentHealth <= 0) //ü���� 0 ���ϰ� ���� ���� ���°� �ƴ� ���� ��� ó��
        {
            if(playerScript != null) playerScript.PlayerDie();//Player��ũ��Ʈ�� PlayerDie�Լ� ȣ��         
            else Debug.LogError("PlayerHealth: PlayerScript ������Ʈ�� ã�� �� �����ϴ�.");
        }
    }
    public void Heal( float healAmount)//ü�� ȸ�� �Լ�
    {
        if (playerScript != null && playerScript.IsDead) return; //Player ��ũ��Ʈ�� isDead Ȯ��

        CurrentHealth += healAmount;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        UpdateHealthUI();
    }
    
    void UpdateHealthUI()//ü�¹� UI�� ������Ʈ�ϴ� �Լ�
    {
        if (healthSlider != null) healthSlider.value = CurrentHealth;  
    }
}
