using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPlayer : MonoBehaviour
{
    [HideInInspector]public float MoveSpeed;//MainMenuCutsceneManager 스크립트가 이 변수를 제어함
    public Animator animator;
    public int direction = 1;//1은 오른쪽, -1은 왼쪽

    void Start()
    {
        transform.localScale = new Vector3(10, 10, 1);
        animator.SetBool("Move", true);
    }

    void Update()
    {
        //direction 값에 따라 이동 방향을 결정
        transform.Translate(Vector3.right * MoveSpeed * Time.deltaTime * direction);

        //방향이 바뀔 때 캐릭터 스프라이트를 반전시켜야 해
        if (direction == 1)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
}
