using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;//UI ����� ���� �߰�


public class SwordWeapon : MonoBehaviour
{

    [Header("UI,������Ʈ ����")]
    public Image SwordSkillIcon;
    public Image CooldownOverlay;
    public TextMeshProUGUI CooldownText;
    private SpriteRenderer SpriteRenderer;

    [Header("�� ���� ����")]
    //�� ���� ����
    public float SwordDamage = 2f;//�� ���ݷ�(�ν����� ����)
    public BoxCollider2D SwordAttackCollider;//SwordPoint ������Ʈ�� BoxCollider2D�� ���� ����
    public string EnemyTag = "Enemy";//���� �ĺ��� �±� (�ν����Ϳ��� ���� ����)
    //�� ���ݽ� ���� �˹� ����
    public float KnockbackForce = 10f;//�˹��� ����(�ν����Ϳ��� ����)
    public float KnockbackDuration = 0.2f;//�˹��� ����Ǵ� �ð� (���Ͱ� ��� �˹� ���°� �ǵ���)
    //�˱� �߻�ü ����
    public float SwordEnergyDamage = 5f;//�˱� �⺻ ���ݷ�(�ν����Ϳ� ����)
    public float SwordEnergySpeed = 15f;//�˱� �߻�ü �ӵ�
    public Transform SwordEnergySpawnPoint;//�˱� �߻�ü ���� ��ġ (�ν����Ϳ� ����)
    public GameObject SwordEnergy;//�˱� �߻�ü ������ (�ν����Ϳ��� ����)

    [Header("�� ���� �ӵ� ����")]
    public float SwordSkillCooldown = 10f;//�� ��ų ��Ÿ��
    public float lastSwordSkillTime = -10f;//���������� ��ų�� ����� �ð�, Time.time�� ���� �ð��� �� ������ �����ؼ� ��Ÿ���� ����� �� ���
    //�� ������ ������ Time.time >= lastSwordSkillTime + swordSkillCooldown ���� ��Ÿ�� üũ ������ �������� �ʾ�.

    [Header("�� ���� ����")]
    [SerializeField] private AudioSource swordAudioSource;
    [SerializeField] private AudioClip swordAttackSound;
    


    void Awake()
    {
        //SpriteRenderer ������Ʈ�� �����ͼ� ������ �Ҵ�
        SpriteRenderer = GetComponent<SpriteRenderer>();

        //������ ����� �Ǿ����� Ȯ��
        if (SpriteRenderer == null)
            Debug.LogError("SwordWeapon: SpriteRenderer ������Ʈ�� ã�� �� ����!");
    }
    void Update()
    {
        //Update �Լ��� �� �����Ӹ��� ȣ��ǰ� �ִ��� Ȯ��
        UpdateSwordSkillUI();
    }

