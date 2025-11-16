using UnityEngine;

public class Sil_Player : MonoBehaviour
{
    [Header("원본 플레이어 연결")]
    //인스펙터에서 원본 Player 오브젝트의 Transform을 연결해
    public Transform playerTransform;


    //--내부 변수--

    //원본 플레이어에서 상태를 읽어올 컴포넌트들
    private Animator playerAnimator;
    private SpriteRenderer playerRenderer;
    //실루엣의 컴포넌트
    private Animator silplayerAnimator;
    private SpriteRenderer silhouetteRenderer;

    void Start()
    {
        silplayerAnimator = GetComponent<Animator>();
        silhouetteRenderer = GetComponent<SpriteRenderer>();

        if (playerTransform != null)
        {
            //원본 플레이어의 컴포넌트 가져오기
            playerAnimator = playerTransform.GetComponent<Animator>();
            playerRenderer = playerTransform.GetComponent<SpriteRenderer>();
        }

        //필수 컴포넌트 누락 시 오류 처리
        if (playerTransform == null || playerAnimator == null || playerRenderer == null)
        {
            Debug.LogError("Sil_Player: 원본 Player의 필수 컴포넌트(Transform, Animator, SpriteRenderer)를 찾을 수 없어! 인스펙터 연결 확인해!");
            enabled = false;//스크립트 비활성화
        }
    }

    void LateUpdate() //원본 플레이어 스크립트의 모든 이동(FixedUpdate) 및 애니메이션(Update) 로직이 
    {                 //완료된 후, 가장 마지막에 정확한 최종 위치를 복사하여
                      //화면 떨림 없이 부드러운 동기화를 보장하기 위해서 사용해
                      //**다른 오브젝트의 최종 상태에 종속되는 시각적 효과를 구현할 때 성능 저하 없이 최고의 부드러움을 보장하는 방법**
        if (!enabled) return;

        //위치 동기화
        transform.position = playerTransform.position;

        //이동 (Idle/Move) 상태 동기화
        bool isMoving = playerAnimator.GetBool("Move");
        silplayerAnimator.SetBool("Move", isMoving);

        //방향 동기화 (좌우 반전)
        //원본 Player 스크립트의 로직이 SpriteRenderer.flipX를 제어하고 있어.
        silhouetteRenderer.flipX = playerRenderer.flipX;
    }

    public void SilPlayerBowAttack()//활 공격 모션 동기화
    {
        if (!enabled) return;//스크립트 비활성화(에러) 상태라면 공격 동기화 무시

        if (silplayerAnimator != null)
            silplayerAnimator.SetTrigger("Attack(Bow)");
    }
    public void SilPlayerSwordAttack()//검 공격 모션 동기화(활 공격과 동일한 방식으로 추가)
    {
        if (!enabled) return;

        if (silplayerAnimator != null)
            silplayerAnimator.SetTrigger("Attack(Sword)");
    }

    public void SilPlayerDie()//Player 스크립트의 PlayerDie()함수에서 호출
    {
        if (silplayerAnimator != null)
        {
            silplayerAnimator.SetTrigger("Die");
            //사망 모션이 시작된 후, 실루엣 복사본도 원본과 거의 같은 시간에 파괴되어야 하니까,
            //원본 Player 스크립트의 PlayerDie함수의 Destroy(gameObject, DieTime);을 호출하므로,
            //실루엣도 이와 동일하게 파괴되도록 (원본의 DieTime 변수와 같은 시간으로)
            Destroy(gameObject, 1.5f);
        }
    }

}
