using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


//10.27일에 유니티 버전을 2022.3.61f1에서 Unity6.1의 6000.2.9f1로 업데이트해서 한글이 전부 깨져서 주석 대부분을 삭제했어
public enum WeaponType
{
    None,
    Bow,
    Sword,
    Axe
}

public class Player : MonoBehaviour
{
    [Header("이동 관련 변수")]
    public float MoveSpeed = 5f;    
    private float horizontalInput;
    private float verticalInput;
    private Vector2 movement;

    [HideInInspector]public bool IsDead = false;//인스펙터 사용하지 않으니 숨김

    [Header("실루엣 연결")]
    //씬에 있는 Sill_Player 복사본 오브젝트를 인스펙터에 연결
    [SerializeField] private Sil_Player silplayer;


    [Header("히트박스,몬스터가 쫒을 콜라이더")]
    [SerializeField] private Collider2D targetCollider;


    [Header("ScoreText UI 연결")]
    [SerializeField] private TextMeshProUGUI ScoreTextUI;


    [Header("Walk 사운드")]
    [SerializeField] private AudioSource walkingaudioSource;
    [SerializeField] private AudioClip walkSound;

    [Header("gameOverManager 스크립트 연결")]
    [SerializeField] private GameOverManager gameOverManager;

    [HideInInspector] public int CurrentScore = 0;//플레이어의 현재 점수(초기값 0)

