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
        //n초 뒤에 발사체 오브젝트를 스스로 파괴
        Destroy(gameObject, DestroyTime);
    }

    public void SetDamage(float amount)//PlayerAttack 스크립트로부터 데미지 값을 받아오는 함수
    {                                  //PlayerAttack 스크립트에서 swordenergy.SetDamage(totalDamage); 코드를 호출하면,
        damage = amount;               //totalDamage 값(ex: 5)이 amount라는 변수에 담겨서 SwordEnergy 스크립트의 SetDamage 함수로 전달돼.
                                       //그렇게 전달받은 5라는 값을 damage 변수에 저장하게 되는 거지.    
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
            ////충돌 후 발사체는 바로 파괴
            //Destroy(gameObject);//삭제 혹은 주석으로 달아. 이걸 지우니 이제 검기 발사체는 몬스터를 관통할거야,
                                  //원래는 Enemy 태그를 가진 몬스터와 닿으면 사라지게 Destroy(gameObject); 했잖아.
        }

    }
}
