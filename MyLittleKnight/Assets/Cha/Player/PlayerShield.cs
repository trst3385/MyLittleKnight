using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//UI ����� ���� �߰�

public class PlayerShield : MonoBehaviour
{
    [Header("���� ���� ����")]
    public float MaxShield = 20f;//�ִ� ����(�ν����ͷ� ���� ����)
    public float CurrentShield;//���� ����
    public Slider ShieldBar;//���� UI �����̴�

    [Header("��ũ��Ʈ ����")]
    [SerializeField] private PlayerHealth playerHealth;//PlayerHealth ��ũ��Ʈ ����
    //[SerializeField]�� private ������ ����Ƽ �ν����Ϳ� ���̰� ������ִ� ����� ��.
    //���� private ������ �ν����Ϳ� ������ ���ݾ�? ������ ������ �ܺο����� ���� ���ϰ� �ϸ鼭, �ν����Ϳ����� ���� �ٲٰ� ���� ���� ����.
    //[SerializeField]�� ���� ���� �ٿ��ָ� ������ ������ private������ �ν����Ϳ��� ��Ÿ��
    //���� �̰� UI�� �巡���ؼ��� ���� �־ 

    void Start()
    {

    }

    //Awake()
    //��ũ��Ʈ�� �ε�� �� �ٷ� �� �� �����.
    //���� ������Ʈ�� ��Ȱ��ȭ ���¶� ȣ���.
    //�ַ� �ڽ��� �ʱ�ȭ�� ����.�ٸ� ��ũ��Ʈ�� �ε�Ǿ����� ������� �ʾƼ� �ٸ� ��ũ��Ʈ�� �����Ϸ� �� �� ������ ���� �� ����.

    //Start()
    //ù ��° �������� ������Ʈ�Ǳ� ������ �� �� �����.
    //Awake()�� ����� ���Ŀ� ȣ���.
    //��� ��ũ��Ʈ�� Awake() �� �̹� ȣ��� ���¶�, �ٸ� ��ũ��Ʈ�� ������ �� ������.

    //PlayerShield ��ũ��Ʈ���� PlayerHealth ��ũ��Ʈ�� �����Ϸ� �� ��, Start() �Լ��� ����ϸ�,
    //PlayerHealth ��ũ��Ʈ�� �̹� �ε�� ���°� ����Ǵ� ���� �߻� Ȯ���� �ξ� �پ���. ���� ���� �����̾�!


    void Awake()
    {
        if (playerHealth == null)            
        CurrentShield = 0;//���� �� ���� 0���� �ʱ�ȭ
        UpdateShieldUI();
    }

    public void TakeShieldDamage(float damage)//���ظ� �޾� ���� ���Ҹ� ��û�� ��
    {
        if(CurrentShield > 0)//������ 0 �̻��̸� ���ش� ������ ����
        {
            CurrentShield -= damage;
            if(CurrentShield < 0)//������ 0���ϸ�
            {
                float remainingDamage = -CurrentShield;//���� ������ ���
                CurrentShield = 0;//���� 0���� ����
                playerHealth.TakeDamage(remainingDamage);//���� �������� ü�¿� ����
                Debug.Log("���� 0! ���� ������ " + remainingDamage + "�� ü�¿� �����!");
            }
            else Debug.Log("���� " + damage + " ����! ���� ����: " + CurrentShield);                
        }
        else//������ �̹� 0�̸� �ٷ� ü�¿� ������ ����
        {
            playerHealth.TakeDamage(damage);
            Debug.Log("ü�� " + damage + " ����!");
        }
        UpdateShieldUI();
    }

    
    public void HealShield(float amount)//������ ������ ���� ȸ�� �� ȣ��
    {
        CurrentShield = Mathf.Min(CurrentShield + amount, MaxShield); // �ִ� ���� �ʰ� ����
        UpdateShieldUI();
    }

    void UpdateShieldUI()//���� UI ������Ʈ
    {
        if(ShieldBar != null)//shieldBar ������ �����̴� UI�� ����Ǿ� �ִ��� Ȯ��
        {
            ShieldBar.maxValue = MaxShield;//�����̴��� �ִ� ���� ���� ������ �ִ� �������� ����
            ShieldBar.value = CurrentShield;//�����̴��� ���� ���� ���� �������� ����
        }

    }
}