    //내부변수
    private PlayerHealth playerHealth;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Awake()//내부 컴포넌트는 Awake에,
                //Null 체크도 Awake에. Start()보다 먼저 호출되기 때문에, 최대한 일찍 초기화하고 검증할수록 안정성이 높아져!
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) Debug.LogError("Rigidbody2D 컴포넌트를 찾을 수 없어! 다시 확인해봐!");

        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animator 컴포넌트를 찾을 수 없어! 플레이어 오브젝트에 Animator 컴포넌트를 확인해!");

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) Debug.LogError("SpriteRenderer 컴포넌트를 찾을수 없어! 다시 확인해봐!");
        
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null) Debug.LogError("PlayerHealth 컴포넌트를 찾을 수 없어! 플레이어 오브젝트에 PlayerHealth 컴포넌트를 확인해!");

        if (walkingaudioSource == null) Debug.LogError("AudioSource 컴포넌트를 찾을 수 없어! 인스펙터 제대로 확인 했어 안했어?!");
        if (gameOverManager == null) Debug.LogError("Player 스크립트에서 GameOverManager를 찾을 수 없어!");
        if (ScoreTextUI == null) Debug.LogError("ScoreTextUI가 인스펙터에 연결되지 않았어!");
    }
    

    void Start()//외부 스크립트, 오브젝트는 Start에
    {
        if (silplayer == null)//실루엣 스크립트가 연결되지 않으면
            Debug.LogWarning("Player: [실루엣 연결 누락!] sillplayer 변수가 인스펙터에 연결되지 않았어! 실루엣 기능이 작동하지 않을 거야!");

        if (ScoreTextUI != null) ScoreTextUI.text = "Score: " + CurrentScore;
        //게임 시작 시 ScoreTextUI에 플레이어의 초기 점수(0)를 표시하여 UI를 초기화
    }


    void FixedUpdate()//물리연산은 (이동 등) FixedUpdate.
    {
        rb.linearVelocity = movement * MoveSpeed;//살아있는 경우 정상적인 물리 이동
        //Rigidbody를 이용해 이동, velocity는 Rigidbody의 현재 속도를 나타냄
        //백터2의 movement값과 내 케릭터의 이동속도(Move)를 계산해 Rigidbody에 적용
    }

    void Update()
    {
        if (IsDead) return;//죽었으면 이후의 모든 로직을 건너뜀

        //살아있을 때만 입력 처리
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        movement = new Vector2(horizontalInput, verticalInput).normalized;//normalized로 이동 속도를 일정하게 유지

        HandleWalkingSound();//이동시엔 걷기 사운드(함수) 재생


        if (animator != null) animator.SetBool("Move", movement.magnitude > 0);

        // 케릭터 방향 전환(좌우반전)
        //<0 = Horizontal값이 0보다 작다, -1이 됐기에 왼쪽으로 이동
        //>0 = 반대로 Horizontal값이 0보다 크기에 오른쪽으로 이동
        if (horizontalInput < 0) spriteRenderer.flipX = true;
        else if (horizontalInput > 0) spriteRenderer.flipX = false;
    }

    public void PlayerDie()//플레이어 사망시
    {
        if (IsDead) return;

        IsDead = true;//죽으면 IsDead 실행
        Debug.Log("플레이어 사망!");

        if(walkingaudioSource != null && walkingaudioSource.isPlaying) walkingaudioSource.Stop();//걷기 사운드를 즉시 중지

        if (animator != null)
        {
            animator.SetTrigger("Die");

            float DieTime = 1.5f;//사망 후 사라지는 시간
            Destroy(gameObject, DieTime);

            Invoke("CallGameOverManager", 1f);//1.5초 뒤에 CallGameOverManager함수를 호출, 플레이어가 죽으면 뜰 UI
        }
        if (rb != null) rb.simulated = false;//물리 시뮬레이션 중지

        if (silplayer != null) silplayer.SilPlayerDie();//실루엣 복사본에게 사망 애니메이션 시작을 명령
    }

    
    private void CallGameOverManager() => gameOverManager?.OnGameOver();
    //플레이어가 죽으면 GameOverManager UI 호출, gameOverManager 스크립트의 OnGameOver함수 호출
    //?. (널 조건부 연산자): C# 6.0부터 도입된 문법, 객체가 Null이 아닐 때만 멤버(속성, 메서드)에 접근하도록 해주는 역할
    //"왼쪽 피연산자가 Null이 아닐 때만 오른쪽의 멤버(함수나 변수)에 접근하라" 라는 뜻이야!

    public Vector3 GetCenterPosition()//몬스터들이 플레이어의 '중앙'이라고 인식하고 추적/공격할 위치, Enemy 스크립트에서 이 함수를 참조
    {                             
        if (targetCollider != null) return targetCollider.bounds.center;//True일때, 플레이어 오브젝트에 Collider2D 컴포넌트가 붙어 있을 때

        return transform.position;//false일때, Collider2D 컴포넌트가 붙어 있지 않을 때
    }

    void HandleWalkingSound()//이동시 걷기 사운드 함수
    {
        //캐릭터가 움직이는지 확인
        bool isMoving = (horizontalInput != 0 || verticalInput != 0);

        if(isMoving)
        {
            if (walkingaudioSource != null && walkSound != null && !walkingaudioSource.isPlaying)
            {
                walkingaudioSource.clip = walkSound;
                walkingaudioSource.loop = true;//반복재생
                walkingaudioSource.Play();//위의 상태로 사운드 플레이 시작, true 상태일때 계속 소리가 들리는거야
            }
        }
        else//움직이지 않을 때 소리 멈춤
        {
            if (walkingaudioSource != null && walkingaudioSource.isPlaying) walkingaudioSource.Stop();
        }
    }

    public void AddScore(int amount)//점수를 추가하는 함수
    {
        CurrentScore += amount;//전달받은 amount(점수)만큼 점수를 더해줌
        Debug.Log("현재 점수: " + CurrentScore);

        if (ScoreTextUI != null) ScoreTextUI.text = "Score: " + CurrentScore.ToString();                             
    }
    public void ResetScore()//점수를 초기화하는 함수
    {
        CurrentScore = 0;
        if (ScoreTextUI != null) ScoreTextUI.text = "Score: 0";
    }

    public void SilhouetteBowAttack()//Sil_Player 스크립트의 SilPlayerBowAttack함수에게 활공격 모션을 한다고 전달
    {
        if (silplayer != null) silplayer.SilPlayerBowAttack();
    }
    public void SilhouetteSwordAttack()//Sil_Player 스크립트의 SilPlayerSwordAttack함수에게 검공격 모션을 한다고 전달
    {
        if (silplayer != null) silplayer.SilPlayerSwordAttack();
    }
}
