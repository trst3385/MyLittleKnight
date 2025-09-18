using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuEnemy : MonoBehaviour
{
    [HideInInspector]public float MoveSpeed;//MainMenuCutsceneManager ��ũ��Ʈ�� �� ������ ������
    public Animator animator;
    public int direction = 1;//1�� ������, -1�� ����

    public Vector3 enemyScale = new Vector3(8, 8, 1);//���͸��� ũ�� ����(�ν����Ϳ��� ����)

    void Start()
    {
        transform.localScale = enemyScale;
        animator.SetBool("Move", true);
    }

    void Update()
    {
        //direction ���� ���� �̵� ������ ����
        transform.Translate(Vector3.right * MoveSpeed * Time.deltaTime * direction);

        //������ �ٲ� �� ĳ���� ��������Ʈ�� �������Ѿ� ��
        if (direction == 1)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);  
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);


    }
}
