using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum WeaponType { None, Bow, Sword, Axe }

public class Player : MonoBehaviour
{
    //---------옵저버 패턴----------//
    //static으로 선언하면 어디서든 'Player. ...'로 접근할 수 있어 편리해.
    public static event Action OnPlayerDead;//방송 채널 개설: "플레이어가 죽으면 송출되는 채널", 옵저버 패턴의 시작이야.
    public static event Action<int> OnScoreChanged;//점수 갱신 방송 채널(이벤트)
    public static event Action<int, int> OnMoveSpeedLevelChanged;//이동 속도 변경 방송 채널 (현재 속도, 최대 레벨 등 전달용)
    //-----------------------------//

    //SO 참조 (인스펙터에서 여기에만 연결)
    [Header("설정 데이터")]
    [SerializeField] private PlayerStatsSO stats;
    public PlayerStatsSO Stats => stats;//다른 스크립트에서 읽을 수 있게 프로퍼티 제공

    [Header("이동 관련 변수")]
    private float moveSpeed;
    private int currentMoveSpeedLevel = 0;//현재 이동속도
    private int maxMoveSpeedLevel = 10;//최대 이속 증가 레벨

    [Header("무적 상태")]
    [Tooltip("체력 감소가 되지 않는 무적 상태인지 여부")]
    public bool isInvincible = false;//플레이어가 지금 무적상태인지 확인(무적, 방어력 스크립트에서 참조)


    //외부 참조 변수 (캐싱용)
    private Sil_Player silplayer;
    private Collider2D targetCollider;
    private AudioSource walkingaudioSource;

    //내부 컴포넌트 변수 (캐싱용)
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    //상태 및 내부 데이터 변수
    [HideInInspector] public bool IsDead = false;
    [HideInInspector] public int CurrentScore = 0;

    private float horizontalInput;
    private float verticalInput;
    private Vector2 movement;


    void Awake()//내부 컴포넌트는 Awake에,              
    {           //Null 체크도 Awake에. Start()보다 먼저 호출되기 때문에, 최대한 일찍 초기화하고 검증할수록 안정성이 높아져

        if (stats != null)//SO에서 데이터를 가져와서 초기화하기
        {
            moveSpeed = stats.moveSpeed;
            maxMoveSpeedLevel = stats.maxMoveSpeedLevel;
        }
        else
        {
            Debug.LogError("PlayerStatsSO가 연결되지 않았어! 인스펙터를 확인해줘.");
        }

        //내부 컴포넌트 자동 연결
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //자식 오브젝트 및 사운드 자동 연결, 자식 중에서 이름으로 찾기, 개별 볼륨 및 믹서 제어를 위해 자식 오브젝트로 분리함
        var sndFootstep = transform.Find("SND_Footstep");
        if (sndFootstep != null)
        {   //이름으로 찾은 자식 오브젝트에서 오디오 컴포넌트를 가져와 캐싱 (인스펙터 드래그 대체)
            walkingaudioSource = sndFootstep.GetComponent<AudioSource>();
        }

        var silObj = transform.Find("Sil_Player");//자식 오브젝트 이름에 맞게!
        if (silObj != null)
        {
            silplayer = silObj.GetComponent<Sil_Player>();
        }

        targetCollider = GetComponent<Collider2D>();//플레이어 본인의 콜라이더 사용


        //[방어적 프로그래밍] 검증 로직
        CheckInitialization();
    }
    private void CheckInitialization()//없거나 제대로 연결이 안되면 뜰 로그 에러
    {
        if (walkingaudioSource == null)
        {
            Debug.LogError("자식 오브젝트 SND_Footstep의 AudioSource를 찾을 수 없어!");
        }

        if (silplayer == null)
        {
            Debug.LogWarning("자식 오브젝트 실루엣을 찾을 수 없어!");
        }
    }

    void Start()
    {
        OnMoveSpeedLevelChanged?.Invoke(currentMoveSpeedLevel, maxMoveSpeedLevel);//실행 시 처음은 0레벨이라고 UI에 전달
    }

    void FixedUpdate()//물리연산은 (이동 등) FixedUpdate.
    {
        rb.linearVelocity = movement * moveSpeed;//살아있는 경우 정상적인 물리 이동
        //Rigidbody를 이용해 이동, velocity는 Rigidbody의 현재 속도를 나타냄
        //백터2의 movement값과 내 케릭터의 이동속도(Move)를 계산해 Rigidbody에 적용
    }

