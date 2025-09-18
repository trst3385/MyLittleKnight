using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//PlayerStatsEffects ��ũ��Ʈ?  �÷��̾��� �ɷ�ġ ���� ������ �ѵ� ��� �����ϴ� ������ ��.
//������ ȹ�� �� �÷��̾��� ���ݷ�, �̵� �ӵ�, ü�� ���� �����ϰ�, �� ���� ������ UI�� �ݿ��ϴ� ��� ���� ����ϴ� �����̾�.

//�߾� ���� ����: Item ��ũ��Ʈ���� ���� PlayerAttack�̳� PlayerHealth ��ũ��Ʈ�� �����ϴ� ���,
//PlayerStatsEffects ��ũ��Ʈ�� �Լ��� ȣ���ؼ� ��� �ɷ�ġ ���� ������ ó����.

//�������� �� Ȯ�强: ���ο� �ɷ�ġ �������� �߰��ǰų� UI�� �ٲ����,
//PlayerStatsEffects ��ũ��Ʈ�� �����ϸ� �Ǳ� ������ �ڵ尡 �ξ� ����ϰ� ������ ������.

//���� å�� ��Ģ: �� ��ũ��Ʈ�� �ɷ�ġ ������� �� ���� å�Ӹ� ���� ������,
//�ٸ� ��ũ��Ʈ(��: Player, PlayerAttack)�� �ڱ� ������ ����(�̵�, ����)���� ������ �� �ְ� ��.


///<summary>
///�÷��̾��� �ɷ�ġ ��ȭ �� UI ������Ʈ�� �����ϴ� ��ũ��Ʈ��.
///</summary> 
//�̷��� �߰��� PlayerStatsEffects ��ũ��Ʈ�� �θ�, Item ��ũ��Ʈ�� "���� �̷� �������̾�"��� �˷��ְ�,
//������ �ɷ�ġ�� �ٲٴ� ������ ���� PlayerStatsEffects�� ����ϰ� �Ǵ� �ڵ尡 �ξ� ��������� ����. ///�ּ��� �̷� �뵵��


public class PlayerStatsEffects : MonoBehaviour
{
    [Header("�ش� �ؽ�Ʈ UI ����")]
    //UI �ؽ�Ʈ ������Ʈ ���� ����(�ν����� ������)
    public TextMeshProUGUI ArrowLevelText;
    public TextMeshProUGUI SwordLevelText;
    public TextMeshProUGUI MoveSpeedLevelText;
    public TextMeshProUGUI ArrowSpeedText;

    [Header("�ִ�ġ ���� ���� ����")]
    public int MaxLevel = 10;//��� �̼�,Ȱ,�� ������ ������ �ִ�ġ


    //Ȱ�� ���� ��ȭ Ƚ���� ������ ����
    //[HideInInspector]�� �ᵵ ������ �� ������ ���� ���� ���� ������ private���� �߾�. �� �� ������ �ܺο����� ���� �����ϱ�
    private int currentArrowLevel = 0;
    private int currentSwordLevel = 0;
    private int currentMoveSpeedLevel = 0;

    private Player player;//Player ��ũ��Ʈ ���� (�̵� �ӵ� ������ ����)
    private PlayerHealth playerHealth;//PlayerHealth ��ũ��Ʈ ���� (ü�� ȸ���� ����)
    private BowWeapon bowWeapon;//PlayerAttack ��ũ��Ʈ ���� (���ݷ� ���� ������ ����)
    private PlayerShield playerShield;//PlayerShield ��ũ��Ʈ ���� (���� ȸ���� ����)
    private SwordWeapon swordWeapon;//SwordWeapon ��ũ��Ʈ ����

