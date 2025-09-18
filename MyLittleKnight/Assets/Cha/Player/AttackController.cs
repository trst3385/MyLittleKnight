using TMPro;//TextMeshPro�� ����ϸ� �߰�
using UnityEngine;
using UnityEngine.UI;//UI ����� ���� �߰�



public class AttackController : MonoBehaviour
{
    //�ܺ����� (�ν����Ϳ��� ����)
    [Header("��ũ��Ʈ, ������Ʈ ����")]
    public Player Player;//Player��ũ��Ʈ ����
    public Animator Animator;//Player������Ʈ�� Animator ����
    public SpriteRenderer SpriteRenderer;//player������Ʈ�� SpriteRenderer����
    public BowWeapon bowWeapon;
    public SwordWeapon swordWeapon;

    //���� ���� ����
    private WeaponType CurrentWeaponType = WeaponType.Bow;//���� ���� ���� Ÿ�� (�ν����� ����)
    private bool isAttacking = false;//���� ���� ������ Ȯ���ϴ� ����
                                     //������ ó�� ������� ���� �ʱⰪ�� false�� �����ִ� �ž�,
                                     //������ �����ϰų� ĳ���Ͱ� ������ �� "���� ���� ���� �ƴ�" ���·� �����ϰڴٴ� �ǹ���.

   
    void Start()
    {
        Player = GetComponent<Player>();
        Animator = GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
         
        //������ ����� �Ǿ����� Ȯ���ϴ� Debug.LogError
        if (Player == null) 
            Debug.LogError("PlayerAttack: Player ��ũ��Ʈ�� ã�� �� ����! Player ������Ʈ�� Player ��ũ��Ʈ�� �ִ��� Ȯ����!");
        if (Animator == null) 
            Debug.LogError("PlayerAttack: Animator ������Ʈ�� ã�� �� ����! Player ������Ʈ�� Animator ������Ʈ�� �ִ��� Ȯ����!");
        if (SpriteRenderer == null) 
            Debug.LogError("PlayerAttack: SpriteRenderer ������Ʈ�� ã�� �� ����! Player ������Ʈ�� SpriteRenderer ������Ʈ�� �ִ��� Ȯ����!");
    }
    
    void Update()
    {
        //�÷��̾ �׾����� ���� �Է� ���� ����
        if (Player != null && Player.IsDead) return;

        AttackInput();//���� �Է� ó�� �Լ� ȣ��        
    }

    void AttackInput()//�� ���� �Է� ó�� �Լ�
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isAttacking)//Ȱ ������ �����̽� ��
        {
            if (CurrentWeaponType == WeaponType.Bow)
            {
                //Ȱ ���� ��Ÿ�� üũ�� BowWeapon�� ����
                if (bowWeapon != null && bowWeapon.CanAttack())
                {
                    weaponAnimator(WeaponType.Bow);//Ȱ �ִϸ��̼� ȣ��
                    isAttacking = true;
                }
            }               
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isAttacking)
        {
            if (swordWeapon != null && swordWeapon.CanAttack())
            {
                weaponAnimator(WeaponType.Sword);//�� �ִϸ��̼� ȣ��
                isAttacking = true;
            }
        }
    }

    void weaponAnimator(WeaponType weaponType)//���� Ÿ�Կ� ���� �ִϸ��̼� �ߵ� �Լ�
    {
        if (Animator == null)
        {
            Debug.Log("weaponAnimator: Animator�� null�̾�!. ���� �ִϸ��̼� �ߵ� ����!");
            return;
        }

        switch (weaponType)
        {
            case WeaponType.Bow://���� ���Ⱑ Ȱ�̸�
                Animator.SetTrigger("Attack(Bow)");                           
                break;
            case WeaponType.Sword://���� ���Ⱑ ���̸�
                Animator.SetTrigger("Attack(Sword)");
                break;
        }
    }
    
    public void OnAttackEnd()//�� ���ݰ� Ȱ ���� �ִϸ��̼��� ������ �� isAttacking�� false�� �ǵ��� �Լ�
    {                        
        isAttacking = false;//����� ���� �̰͵� false. ���� �÷��� ���߿� ������ ������ ���� ���¸� false�� �ٲ��ִ� ������ ��.
                            //����: ������ ���۵Ǹ� isAttacking�� false ��.
        Debug.Log("OnAttackEnd() �Լ��� ȣ��Ǿ���!");
        
    }
}





