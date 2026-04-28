using UnityEngine;

public class Sil_Player : MonoBehaviour
{
    //1.23. 이제 인스펙터에서 드래그하지 않아도 돼!(코드 내 연결)
    private Transform playerTransform;

    //--내부 변수--
    //원본 플레이어에서 상태를 읽어올 컴포넌트들
    private Animator playerAnimator;
    private SpriteRenderer playerRenderer;
    //실루엣의 컴포넌트
    private Animator silplayerAnimator;
    private SpriteRenderer silhouetteRenderer;

    void Awake()//내부 컴포넌트는 Awake
    {
        //실루엣 자신의 컴포넌트 자동 연결
        silplayerAnimator = GetComponent<Animator>();
        silhouetteRenderer = GetComponent<SpriteRenderer>();

        //부모(Player) 오브젝트 및 컴포넌트 자동 연결
        //실루엣은 Player의 자식이니까 transform.parent를 통해 부모를 찾아가면 돼
        if (transform.parent != null)
        {
            playerTransform = transform.parent;
            playerAnimator = playerTransform.GetComponent<Animator>();
            playerRenderer = playerTransform.GetComponent<SpriteRenderer>();
        }

        // [방어적 프로그래밍] 검증 로직
        CheckInitialization();
    }

    private void CheckInitialization()
    {
        if (playerTransform == null || playerAnimator == null || playerRenderer == null)
        {
            Debug.LogError("Sil_Player: 부모 Player의 필수 컴포넌트를 찾을 수 없어! 구조를 확인해봐!");
            enabled = false;//필수 컴포넌트 없으면 작동 중지
        }
    }

    void LateUpdate() //원본 플레이어 스크립트의 모든 이동(FixedUpdate) 및 애니메이션(Update) 로직이 
    {                 //완료된 후, 가장 마지막에 정확한 최종 위치를 복사하여
                      //화면 떨림 없이 부드러운 동기화를 보장하기 위해서 사용해
                      //**다른 오브젝트의 최종 상태에 종속되는 시각적 효과를 구현할 때 성능 저하 없이 최고의 부드러움을 보장하는 방법**

        //playerTransform이 null이거나 스크립트가 비활성 상태면 실행 안 함
        //Awake에서 연결에 실패했을 경우를 대비한 최소한의 안전장치!
        if (!enabled || playerTransform == null || playerAnimator == null)
        {
            return;
        }

        //위치 동기화
        transform.position = playerTransform.position;

        //이동 (Idle/Move) 상태 동기화
        bool isMoving = playerAnimator.GetBool("Move");
        silplayerAnimator.SetBool("Move", isMoving);
        silhouetteRenderer.flipX = playerRenderer.flipX;//방향 동기화 (좌우 반전), Player 스크립트가 SpriteRenderer.flipX를 제어

        //애니메이션 상태 상세 동기화(모든 레이어 반복)
        for (int i = 0; i < playerAnimator.layerCount; i++)//실루엣이 플레이어와 프레임 단위로 완벽하게 똑같이 움직이게 하려고,                                                  
        {                                                  //애니메이션 재생 시간(normalizedTime)을 강제로 맞출 목적
            //원본 플레이어가 '현재' 어떤 상태인지 파악(걷는 중인지, 공격 중인지, 사망했는지)
            AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(i);

            //실루엣 애니메이터를 원본과 동일하게 강제 동기화
            silplayerAnimator.Play(
            stateInfo.fullPathHash, //애니메이션 ID (무엇을?)
            i,                      //애니메이션 레이어 (어디서?)
            stateInfo.normalizedTime//재생 진행률 (어느 지점부터?)
            );                      
        }   
    }

    //---애니메이션 이벤트 수신기 (에러 방지용 빈 함수)---
    // 실루엣은 실제로 화살을 쏘거나 로직을 처리할 필요가 없으므로 
    // 이름만 같은 빈 함수를 만들어 에러를 막아준다.
    public void ShootArrow() { /* 실루엣은 화살을 쏘지 않음 */ }
    public void OnBowAttackEnd() { /* 실루엣은 공격 상태 체크가 필요 없음 */ }
    public void SwordAttack() {/* 위와 동일 */ }
    public void OnSwordAttackEnd() { /* 위와 동일 */ }
}
