using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//UI 사용을 위해 추가


public class BowWeapon : MonoBehaviour
{
    [Header("활 공격 관련")]
    //활 공격 관련
    public GameObject ArrowPrefab;//화살 프리팹을 담을 변수(인스펙터 연결)
    public Transform ArrowSpawnPoint;//화살 생성 위치(인스펙터 연결)
    public float ArrowSpeed = 10f;//화살이 날라가는 속도(인스펙터 설정)
    public float ArrowDamage = 1f;//화살의 데미지(인스펙터 설정)
    //활 공격 360도 화살 공격 관련
    public int NumberOfArrows360 = 8;//360도 공격 시 발사할 화살의 수 (예: 8방향)

    [Header("공격 속도 관련")]
    public float BaseArrowCooldown = 2f;//활의 기본 공격 쿨타임
    [SerializeField]private float lastArrowAttackTime = -1f;//마지막으로 활 공격을 한 시간을 기록
    private float currentArrowCooldown;//활의 현재 공격 쿨타임

    [Header("활 공격 사운드")]//사운드 관련 변수, 하나의 AudioSource 컴포넌트는 한 번에 하나의 AudioClip만 재생할 수 있기 때문
                        //지금 플레이어 오브젝트에는 Player 스크립트에 걸을때 사운드도 있잖아? 사운드 하나당 오디오 컴포넌트 하나씩이야!
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

    public void ShootArrow()//360도 화살 공격 로직
    {
        if (bowAudioSource != null && bowAttackSound != null)//활 공격시 변수들이 잘 작동이 되면
            bowAudioSource.PlayOneShot(bowAttackSound);
        //PlayOneShot은 이름 그대로 현재 재생 중인 다른 소리를 끊지 않고, 새로운 소리를 한 번만 재생하는 함수야.
        else Debug.LogWarning("활 공격 사운드 AudioSource 또는 AudioClip이 연결되지 않았어!");

      
        if (ArrowPrefab == null)//화살 프리팹이 잘 연결되있냐 안되있냐
        {
            Debug.LogError("Arrow Prefab이 연결되지 않았어!");
            return;
        }

        //화살이 발사될때 로그에 알림
        Debug.Log("화살 발사! 설정된 데미지: " + this.ArrowDamage);

        //각 화살 사이의 각도 계산
        float angleStep = 360f / NumberOfArrows360;

        for (int i = 0; i < NumberOfArrows360; i++)
        {
            float angle = i * angleStep;//현재 화살의 각도(0, 45, 90, ...)

            //각도를 라디안으로 변환
            float radianAngle = angle * Mathf.Deg2Rad;

            //발사 방향 벡터 계산
            Vector2 direction = new Vector2(Mathf.Cos(radianAngle), Mathf.Sin(radianAngle)).normalized;

            //화살 생성 (플레이어의 정중앙에서 발사되도록 transform.position 사용)                   
            GameObject arrow = Instantiate(ArrowPrefab, ArrowSpawnPoint.position, Quaternion.identity);

            //화살의 Rigidbody2D 가져오기
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.velocity = direction * ArrowSpeed;

                //화살이 날아가는 방향에 맞춰 회전
                angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);



                //화살의 Arrow 스크립트에 데미지 전달
                Arrow arrowScript = arrow.GetComponent<Arrow>();
                if (arrowScript != null)
                    arrowScript.ArrowDamage = this.ArrowDamage;//기존 arrowDamage 사용                                                            
                else Debug.LogWarning("발사된 화살 프리팹에 'Arrow' 스크립트가 없다! 데미지 설정 불가!");
            }
            else { Debug.LogWarning("발사된 화살 프리팹에 Rigidbody2D가 없어!"); }
        }
        lastArrowAttackTime = Time.time;

    }

    public void DecreaseAttackCooldown(float coolDown, WeaponType weaponType)//활 아이템 획득 시 공속 증가 함수
    {                                                  //코드의 안전성, 재사용성을 위해 따로 함수를 만들어두자!                     
        Debug.Log($"DecreaseAttackCooldown 함수 호출됨. 현재 쿨타임: {currentArrowCooldown}, 감소량: {coolDown}");
        //코드의 안전성, 재사용성을 위해 따로 함수를 만들어두자!                     
        switch (weaponType)
        {
            case WeaponType.Bow:
                currentArrowCooldown -= coolDown;
                if (currentArrowCooldown < 1.0f)//여기서 활 공격의 최대 쿨타임을 1초로 제한
                    currentArrowCooldown = 1.0f;
                break;
        }
        Debug.Log($"쿨타임 감소 후 최종 쿨타임: {currentArrowCooldown}");
    }

    public bool CanAttack()//쿨타임 체크 함수
    {   
        //마지막 공격 시간이 - 1f 이거나(게임 첫 시작) 쿨타임이 끝났을 때 True 반환
        if (lastArrowAttackTime < 0f || Time.time >= lastArrowAttackTime + currentArrowCooldown)
            return true;
        else
        {
            // 쿨타임이 아직 남았을 때 디버그 로그 출력
            float timeRemaining = lastArrowAttackTime + currentArrowCooldown - Time.time;
            Debug.Log("활 공격 쿨타임 중. 남은 시간: " + timeRemaining.ToString("F1") + "초");
            return false;
        }
    }

    private void UpdateBowUI()//활 공격 쿨타임 UI바 함수
    {
        //게임 시작 시, 쿨타임이 없는 상태라면 UI를 바로 비활성화
        if (lastArrowAttackTime < 0f)
        {
            BowCooldownBar.gameObject.SetActive(false);
            return;//함수를 즉시 종료
        }

        //남은 쿨타임 계산
        float timeRemaining = lastArrowAttackTime + currentArrowCooldown - Time.time;

        //쿨타임이 남아있다면
        if (timeRemaining > 0)
        {
            BowCooldownBar.gameObject.SetActive(true);
            BowCooldownBar.maxValue = currentArrowCooldown;//슬라이더의 최대값을 총 쿨타임으로 설정
            BowCooldownBar.value = timeRemaining;//슬라이더 값을 남은 시간으로 설정
        }
        else BowCooldownBar.gameObject.SetActive(false);//쿨타임이 끝나면 바를 숨김          
    }
}
