using UnityEngine;


public class AttackController : MonoBehaviour
{
    //내부 참조 (Awake에서 자동 연결)
    private Player player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BowWeapon bowWeapon;
    private SwordWeapon swordWeapon;
    private InvincibilitySkill invincSkill;//무적 아이템 사용(E 버튼입력)을 위해 추가

    private bool isBowAttacking = false;//현재 활공격 중인지 확인하는 변수
    private bool isSwordAttacking = false;//현재 검공격 중인지 확인하는 변수


    void Awake()
    {
        //같은 오브젝트 내의 컴포넌트들 자동 연결
        player = GetComponent<Player>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bowWeapon = GetComponent<BowWeapon>();
        swordWeapon = GetComponent<SwordWeapon>();
        invincSkill = GetComponent<InvincibilitySkill>();

        CheckInitialization();//[방어적 프로그래밍] 참조 검증
    }

    private void CheckInitialization()
    {
        if (player == null)
        {
            Debug.LogError($"{gameObject.name}: Player 스크립트를 찾을 수 없어!");
        }
        if (animator == null)
        {
            Debug.LogError($"{gameObject.name}: Animator를 찾을 수 없어!");
        }
        if (bowWeapon == null)
        {
            Debug.LogError($"{gameObject.name}: BowWeapon 미연결! (활 공격 불가)");
        }
        if (swordWeapon == null)
        {
            Debug.LogError($"{gameObject.name}: SwordWeapon 미연결! (검 공격 불가)");
        }
        if (invincSkill == null)
        {
            Debug.LogError($"{gameObject.name}: InvincibilitySkill 미연결!");
        }
    }


    void Update()
    {
        //플레이어가 죽었거나, 카운트다운이 아직 안 끝났으면 공격 입력 무시!
        if ((player != null && player.IsDead) || !CountdownManager.isCountdownFinished)
        {
            return;
        }

        HandleAutoAttack();//자동 공격 처리 (활)
        HandleInputs();//수동 입력 처리 (검, 무적)
    }

    //---활: 자동 공격 로직---
    void HandleAutoAttack()//4.17:활 공격 키인 Space 입력 없이 쿨타임만 차면 자동으로 실행(자동 공격)
    {
        if (bowWeapon != null && !isBowAttacking)//활이 있고, 현재 활 쏘는 모션 중이 아닐 때
        {
            //쿨타임까지 찼다면 발사!
            if (bowWeapon.CanAttack())
            {
                ExecuteBowAttack();
            }
        }
    }

    //---검 & 무적: 수동 입력 로직---
    void HandleInputs()
    {
        //검 공격 (Q) - 활 공격 중이어도 검 상태가 false면 실행 가능
        if (Input.GetKeyDown(KeyCode.Space) && !isSwordAttacking)
        {
            if (swordWeapon != null && swordWeapon.CanAttack())
            {
                ExecuteSwordAttack();
            }
        }

        //무적 스킬 (E)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (invincSkill != null && invincSkill.CanUse())
            {
                invincSkill.ActivateSkill();
            }
        }
    }

    //---실행 함수들---
    private void ExecuteBowAttack()
    {
        isBowAttacking = true;//중복 실행 방지 잠금
        animator.SetTrigger("Attack(Bow)");
    }
    private void ExecuteSwordAttack()
    {
        isSwordAttacking = true;//중복 실행 방지 잠금
        isBowAttacking = false;//활 공격 중에 검 공격이 들어온 거니까, 
                               //활 공격이 끝났다고 강제로 표시해줘야 다음 자동 공격이 돌아가
   
        animator.SetTrigger("Attack(Sword)");
    }

    //검 공격과 활 공격 애니메이션이 끝났을 때 is...Attacking변수를 false로 되돌릴 함수
    //선언과 같이 이것도 false. 공격을 해서 true가 된 후, 공격이 끝났을 때의 상태를 false로 바꿔주는 역할을 해.                              
    public void OnBowAttackEnd() => isBowAttacking = false;
    public void OnSwordAttackEnd() => isSwordAttacking = false;
}