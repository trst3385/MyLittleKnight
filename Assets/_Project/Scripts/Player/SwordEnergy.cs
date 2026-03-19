using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordEnergy : MonoBehaviour
{
    [Header("검기 발사체 지속시간")]
    public float DestroyTime = 5f;//발사체가 사라질 시간

    [HideInInspector]public float damage;//외부에서 데미지를 받아서 저장할 변수
    //HideInInspector는 public 변수를 인스펙터에 숨길 수 있어!

    //이미 데미지를 입힌 적들을 저장하는 리스트
    private List<GameObject> hitEnemies = new List<GameObject>();

    void Start()
    {
        Destroy(gameObject, DestroyTime);//(DestroyTime)초 뒤에 발사체 오브젝트를 스스로 파괴
    }

    public void SetDamage(float amount)// 검 에너지의 최종 데미지 값을 설정하는 함수 (SwordWeapon에서 호출)
    {                            
        damage = amount;//SwordWeapon에서 계산된 'amount' 값을 이 스크립트의 'damage' 변수에 저장하여 적용                                           
    }


    private void OnTriggerEnter2D(Collider2D other)//(몬스터의)콜라이더와 충돌했을 때 실행되는 함수
    {
        //부딪힌 오브젝트의 태그가 Enemy인지 확인하기
        if (other.CompareTag("Enemy"))
        {
            if (hitEnemies.Contains(other.gameObject)) return;//리스트에 이미 있는 적이라면 데미지를 주지 않고 무시(return)해

            //EnemyHealth 스크립트를 찾아서 몬스터에게 데미지 추기
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                hitEnemies.Add(other.gameObject);//데미지를 주기 전에 리스트에 추가해서 '이미 맞았음'을 표시

                enemyHealth.TakeDamage(damage);
                Debug.Log(other.name + "에게 검기 발사체 데미지 부여!");
            }
        }
    }
}
