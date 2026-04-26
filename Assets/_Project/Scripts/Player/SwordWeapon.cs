using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class SwordWeapon : MonoBehaviour
{   //---------옵저버 패턴----------//
    public static event Action<float, float> OnSwordCooldownChanged;//(남은 시간, 총 시간)
    //---------옵저버 패턴----------//

    [Header("에셋 설정 (프리팹 & 사운드)")]
    public GameObject SwordEnergy;//검기 프리팹(인스펙터 연결)
    public AudioClip swordAttackSound;//검 공격 사운드

    [Header("검 공격/강화 수치")]
    public float SwordDamage = 2f;//검 데미지
    public float KnockbackForce = 10f;//넉백의 강도
    public float KnockbackDuration = 0.2f;//넉백이 적용되는 시간 (몬스터가 잠시 넉백 상태가 되도록)
    public float SwordEnergyDamage = 5f;//검기 에너지 공격력
    public float SwordEnergySpeed = 15f;//검기 에너지 발사 속도
    public float SwordSkillCooldown = 10f;//검 스킬 쿨타임
    public float EnhancedCooldownDecrease = 2f;//강화 시 줄일 쿨타임 (2초)
    public int MAX_ENHANCE_STACKS = 3;//검 아이템 3회 획득시 강화
    public string EnemyTag = "Enemy";//적(몬스터) 태그

    //--- 내부 참조 (자동 연결) ---
    private SpriteRenderer spriteRenderer;
    private AudioSource swordAudioSource;
    private BoxCollider2D swordAttackCollider;//검 공격 판정 오브젝트 콜라이더(SwordAttackPoint)
    private Transform swordEnergySpawnPoint;//검기 발사체 생성 오브젝트 위치(SwordEnergySpawnPoint)
    //UI 관련
    [HideInInspector] public int currentEnhanceStacks = 0;//현재 검 강화 스택
    public float lastSwordSkillTime = -10f;//마지막으로 검공격을 한 시간,


    void Awake()
    {
        // 같은 오브젝트 컴포넌트
        spriteRenderer = GetComponent<SpriteRenderer>();

        Transform sndSwordTransform = transform.Find("SND_Sword");//자식 오브젝트 중 "SND_Sword"를 찾아서 거기 있는 AudioSource를 가져옴
        if (sndSwordTransform != null)
        {
            swordAudioSource = sndSwordTransform.GetComponent<AudioSource>();
        }
            
        //자식 오브젝트에서 포인트들 이름으로 찾기, 자식 오브젝트와 콜라이더를 가져옴
        Transform swordPoint = transform.Find("SwordAttackPoint");
        if (swordPoint != null)
        {
            swordAttackCollider = swordPoint.GetComponent<BoxCollider2D>();
        }

        swordEnergySpawnPoint = transform.Find("SwordEnergySpawnPoint");

        CheckInitialization();//[방어적 프로그래밍] 검증 로직(Awake 함수의 가독성 문제로 로그 알림 함수로 분리)
    }

    private void CheckInitialization()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError($"{gameObject.name}: SpriteRenderer가 없어!");
        }
        if (swordAttackCollider == null)
        {
            Debug.LogWarning($"{gameObject.name}: SwordPoint(BoxCollider2D)를 찾을 수 없어!");
        }
        if (swordEnergySpawnPoint == null)
        {
            Debug.LogWarning($"{gameObject.name}: SwordEnergySpawnPoint가 없어!");
        }
        if (SwordEnergy == null)//에셋(프리팹) 체크
        {
            Debug.LogError($"{gameObject.name}: SwordEnergy 프리팹이 비어있어!");
        }
    }


    void Update()
    {
        float timeRemaining = lastSwordSkillTime + SwordSkillCooldown - Time.time;//매 프레임 UI를 직접 업데이트하는 대신 방송만 보냄
        //이벤트를 구독 중인 UI 매니저에게 값 전달 (값이 0보다 작아져도 종료 알림을 위해 보냄)
        OnSwordCooldownChanged?.Invoke(Mathf.Max(0, timeRemaining), SwordSkillCooldown);
    }

    public void SwordAttack()//검 공격 함수(애니메이션 이벤트로 호출될 함수)
    {                        //SwordPoint의 BoxCollider2D를 사용하여 OverlapBox로 충돌 감지

        bool isEnhanced = currentEnhanceStacks >= MAX_ENHANCE_STACKS;//강화 체크 검 아이템 3회 획득시 강화

        //검 공격시 사운드 재생
        if (swordAudioSource != null && swordAttackSound != null)
        {
            swordAudioSource.PlayOneShot(swordAttackSound);
            //PlayOneShot은 이름 그대로 현재 재생 중인 다른 소리를 끊지 않고, 새로운 소리를 한 번만 재생하는 함수야
        }

        if (swordAttackCollider == null)
        {
            return;
        }

        PerformOverlapAttack();//공격 범위 내 대상 감지 로직(4.26 메서드 추출)
        LaunchSwordEnergy();//검기 에너지 발사
        lastSwordSkillTime = Time.time;//공격 후 다시 쿨타임 시작
    }

    private void PerformOverlapAttack()
    {
        //SwordPoint 오브젝트의 BoxCollider2D의 월드 공간 위치와 크기를 가져옴
        Vector2 colliderCenter = swordAttackCollider.transform.position + (Vector3)swordAttackCollider.offset;
        Vector2 colliderSize = swordAttackCollider.size;
        float colliderAngle = swordAttackCollider.transform.rotation.eulerAngles.z;

        //BoxCollider2D 영역 내의 모든 콜라이더를 감지해. 밑의 foreach문에서 몬스터에겐 특정한 행동을 하게 하는거지!
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(colliderCenter, colliderSize, colliderAngle);
        //OverlapBoxAll()로 콜라이더 범위 안에 있는 것들을 감지하면, 그 결과물(감지된 모든 콜라이더들)은 hitColliders라는 배열 변수에 담겨
        //이 hitColliders 변수는 이제 '감지된 오브젝트들의 목록'이 되는 거지


        foreach (Collider2D hitCollider in hitColliders)
        {//in 키워드는 "~안에 있는" 또는 "~의 구성원인" 이라는 의미로 이해하면 돼.
         //"오른쪽에 있는 컬렉션(hitColliders) 안에 있는 각각의 요소(hitCollider)에 대해" 반복하라는 지시를 내리는 역할을 해
         //foreach는 in 키워드를 반드시 써야 해. in이 없으면 foreach 문은 어떤 컬렉션에서 요소를 하나씩 꺼내와야 할지 알 수 없어

            //플레이어 자신이나 SwordPoint 오브젝트는 건너뛰기
            if (hitCollider.gameObject == this.gameObject || hitCollider.gameObject == swordAttackCollider.gameObject)
            {
                continue;
            }

            //continue는 foreach 문이나 다른 반복문(for, while 등)에서 현재 반복을 즉시 건너뛰고 다음 반복으로 넘어가게 하는 명령어

            if (hitCollider.CompareTag(EnemyTag))//몬스터의 콜라이더의 태그가 EnemyTag 변수와 일치하는지 확인
            {
                EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(SwordDamage);//검 공격력 만큼 피해를 줌

                    Enemy enemyScript = hitCollider.GetComponent<Enemy>();//넉백 함수 호출 (Enemy 스크립트의 TakeKnockback함수 호출)
                    if (enemyScript != null)
                    {
                        //플레이어 위치에서 몬스터 위치로 향하는 방향 벡터 계산
                        Vector2 knockbackDirection = hitCollider.transform.position - transform.position;//플레이어 -> 몬스터 방향 벡터
                        if (knockbackDirection.x > 0) //몬스터가 플레이어 오른쪽에 있으니 오른쪽으로 넉백
                        {
                            knockbackDirection = Vector2.right;
                        }
                        else//몬스터가 플레이어 왼쪽에 있으니 왼쪽으로 넉백
                        {
                            knockbackDirection = Vector2.left;
                        }

                        enemyScript.TakeKnockback(knockbackDirection, KnockbackForce, KnockbackDuration);
                    }
                }
            }
        }
    }

    private void LaunchSwordEnergy()//검 에너지를 생성하고 초기 속도 및 방향을 설정하는 함수
    {
        if (SwordEnergy == null || swordEnergySpawnPoint == null)
        {
            return;
        }

        //플레이어가 보는 방향에 따라 발사 방향을 결정
        Vector2 launchDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        //검 에너지 프리팹을 생성하고, SwordEnergySpawnPoint 변수에 연결된 위치에서 발사
        GameObject SwordEnergyInstance = Instantiate(SwordEnergy, swordEnergySpawnPoint.position, Quaternion.identity);

        SwordEnergy swordenergy = SwordEnergyInstance.GetComponent<SwordEnergy>();//SwordEnergy 스크립트를 가져와서 데미지 값을 넘겨줌
        if (swordenergy != null)
        {
            float totalDamage = SwordEnergyDamage;
            swordenergy.SetDamage(totalDamage);
        }

        //플레이어가 보는 방향에 따라 에너지의 방향 설정
        if (spriteRenderer.flipX)
        {
            SwordEnergyInstance.transform.rotation = Quaternion.AngleAxis(180, Vector3.forward);
        }
        else
        {
            SwordEnergyInstance.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
        }
        //캐릭터가 왼쪽을 보고 있으면 에너지를 180도 회전
        //캐릭터가 오른쪽을 보고 있으면 에너지를 0도(원래 방향)로 설정

        //SwordEnergyInstance 오브젝트에서 Rigidbody2D 컴포넌트를 가져와
        Rigidbody2D rb = SwordEnergyInstance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = launchDirection * SwordEnergySpeed;
        }
    }

    //쿨타임이 끝났는지 여부를 반환하는 간결한 메서드(표현식 본문 메서드)
    public bool CanAttack() => Time.time >= lastSwordSkillTime + SwordSkillCooldown;

    ///<summary>
    ///외부 (아이템 스크립트)에서 호출되어 강화 스택을 1 증가.
    ///</summary>
    public void AcquireSwordEnhanceItem(float cooldownDecrease)//외부(Item스크립트)에서 호출되어 검 강화 스택을 1 증가시키는 함수
    {
        currentEnhanceStacks++; // 누적 스택 증가 (초기화 안 함)

        // 3개 쌓일 때마다 쿨타임 감소 (3, 6, 9... 번째에 발동)
        if (currentEnhanceStacks > 0 && currentEnhanceStacks % 3 == 0)
        {
            SwordSkillCooldown -= cooldownDecrease;
            if (SwordSkillCooldown < 5f)
            {
                SwordSkillCooldown = 5f;
            }
            Debug.Log($"[강화 발동] 현재 쿨타임: {SwordSkillCooldown}초");
        }
        Debug.Log($"검 강화 아이템 획득! 누적 스택: {currentEnhanceStacks}");
    }
}
