using TMPro;//TextMeshPro를 사용하면 추가
using UnityEngine;
using UnityEngine.UI;//UI 사용을 위해 추가



public class AttackController : MonoBehaviour
{
    //외부참조 (인스펙터에서 연결)
    [Header("스크립트, 컴포넌트 참조")]
    public Player Player;//Player스크립트 참조
    public Animator Animator;//Player오브젝트의 Animator 참조
    public SpriteRenderer SpriteRenderer;//player오브젝트의 SpriteRenderer참조
    public BowWeapon bowWeapon;
    public SwordWeapon swordWeapon;

    //공통 공격 변수
    private WeaponType CurrentWeaponType = WeaponType.Bow;//현재 장착 무기 타입 (인스펙터 설정)
    private bool isAttacking = false;//현재 공격 중인지 확인하는 변수
                                     //변수가 처음 만들어질 때의 초기값을 false로 정해주는 거야,
                                     //게임이 시작하거나 캐릭터가 생성될 때 "현재 공격 중이 아님" 상태로 시작하겠다는 의미지.

   
    void Start()
    {
        Player = GetComponent<Player>();
        Animator = GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
         
        //참조가 제대로 되었는지 확인하는 Debug.LogError
        if (Player == null) 
            Debug.LogError("PlayerAttack: Player 스크립트를 찾을 수 없어! Player 오브젝트에 Player 스크립트가 있는지 확인해!");
        if (Animator == null) 
            Debug.LogError("PlayerAttack: Animator 컴포넌트를 찾을 수 없어! Player 오브젝트에 Animator 컴포넌트가 있는지 확인해!");
        if (SpriteRenderer == null) 
            Debug.LogError("PlayerAttack: SpriteRenderer 컴포넌트를 찾을 수 없어! Player 오브젝트에 SpriteRenderer 컴포넌트가 있는지 확인해!");
    }
    
    void Update()
    {
        //플레이어가 죽었으면 공격 입력 받지 않음
        if (Player != null && Player.IsDead) return;

        AttackInput();//공격 입력 처리 함수 호출        
    }

    void AttackInput()//각 공격 입력 처리 함수
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)//활 공격은 스페이스 바
        {
            if (CurrentWeaponType == WeaponType.Bow)
            {
                //활 공격 쿨타임 체크를 BowWeapon에 위임
                if (bowWeapon != null && bowWeapon.CanAttack())
                {
                    weaponAnimator(WeaponType.Bow);//활 애니메이션 호출
                    isAttacking = true;
                }
            }               
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking)
        {
            if (swordWeapon != null && swordWeapon.CanAttack())
            {
                weaponAnimator(WeaponType.Sword);//검 애니메이션 호출
                isAttacking = true;
            }
        }
    }

    void weaponAnimator(WeaponType weaponType)//무기 타입에 따라 애니메이션 발동 함수
    {
        if (Animator == null)
        {
            Debug.Log("weaponAnimator: Animator가 null이야!. 공격 애니메이션 발동 못해!");
            return;
        }

        switch (weaponType)
        {
            case WeaponType.Bow://현재 무기가 활이면
                Animator.SetTrigger("Attack(Bow)");                           
                break;
            case WeaponType.Sword://현재 무기가 검이면
                Animator.SetTrigger("Attack(Sword)");
                break;
        }
    }
    
    public void OnAttackEnd()//검 공격과 활 공격 애니메이션이 끝났을 때 isAttacking을 false로 되돌릴 함수
    {                        
        isAttacking = false;//선언과 같이 이것도 false. 게임 플레이 도중에 공격이 끝났을 때의 상태를 false로 바꿔주는 역할을 해.
                            //시작: 게임이 시작되면 isAttacking은 false 야.
        Debug.Log("OnAttackEnd() 함수가 호출되었어!");
        
    }
}





