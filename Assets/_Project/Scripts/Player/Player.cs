using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//10.27일에 유니티 버전을 2022.3.61f1에서 Unity6.1의 6000.2.9f1로 업데이트해서 한글이 전부 깨져서 주석 대부분을 삭제했어
public enum WeaponType { None, Bow, Sword, Axe }

public class Player : MonoBehaviour
{
    //---------옵저버 패턴----------//
    //static으로 선언하면 어디서든 'Player. ...'로 접근할 수 있어 편리해.
    public static event Action OnPlayerDead;//방송 채널 개설: "플레이어가 죽으면 송출되는 채널", 옵저버 패턴의 시작이야.
    public static event Action<int> OnScoreChanged;//점수 갱신 방송 채널(이벤트)
    //-----------------------------//

    [Header("이동 관련 변수")]
    public float MoveSpeed = 5f;

    [Header("무적 상태")]//[Tooltip("")]이란? 인스펙터에 해당 변수 이름 위에 마우스 올렸을때 툴팁에 적은 설명이 나타나.
    [Tooltip("체력 감소가 되지 않는 무적 상태인지 여부")]
    public bool isInvincible = false;//플레이어가 지금 무적상태인지 확인

    [Header("사운드 에셋")]
    [SerializeField] private AudioClip walkSound;//소리 파일 자체는 드래그가 필요해


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

        //내부 컴포넌트 자동 연결
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        //자식 오브젝트 및 사운드 자동 연결, 자식 중에서 이름으로 찾기
        var sndFootstep = transform.Find("SND_Footstep");
        if (sndFootstep != null) walkingaudioSource = sndFootstep.GetComponent<AudioSource>();

        var silObj = transform.Find("Sil_Player");//자식 오브젝트 이름에 맞게!
        if (silObj != null) silplayer = silObj.GetComponent<Sil_Player>();

        targetCollider = GetComponent<Collider2D>();//플레이어 본인의 콜라이더 사용


        //[방어적 프로그래밍] 검증 로직
        CheckInitialization();
    }

    private void CheckInitialization()//없거나 제대로 연결이 안되면 뜰 로그 에러
    {
        if (walkingaudioSource == null) Debug.LogError("자식 오브젝트 SND_Footstep의 AudioSource를 찾을 수 없어!");
        if (silplayer == null) Debug.LogWarning("자식 오브젝트 실루엣을 찾을 수 없어!");
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

        if (walkingaudioSource != null && walkingaudioSource.isPlaying) walkingaudioSource.Stop();//걷기 사운드를 즉시 중지

        if (animator != null)
        {
            animator.SetTrigger("Die");

            float DieTime = 1.5f;//사망 후 사라지는 시간1.5초 (플레이어 오브젝트 파괴)
            Destroy(gameObject, DieTime);

            StartCoroutine(GameOverSequence(1.0f));//1초 뒤에 CallGameOverManager의 게임오버 UI창을 호출
            //02.21 invoke에서 코루틴으로 변경, 문자열 기반의 불안정한 방식에서 안정적이고 확장성 있는 코드(코루틴)으로 변경,
        }
        if (rb != null) rb.simulated = false;//물리 시뮬레이션 중지
    }
    private IEnumerator GameOverSequence(float delay)//플레이어가 죽으면 1초 뒤 게임오버창이 뜰 코루틴
    {

        yield return new WaitForSeconds(delay);//플레이어가 죽은 후 지정된 시간(1.0f)만큼 대기 후 게임오버창 생성

        Debug.Log("1초 지남: 사망 방송 송출!");
        //2. 방송 송출 (Invoke): 이 채널을 구독 중인 모든 시청자(스크립트)에게 신호를 보내.

        //?. 은 "만약 구독자가 한 명도 없으면 송출하지마!"라는 안전장치야.
        Player.OnPlayerDead?.Invoke();
    }
    

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


    public void SetInvincible(bool active)//무적 효과 발동시
    {
        isInvincible = active;

        //시각적 피드백 (스킬 스크립트에서 직접 해도 되지만 여기서 하면 관리가 편해)
        if (spriteRenderer != null)
            spriteRenderer.color = active ? new Color(1f, 1f, 0.5f, 0.8f) : Color.white;
    }

    public void AddScore(int amount)//점수를 추가하는 함수
    {
        CurrentScore += amount;//전달받은 amount(점수)만큼 점수를 더해줌
        Debug.Log("현재 점수: " + CurrentScore);

        OnScoreChanged?.Invoke(CurrentScore);//기존의 UI 업데이트하던 코드는 지우고, 이 한 줄만 넣어! 옵저버 패턴으로 점수를 UI로 보내.
        //if (ScoreTextUI != null) ScoreTextUI.text = "Score: " + CurrentScore.ToString();                             
    }
    public void ResetScore()//점수를 초기화하는 함수
    {
        CurrentScore = 0;
        OnScoreChanged?.Invoke(0);//옵저버 패턴으로 변경 02.22
        //if (ScoreTextUI != null) ScoreTextUI.text = "Score: 0";
    }
}
