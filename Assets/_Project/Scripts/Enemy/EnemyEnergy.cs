using System.Collections.Generic;
using UnityEngine;

public class EnemyEnergy : MonoBehaviour
{
    [Header("검기 발사체 지속시간")]
    public float DestroyTime = 5f;//발사체가 사라질 시간
    [HideInInspector] public float damage;//외부에서 데미지를 받아서 저장할 변수

    //플레이어는 한 명이지만, 혹시 모를 중복 충돌 방지용
    private List<GameObject> hitPlayer = new List<GameObject>();

    void Start()
    {
        Destroy(gameObject, DestroyTime);//(DestroyTime)초 뒤에 발사체 오브젝트를 스스로 파괴
    }

    public void SetDamage(float amount)//보스 스크립트에서 데미지를 넣어줄 함수
    {
        damage = amount;                                     
    }


    private void OnTriggerEnter2D(Collider2D other)//플레이어 콜라이더와 충돌했을 때 실행되는 함수
    {
        //부딪힌 오브젝트의 태그가 Player인지 확인하기
        if (other.CompareTag("Player"))
        {
            if (hitPlayer.Contains(other.gameObject)) return;//중복 데미지 방지

            //플레이어의 방패/체력 스크립트 가져오기(방어력 스크립트 내부에 체력 스크립트로 전달하는 로직이 있다)
            PlayerShield shield = other.GetComponent<PlayerShield>();

            if (shield != null)
            {
                hitPlayer.Add(other.gameObject);

                //데미지를 주면 Shield 스크립트가 알아서 Health까지 관리해줌
                shield.TakeShieldDamage(damage);

                Debug.Log($"보스 발사체가 플레이어에게 {damage} 데미지를 전달했어!");

                //보스 에너지파는 보통 부딪히면 사라지니까 바로 파괴
                Destroy(gameObject);
            }
        }
    }
}
