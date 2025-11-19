using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("화살  데미지, 생성시간")]
    public float ArrowDamage = 1f;
    public float LifeTime = 3f;//()초가 지나면 화살이 사라짐
   
    void Start()
    {   
        Destroy(gameObject, LifeTime);   
    }
    
    private void OnTriggerEnter2D(Collider2D other)//다른 Collider와 충돌 했을때 호출
    {
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
        else if (other.CompareTag("Ground")) Destroy(gameObject);//화살이 타일맵 경계선에 닿으면 파괴
        //콜라이더 범위(타일맵 밖) 밖으로 화살이 나가면 화살 파괴, 타일맵(Tag:Ground) 안에 있어서 OnTriggerEnter2D의 상황이 아님!
        //타일맵의 콜라이더 밖 (경계선)에 화살이 닿으면 그때 함수가 작동해 화살이 파괴
    }
}
