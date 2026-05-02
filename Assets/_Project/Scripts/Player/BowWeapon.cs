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


    //--- 내부 참조 (자동 연결) ---
    private Player player;//Player 스크립트 참조를 통해 SO에 접근
    private Transform arrowSpawnPoint;//플레이어 오브젝트 자식에 있는 화살 생성 위치 오브젝트
    private AudioSource bowAudioSource;//화살 사운드 오디오소스

    private int currentArrowLevel = 0;//활의 강화 횟수를 저장할 변수
    private float currentArrowCooldown;//현재 활 쿨타임
    private float currentArrowDamage;//강화로 인해 변하는 실시간 데미지
    private int currentEnhanceStacks = 0;
    private float lastArrowAttackTime = -1f;//마지막으로 화살을 발사한 시점의 시간 (쿨타임 계산 및 UI 업데이트용)
                                            //초기값 -1f의 의미: 게임 시작 직후에는 아직 한 번도 쏜 적이 없다는 걸,
                                            //컴퓨터에게 알려주기 위한 신호

    void Awake()
    {
        player = GetComponent<Player>();//플레이어 스크립트 참조 가져오기

        arrowSpawnPoint = transform.Find("ArrowSpawnPoint");//플레이어의 자식 오브젝트에서 "ArrowSpawnPoint" 찾기
        Transform sndBowTransform = transform.Find("SND_Bow");//자식 오브젝트 중 "SND_Bow"를 찾아서 거기 있는 AudioSource를 가져옴
        if (sndBowTransform != null)
        {
            bowAudioSource = sndBowTransform.GetComponent<AudioSource>();
        }
            
        CheckInitialization();//[방어적 프로그래밍] 검증 로직(Awake 함수의 가독성 문제로 로그 알림 함수로 분리)
    }

    private void CheckInitialization()
    {
        if (player == null || player.Stats == null)
        {
            Debug.LogError($"{gameObject.name}: Player 또는 PlayerStatsSO를 찾을 수 없어!");
        }

        if (arrowSpawnPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: ArrowSpawnPoint를 자식에서 찾을 수 없어!");
        }
        if (bowAudioSource == null)
        {
            Debug.LogWarning($"{gameObject.name}: AudioSource 미연결!");
        }
    }

    void Start()
    {
        if (player != null && player.Stats != null)//SO에서 초기값 가져오기
        {
            currentArrowCooldown = player.Stats.baseArrowCooldown;
            currentArrowDamage = player.Stats.baseArrowDamage;
            OnArrowLevelChanged?.Invoke(currentArrowLevel, player.Stats.maxBowLevel);
        }
    }

    void Update()
    {
        if (lastArrowAttackTime > 0)//[옵저버] 매 프레임 UI 함수를 부르는 대신, 방송만 쏜다
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
                lastArrowAttackTime = -1f;
            }
        }
    }

    public void ShootArrow()//360도 화살 발사
    {
        var stats = player.Stats;//타이핑을 줄이기 위해 변수화
        if (stats == null)//SO가 없으면 종료
        {
            return;
        }

        //현재 스택이 SO에 설정된 최대 스택(3) 이상인지 체크해서 true / false를 확인
        bool isEnhanced = currentEnhanceStacks >= stats.maxEnhanceStacks;//강화 화살

        //위에서 판단한 결과(isEnhanced)에 따라 발사할 프리팹을 선택하고,
        //true면 강화 화살(enhancedArrowPrefab), false면 일반 화살(arrowPrefab)을 변수에 담아
        GameObject activeArrowPrefab = isEnhanced ? stats.enhancedArrowPrefab : stats.arrowPrefab;

  
        if (bowAudioSource != null && stats.bowAttackSound != null)// 발사 사운드 재생
        {
            bowAudioSource.PlayOneShot(stats.bowAttackSound);
        }

        if (activeArrowPrefab == null)//화살 프리팹이 없으면 종료
        {
            return;
        }

        float angleStep = 360f / stats.numberOfArrows360;//각 발사될 화살의 각도 계산
        for (int i = 0; i < stats.numberOfArrows360; i++)
        {
            SpawnArrow(isEnhanced, activeArrowPrefab, currentArrowDamage, angleStep, i);
        }

        lastArrowAttackTime = Time.time;

        if (isEnhanced)//강화공격을 했다면?
        {
            ResetEnhanceStackAfterShoot();//강화 화살 발사 후 강화 화살 초기화 
        }
    }

    //4.26 ShootArrow() 함수의 화살 발사 로직을 '메서드 추출'로 분리(ShootArrow함수의 가독성 향상)
    private void SpawnArrow(bool isEnhanced, GameObject activeArrowPrefab, float damage, float angleStep, int i)
    {
        var stats = player.Stats;
        float angle = i * angleStep;//현재 화살의 각도(0, 45, 90, ...)
        float radianAngle = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(radianAngle), Mathf.Sin(radianAngle)).normalized;//발사 방향 벡터 계산

        //화살 생성 (플레이어의 중앙에서 발사되도록 ArrowSpawnPoint 오브젝트 사용)                  
        GameObject arrow = Instantiate(activeArrowPrefab, arrowSpawnPoint.position, Quaternion.identity);
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
        if (arrowRb != null)
        {
            arrowRb.linearVelocity = direction * stats.arrowSpeed;
            //화살을 움직이는 방향에 따라 회전
            float rotAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.AngleAxis(rotAngle, Vector3.forward);

            Arrow arrowScript = arrow.GetComponent<Arrow>();
            if (arrowScript != null)
            {
                arrowScript.ArrowDamage = damage;
                if (isEnhanced)
                {
                    arrowScript.IsEnhanced = true;
                    arrowScript.SlowFactor = stats.slowFactor;
                }
            }         
        }
    }

    //활 강화 스택을 알려주는 함수(람다식)
    //현재 쌓인 0, 1, 2, 3 스택값을 알려줌
    public int GetCurrentStacks() => currentEnhanceStacks;

    public void UpgradeBow(float damagePlus, float cooldownMinus)//아이템 스크립트에서 직접 호출할 강화 함수
    {
        var stats = player.Stats;
        if (stats == null || currentArrowLevel >= stats.maxBowLevel)//활이 최대 레벨(10)에 도달하면 더 이상 강화하지 않아
        {
            return;
        }

        currentArrowDamage += damagePlus;//아이템 획득 시 현재 데미지 증가
        currentArrowCooldown = Mathf.Max(1.0f, currentArrowCooldown - cooldownMinus);//쿨타임도 감소

        currentArrowLevel++;//활 레벨 증가
        currentEnhanceStacks++;//아이템 하나 획득 시 강화 스텍 1 증가

        OnArrowLevelChanged?.Invoke(currentArrowLevel, stats.maxBowLevel);//[옵저버] 강화됐다고 UI에 알림

        if (currentEnhanceStacks >= stats.maxEnhanceStacks)//만약 현재 쌓인 강화 스택이 목표치(3스택) 이상이라면?
        {
            OnArrowEnhancedEffect?.Invoke();//활 강화시 UI 텍스트 연출
            OnArrowColorStateChanged?.Invoke(true);//활 강화 시 UI 텍스트 색 변화
        }
    }

    public void ResetEnhanceStackAfterShoot()//강화 화살 발사 후 스택 초기화 함수
    {
        currentEnhanceStacks -= player.Stats.maxEnhanceStacks;//강화 활 사용 후 강화 스택을 0으로 되돌려(다시 3번 획득하면 강화)

        //3스택 미만이 되면 다시 일반 색상(false)으로 방송
        OnArrowColorStateChanged?.Invoke(currentEnhanceStacks >= player.Stats.maxEnhanceStacks);
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