    void Start()
    {
        player = GetComponent<Player>();
        playerHealth = GetComponent<PlayerHealth>();
        bowWeapon = GetComponent<BowWeapon>();
        playerShield = GetComponent<PlayerShield>();
        swordWeapon = GetComponent<SwordWeapon>();

        if (player == null) Debug.LogWarning("PlayerStatsEffects: Player ��ũ��Ʈ�� ã�� �� �����ϴ�!");
        if (playerHealth == null) Debug.LogWarning("PlayerStatsEffects: PlayerHealth ��ũ��Ʈ�� ã�� �� �����ϴ�!");
        if (bowWeapon == null) Debug.LogWarning("PlayerStatsEffects: PlayerAttack ��ũ��Ʈ�� ã�� �� �����ϴ�!");
        if (playerShield == null) Debug.LogWarning("PlayerStatsEffects: PlayerShield ��ũ��Ʈ�� ã�� �� �����ϴ�!");

        //���� ���� �� UI �ؽ�Ʈ �ʱ�ȭ
        UpdateWeaponLevelUI();
    }


    //������ ȿ�� �Լ���
    public void ArrowDamageUp(float ItemCSdamage, float coolDown)
    {
        if (currentArrowLevel < MaxLevel)//���� ������ MaxLevel���� ���� ���� ����
        {
            if(bowWeapon != null)
            {
                bowWeapon.ArrowDamage += ItemCSdamage;//PlayerAttack ��ũ��Ʈ�� Ȱ ���ݷ� ����
                bowWeapon.DecreaseAttackCooldown(coolDown, WeaponType.Bow);//PlayerAttack ��ũ��Ʈ�� ���ݼӵ� ����
                currentArrowLevel++;//Ȱ ��ȭ ���� ����
                UpdateWeaponLevelUI();//UI ������Ʈ
                Debug.Log("PlayerStatsEffects: ȭ�� ���ݷ��� " + ItemCSdamage + " ����! ���� ���ݷ�: " + bowWeapon.ArrowDamage);
            }
        }
        else//�ִ� ������ �������� ���� �޽���
            Debug.Log("PlayerStatsEffects: ȭ�� ������ �̹� �ִ�ġ��!");
    }

