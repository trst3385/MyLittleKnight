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

    [Header("사망 관련 변수")]
    public bool IsDead = false;


    [Header("히트박스,몬스터가 쫒을 콜라이더")]
    [SerializeField] private Collider2D targetCollider;


    [Header("ScoreText UI 연결")]
    public TextMeshProUGUI ScoreTextUI;


    [Header("Walk 사운드")]
    [SerializeField] private AudioSource walkingaudioSource;
    [SerializeField] private AudioClip walkSound;


    [HideInInspector] public int CurrentScore = 0;//플레이어의 현재 점수(초기값 0)

    //내부변수
    private PlayerHealth playerHealth;
    private GameOverManager gameOverManager;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
        gameOverManager = FindAnyObjectByType<GameOverManager>();
        
        if (walkingaudioSource == null)
            Debug.LogError("Player: AudioSource 컴포넌트를 찾을 수 없어! 인스펙터 제대로 확인 했어 안했어?!");
        if (animator == null)
            Debug.LogError("Start: Animator 컴포넌트를 찾을 수 없어! 플레이어 오브젝트에 Animator 컴포넌트를 확인해!");
        if (playerHealth == null)
            Debug.LogError("Start: PlayerHealth 컴포넌트를 찾을 수 없어! 플레이어 오브젝트에 PlayerHealth 컴포넌트를 확인해!");
        if (gameOverManager == null)
            Debug.LogError("Player 스크립트에서 GameOverManager를 찾을 수 없어!");

        if (ScoreTextUI != null)//!=은 반대로 null상태가 아닌 상태
            ScoreTextUI.text = "Score: " + CurrentScore.ToString();
        //currentScore int형 변수라서 ToString이 없어도 컴파일러가 자동으로 문자열로 변환.
        //하지만 ToString()을 사용하는 것은 코드의 명확성,안정성,미래에 더 복잡한 형식 지정이 필요할 때를 대비한 좋은 습관이야
    }
    void FixedUpdate()//물리연산은 (이동 등) FixedUpdate.
    {
        if (IsDead)//플레어가 죽으면 모든 행동을 중지하고 사망 모션 행동
        {
            rb.linearVelocity = Vector2.zero;//물리 속도 0으로 설정
            return;                          //이동 연산 차단
        }
        rb.linearVelocity = movement * MoveSpeed;//살아있는 경우 정상적인 물리 이동

        //Rigidbody를 이용해 이동, velocity는 Rigidbody의 현재 속도를 나타냄
        //백터2의 movement값과 내 케릭터의 이동속도(Move)를 계산해 Rigidbody에 적용
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        movement = new Vector2(horizontalInput, verticalInput).normalized;//normalized로 이동 속도를 일정하게 유지

        HandleWalkingSound();//이동시엔 걷기 사운드(함수) 재생

        if (IsDead)//죽었으면 아무 입력도 받지 않고 움직이지 않기
        {
            horizontalInput = 0;
            verticalInput = 0;
            movement = Vector2.zero;
            return;
        }


        if (animator != null)
            animator.SetBool("Move", movement.magnitude > 0);

        // 케릭터 방향 전환(좌우반전)
        //<0 = Horizontal값이 0보다 작다, -1이 됐기에 왼쪽으로 이동
        //>0 = 반대로 Horizontal값이 0보다 크기에 오른쪽으로 이동
        if (horizontalInput < 0)
            spriteRenderer.flipX = true;
        else if (horizontalInput > 0)
            spriteRenderer.flipX = false;
    }

    public void PlayerDie()//플레이어 사망시
    {
        if (IsDead) 
            return;

        IsDead = true;
        Debug.Log("플레이어 사망!");

        if (animator != null)
        {
            animator.SetTrigger("Die");

            float DieTime = 1.5f;//사망 후 사라지는 시간
            Destroy(gameObject, DieTime);

          
            Invoke("CallGameOverManager", 1f);//1.5초 뒤에 CallGameOverManager함수를 호출, 플레이어가 죽으면 뜰 UI
        }
        //사망시에는 이동 입력 받지 않기
        horizontalInput = 0;
        verticalInput = 0;
        movement = Vector2.zero;

        if (rb != null) 
            rb.simulated = false;//물리 시뮬레이션 중지  
    }
    private void CallGameOverManager()//플레이어가 죽으면 GameOverManager UI 호출
    {
        if (gameOverManager != null) 
            gameOverManager.OnGameOver();//gameOverManager 스크립트의 OnGameOver함수 호출
    }   

    public Vector3 GetCenterPosition()//몬스터들이 플레이어의 '중앙'이라고 인식하고 추적/공격할 위치, Enemy 스크립트에서 이 함수를 참조
    {                             
        if (targetCollider != null)
        {
            return targetCollider.bounds.center;//True일때, 플레이어 오브젝트에 Collider2D 컴포넌트가 붙어 있을 때.
        }
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
            if (walkingaudioSource != null && walkingaudioSource.isPlaying) 
                walkingaudioSource.Stop();
        }
    }

    public void AddScore(int amount)//점수를 추가하는 함수
    {
        CurrentScore += amount;//전달받은 amount(점수)만큼 점수를 더해줌
        Debug.Log("현재 점수: " + CurrentScore);

        if (ScoreTextUI != null) 
            ScoreTextUI.text = "Score: " + CurrentScore.ToString();
                               
    }
    public void ResetScore()//점수를 초기화하는 함수
    {
        CurrentScore = 0;
        if (ScoreTextUI != null) 
            ScoreTextUI.text = "Score: 0";
    }
}
