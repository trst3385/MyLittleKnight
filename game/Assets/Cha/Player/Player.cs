using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR//UnityEditor 네임스페이스를 사용하고 있는 스크립트가 빌드에 포함될 때 발생해.
using UnityEditor.UIElements;
//UnityEditor는 에디터에서만 작동하는 기능이라, 실제 게임 빌드에는 포함되면 안 되거든.
#endif//#if UNITY_EDITOR 사용해 해당 코드를 다음과 같이 전처리기 지시문으로 감싸줘.
using UnityEngine;
using UnityEngine.UI;
using TMPro;//TextMeshPro 사용 시 using 문에 TMPro를 추가해


//함수가 길어진다 싶으면 따로 함수 만들기!!
//나중에 if문이 많아지면 bool로 선언문으로 짧게 하기!!

//WeaponType은 PlayerAttack 스크립트에서 관리하는게 더 적절하지만
//만약 Item 스크립트에서 Player.currentWeaponType에 바로 접근해서 무기를 변경하는 식이라면
//여기에 남겨두는 것도 나쁘지 않아. (선택사항)

public enum WeaponType
{
    None,
    Bow,
    Sword,
    Axe
    //나중에 다른 무기 종류를 추가할 때 여기에 더 적어주자
}

public class Player : MonoBehaviour
{
    [Header("이동 관련 변수")]
    //이동 관련 변수
    public float MoveSpeed = 5f;//이동 속도     
    private float horizontalInput;
    private float verticalInput;
    private Vector2 movement;

    [Header("사망 관련 변수")]
    //사망 관련 변수
    public bool IsDead = false;//플레이어의 사망 변수,PlayerHealth스크립트에 쓰기 위해 public으로 선언   
    private PlayerHealth playerHealth; // PlayerHealth 스크립트 참조
    private GameOverManager gameOverManager;//GameOverManager 스크립트 참조


    //몬스터 처치 후 받을 점수를 저장할 변수
    //이 변수 값을 다룰 일이 없고, 또 이 변수도 다른 스크립트 등에서 써야 하니 [HideInInspector]로 인스펙터에 보이지 않게 했어
    [HideInInspector] public int CurrentScore = 0;//플레이어의 현재 점수(초기값 0)

    [Header("ScoreText UI 연결")]
    public TextMeshProUGUI ScoreTextUI;
    //TextMeshProUGUI: 주로 UI Canvas 위에 텍스트를 배치할 때 사용하는 컴포넌트야.
    //UGUI(Unity UI) 시스템의 일부로, 화면에 고정되어 게임 플레이와 별도로 표시되는 인터페이스 요소에 사용해.
    //예를 들어, 현재 점수, 체력 바, 인벤토리 창, 메뉴 버튼의 텍스트처럼 게임 화면 위에 오버레이되는 텍스트에 쓰여.

    //TextMeshPro: 주로 3D 공간에 텍스트를 배치할 때 사용하는 컴포넌트야.
    //게임 오브젝트의 일부로 3D 월드 안에 텍스트를 띄우고 싶을 때 사용해.예를 들어,
    //몬스터의 이름표나 머리 위에 뜨는 데미지 숫자, 퀘스트 오브젝트의 설명 텍스트 같은 경우에 쓰여.
    //이 텍스트는 유니티 씬의 3D 공간에 존재하고, 카메라에 의해 렌더링 돼.

    //GetComponent<T>() vs [SerializeField] 차이
    //GetComponent<T>(): 이 함수는 코드를 통해서 게임 오브젝트의 컴포넌트를 가져올 때 사용하는 거야. 주로 Start()나 Awake() 함수에서 실행돼.
    //[SerializeField]: 이 키워드는 변수를 인스펙터에 노출시켜서 개발자가 직접 컴포넌트를 연결하게 만드는 역할을 해.
    //GetComponent<T>()를 꼭 써야 하는 건 아니고, [SerializeField]로 선언된 변수는 인스펙터에서 직접 드래그해서 연결하면 돼
    //그래서 GetComponent<AudioSource>()를 쓰지 않고 [SerializeField]를 사용해 인스펙터에서 오디오 컴포넌트를 그래드 해서 연결했어.
    //한 오브젝트에 여러 사운드를 넣을려면 [SerializeField] private를 써서 드래그해서 연결 시켜주자!

