using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("ȭ��  ������, �����ð�")]
    public float ArrowDamage = 1f;
    public float LifeTime = 3f;// ()�ʰ� ������ ȭ���� �����, �ν����Ϳ��� ����
   
    void Start()
    {   //ȭ���� �����ǰ� lifeTime�� ������ �ð��� ������ ȭ�������Ʈ(gameObject)�� �����(�ı�)
        Destroy(gameObject, LifeTime);
        //+�� * ���� �����ڰ� �ƴ϶� ,�� ���� ����:
        //+�� * ���� �����ڴ� �����̳� ������ �� �� ����ϴ� �Ű�, �Լ��� ���ڸ� ������ ���� ','
        //��ǥ(,)�� �Լ����� ���� ���� ����(�Ű�����)�� ������ �� ����ϴ� ��ȣ.
    }
    
    private void OnTriggerEnter2D(Collider2D other)//�ٸ� Collider�� �浹 ������ ȣ���
    {//ȭ�� ������Ʈ�� Collider2D�� Is Trigger�� üũ�Ǿ� ���� �� �۵�

        //�浹�� ������Ʈ �±װ� Enemy���� Ȯ��
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();//EnemyHealth��ũ��Ʈ�� ����
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(ArrowDamage);
                Debug.Log($"{other.name}���� {ArrowDamage} �������� �־���!");
            }
            Destroy(gameObject);//���� �浹������ �����(�ı�)
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);//|| other.CompareTag("Obstacle"))//Ÿ�ϸʿ� ��ֹ��� �ִٸ�
        }                 
             
    }
}
