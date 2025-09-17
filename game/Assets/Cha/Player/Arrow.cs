using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("화살  데미지, 생성시간")]
    public float ArrowDamage = 1f;
    public float LifeTime = 3f;// ()초가 지나면 화살이 사라짐, 인스펙터에서 조절
   
    void Start()
    {   //화살이 생성되고 lifeTime에 설정한 시간이 지나면 화살오브젝트(gameObject)가 사라짐(파괴)
        Destroy(gameObject, LifeTime);
        //+나 * 같은 연산자가 아니라 ,를 쓰는 이유:
        //+나 * 같은 연산자는 덧셈이나 곱셈을 할 때 사용하는 거고, 함수에 인자를 전달할 때는 ','
        //쉼표(,)는 함수에서 여러 개의 인자(매개변수)를 구분할 때 사용하는 기호.
    }
    
    private void OnTriggerEnter2D(Collider2D other)//다른 Collider와 충돌 했을때 호출됨
    {//화살 오브젝트는 Collider2D에 Is Trigger가 체크되어 있을 때 작동

        //충돌한 오브젝트 태그가 Enemy인지 확인
        if (other.CompareTag("Enemy"))
        {
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();//EnemyHealth스크립트와 연결
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(ArrowDamage);
                Debug.Log($"{other.name}에게 {ArrowDamage} 데미지를 주었다!");
            }
            Destroy(gameObject);//적과 충돌했으니 사라짐(파괴)
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);//|| other.CompareTag("Obstacle"))//타일맵에 장애물이 있다면
        }                 
             
    }
}
