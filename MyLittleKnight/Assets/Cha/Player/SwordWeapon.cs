using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;//UI 사용을 위해 추가


public class SwordWeapon : MonoBehaviour
{

    [Header("UI,컴포넌트 연결")]
    public Image SwordSkillIcon;
    public Image CooldownOverlay;
    public TextMeshProUGUI CooldownText;
    private SpriteRenderer SpriteRenderer;

    [Header("검 공격 관련")]
    //검 공격 관련
    public float SwordDamage = 2f;//검 공격력(인스펙터 설정)
    public BoxCollider2D SwordAttackCollider;//SwordPoint 오브젝트의 BoxCollider2D를 직접 연결
    public string EnemyTag = "Enemy";//적을 식별할 태그 (인스펙터에서 설정 가능)
    //검 공격시 몬스터 넉백 관련
    public float KnockbackForce = 10f;//넉백의 강도(인스펙터에서 조절)
    public float KnockbackDuration = 0.2f;//넉백이 적용되는 시간 (몬스터가 잠시 넉백 상태가 되도록)
    //검기 발사체 관련
    public float SwordEnergyDamage = 5f;//검기 기본 공격력(인스펙터에 설정)
    public float SwordEnergySpeed = 15f;//검기 발사체 속도
    public Transform SwordEnergySpawnPoint;//검기 발사체 생성 위치 (인스펙터에 연결)
    public GameObject SwordEnergy;//검기 발사체 프리팹 (인스펙터에서 연결)

    [Header("검 공격 속도 관련")]
    public float SwordSkillCooldown = 10f;//검 스킬 쿨타임
    public float lastSwordSkillTime = -10f;//마지막으로 스킬을 사용한 시간, Time.time이 현재 시간을 이 변수에 저장해서 쿨타임을 계산할 때 사용
    //이 변수가 없으면 Time.time >= lastSwordSkillTime + swordSkillCooldown 같은 쿨타임 체크 공식이 성립되지 않아.

    [Header("검 공격 사운드")]
    [SerializeField] private AudioSource swordAudioSource;
    [SerializeField] private AudioClip swordAttackSound;
    


    void Awake()
    {
        //SpriteRenderer 컴포넌트를 가져와서 변수에 할당
        SpriteRenderer = GetComponent<SpriteRenderer>();

        //참조가 제대로 되었는지 확인
        if (SpriteRenderer == null)
            Debug.LogError("SwordWeapon: SpriteRenderer 컴포넌트를 찾을 수 없어!");
    }
    void Update()
    {
        //Update 함수가 매 프레임마다 호출되고 있는지 확인
        UpdateSwordSkillUI();
    }

    public void SwordAttack()//검 공격 함수(애니메이션 이벤트로 호출될 함수)
    {                        //SwordPoint의 BoxCollider2D를 사용하여 OverlapBox로 충돌 감지

        //검공격 사운드 재생
        if (swordAudioSource != null && swordAttackSound != null)//audioSource랑 swordAttackSound가 != null. 즉 정상적으로 연결된 상태에서 사운드 재생
            swordAudioSource.PlayOneShot(swordAttackSound);
        //PlayOneShot은 이름 그대로 현재 재생 중인 다른 소리를 끊지 않고, 새로운 소리를 한 번만 재생하는 함수야.

        Debug.Log("SwordAttack 함수 호출됨!");

        if (SwordAttackCollider == null)//swordAttackCollider가 null상태와 같다면? 반대는 !=
        {
            Debug.LogError("PlayerAttack: Sword Attack Collider가 연결되지 않아 검 공격을 수행할 수 없어!");
            return;
        }


        //SwordPoint 오브젝트의 BoxCollider2D의 월드 공간 위치와 크기를 가져옴.
        Vector2 colliderCenter = SwordAttackCollider.transform.position + (Vector3)SwordAttackCollider.offset;
        Vector2 colliderSize = SwordAttackCollider.size;
        float colliderAngle = SwordAttackCollider.transform.rotation.eulerAngles.z;

        //BoxCollider2D 영역 내의 모든 콜라이더를 감지해. 밑의 foreach문에서 몬스터에겐 특정한 행동을 하게 하는거지!
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(colliderCenter, colliderSize, colliderAngle);
        Debug.Log($"SwordAttack: {hitColliders.Length}개의 콜라이더 감지됨.");
        //OverlapBoxAll()로 콜라이더 범위 안에 있는 것들을 감지하면, 그 결과물(감지된 모든 콜라이더들)은 hitColliders라는 배열 변수에 담겨.
        //이 hitColliders 변수는 이제 '감지된 오브젝트들의 목록'이 되는 거지.


        //배열(array)이나 컬렉션(collection)의 모든 요소(item)들을 순서대로 하나씩 꺼내와서 처리할 때 사용하는 반복문 foreach.
        //for 문처럼 인덱스(순서 번호)를 직접 관리할 필요 없이, 컬렉션에 있는 모든 요소를 간편하게 반복할 수 있다는 장점이 있어
        //결론적으로, foreach 문 덕분에 hitColliders 배열에 몬스터가 몇 마리가 들어있든,
        //모든 몬스터에게 데미지를 주는 코드를 효율적으로 작성할 수 있는 거야. 인덱스를 직접 관리하는 복잡함 없이!
        foreach (Collider2D hitCollider in hitColliders)
        {//in 키워드는 "~안에 있는" 또는 "~의 구성원인" 이라는 의미로 이해하면 돼.
         //foreach 에서는 "오른쪽에 있는 컬렉션(hitColliders) 안에 있는 각각의 요소(hitCollider)에 대해" 반복하라는 지시를 내리는 역할을 해.

            //foreach 문을 쓸 때는 in 키워드를 반드시 써야 해. in이 없으면 foreach 문은 어떤 컬렉션에서 요소를 하나씩 꺼내와야 할지 알 수 없어.
            //foreach는 in과 함께 하나의 문법적인 짝꿍이라고 생각하면 돼.
            //SetDamage
            //플레이어 자신이나 SwordPoint 오브젝트는 건너뛰기
            if (hitCollider.gameObject == this.gameObject || hitCollider.gameObject == SwordAttackCollider.gameObject)
                continue;
            //continue는 foreach 문이나 다른 반복문(for, while 등)에서 현재 반복을 즉시 건너뛰고 다음 반복으로 넘어가게 하는 명령어야.

            //감지된 콜라이더의 태그가 'enemyTag' 변수와 일치하는지 확인
            if (hitCollider.CompareTag(EnemyTag))
            {
                EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(SwordDamage);//검 공격력 만큼 데미지 적용
                    Debug.Log(hitCollider.name + "에게 " + SwordDamage + "만큼의 검 데미지 부여! (SwordPoint 감지)");

                    Enemy enemyScript = hitCollider.GetComponent<Enemy>();//넉백 로직 호출 (Enemy 스크립트의 함수 호출)
                    if (enemyScript != null)
                    {
                        //플레이어 위치에서 몬스터 위치로 향하는 방향 벡터 계산
                        Vector2 knockbackDirection = hitCollider.transform.position - transform.position;//플레이어 -> 몬스터 방향 벡터
                        if (knockbackDirection.x > 0)
                            knockbackDirection = Vector2.right;//몬스터가 플레이어 오른쪽에 있으니 오른쪽으로 밀기
                        else knockbackDirection = Vector2.left;//몬스터가 플레이어 왼쪽에 있으니 왼쪽으로 밀기

                        enemyScript.TakeKnockback(knockbackDirection, KnockbackForce, KnockbackDuration);
                    }
                    else
                        Debug.LogWarning(hitCollider.name + "에는 Enemy 스크립트가 없어서 넉백을 적용할 수 없어!");


                }
                else
                    Debug.LogWarning(hitCollider.name + "에는 EnemyHealth 스크립트가 없습니다!");
            }
        }