    [Header("Walk사운드")]//사운드 관련 변수, 헤더로 인스펙터에 잘 보이게 하자!
    [SerializeField] private AudioSource walkingaudioSource;
    [SerializeField] private AudioClip walkSound;
    //소리를 더 빠르게 재생하고 싶으면 AudioSource 컴포넌트의 Pitch 속성을 조절하면 돼.
    //AudioSource 컴포넌트에서 Play On Awake이란?
    //켜는 경우: 게임 시작 시(오브젝트가 생성/활성화될 때) 자동으로 배경 음악처럼 소리를 재생하고 싶을 때 사용해.
    //끄는 경우: 특정 이벤트(예: 플레이어가 걷기 시작하거나, 몬스터가 죽거나, 공격할 때)에 맞춰서 소리를 재생하고 싶을 때 사용해.
    //내 캐릭터의 경우, isMoving 상태에 따라 걷기 소리를 재생/정지하는 로직을 코드로 직접 구현했기 때문에,
    //Play On Awake가 켜져 있어도 시작 시 소리가 들리지 않는 거야.네 코드가 먼저 "안 움직이고 있으니 소리를 재생하지 마"라고 명령하기 때문.

    //컴포넌트 참조
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
        gameOverManager = FindObjectOfType<GameOverManager>();

