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

    void Awake()//내부 컴포넌트는 Awake
    {
        silplayerAnimator = GetComponent<Animator>();
        silhouetteRenderer = GetComponent<SpriteRenderer>();
    }
    void Start()//외부 컴포넌트는 Start
    {
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
        silhouetteRenderer.flipX = playerRenderer.flipX;//방향 동기화 (좌우 반전), Player 스크립트가 SpriteRenderer.flipX를 제어

        //원본 Animator에 레이어가 여러 개 있을 수 있으니, 모든 레이어를 하나씩 검사.
        //각 모션을 모두 복사해야 하므로 layerCount만큼 반복 
        for (int i = 0; i < playerAnimator.layerCount; i++)
        {
            //1. 원본 플레이어가 '현재' 어떤 상태인지 파악(걷는 중인지, 공격 중인지, 사망했는지)
            AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(i);

            //[실루엣에게 복사 명령] 실루엣 애니메이터에게 원본과 똑같이 따라 하라고 명령해.
            silplayerAnimator.Play(stateInfo.fullPathHash,//(무엇을?)원본이 지금 하는 그 애니메이션의 이름표(ID)
                                                        i,//(어디서?)원본과 같은 레이어(층)에서
                stateInfo.normalizedTime);//(어디부터?)원본이 진행된 그 지점(%)부터 이어서 재생해서 끊김 없는 부드러운 동기화
        }
    }
}
