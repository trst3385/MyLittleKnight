using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ObstacleFireBall : MonoBehaviour
{
    //08.3
    //MoveSpeed나 Damage 변수는 여기서나 인스펙터에서 수정한다고 바뀌지 않아.
    //ObstacleDifficultyManager 스크립트가 ObstacleFireBall에게 "야, 너 속도랑 데미지 이 값으로 해!" 라고 명령을 내리려면,
    //이 스크립트엔 그 값을 받을 수 있는 **'공간'**이 있어야 하기 때문이야.
    //그래서 ObstacleFireBall 스크립트의 moveSpeed와 damage 변수는 값을 받는 통로로서 반드시 필요하니까 그대로 두는 게 맞아. 
    //11/8일. MoveSpeed, Damage는 수정하지 않아도 되니 HideInInspector로 숨겼어

    [Header("발사체 관련 변수")]
    [HideInInspector] public float MoveSpeed = 5f;//발사체 이동속도 변수
    [HideInInspector] public int Damage = 5;//발사체의 공격력
    public float DestroyTime = 10f;//발사체가 사라지는 시간 변수
    [HideInInspector] public Vector2 MoveDirection;//발사체의 방향을 저장할 변수    
    //몬스터랑 발사체도 같은 프리팹인데 이곳엔 PlayerShield 선언변수가 없어,
    //이 발사체는 충돌 후 즉시 파괴되는 '단명 오브젝트'이라서, 
    //GetComponent로 찾은 주소를 private 변수에 따로 저장(캐싱)할 필요가 없기 때문이야,
    //Enemy 스크립트 처럼 반복 사용되는 오브젝트만 캐싱이 필요해
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
                playerShield.TakeShieldDamage(Damage);////PlayerShield 내부 로직이 쉴드 뚫리면 플레이어의 체력으로 데미지를 넘겨주므로,
                                                      //이 스크립트는 체력(PlayerHealth)을 신경 쓸 필요가 없음. (책임 분리)
            //데미지를 준 후 발사체는 파괴
            Destroy(gameObject);
        }
    }
}