    public void SwordAttack()//�� ���� �Լ�(�ִϸ��̼� �̺�Ʈ�� ȣ��� �Լ�)
    {                        //SwordPoint�� BoxCollider2D�� ����Ͽ� OverlapBox�� �浹 ����

        //�˰��� ���� ���
        if (swordAudioSource != null && swordAttackSound != null)//audioSource�� swordAttackSound�� != null. �� ���������� ����� ���¿��� ���� ���
            swordAudioSource.PlayOneShot(swordAttackSound);
        //PlayOneShot�� �̸� �״�� ���� ��� ���� �ٸ� �Ҹ��� ���� �ʰ�, ���ο� �Ҹ��� �� ���� ����ϴ� �Լ���.

        Debug.Log("SwordAttack �Լ� ȣ���!");

        if (SwordAttackCollider == null)//swordAttackCollider�� null���¿� ���ٸ�? �ݴ�� !=
        {
            Debug.LogError("PlayerAttack: Sword Attack Collider�� ������� �ʾ� �� ������ ������ �� ����!");
            return;
        }


        //SwordPoint ������Ʈ�� BoxCollider2D�� ���� ���� ��ġ�� ũ�⸦ ������.
        Vector2 colliderCenter = SwordAttackCollider.transform.position + (Vector3)SwordAttackCollider.offset;
        Vector2 colliderSize = SwordAttackCollider.size;
        float colliderAngle = SwordAttackCollider.transform.rotation.eulerAngles.z;

        //BoxCollider2D ���� ���� ��� �ݶ��̴��� ������. ���� foreach������ ���Ϳ��� Ư���� �ൿ�� �ϰ� �ϴ°���!
        Collider2D[] hitColliders = Physics2D.OverlapBoxAll(colliderCenter, colliderSize, colliderAngle);
        Debug.Log($"SwordAttack: {hitColliders.Length}���� �ݶ��̴� ������.");
        //OverlapBoxAll()�� �ݶ��̴� ���� �ȿ� �ִ� �͵��� �����ϸ�, �� �����(������ ��� �ݶ��̴���)�� hitColliders��� �迭 ������ ���.
        //�� hitColliders ������ ���� '������ ������Ʈ���� ���'�� �Ǵ� ����.


        //�迭(array)�̳� �÷���(collection)�� ��� ���(item)���� ������� �ϳ��� �����ͼ� ó���� �� ����ϴ� �ݺ��� foreach.
        //for ��ó�� �ε���(���� ��ȣ)�� ���� ������ �ʿ� ����, �÷��ǿ� �ִ� ��� ��Ҹ� �����ϰ� �ݺ��� �� �ִٴ� ������ �־�
        //���������, foreach �� ���п� hitColliders �迭�� ���Ͱ� �� ������ ����ֵ�,
        //��� ���Ϳ��� �������� �ִ� �ڵ带 ȿ�������� �ۼ��� �� �ִ� �ž�. �ε����� ���� �����ϴ� ������ ����!
        foreach (Collider2D hitCollider in hitColliders)
        {//in Ű����� "~�ȿ� �ִ�" �Ǵ� "~�� ��������" �̶�� �ǹ̷� �����ϸ� ��.
         //foreach ������ "�����ʿ� �ִ� �÷���(hitColliders) �ȿ� �ִ� ������ ���(hitCollider)�� ����" �ݺ��϶�� ���ø� ������ ������ ��.

            //foreach ���� �� ���� in Ű���带 �ݵ�� ��� ��. in�� ������ foreach ���� � �÷��ǿ��� ��Ҹ� �ϳ��� �����;� ���� �� �� ����.
            //foreach�� in�� �Բ� �ϳ��� �������� ¦���̶�� �����ϸ� ��.
            //SetDamage
            //�÷��̾� �ڽ��̳� SwordPoint ������Ʈ�� �ǳʶٱ�
            if (hitCollider.gameObject == this.gameObject || hitCollider.gameObject == SwordAttackCollider.gameObject)
                continue;
            //continue�� foreach ���̳� �ٸ� �ݺ���(for, while ��)���� ���� �ݺ��� ��� �ǳʶٰ� ���� �ݺ����� �Ѿ�� �ϴ� ��ɾ��.

            //������ �ݶ��̴��� �±װ� 'enemyTag' ������ ��ġ�ϴ��� Ȯ��
            if (hitCollider.CompareTag(EnemyTag))
            {
                EnemyHealth enemyHealth = hitCollider.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(SwordDamage);//�� ���ݷ� ��ŭ ������ ����
                    Debug.Log(hitCollider.name + "���� " + SwordDamage + "��ŭ�� �� ������ �ο�! (SwordPoint ����)");

                    Enemy enemyScript = hitCollider.GetComponent<Enemy>();//�˹� ���� ȣ�� (Enemy ��ũ��Ʈ�� �Լ� ȣ��)
                    if (enemyScript != null)
                    {
                        //�÷��̾� ��ġ���� ���� ��ġ�� ���ϴ� ���� ���� ���
                        Vector2 knockbackDirection = hitCollider.transform.position - transform.position;//�÷��̾� -> ���� ���� ����
                        if (knockbackDirection.x > 0)
                            knockbackDirection = Vector2.right;//���Ͱ� �÷��̾� �����ʿ� ������ ���������� �б�
                        else knockbackDirection = Vector2.left;//���Ͱ� �÷��̾� ���ʿ� ������ �������� �б�

                        enemyScript.TakeKnockback(knockbackDirection, KnockbackForce, KnockbackDuration);
                    }
                    else
                        Debug.LogWarning(hitCollider.name + "���� Enemy ��ũ��Ʈ�� ��� �˹��� ������ �� ����!");


                }
                else
                    Debug.LogWarning(hitCollider.name + "���� EnemyHealth ��ũ��Ʈ�� �����ϴ�!");
            }
        }

