using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordEnergy : MonoBehaviour
{
    [Header("검기 발사체 지속시간")]
    public float DestroyTime = 5f;//발사체가 사라질 시간

    [HideInInspector]public float damage;//외부에서 데미지를 받아서 저장할 변수
    //HideInInspector는 public 변수를 인스펙터에 숨길 수 있어!

    void Start()
    {
        Destroy(gameObject, DestroyTime);//(DestroyTime)초 뒤에 발사체 오브젝트를 스스로 파괴
    }

    public void SetDamage(float amount)// 검 에너지의 최종 데미지 값을 설정하는 함수 (SwordWeapon에서 호출)
    {                            
        damage = amount;//SwordWeapon에서 계산된 'amount' 값을 이 스크립트의 'damage' 변수에 저장하여 적용                                           
    }


    private void OnTriggerEnter2D(Collider2D other)//콜라이더와 충돌했을 때 실행되는 함수
    {//콜라이더의 Is Trigger가 켜져 있어야한다구!

        //부딪힌 오브젝트의 태그가 Enemy인지 확인하기
        if (other.CompareTag("Enemy"))
        {
            //EnemyHealth 스크립트를 찾아서 몬스터에게 데미지 추기
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log(other.name + "에게 검기 발사체 데미지 부여!");
            }
        }
    }
}
