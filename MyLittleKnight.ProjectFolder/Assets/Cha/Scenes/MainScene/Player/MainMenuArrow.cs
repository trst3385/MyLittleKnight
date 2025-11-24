using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuArrow: MonoBehaviour
{
    [Header("화살 이동 속도")]
    public float MoveSpeed = 20f;//화살이 날아가는 속도 (엄청 빠르게!)

    [Header("화살 방향 (1: 오른쪽, -1: 왼쪽)")]
    public int direction = -1;

    [Header("화살 수명")]
    public float LifeTime = 3f;//3초가 지나면 화살이 사라짐

    void Start()
    {
        Destroy(gameObject, LifeTime);//3초 후 화살 파괴 (화면 밖으로 나가든 안 나가든)
    }

    void Update()
    {
        //화살 이동: 방향 * 속도 * 시간
        float moveAmount = direction * MoveSpeed * Time.deltaTime;
        transform.position += new Vector3(moveAmount, 0, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //컷신에서는 몬스터에게 데미지를 주지 않고,
        //화살이 어떤 오브젝트에 닿으면(예: 몬스터, 배경) 그냥 사라지게만 처리

        //몬스터와 충돌했을 때 (옵션: 몬스터 태그가 있다면)
        if (other.CompareTag("Enemy")) Destroy(gameObject);
        else if (other.CompareTag("Ground")) Destroy(gameObject);//그 외 배경, 바닥에 충돌했을 때 (옵션: Ground 태그가 있다면)
    }
}
