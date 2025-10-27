using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//UI ����� ���� �߰�


public class BowWeapon : MonoBehaviour
{
    [Header("Ȱ ���� ����")]
    //Ȱ ���� ����
    public GameObject ArrowPrefab;//ȭ�� �������� ���� ����(�ν����� ����)
    public Transform ArrowSpawnPoint;//ȭ�� ���� ��ġ(�ν����� ����)
    public float ArrowSpeed = 10f;//ȭ���� ���󰡴� �ӵ�(�ν����� ����)
    public float ArrowDamage = 1f;//ȭ���� ������(�ν����� ����)
    //Ȱ ���� 360�� ȭ�� ���� ����
    public int NumberOfArrows360 = 8;//360�� ���� �� �߻��� ȭ���� �� (��: 8����)

    [Header("���� �ӵ� ����")]
    public float BaseArrowCooldown = 2f;//Ȱ�� �⺻ ���� ��Ÿ��
    [SerializeField]private float lastArrowAttackTime = -1f;//���������� Ȱ ������ �� �ð��� ���
    private float currentArrowCooldown;//Ȱ�� ���� ���� ��Ÿ��

    [Header("Ȱ ���� ����")]//���� ���� ����, �ϳ��� AudioSource ������Ʈ�� �� ���� �ϳ��� AudioClip�� ����� �� �ֱ� ����
                        //���� �÷��̾� ������Ʈ���� Player ��ũ��Ʈ�� ������ ���嵵 ���ݾ�? ���� �ϳ��� ����� ������Ʈ �ϳ����̾�!
    [SerializeField] private AudioSource bowAudioSource;
    [SerializeField] private AudioClip bowAttackSound;


    [Header("UI ����")]
    public Slider BowCooldownBar;

    void Start()
    {
        currentArrowCooldown = BaseArrowCooldown;
    }
    void Update()
    {
        UpdateBowUI();
    }

    public void ShootArrow()//360�� ȭ�� ���� ����
    {
        if (bowAudioSource != null && bowAttackSound != null)//Ȱ ���ݽ� �������� �� �۵��� �Ǹ�
            bowAudioSource.PlayOneShot(bowAttackSound);
        //PlayOneShot�� �̸� �״�� ���� ��� ���� �ٸ� �Ҹ��� ���� �ʰ�, ���ο� �Ҹ��� �� ���� ����ϴ� �Լ���.
        else Debug.LogWarning("Ȱ ���� ���� AudioSource �Ǵ� AudioClip�� ������� �ʾҾ�!");

      
        if (ArrowPrefab == null)//ȭ�� �������� �� ������ֳ� �ȵ��ֳ�
        {
            Debug.LogError("Arrow Prefab�� ������� �ʾҾ�!");
            return;
        }

        //ȭ���� �߻�ɶ� �α׿� �˸�
        Debug.Log("ȭ�� �߻�! ������ ������: " + this.ArrowDamage);

        //�� ȭ�� ������ ���� ���
        float angleStep = 360f / NumberOfArrows360;

        for (int i = 0; i < NumberOfArrows360; i++)
        {
            float angle = i * angleStep;//���� ȭ���� ����(0, 45, 90, ...)

            //������ �������� ��ȯ
            float radianAngle = angle * Mathf.Deg2Rad;

            //�߻� ���� ���� ���
            Vector2 direction = new Vector2(Mathf.Cos(radianAngle), Mathf.Sin(radianAngle)).normalized;

            //ȭ�� ���� (�÷��̾��� ���߾ӿ��� �߻�ǵ��� transform.position ���)                   
            GameObject arrow = Instantiate(ArrowPrefab, ArrowSpawnPoint.position, Quaternion.identity);

            //ȭ���� Rigidbody2D ��������
            Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();
            if (arrowRb != null)
            {
                arrowRb.linearVelocity = direction * ArrowSpeed;

                //ȭ���� ���ư��� ���⿡ ���� ȸ��
                angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);



                //ȭ���� Arrow ��ũ��Ʈ�� ������ ����
                Arrow arrowScript = arrow.GetComponent<Arrow>();
                if (arrowScript != null)
                    arrowScript.ArrowDamage = this.ArrowDamage;//���� arrowDamage ���                                                            
                else Debug.LogWarning("�߻�� ȭ�� �����տ� 'Arrow' ��ũ��Ʈ�� ����! ������ ���� �Ұ�!");
            }
            else { Debug.LogWarning("�߻�� ȭ�� �����տ� Rigidbody2D�� ����!"); }
        }
        lastArrowAttackTime = Time.time;

    }

    public void DecreaseAttackCooldown(float coolDown, WeaponType weaponType)//Ȱ ������ ȹ�� �� ���� ���� �Լ�
    {                                                  //�ڵ��� ������, ���뼺�� ���� ���� �Լ��� ��������!                     
        Debug.Log($"DecreaseAttackCooldown �Լ� ȣ���. ���� ��Ÿ��: {currentArrowCooldown}, ���ҷ�: {coolDown}");
        //�ڵ��� ������, ���뼺�� ���� ���� �Լ��� ��������!                     
        switch (weaponType)
        {
            case WeaponType.Bow:
                currentArrowCooldown -= coolDown;
                if (currentArrowCooldown < 1.0f)//���⼭ Ȱ ������ �ִ� ��Ÿ���� 1�ʷ� ����
                    currentArrowCooldown = 1.0f;
                break;
        }
        Debug.Log($"��Ÿ�� ���� �� ���� ��Ÿ��: {currentArrowCooldown}");
    }

    public bool CanAttack()//��Ÿ�� üũ �Լ�
    {   
        //������ ���� �ð��� - 1f �̰ų�(���� ù ����) ��Ÿ���� ������ �� True ��ȯ
        if (lastArrowAttackTime < 0f || Time.time >= lastArrowAttackTime + currentArrowCooldown)
            return true;
        else
        {
            // ��Ÿ���� ���� ������ �� ����� �α� ���
            float timeRemaining = lastArrowAttackTime + currentArrowCooldown - Time.time;
            Debug.Log("Ȱ ���� ��Ÿ�� ��. ���� �ð�: " + timeRemaining.ToString("F1") + "��");
            return false;
        }
    }

    private void UpdateBowUI()//Ȱ ���� ��Ÿ�� UI�� �Լ�
    {
        //���� ���� ��, ��Ÿ���� ���� ���¶�� UI�� �ٷ� ��Ȱ��ȭ
        if (lastArrowAttackTime < 0f)
        {
            BowCooldownBar.gameObject.SetActive(false);
            return;//�Լ��� ��� ����
        }

        //���� ��Ÿ�� ���
        float timeRemaining = lastArrowAttackTime + currentArrowCooldown - Time.time;

        //��Ÿ���� �����ִٸ�
        if (timeRemaining > 0)
        {
            BowCooldownBar.gameObject.SetActive(true);
            BowCooldownBar.maxValue = currentArrowCooldown;//�����̴��� �ִ밪�� �� ��Ÿ������ ����
            BowCooldownBar.value = timeRemaining;//�����̴� ���� ���� �ð����� ����
        }
        else BowCooldownBar.gameObject.SetActive(false);//��Ÿ���� ������ �ٸ� ����          
    }
}