        if (SwordEnergy != null)//�� ���ݽ� �˱� �߻�ü ����
        {//SwordPower ������ ����� �ִٸ�. != null�� ~~�� null ���°� �ƴ϶��. ���ΰ� ���� �� �˰���?

            //�÷��̾��� ���⿡ ���� �߻� ������ ����
            Vector2 launchDirection = SpriteRenderer.flipX ? Vector2.left : Vector2.right;
            //�߻�ü ����, swordPowerSpawnPoint ������Ʈ�� �߻�ü ���� ��ġ��
            GameObject swordPowerInstance = Instantiate(SwordEnergy, SwordEnergySpawnPoint.position, Quaternion.identity);

            SwordEnergy swordenergy = swordPowerInstance.GetComponent<SwordEnergy>();//SwordEnergy ��ũ��Ʈ�� �����ͼ� ������ ���� �Ѱ���
            if (swordenergy != null)
            {
                float totalDamage = SwordEnergyDamage;
                swordenergy.SetDamage(totalDamage);
            }

            if (SpriteRenderer.flipX)//�ɸ��Ͱ� ���� ���⿡ ���� �߻�ü�� ���� ����
                //ĳ���Ͱ� ������ ���� ������ �߻�ü�� 180�� ȸ��
                swordPowerInstance.transform.rotation = Quaternion.AngleAxis(180, Vector3.forward);
            else
                swordPowerInstance.transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
            //ĳ���Ͱ� �������� ���� ������ �߻�ü�� 0��(���� ����)�� ����


            //�߻�ü�� Rigidbody2D�� �����ͼ� ���� ����
            Rigidbody2D rb = swordPowerInstance.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.velocity = launchDirection * SwordEnergySpeed;

        }
        else Debug.LogWarning("PlayerAttack: swordProjectilePrefab�� ������� �ʾҾ�! �� �߻�ü ������ �� �� ����.");
        lastSwordSkillTime = Time.time;
    }

    public bool CanAttack() // �� ���� ��Ÿ�� üũ �Լ�
    {
        //��Ÿ���� ������ ���� True�� ��ȯ
        return Time.time >= lastSwordSkillTime + SwordSkillCooldown;
    }

    private void UpdateSwordSkillUI()//�� ��ų�� ��Ÿ���� ����ϰ� UI�� ������Ʈ�ϴ� �Լ�
    {                                //Update �Լ��� ȣ�������� �� �����Ӹ��� ȣ�����!
        //���� ��Ÿ�� ���
        float timeRemaining = lastSwordSkillTime + SwordSkillCooldown - Time.time;//������ ��ų ��� ���� + �� ��Ÿ�� �ð� - ���� �ð�

        //��Ÿ���� �����ִٸ�, timeRemaining�� 0���� ũ�ٴ� ���� ���� ��Ÿ���� ���� ���̶�� �����ݾ�.
        if (timeRemaining > 0)
        {
            CooldownText.gameObject.SetActive(true);//��Ÿ�� ����(�ؽ�Ʈ)�� ���̰� ��
            CooldownText.text = Mathf.Ceil(timeRemaining).ToString("F0");

            //�������� �̹����� fillAmount�� �����ؼ� �ð������� ������
            CooldownOverlay.gameObject.SetActive(true);//������ �������̸� ���̰� ��.
            CooldownOverlay.fillAmount = timeRemaining / SwordSkillCooldown;
            //���� ��Ÿ�� ������ ���� ������ �������̰� ������ ��������� �����.
        }
        else//timeRemaining�� 0���� �۰ų� ���� ���(��Ÿ���� ���� ���) else �� ���� �ڵ尡 ����
        {
            //��Ÿ���� ������
            CooldownText.gameObject.SetActive(false);//��Ÿ�� ���ڸ� ����
            CooldownOverlay.gameObject.SetActive(false);//������ �������̸� ����(��Ȱ��ȭ)
        }
    }
}