        if (SwordEnergy != null)//검 공격시 검기 발사체 생성
        {//SwordPower 변수가 제대로 있다면. != null은 ~~이 null 상태가 아니라면. 뜻인거 이젠 잘 알겠지?

            //플레이어의 방향에 따라 발사 방향을 결정
            Vector2 launchDirection = SpriteRenderer.flipX ? Vector2.left : Vector2.right;
            //발사체 생성, swordPowerSpawnPoint 오브젝트가 발사체 생성 위치야
            GameObject swordPowerInstance = Instantiate(SwordEnergy, SwordEnergySpawnPoint.position, Quaternion.identity);

            SwordEnergy swordenergy = swordPowerInstance.GetComponent<SwordEnergy>();//SwordEnergy 스크립트를 가져와서 데미지 값을 넘겨줌
            if (swordenergy != null)
            {
                float totalDamage = SwordEnergyDamage;
                swordenergy.SetDamage(totalDamage);
            }

            if (SpriteRenderer.flipX)//케릭터가 보는 방향에 따라 발사체의 방향 설정
                //캐릭터가 왼쪽을 보고 있으면 발사체를 180도 회전
                swordPowerInstance.transform.rotation = Quaternion.AngleAxis(180, Vector3.forward);
            else
                swordPowerInstance.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
            //캐릭터가 오른쪽을 보고 있으면 발사체를 0도(원래 방향)로 설정


            //발사체의 Rigidbody2D를 가져와서 힘을 가해
            Rigidbody2D rb = swordPowerInstance.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = launchDirection * SwordEnergySpeed;

        }
        else Debug.LogWarning("PlayerAttack: swordProjectilePrefab이 연결되지 않았어! 검 발사체 공격을 할 수 없어.");
        lastSwordSkillTime = Time.time;
    }

    public bool CanAttack() // 검 공격 쿨타임 체크 함수
    {
        //쿨타임이 끝났을 때만 True를 반환
        return Time.time >= lastSwordSkillTime + SwordSkillCooldown;
    }

    private void UpdateSwordSkillUI()//검 스킬의 쿨타임을 계산하고 UI를 업데이트하는 함수
    {                                //Update 함수로 호출했으니 매 프레임마다 호출되지!
        //남은 쿨타임 계산
        float timeRemaining = lastSwordSkillTime + SwordSkillCooldown - Time.time;//마지막 스킬 사용 시점 + 총 쿨타임 시간 - 현재 시간

        //쿨타임이 남아있다면, timeRemaining이 0보다 크다는 것은 아직 쿨타임이 진행 중이라는 뜻이잖아.
        if (timeRemaining > 0)
        {
            CooldownText.gameObject.SetActive(true);//쿨타임 숫자(텍스트)를 보이게 해
            CooldownText.text = Mathf.Ceil(timeRemaining).ToString("F0");

            //오버레이 이미지의 fillAmount를 조절해서 시각적으로 보여줌
            CooldownOverlay.gameObject.SetActive(true);//검은색 오버레이를 보이게 해.
            CooldownOverlay.fillAmount = timeRemaining / SwordSkillCooldown;
            //남은 쿨타임 비율에 맞춰 검은색 오버레이가 서서히 사라지도록 만들어.
        }
        else//timeRemaining이 0보다 작거나 같을 경우(쿨타임이 끝난 경우) else 문 안의 코드가 실행
        {
            //쿨타임이 끝나면
            CooldownText.gameObject.SetActive(false);//쿨타임 숫자를 숨겨
            CooldownOverlay.gameObject.SetActive(false);//검은색 오버레이를 숨겨(비활성화)
        }
    }
}
