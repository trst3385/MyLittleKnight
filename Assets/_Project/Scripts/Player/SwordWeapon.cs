using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SwordWeapon : MonoBehaviour
{   
    //---------옵저버 패턴----------//
    public static event Action<float, float> OnSwordCooldownChanged;//(남은 시간, 총 시간)
    public static event Action<int, int> OnSwordLevelChanged;//검 레벨 방송
    //---------옵저버 패턴----------//

  

    //---내부 참조 (자동 연결)---
    private Player player;//SO 접근을 위한 Player 스크립트 참조
    private SpriteRenderer spriteRenderer;
    private AudioSource swordAudioSource;
    private BoxCollider2D swordAttackCollider;//검 공격 판정 오브젝트 콜라이더(SwordAttackPoint)
    private Transform swordEnergySpawnPoint;//검기 발사체 생성 오브젝트 위치(SwordEnergySpawnPoint)

    //---실시간 상태 변수 (게임 중 강화로 인해 변하는 값들)---
    private float currentSwordDamage;
    private float currentSwordEnergyDamage;
    private float currentSwordSkillCooldown;
    private int currentSwordLevel = 0;//현재 검 레벨
    private int currentEnhanceStacks = 0;//현재 검 강화 스택
    private float lastSwordSkillTime = -10f;//마지막으로 검공격을 한 시간


    void Awake()
    {
        player = GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        Transform sndSwordTransform = transform.Find("SND_Sword");//자식 오브젝트 중 "SND_Sword"를 찾아서 거기 있는 AudioSource를 가져옴
        if (sndSwordTransform != null)
        {
            swordAudioSource = sndSwordTransform.GetComponent<AudioSource>();
        }

        //자식 오브젝트에서 검 공격 판정 콜라이더를 이름으로 찾기
        Transform swordPoint = transform.Find("SwordAttackPoint");
        if (swordPoint != null)
        {
            swordAttackCollider = swordPoint.GetComponent<BoxCollider2D>();
        }

        swordEnergySpawnPoint = transform.Find("SwordEnergySpawnPoint");//자식 오브젝트에서 검기 생성 위치 오브젝트 찾기

        CheckInitialization();//[방어적 프로그래밍] 검증 로직(Awake 함수의 가독성 문제로 로그 알림 함수로 분리)
    }

    private void CheckInitialization()
    {
        if (player == null || player.Stats == null)
        {
            Debug.LogError($"{gameObject.name}: Player 또는 PlayerStatsSO를 찾을 수 없어!");
        }          
        if (swordAttackCollider == null)
        {
            Debug.LogWarning($"{gameObject.name}: SwordPoint(BoxCollider2D)를 찾을 수 없어!");
        }
        if (swordEnergySpawnPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: SwordEnergySpawnPoint가 없어!");
        }
    }

    void Start()
    {
        if (player != null && player.Stats != null)//SO에서 초기 데이터를 가져와서 실시간 변수에 할당
        {
            var stats = player.Stats;//SO 데이터 시트로 연결되는 최신 통로를 'stats'라는 별명으로 참조 (최신 데이터 보장)

            currentSwordDamage = stats.baseSwordDamage;
            currentSwordEnergyDamage = stats.baseSwordEnergyDamage;
            currentSwordSkillCooldown = stats.baseSwordSkillCooldown;

            OnSwordLevelChanged?.Invoke(currentSwordLevel, stats.maxSwordLevel);//시작 시 검 레벨 0으로 시작
        }
    }

    void Update()
    {
        //쿨타임 UI 방송 로직
        //1. 현재 남은 쿨타임 계산(마지막 공격 시간 +쿨타임 - 현재 시간)
        float timeRemaining = lastSwordSkillTime + currentSwordSkillCooldown - Time.time;
        //2. 이벤트 발행(Broadcasting): UI 스크립트를 직접 참조하여 수정하는 대신, 
        //현재 수치를 이벤트에 담아 외부로 보내. 이를 통해 무기-UI 간의 결합도를 낮춰
        OnSwordCooldownChanged?.Invoke(Mathf.Max(0, timeRemaining), currentSwordSkillCooldown);
    }

    public void SwordAttack()//검 공격 함수(애니메이션 이벤트로 호출될 함수)
    {                        //SwordPoint의 BoxCollider2D를 사용하여 OverlapBox로 충돌 감지
        var stats = player.Stats;//함수 실행 시점에 가장 최신화된 능력치(SO)를 안전하게 가져옴 (지역 변수로 데이터 오염 방지)
        if (stats == null)
        {
            return;
        }

        if (swordAudioSource != null && stats.swordAttackSound != null)//공격 사운드 재생 (SO의 사운드 사용)
        {
            swordAudioSource.PlayOneShot(stats.swordAttackSound);//현재 재생 중인 다른 소리를 끊지 않고, 새로운 소리를 한 번만 재생
        }

        if (swordAttackCollider == null)
        {
            return;
        }

        PerformOverlapAttack();//근접 공격 판정
        LaunchSwordEnergy();   //검기 발사
        lastSwordSkillTime = Time.time;//공격 후 다시 쿨타임 시작
    }

    private void PerformOverlapAttack()
    {
        var stats = player.Stats;//함수 실행 시점에 가장 최신화된 능력치(SO)를 안전하게 가져옴 (지역 변수로 데이터 오염 방지)

        //SwordPoint 오브젝트의 BoxCollider2D의 월드 공간 위치와 크기를 가져옴
        Vector2 colliderCenter = (Vector2)swordAttackCollider.transform.position + swordAttackCollider.offset;
        Vector2 colliderSize = swordAttackCollider.size;
        float colliderAngle = swordAttackCollider.transform.rotation.eulerAngles.z;

        //BoxCollider2D 영역 내의 모든 콜라이더를 감지해. 밑의 foreach문에서 몬스터에겐 특정한 행동을 하게 하는거지
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(colliderCenter, colliderSize, colliderAngle);
        //OverlapBoxAll()로 콜라이더 범위 안에 있는 것들을 감지하면, 그 결과물(감지된 모든 콜라이더들)은 hitColliders라는 배열 변수에 담겨
        //이 hitColliders 변수는 이제 '감지된 오브젝트들의 목록'이 되는 거지


        foreach (Collider2D hitCollider in hitColliders)//감지된 적들의 목록을 순회하며 넉백과 데미지 처리
        {
            //플레이어 자신이나 SwordPoint 오브젝트는 건너뛰기
            if (hitCollider.gameObject == this.gameObject || hitCollider.gameObject == swordAttackCollider.gameObject)
            {
                continue;
            }//continue는 foreach 문이나 다른 반복문(for, while 등)에서 현재 반복을 즉시 건너뛰고 다음 반복으로 넘어가게 하는 명령어

            if (hitCollider.CompareTag(stats.enemyTag))//SO에 정의된 태그(stats.enemyTag)를 사용하여 체크
            {
                EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(currentSwordDamage);//검 공격력 만큼 피해를 줌

                    Enemy enemyScript = hitCollider.GetComponent<Enemy>();//넉백 함수 호출 (Enemy 스크립트의 TakeKnockback함수 호출)
                    if (enemyScript != null)
                    {
                        //5.3 넉백 방향 계산을 삼항 연산자로 간결하게 수정했어
                        Vector2 knockbackDirection = (hitCollider.transform.position.x > transform.position.x) ? Vector2.right : Vector2.left;
                        enemyScript.TakeKnockback(knockbackDirection, stats.knockbackForce, stats.knockbackDuration);
                    }
                    //if (enemyScript != null)
                    //{
                    //    //플레이어 위치에서 몬스터 위치로 향하는 방향 벡터 계산
                    //    Vector2 knockbackDirection = hitCollider.transform.position - transform.position;//플레이어 -> 몬스터 방향 벡터
                    //    if (knockbackDirection.x > 0) //몬스터가 플레이어 오른쪽에 있으니 오른쪽으로 넉백
                    //    {
                    //        knockbackDirection = Vector2.right;
                    //    }
                    //    else//몬스터가 플레이어 왼쪽에 있으니 왼쪽으로 넉백
                    //    {
                    //        knockbackDirection = Vector2.left;
                    //    }

                    //    enemyScript.TakeKnockback(knockbackDirection, KnockbackForce, KnockbackDuration);
                    //}
                }
            }
        }
    }

    private void LaunchSwordEnergy()//검 에너지를 생성하고 초기 속도 및 방향을 설정하는 함수
    {
        var stats = player.Stats;//함수 실행 시점에 가장 최신화된 능력치(SO)를 안전하게 가져옴 (지역 변수로 데이터 오염 방지)

        if (stats.swordEnergyPrefab == null || swordEnergySpawnPoint == null)
        {
            return;
        }

        //플레이어가 보는 방향에 따라 발사 방향을 결정
        Vector2 launchDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        //검 에너지 프리팹을 생성하고, SwordEnergySpawnPoint 변수에 연결된 위치에서 발사
        GameObject instance = Instantiate(stats.swordEnergyPrefab, swordEnergySpawnPoint.position, Quaternion.identity);

        //SwordEnergy 컴포넌트를 참조해서 계산된 최종 데미지 값을 해당 스크립트로 전달
        SwordEnergy energyScript = instance.GetComponent<SwordEnergy>();
        if (energyScript != null)
        {
            energyScript.SetDamage(currentSwordEnergyDamage);//SwordEnergy의 SetDamage 함수를 호출하여 외부 데미지 값을 전달 및 설정
        }

        //플레이어 방향에 따른 회전 설정
        instance.transform.rotation = Quaternion.AngleAxis(spriteRenderer.flipX ? 180 : 0, Vector3.forward);

        Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = launchDirection * stats.swordEnergySpeed;
        }
        //캐릭터가 왼쪽을 보고 있으면 에너지를 180도 회전
        //캐릭터가 오른쪽을 보고 있으면 에너지를 0도(원래 방향)로 설정
    }

    //쿨타임이 끝났는지 여부를 반환하는 간결한 메서드(표현식 본문 메서드)
    public bool CanAttack() => Time.time >= lastSwordSkillTime + currentSwordSkillCooldown;


    public void UpgradeSword(float damagePlus, float cooldownMinus)//검 강화 함수
    {
        var stats = player.Stats;//함수 실행 시점에 가장 최신화된 능력치(SO)를 안전하게 가져옴 (지역 변수로 데이터 오염 방지)

        //1. 최대 레벨 체크
        if (stats == null || currentSwordLevel >= stats.maxSwordLevel)
        {
            Debug.Log("검 레벨이 이미 최대치야!");
            return;
        }

        //2. 능력치 상승 (검 + 검기 둘 다)
        currentSwordDamage += damagePlus;
        currentSwordEnergyDamage += damagePlus;

        //3. 레벨 및 스택 관리
        currentSwordLevel++;
        currentEnhanceStacks++;

        // SO에 설정된 스택 기준. 3스택마다 검 쿨타임 감소 로직(stats.swordEnhanceStackLimit)
        if (currentEnhanceStacks > 0 && currentEnhanceStacks % stats.swordEnhanceStackLimit == 0)
        {
            currentSwordSkillCooldown = Mathf.Max(stats.minSwordCooldown, currentSwordSkillCooldown - cooldownMinus);
            Debug.Log($"[검 강화] 현재 쿨타임: {currentSwordSkillCooldown}초");
        }

        //4. UI에게 방송 (현재 레벨로 방송)
        OnSwordLevelChanged?.Invoke(currentSwordLevel, stats.maxSwordLevel);
    }  
}