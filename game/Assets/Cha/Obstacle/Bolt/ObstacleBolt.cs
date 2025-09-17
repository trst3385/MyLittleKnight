using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ObstacleBolt : MonoBehaviour
{
    //08.3
    //moveSpeed�� damage ������ ���⼭�� �ν����Ϳ��� �����Ѵٰ� �ٲ��� �ʾ�.
    //ObstacleSpawner ��ũ��Ʈ�� ObstacleBolt���� "��, �� �ӵ��� ������ �� ������ ��!" ��� ����� ��������,
    //ObstacleBolt�� �� ���� ���� �� �ִ� **'����'**�� �־�� �ϱ� �����̾�.
    //�׷��� ObstacleBolt ��ũ��Ʈ�� moveSpeed�� damage ������ ���� �޴� ��ημ� �ݵ�� �ʿ��ϴϱ� �״�� �δ� �� �¾�. 
    //���� �ٸ� ��ũ��Ʈ���� �ٷ����� �⺻���� �־�θ� ���߿� ������ �׽�Ʈ�� ���� ���ܴ� ��!

    [Header("��ֹ� ���� ����")]
    public float MoveSpeed = 5f;//�߻�ü �̵��ӵ� ����
    public int Damage = 5;//�߻�ü�� ���ݷ�
    public float DestroyTime = 10f;//�߻�ü�� ������� �ð� ����
    [HideInInspector] public Vector2 MoveDirection;//�߻�ü�� ������ ������ ����
    //[HideInInspector]�� public ������ �ν����Ϳ� �Ⱥ��̰� �ϴ� ����̾�!!!


    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (MoveDirection.x < 0)//�������� �� ��
            //�������� �� ���� ��������Ʈ�� ������ �ʾ�
            spriteRenderer.flipX = false;
        else if (MoveDirection.x > 0)//���������� �� ��
            spriteRenderer.flipX = true;//���������� �� ���� ��������Ʈ�� �¿� ��������

        Destroy(gameObject, DestroyTime);//����� destroyTime ������ ��(�ð�)�� ���� ����� ������Ʈ�� ����� �ð��� ����
    }


    void FixedUpdate()//�߻�ü�� �������� �������̹Ƿ� FixedUpdate�� ����ϴ°� ����
    {
        //���� ��ġ�� ����ؼ� �̵� ����� �ӵ��� ���� ������Ʈ
        //Kinematic Rigidbody�� ����ϹǷ� ���� transform.position�� ������
        transform.Translate(MoveDirection * MoveSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)//Is Trigger�� üũ�� �ݶ��̴��� �浹���� �� ȣ��Ǵ� �Լ�
    {                                              //�� �Լ��� Is Trigger�� üũ�� �ݶ��̴����� �浹���� �� ȣ���.
        if (other.CompareTag("Player"))//�浹�� ������ �±װ� "Player"���� Ȯ��
        {
            PlayerShield playerShield = other.GetComponent<PlayerShield>();
            if (playerShield != null)
                //PlayerShield ��ũ��Ʈ�� ������ �Լ��� ȣ���ؼ� ������ ���� ���.
                playerShield.TakeShieldDamage(Damage);


            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            //�÷��̾��� PlayerHealth ��ũ��Ʈ�� �����ͼ� �������� ������ ��
            if (playerHealth != null)//!= null�� null ���°� �ƴҶ� ��, ��ũ��Ʈ�� ����� ��������� ����Ǵ°ž�
                playerHealth.TakeDamage(Damage);

            //�������� �� �� �߻�ü�� �ı�
            Destroy(gameObject);
        }

    }

}
