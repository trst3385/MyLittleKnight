using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordEnergy : MonoBehaviour
{
    [Header("�˱� �߻�ü ���ӽð�")]
    public float DestroyTime = 5f;//�߻�ü�� ����� �ð�

    [HideInInspector]public float damage;//�ܺο��� �������� �޾Ƽ� ������ ����
    //HideInInspector�� public ������ �ν����Ϳ� ���� �� �־�!

    void Start()
    {
        //n�� �ڿ� �߻�ü ������Ʈ�� ������ �ı�
        Destroy(gameObject, DestroyTime);
    }

    public void SetDamage(float amount)//PlayerAttack ��ũ��Ʈ�κ��� ������ ���� �޾ƿ��� �Լ�
    {                                  //PlayerAttack ��ũ��Ʈ���� swordenergy.SetDamage(totalDamage); �ڵ带 ȣ���ϸ�,
        damage = amount;               //totalDamage ��(ex: 5)�� amount��� ������ ��ܼ� SwordEnergy ��ũ��Ʈ�� SetDamage �Լ��� ���޵�.
                                       //�׷��� ���޹��� 5��� ���� damage ������ �����ϰ� �Ǵ� ����.    
    }


    private void OnTriggerEnter2D(Collider2D other)//�ݶ��̴��� �浹���� �� ����Ǵ� �Լ�
    {//�ݶ��̴��� Is Trigger�� ���� �־���Ѵٱ�!

        //�ε��� ������Ʈ�� �±װ� Enemy���� Ȯ���ϱ�
        if (other.CompareTag("Enemy"))
        {
            //EnemyHealth ��ũ��Ʈ�� ã�Ƽ� ���Ϳ��� ������ �߱�
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log(other.name + "���� �˱� �߻�ü ������ �ο�!");
            }
            ////�浹 �� �߻�ü�� �ٷ� �ı�
            //Destroy(gameObject);//���� Ȥ�� �ּ����� �޾�. �̰� ����� ���� �˱� �߻�ü�� ���͸� �����Ұž�,
                                  //������ Enemy �±׸� ���� ���Ϳ� ������ ������� Destroy(gameObject); ���ݾ�.
        }

    }
}
