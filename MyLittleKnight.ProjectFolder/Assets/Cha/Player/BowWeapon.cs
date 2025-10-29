using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BowWeapon : MonoBehaviour
{
    [Header("활 공격,방향,프리팹,오브젝트")]
    //활 공격 설정
    public GameObject ArrowPrefab;//화살 프리팹을 보관할 변수 (인스펙터 설정)
    public Transform ArrowSpawnPoint;//ȭ�� ���� ��ġ(�ν����� ����)
    public float ArrowSpeed = 10f;//발사 속도
    public float ArrowDamage = 1f;//화살 데미지
    public int NumberOfArrows360 = 8;//360도 회전 시 발사할 화살의 수 (예: 8방향)

    [Header("공격 속도")]
    public float BaseArrowCooldown = 2f;//활의 초기/최대 공격 쿨타임 (아이템 효과 미적용 원본)
    [SerializeField]private float lastArrowAttackTime = -1f;//마지막으로 화살을 발사한 유니티 시간 (쿨타임 계산 시작점)
    private float currentArrowCooldown;//아이템,스킬 효과가 적용된 현재 활의 공격 쿨타임(이 값으로 공격 주기 결정)

    [Header("사운드")]//사운드 설정.하나의 AudioSource 컴포넌트에 여러 AudioClip을 사용할 수 있음.
    [SerializeField] private AudioSource bowAudioSource;
    [SerializeField] private AudioClip bowAttackSound;


    [Header("UI 연결")]
    public Slider BowCooldownBar;

    void Start()
    {
        currentArrowCooldown = BaseArrowCooldown;
    }
    void Update()
    {
        UpdateBowUI();
    }

    public void ShootArrow()//360도 화살 발사
    {
        if (bowAudioSource != null && bowAttackSound != null)//활 공격 시 사운드 재생
            bowAudioSource.PlayOneShot(bowAttackSound);
        //PlayOneShot은 이미 재생 중인 소리가 있어도 다른 소리를 끊지 않고, 새로운 소리를 겹쳐서 재생시키는 함수
        else Debug.LogWarning("활 공격 사운드 AudioSource 또는 AudioClip이 설정되지 않았어!");

      
        if (ArrowPrefab == null)//화살 프리팹이 설정되었는지 안 되었는지
        {
            Debug.LogError("Arrow Prefab이 설정되지 않았어!");
            return;
        }
        //화살을 발사할 때 로그에 알림
        Debug.Log("화살 발사! 설정된 데미지: " + this.ArrowDamage);

        //각 화살의 각도 계산
        float angleStep = 360f / NumberOfArrows360;

        for (int i = 0; i < NumberOfArrows360; i++)
        {
            float angle = i * angleStep;//현재 화살의 각도(0, 45, 90, ...)

            //Cos/Sin 계산을 위해 '도(Degree)' 단위를 '라디안(Radian)'으로 변환 (컴퓨터가 이해하는 각도 단위로 변경)
            float radianAngle = angle * Mathf.Deg2Rad;

            //발사 방향 벡터 계산
            Vector2 direction = new Vector2(Mathf.Cos(radianAngle), Mathf.Sin(radianAngle)).normalized;

            //화살 생성 (플레이어의 중앙에서 발사되도록 ArrowSpawnPoint 오브젝트 사용)                  
            GameObject arrow = Instantiate(ArrowPrefab, ArrowSpawnPoint.position, Quaternion.identity);

            
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.linearVelocity = direction * ArrowSpeed;

                //화살을 움직이는 방향에 따라 회전
                angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                //화살의 Arrow 스크립트에 데미지 설정
                Arrow arrowScript = arrow.GetComponent<Arrow>();
                if (arrowScript != null)
                    arrowScript.ArrowDamage = this.ArrowDamage;//현재 ArrowDamage 값                                                         
                else Debug.LogWarning("생성된 화살 프리팹에 'Arrow' 스크립트가 없어! 데미지 설정 불가능!");
            }
            else Debug.LogWarning("생성된 화살 프리팹에 Rigidbody2D가 없어!");
        }
        lastArrowAttackTime = Time.time;
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

    private void UpdateBowUI()//활 공격 쿨타임 UI 업데이트
    {
        //아직 공격 전, 쿨타임이 끝난 상태라면 UI를 바로 비활성화
        if (lastArrowAttackTime < 0f)
        {
            BowCooldownBar.gameObject.SetActive(false);
            return;//함수 종료
        }

        //현재 남은 쿨타임 계산
        float timeRemaining = lastArrowAttackTime + currentArrowCooldown - Time.time;

        //쿨타임이 남아 있다면
        if (timeRemaining > 0)
        {
            BowCooldownBar.gameObject.SetActive(true);
            BowCooldownBar.maxValue = currentArrowCooldown;//슬라이더의 최댓값을 총 쿨타임으로 설정
            BowCooldownBar.value = timeRemaining;//슬라이더 값은 남은 시간으로 설정
        }
        else BowCooldownBar.gameObject.SetActive(false);//쿨타임이 끝났을 경우 (UI 비활성화)        
    }
}
