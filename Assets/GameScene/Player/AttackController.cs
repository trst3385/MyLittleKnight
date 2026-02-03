using UnityEngine;


public class AttackController : MonoBehaviour
{
    //내부 참조 (Awake에서 자동 연결)
    private Player player;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BowWeapon bowWeapon;
    private SwordWeapon swordWeapon;

    private bool isAttacking = false;//현재 공격 중인지 확인하는 변수


    void Awake()
    {
        //같은 오브젝트 내의 컴포넌트들 자동 연결
        player = GetComponent<Player>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bowWeapon = GetComponent<BowWeapon>();
        swordWeapon = GetComponent<SwordWeapon>();

        //[방어적 프로그래밍] 참조 검증
        CheckInitialization();
    }

    private void CheckInitialization()
    {
        if (player == null) Debug.LogError($"{gameObject.name}: Player 스크립트를 찾을 수 없어!");
        if (animator == null) Debug.LogError($"{gameObject.name}: Animator를 찾을 수 없어!");

        //무기는 없을 수도 있으니 경고(Warning) 정도로 처리
        if (bowWeapon == null) Debug.LogWarning($"{gameObject.name}: BowWeapon 미연결! (활 공격 불가)");
        if (swordWeapon == null) Debug.LogWarning($"{gameObject.name}: SwordWeapon 미연결! (검 공격 불가)");
    }


    void Update()
    {
        //플레이어가 죽었거나, 카운트다운이 아직 안 끝났으면 공격 입력 무시!
        if ((player != null && player.IsDead) || !CountdownManager.isCountdownFinished)
            return;

        AttackInput();//공격 입력 처리 함수 호출        
    }

    void AttackInput()//각 공격 입력 처리 함수
    {
        //활 공격 (Space)
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)
        {
            if (bowWeapon != null && bowWeapon.CanAttack()) ExecuteAttack("Attack(Bow)");
        }

        //검 공격 (Q)
        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking)
        {
            if (swordWeapon != null && swordWeapon.CanAttack()) ExecuteAttack("Attack(Sword)");
        }
    }

    //공격 실행 및 상태 관리 집중화
    private void ExecuteAttack(string triggerName)
    {
        isAttacking = true;
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }

    public void OnAttackEnd() => isAttacking = false;//검 공격과 활 공격 애니메이션이 끝났을 때 isAttacking을 false로 되돌릴 함수
    //선언과 같이 이것도 false. 게임 플레이 도중에 공격이 끝났을 때의 상태를 false로 바꿔주는 역할을 해.                              
    //애니메이션의 SwordAttack, ShootArrow 이벤트와 겹쳐져 있어
}





