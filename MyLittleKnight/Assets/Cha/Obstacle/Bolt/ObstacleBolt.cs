using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ObstacleBolt : MonoBehaviour
{
    //08.3
    //moveSpeed나 damage 변수는 여기서나 인스펙터에서 수정한다고 바뀌지 않아.
    //ObstacleSpawner 스크립트가 ObstacleBolt에게 "야, 너 속도랑 데미지 이 값으로 해!" 라고 명령을 내리려면,
    //ObstacleBolt에 그 값을 받을 수 있는 **'공간'**이 있어야 하기 때문이야.
    //그래서 ObstacleBolt 스크립트의 moveSpeed와 damage 변수는 값을 받는 통로로서 반드시 필요하니까 그대로 두는 게 맞아. 
    //값은 다른 스크립트에서 다루지만 기본값을 넣어두면 나중에 프리팹 테스트를 위해 남겨는 둬!

    [Header("장애물 관련 변수")]
    public float MoveSpeed = 5f;//발사체 이동속도 변수
    public int Damage = 5;//발사체의 공격력
    public float DestroyTime = 10f;//발사체가 사라지는 시간 변수
    [HideInInspector] public Vector2 MoveDirection;//발사체의 방향을 저장할 변수
    //[HideInInspector]는 public 변수를 인스펙터에 안보이게 하는 기능이야!!!


    void Start()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (MoveDirection.x < 0)//왼쪽으로 갈 때
            //왼쪽으로 갈 때는 스프라이트를 뒤집지 않아
            spriteRenderer.flipX = false;
        else if (MoveDirection.x > 0)//오른쪽으로 갈 때
            spriteRenderer.flipX = true;//오른쪽으로 갈 때는 스프라이트를 좌우 반전시켜

        Destroy(gameObject, DestroyTime);//선언된 destroyTime 변수의 값(시간)에 맞춰 사라질 오브젝트가 사라질 시간을 설정
    }


    void FixedUpdate()//발사체는 물리적인 움직임이므로 FixedUpdate를 사용하는게 좋아
    {
        //현재 위치를 계속해서 이동 방향과 속도에 따라 업데이트
        //Kinematic Rigidbody를 사용하므로 직접 transform.position을 조절해
        transform.Translate(MoveDirection * MoveSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)//Is Trigger가 체크된 콜라이더와 충돌했을 때 호출되는 함수
    {                                              //이 함수는 Is Trigger가 체크된 콜라이더끼리 충돌했을 때 호출돼.
        if (other.CompareTag("Player"))//충돌한 상대방의 태그가 "Player"인지 확인
        {
            PlayerShield playerShield = other.GetComponent<PlayerShield>();
            if (playerShield != null)
                //PlayerShield 스크립트의 데미지 함수를 호출해서 방어력을 먼저 깎아.
                playerShield.TakeShieldDamage(Damage);


            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            //플레이어의 PlayerHealth 스크립트를 가져와서 데미지를 입히게 해
            if (playerHealth != null)//!= null은 null 상태가 아닐때 즉, 스크립트랑 제대로 연결됐을때 실행되는거야
                playerHealth.TakeDamage(Damage);

            //데미지를 준 후 발사체는 파괴
            Destroy(gameObject);
        }

    }

}