        if (walkingaudioSource == null) 
            Debug.LogError("Player: AudioSource 컴포넌트를 찾을 수 없어! 인스펙터 제대로 확인 했어 안했어?!");
        if (animator == null) 
            Debug.LogError("Start: Animator 컴포넌트를 찾을 수 없어! 플레이어 오브젝트에 Animator 컴포넌트를 확인해!"); 
        if (playerHealth == null) 
            Debug.LogError("Start: PlayerHealth 컴포넌트를 찾을 수 없어! 플레이어 오브젝트에 PlayerHealth 컴포넌트를 확인해!");
        if (gameOverManager == null) 
            Debug.LogError("Player 스크립트에서 GameOverManager를 찾을 수 없습니다!");
        //== null은 그 컴포넌트나 오브젝트가 null상태. 즉 없거나 비어있는 상태일때 나올 디버그 에러
        //!=은 반대로 null상태가 아닌 상태
        if (ScoreTextUI != null) 
            ScoreTextUI.text = "Score: " + CurrentScore.ToString();
        //게임 시작 시 ScoreText UI에 초기 점수 표시
        //currentScore int형 변수라서 ToString이 없어도 컴파일러가 자동으로 문자열로 변환.
        //하지만 ToString()을 사용하는 것은 코드의 명확성,안정성,미래에 더 복잡한 형식 지정이 필요할 때를 대비한 좋은 습관이야
        
    }
    void FixedUpdate() //물리연산때는 FixedUpdate.시간에 관계없이 정확하고 일관된 결과가 필요한 물리 시뮬레이션.
    {//물리연산때는 이 함수에!!   
       
        if (IsDead)//사망 상태면 움직이지 않게하기
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = movement * MoveSpeed;
        //Rigidbody를 이용해 이동, velocity는 Rigidbody의 현재 속도를 나타냄
        //백터2의 movement값과 내 케릭터의 이동속도(Move)를 계산해 Rigidbody에 적용
    }
    public void PlayerDie()//플레이어 사망시 호출될 함수
    {
        if (IsDead) 
            return;

        IsDead = true;//사망 상태로 변경, 맨 위에 선언에는 아직 사망 모션을 하면 안되니 false로
        Debug.Log("플레이어 사망!");

        if (animator != null)
        {
            animator.SetTrigger("Die");//Animator의 "Die" 트리거 발동

            float DieTime = 1.5f; //사망 후 사라지는 시간
            Destroy(gameObject, DieTime);

            //1.5초 뒤에 CallGameOverManager함수를 호출, 플레이어가 죽으면 뜰 UI
            Invoke("CallGameOverManager", 1f);
        }
        //사망시에는 이동 입력 받지 않기
        horizontalInput = 0;
        verticalInput = 0;
        movement = Vector2.zero;

        if (rb != null) 
            rb.simulated = false;//물리 시뮬레이션 중지  
    }
    private void CallGameOverManager()//플레이어가 죽으면 GameOverManager UI 호출     
    {//Invoke는 오직 string으로 된 함수 이름만 받아서 호출할 수 있거든,
     //PlayerDie() 함수 안에 있는 if문 코드 블록은 Invoke가 직접 실행할 수 없는 형식이야,
     //그래서 if문을 CallGameOverManager라는 함수 안에 넣고, Invoke는 그 함수 이름,
     //("CallGameOverManager")을 호출하게 만든 거지, 이렇게 하면 Invoke의 규칙도 지키면서 네가 원하는 기능을 구현할 수 있어.
     //CallGameOverManager 함수는 Invoke를 위해 만들어진 '작은 도우미 함수' 라고 보면 돼.
        if (gameOverManager != null) 
            gameOverManager.OnGameOver();//gameOverManager 스크립트의 OnGameOver함수 호출
        
    }   

    void Update()//프레임당 업데이트 로직
    {      
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        movement = new Vector2(horizontalInput, verticalInput).normalized;//normalized로 이동 속도를 일정하게 유지

        //걷기 사운드 재생 로직 호출
        HandleWalkingSound();

        if (IsDead)//죽었으면 아무 입력도 받지 않고 움직이지 않음
        {
            horizontalInput = 0;
            verticalInput = 0;
            movement = Vector2.zero;
            return;//죽었으면 여기서 함수 종료
        }


        if (animator != null) 
            animator.SetBool("Move", movement.magnitude > 0);

        // 케릭터 방향 전환(좌우반전)
        //<0 = Horizontal값이 0보다 작다, -1이 됐기에 왼쪽으로 이동
        //>0 = 반대로 Horizontal값이 0보다 크기에 오른쪽으로 이동
        if (horizontalInput < 0) 
            spriteRenderer.flipX = true;//왼쪽으로 이동
        else if (horizontalInput > 0) 
            spriteRenderer.flipX = false;//오른쪽으로 이동
    }

    public Vector3 GetCenterPosition()//몬스터들이 플레이어의 '중앙'이라고 인식하고 추적/공격할 위치
    {
        //BoxCollider2D가 있다면 콜라이더의 중심 사용
        Collider2D playerCollider = GetComponent<Collider2D>();

        if (playerCollider != null)
        {
            return playerCollider.bounds.center;//True일때 
        }
        return transform.position;//false일때
    }

    void HandleWalkingSound()
    {
        //캐릭터가 움직이는지 확인
        bool isMoving = (horizontalInput != 0 || verticalInput != 0);

        if(isMoving)
        {
            //소리가 재생 중이 아닐 때만 재생
            //오디오 컴포넌트와 소리 파일이 모두 연결되어 있고, 현재 걷기 소리가 재생 중이 아니라면, 소리를 재생해! 라는 의미야.
            if (walkingaudioSource != null && walkSound != null && !walkingaudioSource.isPlaying)
            {
                walkingaudioSource.clip = walkSound;
                walkingaudioSource.loop = true;//반복 재생
                walkingaudioSource.Play();//위의 상태로 사운드 플레이 시작!
            }                             //true 상태일때 계속 소리가 들리는거야!
        }
        else
        {
            //움직이지 않을 때 소리 멈춤
            if (walkingaudioSource != null && walkingaudioSource.isPlaying) 
                walkingaudioSource.Stop();
        }
    }

    public void AddScore(int amount)//점수를 추가하는 함수
    {
        CurrentScore += amount;//전달받은 amount(점수)만큼 점수를 더해줌
        Debug.Log("현재 점수: " + CurrentScore);

        //UI 텍스트가 연결되어 있는지 확인
        if (ScoreTextUI != null) 
            ScoreTextUI.text = "Score: " + CurrentScore.ToString();
                                 //ToString()은 int 값을 문자열로 바꿔주는 함수야
    }
    public void ResetScore()//점수를 초기화하는 함수
    {
        CurrentScore = 0;
        //인게임 점수 UI도 초기화하고 싶다면 이 줄도 추가
        if (ScoreTextUI != null) 
            ScoreTextUI.text = "Score: 0";
    }
}
