using System;//옵저버 패턴의 Action을 위해 추가
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BowWeapon : MonoBehaviour
{
    //---------옵저버 패턴----------//
    public static event Action<float, float> OnBowCooldownChanged;//[옵저버] 쿨타임바 방송국 (남은 시간, 총 쿨타임)
    public static event Action<int, int> OnArrowLevelChanged; //레벨 방송국 이동
    public static event Action OnArrowEnhancedEffect;         //강화 연출 방송국 이동
    public static event Action<bool> OnArrowColorStateChanged;//색상 변경 방송국 이동
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

    private int currentArrowLevel = 0;//활의 강화 횟수를 저장할 변수
    private const int BOW_ITEM_MAX_LEVEL = 10;//활 레벨에 적용할 최대치
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
        {
            bowAudioSource = sndBowTransform.GetComponent<AudioSource>();
        }
            
        CheckInitialization();//[방어적 프로그래밍] 검증 로직(Awake 함수의 가독성 문제로 로그 알림 함수로 분리)
    }

    private void CheckInitialization()
    {
        if (arrowSpawnPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: ArrowSpawnPoint를 자식에서 찾을 수 없어!");
        }
        if (bowAudioSource == null)
        {
            Debug.LogWarning($"{gameObject.name}: AudioSource 미연결!");
        }

        //에셋 체크 (드래그 필수 항목)
        if (ArrowPrefab == null)
        {
            Debug.LogError($"{gameObject.name}: ArrowPrefab이 비어있어!");
        }
        if (bowAttackSound == null)
        {
            Debug.LogWarning($"{gameObject.name}: bowAttackSound 클립이 비어있어!");
        }
    }

    void Start()
    {
        currentArrowCooldown = BaseArrowCooldown;
        
        OnArrowLevelChanged?.Invoke(currentArrowLevel, BOW_ITEM_MAX_LEVEL);//게임 시작 시 초기 레벨(0)
    }

    void Update()
    {
        if (lastArrowAttackTime > 0)//[옵저버] 매 프레임 UI 함수를 부르는 대신, 방송만 쏜다! 
        {
            float timeRemaining = lastArrowAttackTime + currentArrowCooldown - Time.time;

            if (timeRemaining > 0)
            {
                OnBowCooldownChanged?.Invoke(timeRemaining, currentArrowCooldown);//방송: "아직 쿨타임 중이야! 남은 시간은 이만큼이야!"
            }   
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
        bool isEnhanced = currentEnhanceStacks >= MAX_ENHANCE_STACKS;//강화 화살
        GameObject activeArrowPrefab = ArrowPrefab;
        float activeArrowDamage = ArrowDamage;
        if (isEnhanced)
        {
            activeArrowPrefab = EnhancedArrowPrefab;//1.강화 화살 프리팹 사용
            Debug.Log(">>> 특수 강화 공격 발동! (관통 + 슬로우)");
        }

        if (bowAudioSource != null && bowAttackSound != null)//활 공격 시 사운드 재생
        {
            bowAudioSource.PlayOneShot(bowAttackSound);
        }//PlayOneShot은 이미 재생 중인 소리가 있어도 다른 소리를 끊지 않고, 새로운 소리를 겹쳐서 재생시키는 함수
        else
        {
            Debug.LogWarning("활 공격 사운드 AudioSource 또는 AudioClip이 설정되지 않았어!");
        }

        if (activeArrowPrefab == null)
        {
            Debug.LogError("사용할 화살 Prefab이 설정되지 않았어!");
            return;
        }
        Debug.Log("화살 발사! 설정된 데미지: " + activeArrowDamage);//화살을 발사할 때 로그에 알림

        float angleStep = 360f / NumberOfArrows360;//각 화살의 각도 계산

        for (int i = 0; i < NumberOfArrows360; i++)
        {
            SpawnArrow(isEnhanced, activeArrowPrefab, activeArrowDamage, angleStep, i);
        }

        lastArrowAttackTime = Time.time;

        if (isEnhanced)//강화공격을 했다면
        {
            ResetEnhanceStackAfterShoot();
        }
    }

    //4.26 ShootArrow() 함수의 화살 발사 로직을 '메서드 추출'로 분리(ShootArrow함수의 가독성 향상)
    private void SpawnArrow(bool isEnhanced, GameObject activeArrowPrefab, float activeArrowDamage, float angleStep, int i)
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
            else
            {
                Debug.LogWarning("생성된 화살 프리팹에 'Arrow' 스크립트가 없어! 데미지 설정 불가능!");
            }
        }
        else
        {
            Debug.LogWarning("생성된 화살 프리팹에 Rigidbody2D가 없어!");
        }
    }

    //활 강화 스택을 알려주는 함수(람다식)
    //현재 쌓인 0, 1, 2, 3 스택값을 알려줌
    public int GetCurrentStacks() => currentEnhanceStacks;

    public void UpgradeBow(float damagePlus, float cooldownMinus)//아이템 스크립트에서 직접 호출할 강화 함수
    {
        //1. 최대 레벨 체크
        if (currentArrowLevel >= BOW_ITEM_MAX_LEVEL)
        {
            Debug.Log("활 레벨이 이미 최대치야!");
            return;
        }

        //2. 실제 수치 강화
        ArrowDamage += damagePlus;
        currentArrowCooldown -= cooldownMinus;

        //쿨타임 최소 제한 (1초)
        if (currentArrowCooldown < 1.0f)
        {
            currentArrowCooldown = 1.0f;
        }

        //3. 레벨 및 스택 증가
        currentArrowLevel++;
        currentEnhanceStacks++;

        //4. 옵저버 패턴: UI 및 연출 방송
        OnArrowLevelChanged?.Invoke(currentArrowLevel, BOW_ITEM_MAX_LEVEL);

        //3스택 달성 시 강화 효과 방송 (이 로직도 무기 안으로 들어옴)
        if (currentEnhanceStacks >= MAX_ENHANCE_STACKS)
        {
            OnArrowEnhancedEffect?.Invoke();
            OnArrowColorStateChanged?.Invoke(true);
        }

        Debug.Log($"[BowUpgrade] LV:{currentArrowLevel} ATK:{ArrowDamage} Cool:{currentArrowCooldown} Stack:{currentEnhanceStacks}");
    }

    public void ResetEnhanceStackAfterShoot()//강화 화살 발사 후 스택 초기화 함수
    {
        currentEnhanceStacks -= MAX_ENHANCE_STACKS;

        //3스택 미만이 되면 다시 일반 색상(false)으로 방송
        OnArrowColorStateChanged?.Invoke(currentEnhanceStacks >= MAX_ENHANCE_STACKS);
    }

    public bool CanAttack()//쿨타임 체크
    {
        //마지막 공격 시간이 -1f 이거나 쿨타임이 지났으면 True 반환, 이 조건문 결과 자체가 true 아니면 false니까 바로 리턴
        //4.26 더 간결하게 논리 연산자를 활용
        return lastArrowAttackTime < 0f || Time.time >= lastArrowAttackTime + currentArrowCooldown;

        ////마지막 공격 시간이 -1f 이거나 쿨타임이 지났으면 True 반환
        //if (lastArrowAttackTime < 0f || Time.time >= lastArrowAttackTime + currentArrowCooldown)
        //    return true;
        //else
        //{
        //    return false;
        //}
    }
}