    void Update()
    {
        if (IsDead)//죽었으면 이후의 모든 로직을 건너뜀
        {
            return;
        }

        //살아있을 때만 입력 처리
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        movement = new Vector2(horizontalInput, verticalInput).normalized;//normalized로 이동 속도를 일정하게 유지

        HandleWalkingSound();//이동시엔 걷기 사운드(함수) 재생


        if (animator != null)
        {
            animator.SetBool("Move", movement.magnitude > 0);
        }

        //캐릭터 좌우 반전 처리
        //키 입력값(-1, 0, 1)에 따른 방향 전환
        //0(입력 없음)일 때는 조건문을 타지 않아 마지막 방향이 유지
        //else if를 사용해서 0(입력 없음)일 때는 방향을 바꾸지 않고 유지
        if (horizontalInput < 0)
        {
            spriteRenderer.flipX = true;//왼쪽
        }
        else if (horizontalInput > 0)//else를 쓰면 "아무것도 안 누를 때"를 처리 못 해서 캐릭터가 오른쪽만 바라보게 돼.
        {                            //그래서 정확히 방향이 있을 때만 반응하도록 else if로 칸막이를 쳐주는 거야
            spriteRenderer.flipX = false;//오른쪽
        }
    }

    public void PlayerDie()//플레이어 사망시
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;//죽으면 IsDead 실행
        Debug.Log("플레이어 사망!");

        if (walkingaudioSource != null && walkingaudioSource.isPlaying)//걷기 사운드를 즉시 중지
        {
            walkingaudioSource.Stop();
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");

            float DieTime = 1.5f;//사망 후 사라지는 시간1.5초 (플레이어 오브젝트 파괴)
            Destroy(gameObject, DieTime);

            StartCoroutine(GameOverSequence(1.0f));//1초 뒤에 CallGameOverManager의 게임오버 UI창을 호출
            //02.21 invoke에서 코루틴으로 변경, 문자열 기반의 불안정한 방식에서 안정적이고 확장성 있는 코드(코루틴)으로 변경,
        }
        if (rb != null)//물리 시뮬레이션 중지
        {
            rb.simulated = false;
        }
    }
    private IEnumerator GameOverSequence(float delay)//플레이어가 죽으면 1초 뒤 게임오버창이 뜰 코루틴
    {

        yield return new WaitForSeconds(delay);//플레이어가 죽은 후 지정된 시간(1.0f)만큼 대기 후 게임오버창 생성

        //2. 방송 송출 (Invoke): 이 채널을 구독 중인 모든 시청자(스크립트)에게 신호를 보내.
        //?. 은 "만약 구독자가 한 명도 없으면 송출하지마!"라는 안전장치야.
        Player.OnPlayerDead?.Invoke();
    }
    

    public Vector3 GetCenterPosition()//몬스터들이 플레이어의 '중앙'이라고 인식하고 추적/공격할 위치, Enemy 스크립트에서 이 함수를 참조
    {                             
        if (targetCollider != null)//True일때, 플레이어 오브젝트에 Collider2D 컴포넌트가 붙어 있을 때
        {
            return targetCollider.bounds.center;
        }
        return transform.position;//false일때, Collider2D 컴포넌트가 붙어 있지 않을 때
    }

    void HandleWalkingSound()//이동시 사운드 함수
    {
        //캐릭터가 움직이는지 확인
        bool isMoving = (horizontalInput != 0 || verticalInput != 0);

        if(isMoving)
        {
            if (walkingaudioSource != null && stats.walkSound != null && !walkingaudioSource.isPlaying)
            {
                walkingaudioSource.clip = stats.walkSound;
                walkingaudioSource.loop = true;//반복재생
                walkingaudioSource.Play();//위의 상태로 사운드 플레이 시작, true 상태일때 계속 소리가 들리는거야
            }
        }
        else//움직이지 않을 때 소리 멈춤
        {
            if (walkingaudioSource != null && walkingaudioSource.isPlaying)
            {
                walkingaudioSource.Stop();
            }
        }
    }

    public void UpgradeMoveSpeed(float amount)//이속 아이템 획득 시 처리
    {
        if (currentMoveSpeedLevel >= maxMoveSpeedLevel)
        {
            Debug.Log("이동속도가 이미 최대치야!");
            return;//레벨이10(MAX)이 되면 이속 아이템을 먹어도 이속이 증가하지 않아
        }

        moveSpeed += amount;
        currentMoveSpeedLevel++;

        //내 데이터가 바뀌었으니 UI에게 방송
        OnMoveSpeedLevelChanged?.Invoke(currentMoveSpeedLevel, maxMoveSpeedLevel);
        Debug.Log($"[이속 강화] 현재 속도: {moveSpeed}");
    }

    public void AddScore(int amount)//점수를 추가하는 함수
    {
        CurrentScore += amount;//전달받은 amount(점수)만큼 점수를 더해줌
        Debug.Log("현재 점수: " + CurrentScore);

        OnScoreChanged?.Invoke(CurrentScore);//기존의 UI 업데이트하던 코드는 지우고, 이 한 줄만 넣어! 옵저버 패턴으로 점수를 UI로 보내.
    }
    public void ResetScore()//점수를 초기화하는 함수
    {
        CurrentScore = 0;
        OnScoreChanged?.Invoke(0);//옵저버 패턴으로 변경 02.22
    }
}
