using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("화살  데미지, 생성시간")]
    [HideInInspector]public float ArrowDamage = 1f;
    public float LifeTime = 3f;//()초가 지나면 화살이 사라짐

    [Header("강화 화살 속성")]
    [HideInInspector] public bool IsEnhanced = false;//강화 화살인지 (BowWeapon 스크립트에서 설정됨)
    [HideInInspector] public float SlowFactor = 0f;//몬스터 이동 속도 감소 비율 (0.5f = 50% 느려짐)

    private List<GameObject> hitEnemies = new List<GameObject>();//이미 강화화살에 맞은 적들을 기억할 리스트

    void Start()
    {   
        Destroy(gameObject, LifeTime);   
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))//충돌한 오브젝트 태그가 Enemy인지 확인
        {
            if (hitEnemies.Contains(other.gameObject))//이미 맞은 적이면 무시 (다단 히트 방지)
            {
                return;
            }

            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                hitEnemies.Add(other.gameObject);//이미 화살에 맞은 적인지 확인

                enemyHealth.TakeDamage(ArrowDamage);
                Debug.Log($"{other.name}에게 {ArrowDamage} 데미지!");

                if (IsEnhanced)
                {
                    Enemy enemyScript = other.GetComponent<Enemy>();//EnemyHealth 스크립트에서 몬스터 오브젝트의 Enemy 스크립트를 찾음
                    if (enemyScript != null)
                    {
                        enemyScript.ApplySlowEffect(SlowFactor);//Enemy 스크립트의 ApplySlowEffect(강화 화살에 맞으면 슬로우) 호출
                        Debug.Log($"{other.name}에게 슬로우 효과 적용됨!");
                    }
                }
            }

            if (!IsEnhanced)//일반 화살은 충돌 후 파괴
            {
                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Ground"))//화살이 Ground태그인 타일맵 경계선에 닿으면 파괴
        {
            Destroy(gameObject);
        }
    }
}
