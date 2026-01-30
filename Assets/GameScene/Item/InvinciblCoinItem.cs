using UnityEngine;

public class InvinciblCoinItem : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))//닿은 오브젝트가 플레이어 태그라면 실행
        {
            Player player = collision.GetComponent<Player>();

            if (player != null)
            {
                player.ActivateInvincibility();//플레이어의 무적 함수 실행


                Debug.Log("무적 코인 획득!");//획득 후 아이템 삭제
                Destroy(gameObject);
            }
        }
    }
}