    public void SwordDamageUp(float ItemCSdamage)
    {
        if (currentSwordLevel < MaxLevel)//���� ������ MaxLevel���� ���� ���� ����
        {
            if(bowWeapon != null)
            {
                swordWeapon.SwordDamage += ItemCSdamage;//SwordWeapon�� �� ���ݷ� ����
                swordWeapon.SwordEnergyDamage += ItemCSdamage;//SwordWeapon�� �˱� ���ݷ� ����
                currentSwordLevel++;//�� ��ȭ ���� ����
                UpdateWeaponLevelUI();//UI ������Ʈ
                Debug.Log("PlayerStatsEffects: �� ���ݷ��� " + ItemCSdamage + " �����ߴ�! ���� ���ݷ�: " + swordWeapon.SwordDamage);
                Debug.Log("PlayerStatsEffects: �˱� �߻�ü ���ݷ��� " + ItemCSdamage + " �����ߴ�! ���� ���ݷ�: " + swordWeapon.SwordEnergyDamage);
            }
        }
        else//�ִ� ������ �������� ���� �޽���
            Debug.Log("PlayerStatsEffects: �� ������ �̹� �ִ�ġ��!");
    
    }
    public void MoveSpeedUp(float amount)
    {
        if (currentMoveSpeedLevel < MaxLevel)//���� ������ MaxLevel���� ���� ���� ����
        {
            if(player != null)
            {
                player.MoveSpeed += amount;//Player ��ũ��Ʈ�� �̵� �ӵ� ����
                currentMoveSpeedLevel++;//�̵� �ӵ� Ƚ�� ����
                UpdateWeaponLevelUI();//UI ������Ʈ
                                      //�ڵ�� �׻� ������ �Ʒ� ������ ����Ǵϱ� ������ �� ���Ѿ���!
                                      //���� currentMoveSpeedLevel++�� UpdateWeaponLevelUI()�� ������ ���� ���� �޶�������
                                      //�ᱹ UI�� currentMoveSpeedLevel�� 0�� �� �̹� ������Ʈ�� ���Ʊ� ������,
                                      //���߿� currentMoveSpeedLevel�� 1�� �ٲ���� UI���� �ݿ����� �ʴ� �ž�.
                Debug.Log("PlayerStatsEffects: �̵� �ӵ��� " + amount + " �����ߴ�! ���� �ӵ�: " + player.MoveSpeed);
            }
        }
        else// �ִ� ������ �����ϸ� ��� �޽��� ���
            Debug.Log("PlayerStatsEffects: �̵��ӵ� ������ �̹� �ִ�ġ��!");
    }
    public void Heal(float amount)
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(amount);//PlayerHealth ��ũ��Ʈ�� Heal �Լ��� ȣ��!
            Debug.Log("PlayerStatsEffects: ü���� " + amount + " ȸ���Ǿ���! ���� ü��: " + playerHealth.CurrentHealth);
        }
    }

    public void HealShield(float amount)
    {
        if (playerShield != null)
        {
            playerShield.HealShield(amount);//PlayerShield ��ũ��Ʈ�� HealShield �Լ� ȣ��
            Debug.Log("PlayerStatsEffects: ������ " + amount + " ȸ���Ǿ���! ���� ����: " + playerShield.CurrentShield);
        }
    }

    void UpdateWeaponLevelUI()//���� ��ȭ Ƚ�� UI�� ������Ʈ�ϴ� �Լ�
    {
        //if���� != null�̶�? ������ �ƹ��͵� �����ϰ� ���� ���� ��(null)�� �ƴ� �� ��,
        //���� ����� ����Ǿ� ���� �� ��� �ǹ̾�.

        //if�� �ȿ� ? �̶� : �� ����? �̰� ���� ������(Ternary Operator) ��� ��.
        //���� if���� ª�� �ҷ��� �� �ٸ� ������ �߰�ȣ {}�� �Ⱦ��ݾ�. ���� �����ڵ� ����ϸ� if-else���� �� �ٷ� �ٲ� �� �־�.
        //? ���� �� ������ �ڵ�
        //: ������ �� ������ �ڵ�;

        //�� ���̶� ���� �����ڸ� ���°� ����. if���� {}�� ��� �������� ���� �� �ȿ� ���� �ڵ带 ���� �� �����ϱ�.

        //�Ϲ� if���̾����� �̷� ���̾�. 
        //if (currentArrowLevel >= MaxLevel)
        //    ArrowLevelText.text = "B Level: Max";
        //else
        //    ArrowLevelText.text = $"B Level: {currentArrowLevel}";

        if (ArrowLevelText != null)//Ȱ ������ ��ȭ �ƴٰ� ArrowLevelText UI�� ����
        {
            ArrowLevelText.text = (currentArrowLevel >= MaxLevel)//��ȭ ���ڰ� MaxLevel���� ������
            ? "B Level: Max"
            : $"B Level: {currentArrowLevel}";//MaxLevel�� ���ٸ�  
        }
        else Debug.LogWarning("PlayerStatsEffects: ArrowLevelText UI�� �Ҵ���� �ʾҾ�! �ν����͸� Ȯ����!");

        if (ArrowSpeedText != null)//Ȱ ���� �ӵ� UI ������Ʈ
        {
            ArrowSpeedText.text = (currentArrowLevel >= 5)
            ? "SPD: Max"
            : $"SPD: {currentArrowLevel}";

            if (currentArrowLevel >= 5) Debug.Log("�ְ� �ӵ�!");//Debug.Log�� ���� if����...
        }

        if (SwordLevelText != null)//�� ������ ��ȭ�ƴٰ� SwordLevelText UI�� ���� 
        {
            SwordLevelText.text = (currentSwordLevel >= MaxLevel)
            ? "S Level: Max"
            : $"S Level: {currentSwordLevel}";
        }
        else Debug.LogWarning("PlayerStatsEffects: SwordLevelText UI�� �Ҵ���� �ʾҾ�! �ν����͸� Ȯ����!");
        

        if(MoveSpeedLevelText != null)//�̵��ӵ��� ��ȭ�ƴٰ� MoveSpeedLevelText UI�� ����
        {
            MoveSpeedLevelText.text = (currentMoveSpeedLevel >= MaxLevel)
            ? "M Level: Max"
            : $"M Level: {currentMoveSpeedLevel}";
        }
        else Debug.LogWarning("PlayerStatsEffects: MoveSpeedLevelText UI�� �Ҵ���� �ʾҾ�! �ν����͸� Ȯ����!");      
    }
}

   
