using System;//옵저버 패턴의 Action을 위해 추가
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BowWeapon : MonoBehaviour
{
    //---------옵저버 패턴----------//
    public static event Action<float, float> OnBowCooldownChanged;//[옵저버] 쿨타임바 방송국 (남은 시간, 총 쿨타임)
    //------------------------------//

    [Header("활 설정 (데이터 & 에셋)")]
    public GameObject ArrowPrefab;//화살 프리팹
    public GameObject EnhancedArrowPrefab;//강화 화살 프리팹
    public AudioClip bowAttackSound;//화살 발사 사운드
    public float ArrowSpeed = 10f;//발사속도
    public float ArrowDamage = 1f;//데미지
    public int NumberOfArrows360 = 8;//360도 회전 시 발사할 화살의 수 (예: 8방향)
    public float SlowFactor = 0.5f;//강화 화살에맞은 몬스터의 이동 속도 감소 비율(0.5f = 50% 느려짐)
    public float BaseArrowCooldown = 2f;//활의 초기/최대 공격 쿨타임 (아이템 효과 미적용 원본)

    //--- 내부 참조 (자동 연결) ---
    private Transform arrowSpawnPoint;//플레이어 오브젝트 자식에 있는 화살 생성 위치 오브젝트
    private AudioSource bowAudioSource;//화살 사운드 오디오소스
    private PlayerStatsEffects statsEffects;//PlayerStatsEffects 스크립트 참조

    private float currentArrowCooldown;
    private float lastArrowAttackTime = -1f;
    private int currentEnhanceStacks = 0;
    private const int MAX_ENHANCE_STACKS = 3;//활 아이템 3회 획득시 강화

    void Awake()
    {
        arrowSpawnPoint = transform.Find("ArrowSpawnPoint");//플레이어의 자식 오브젝트에서 "ArrowSpawnPoint" 찾기

        //같은 오브젝트의 컴포넌트,스크립트
        Transform sndBowTransform = transform.Find("SND_Bow");//자식 오브젝트 중 "SND_Bow"를 찾아서 거기 있는 AudioSource를 가져옴
        if (sndBowTransform != null)
            bowAudioSource = sndBowTransform.GetComponent<AudioSource>();

        statsEffects = GetComponent<PlayerStatsEffects>();

        CheckInitialization();//[방어적 프로그래밍] 검증 로직(Awake 함수의 가독성 문제로 로그 알림 함수로 분리)
    }

    private void CheckInitialization()
    {
        if (arrowSpawnPoint == null) Debug.LogWarning($"{gameObject.name}: ArrowSpawnPoint를 자식에서 찾을 수 없어!");
        if (bowAudioSource == null) Debug.LogWarning($"{gameObject.name}: AudioSource 미연결!");

        //에셋 체크 (드래그 필수 항목)
        if (ArrowPrefab == null) Debug.LogError($"{gameObject.name}: ArrowPrefab이 비어있어!");
        if (bowAttackSound == null) Debug.LogWarning($"{gameObject.name}: bowAttackSound 클립이 비어있어!");
    }

    void Start()
    {
        currentArrowCooldown = BaseArrowCooldown;
    }
    void Update()
    {
        if (lastArrowAttackTime > 0)//[옵저버] 매 프레임 UI 함수를 부르는 대신, 방송만 쏜다! 
        {
            float timeRemaining = lastArrowAttackTime + currentArrowCooldown - Time.time;

            if (timeRemaining > 0)
                OnBowCooldownChanged?.Invoke(timeRemaining, currentArrowCooldown);//방송: "아직 쿨타임 중이야! 남은 시간은 이만큼이야!"
            else
            {
                //방송: "쿨타임 끝! 이제 UI 꺼도 돼!"
                OnBowCooldownChanged?.Invoke(0, currentArrowCooldown);
                lastArrowAttackTime = -1f;//방송 한 번만 하고 멈추기 위해
            }
        }
    }

    public void ShootArrow()//360도 화살 발사
    {
        //강화 화살
        bool isEnhanced = currentEnhanceStacks >= MAX_ENHANCE_STACKS;
        GameObject activeArrowPrefab = ArrowPrefab;
        float activeArrowDamage = ArrowDamage;
        if (isEnhanced)
        {
            activeArrowPrefab = EnhancedArrowPrefab;//1.강화 화살 프리팹 사용
            Debug.Log(">>> 특수 강화 공격 발동! (관통 + 슬로우)");
        }

        if (bowAudioSource != null && bowAttackSound != null) bowAudioSource.PlayOneShot(bowAttackSound);//활 공격 시 사운드 재생
        else Debug.LogWarning("활 공격 사운드 AudioSource 또는 AudioClip이 설정되지 않았어!");
        //PlayOneShot은 이미 재생 중인 소리가 있어도 다른 소리를 끊지 않고, 새로운 소리를 겹쳐서 재생시키는 함수

        if (activeArrowPrefab == null)
        {
            Debug.LogError("사용할 화살 Prefab이 설정되지 않았어!");
            return;
        }
        Debug.Log("화살 발사! 설정된 데미지: " + activeArrowDamage);//화살을 발사할 때 로그에 알림

        float angleStep = 360f / NumberOfArrows360;//각 화살의 각도 계산

        for (int i = 0; i < NumberOfArrows360; i++)
        {
            float angle = i * angleStep;//현재 화살의 각도(0, 45, 90, ...)

            //Cos/Sin 계산을 위해 '도(Degree)' 단위를 '라디안(Radian)'으로 변환 (컴퓨터가 이해하는 각도 단위로 변경)
            float radianAngle = angle * Mathf.Deg2Rad;

            //발사 방향 벡터 계산
            Vector2 direction = new Vector2(Mathf.Cos(radianAngle), Mathf.Sin(radianAngle)).normalized;

            //화살 생성 (플레이어의 중앙에서 발사되도록 ArrowSpawnPoint 오브젝트 사용)                  
            GameObject arrow = Instantiate(activeArrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
            
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.linearVelocity = direction * ArrowSpeed;

                //화살을 움직이는 방향에 따라 회전
                angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                Arrow arrowScript = arrow.GetComponent<Arrow>();//화살의 Arrow 스크립트에 데미지 설정
                if (arrowScript != null)
                {
                    arrowScript.ArrowDamage = activeArrowDamage;//현재 ArrowDamage 값 

                    if (isEnhanced)//강화 화살에 슬로우 및 관통 정보 전달
                    {
                        arrowScript.IsEnhanced = true;//Arrow 스크립트에서 IsEnhanced 변수를 추가해야 해.
                        arrowScript.SlowFactor = SlowFactor;
                    }
                }                                                    
                else Debug.LogWarning("생성된 화살 프리팹에 'Arrow' 스크립트가 없어! 데미지 설정 불가능!");
            }
            else Debug.LogWarning("생성된 화살 프리팹에 Rigidbody2D가 없어!");
        }
        lastArrowAttackTime = Time.time;

        if (isEnhanced)//강화공격을 하면 강화 스택3 감소
        {
            //예시로 아이템 6회 획득 때 강화 공격 후, 3이 남아서 다음 발사도 강화가 됨.
            currentEnhanceStacks -= MAX_ENHANCE_STACKS;
            if (statsEffects != null) statsEffects.UpdateWeaponLevelUI();
        } 
    }

    //활 강화 스택을 알려주는 함수(람다식)
    //현재 쌓인 0, 1, 2, 3 스택값을 알려줌
    public int GetCurrentStacks() => currentEnhanceStacks;


    public void AcquireBowEnhanceItem()//활 아이템 획득 시 강화 스택를 갱신하고, 이를 UI에 즉시 반영하도록 요청하는 함수.
    {
        //1. 강화 스택 숫자를 1 증가 (3, 6, 9... 계속 쌓임)
        currentEnhanceStacks++;
        Debug.Log($"활 강화 아이템 획득! 현재 누적 스택: {currentEnhanceStacks}");

        //2.PlayerStatsEffects 스크립트를 찾아가서 "바뀐 스택 확인해서 활 레벨 UI 다시 그려!"라고 시켜
        if (statsEffects != null) statsEffects.UpdateWeaponLevelUI();
    }

    public void DecreaseAttackCooldown(float coolDown, WeaponType weaponType)//활 공격력 강화(아이템 획득) 시 쿨타임 감소 함수
    {                                                                   
        switch (weaponType)
        {
            case WeaponType.Bow://전달받은 타입이 활(Bow)일 경우에만 쿨타임 감소 로직 실행
                currentArrowCooldown -= coolDown;//현재 적용되는 쿨타임에서 감소량(coolDown)을 뺀다
                if (currentArrowCooldown < 1.0f)//여기서 활 공격의 최소 쿨타임을 1초로 제한
                    currentArrowCooldown = 1.0f;//1.0초 이하로는 쿨타임이 줄어들지 않도록 제한
                break;
        }
        Debug.Log($"쿨타임 감소 후 현재 쿨타임: {currentArrowCooldown}");
    }

    public bool CanAttack()//쿨타임 체크
    {
        //마지막 공격 시간이 -1f 이거나 쿨타임이 지났으면 True 반환
        if (lastArrowAttackTime < 0f || Time.time >= lastArrowAttackTime + currentArrowCooldown)
            return true;
        else
        {
            //쿨타임이 남았을 경우 남은 시간을 로그 출력
            float timeRemaining = lastArrowAttackTime + currentArrowCooldown - Time.time;
            Debug.Log("활 공격 쿨타임 중. 남은 시간: " + timeRemaining.ToString("F1") + "초");
            return false;
        }
    }
}
