using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static EnemyDifficulty;//Enemy ��ũ��Ʈ���� EnemyDifficulty Ŭ������ static ����� �� ���ϰ� ����ϱ� ���� �����̾�.
                             //���� Enemy ��ũ��Ʈ���� EnemyDifficulty�� StatType�� ����Ϸ��� EnemyDifficulty.StatType�̶�� ��� ��.
                             //������ using static EnemyDifficulty;�� �����ϸ�, EnemyDifficulty�� �����ϰ� �׳� StatType�̶�� �ᵵ ��.

public class Enemy : MonoBehaviour
{ 
    public enum EnemyType { Normal, Strong, Elite }//���� Ÿ�� ������,Normal�� �⺻Ÿ��, Strong�� ������, Elite�� ����� 
    //�ν�����â�� ��Ӵٿ����� Normal, Strong, Eliteǥ��
    public EnemyType enemyType = EnemyType.Normal;//�ν����Ϳ��� ������ ���� �⺻ Ÿ��
     
    [Header("���� Ÿ�Ժ� ����")]//�ν����Ϳ��� �����ϱ� ���� ���,
    //�ð������� "Enemy Stats by Type"�̶�� ������ �޾��ִ� ���Ҹ� �ϴ� �ž�. 
    //[Serializable] �Ӽ��� EnemyStats Ŭ���� ��ü�� �پ��־ public EnemyStats Ÿ�� ��������
    //�ν������� EnemyStats ��Ӵٿ ǥ��
    //[Serializable]�� �����ٸ� �̵��� �ν����Ϳ� ������ �ʾ�
    public EnemyStats NormalStats = new EnemyStats();//Normal Ÿ���� �ɷ�ġ ��Ʈ
    public EnemyStats StrongStats = new EnemyStats();//Strong Ÿ���� �ɷ�ġ ��Ʈ
    public EnemyStats EliteStats = new EnemyStats();//Elite Ÿ���� �ɷ�ġ ��Ʈ

    [Header("EnemySpawner ����")]
    public EnemySpawn EnemySpawner;//EnemySpawn ��ũ��Ʈ ����, [Header("����")] ��� ������ ��ġ�� �������� �Űܳ���


    //[Header]�� '����'�̶�, �� �ؿ� ������ �ٿ��� '���빰(����)'�� �� �ʿ���!
    //[Header] �Ӽ��� �׻� �� �Ʒ��� ���� ������ �ٸ��ִ� �����̾�.
    [Header("Targeting Offset")]//����� �̰� ������ ����Ƽ �ν����� â�� �����ϰ� ���� ���� ����� ���� ����̾�
    [SerializeField] private float playerTargetOffsetY = -2.7f;
    //�÷��̾� ��ǥ Y�� ������ (�ν����ͳ� ��ũ��Ʈ���� ������. ������ -2.7�� ���� ������)

    //GetComponent<T>() vs [SerializeField] ����
    //GetComponent<T>(): �� �Լ��� �ڵ带 ���ؼ� ���� ������Ʈ�� ������Ʈ�� ������ �� ����ϴ� �ž�. �ַ� Start()�� Awake() �Լ����� �����.
    //[SerializeField]: �� Ű����� private ������ �ν����Ϳ� ������Ѽ� �����ڰ� ���� ������Ʈ�� �����ϰ� ����� ������ ��.
    //GetComponent<T>()�� �� ��� �ϴ� �� �ƴϰ�, [SerializeField]�� ����� ������ �ν����Ϳ��� ���� �巡���ؼ� �����ϸ� ��
    //�׷��� GetComponent<AudioSource>()�� ���� �ʰ� [SerializeField]�� ����� �ν����Ϳ��� ����� ������Ʈ�� �׷��� �ؼ� �����߾�.
    //�� ������Ʈ�� ���� ���带 �������� [SerializeField] private�� �Ἥ �巡���ؼ� ���� ��������!
    //���� ���� ����
    [Header("����")]
    [SerializeField] private AudioSource deathAudioSource;
    [SerializeField] private AudioClip deathSound;

   
    [Serializable]//�� Ÿ�Ժ� �ɷ�ġ�� ���� ����ü. Normal, Strong, Elite ���ݿ� ���
    public class EnemyStats
    { 
        public float MoveSpeed = 4f;
        public float StopDistance = 0.5f;
        public float AttackCooldown = 1f;
        public float AttackDamage = 2f;
        public float DetectionRange = 100f;
        public Color SpriteColor = Color.white;
        //[Serializable] �Ӽ��� EnemyStats Ŭ������ ����Ǿ����Ƿ� �� Ŭ���� ���ο�
        //public���� ����� �������� �ν����Ϳ��� �ش� "Stats" ��Ӵٿ��� ������ �� ��Ÿ���� �ž�.
        public int ScoreValue = 10;//���� óġ �� ���� ���� (�⺻�� 10��, �ν����Ϳ��� ���� ����)
    }

    //[Serializable]�� [Header("Enemy Stats by Type")] ���� �Ӽ�(Attribute)��
    //�ٷ� �� �Ʒ��� ����� Ŭ������ ������ ����Ǵ� �ž�.

    //���� ������ (�� �����鿡 ������ ������ Ÿ�Ժ� ������ �Ҵ��)
    //private�̶� �ν����Ϳ��� ���� �Ұ�
    private float currentMoveSpeed;//�̵� �ӵ�
    private float currentStopDistance;//�÷��̾�� �� �Ÿ��� ������ ����
    private float currentAttackCooldown;//���� ��Ÿ��
    private float currentAttackDamage;//���� ������
    private float currentDetectionRange;//���Ͱ� �÷��̾ �����ϴ� �Ÿ�
    private int currentScoreValue;//���� óġ �� �� ����


    private bool playerWasDead = false;//�÷��̾ ������ �׾������� �����ϴ� ����
    private float lastAttackTime;//���������� ������ �ð�
    private bool isDead = false;//��� ����(�⺻�� false)
    private bool isKnockedBack = false;//�˹� ������ ���θ� ��Ÿ���� �÷���

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Player playerScript;//Player ��ũ��Ʈ ����
    


    public void SetEnmeyStats()//���� ���� �� �ɷ�ġ�� ������ �����ϴ� �Լ�, ���Ƿ� ���� �Լ� �̸�
    {
        if (spriteRenderer == null) return;//SpriteRenderer�� ������ �� �̻� �������� ����
                                           //spriteRenderer�� ���� ��� �α�

        EnemyStats selectedStats;//���� Ÿ�Կ� �´� �ɷ�ġ ��Ʈ�� ������ ����
       
        switch (enemyType)
        {
            case EnemyType.Normal:
                selectedStats = NormalStats;
                break;
            case EnemyType.Strong:
                selectedStats = StrongStats;
                break;
            case EnemyType.Elite:
                selectedStats = EliteStats;
                break;
            default:
                selectedStats = NormalStats;//�⺻��
                break;
        }

        if(EnemyDifficulty.Instance != null)
        {   //using static EnemyDifficulty;�� �����߱⿡,
            //������public float GetAdjustedMonsterStat(float baseStat, EnemyDifficulty.StatType statType) ó�� EnemyDifficulty�� ���������.
            //������ Enemy ��ũ��Ʈ������ EnemyDifficulty�� StatType�� �����ͼ� ����ؾ� �ϹǷ� using static EnemyDifficulty;�� �ſ� �����ϰ� ����.
            //�� ������ ����ϸ� �ڵ尡 �� ���������� �������� ������.
            currentMoveSpeed = EnemyDifficulty.Instance.GetAdjustedMonsterStat(selectedStats.MoveSpeed, StatType.MoveSpeed);//selectedStats ���
            currentStopDistance = selectedStats.StopDistance;//���̵� ���� ������ �״��
            currentAttackCooldown = selectedStats.AttackCooldown;//���̵� ���� ������ �״��
            currentAttackDamage = EnemyDifficulty.Instance.GetAdjustedMonsterStat(selectedStats.AttackDamage, StatType.AttackDamage);//selectedStats ���
            currentDetectionRange = selectedStats.DetectionRange;//���̵� ���� ������ �״��
            currentScoreValue = selectedStats.ScoreValue;//���̵� ���� ������ �״��
        }
        else//EnemyDifficulty �ν��Ͻ��� ������ �⺻ ���� ���
        {
            
            Debug.LogWarning("EnemyDifficulty.Instance�� ã�� �� ����, ���Ͱ� �⺻ �������� �����Ұ�.");
            currentMoveSpeed = selectedStats.MoveSpeed;
            currentStopDistance = selectedStats.StopDistance;
            currentAttackCooldown = selectedStats.AttackCooldown;
            currentAttackDamage = selectedStats.AttackDamage;
            currentDetectionRange = selectedStats.DetectionRange;
            currentScoreValue = selectedStats.ScoreValue;
        }       
        spriteRenderer.color = selectedStats.SpriteColor;
        //spriteRenderer.color�� ���̵��� ������� ���� Ÿ�Կ� ���� ������ �״�, �� ���� ������ �ʰ� �״�� ����
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();


        //"Player" �±׸� ���� ������Ʈ�� �̵�(Transform�� �Ҵ�)
        //public Transform playerTransform�� ������ �÷��̾��� ������Ʈ�� �����
        GameObject playerGameObject = GameObject.FindWithTag("Player");//�÷��̾� ������Ʈ ����
        if(playerGameObject != null)
            playerScript = playerGameObject.GetComponent<Player>();//Player ��ũ��Ʈ ���� ��������
        else
        {         
            playerScript = null;
            Debug.LogWarning("Enemy: Player ������Ʈ�� ã�� �� ���ٰ�! 'Player' �±׸� Ȯ����!");
        }
        //���� ���� �� �ɷ�ġ�� ������ �����ϴ� �Լ� ȣ��
        SetEnmeyStats();

        lastAttackTime = Time.time - currentAttackCooldown;//�������ڸ��� ���ݰ���, ����ǰ� ��Ÿ�� ��ٸ��� �ʰ� �ٷ� ����

        //����� ������Ʈ ��������
        if (deathAudioSource == null)
            Debug.LogError("Enemy: AudioSource ������Ʈ�� ã�� �� ����!");     
    }



    void FixedUpdate()//�������� ������ FixedUpdate�� 
    {//FixedUpdate���� Time.deltaTime���� Time.fixedDeltaTime(��Ȯ�� ���� ���� �ϰ��� �̵� �ӵ��� ����)

        if (isDead)//���� ���
        {
            rb.velocity = Vector2.zero;//����� ��� ����
            return;//��� �� �̵��� ���� ����
        }

        if (isKnockedBack)//�˹� ���� ���� �̵� ������ �ǳʶ�
            return;


        if (HandlePlayerDeath()) return;//�÷��̾� ��� �� �ൿ ó�� �� FixedUpdate ����, �Լ� ȣ��
        if (playerScript == null) return;//�÷��̾� ��ũ��Ʈ�� ������ �̵�/���� ���� �������� ����

        //�÷��̾���� �Ÿ� ���
        Vector3 playerCenterPosition = playerScript.GetCenterPosition();//�÷��̾��� �߾� ��ġ ��������
        playerCenterPosition.y += playerTargetOffsetY;//Y�� ������ ����


        float distanceToPlayer = Vector2.Distance(transform.position, playerCenterPosition);

        //if���� ���� �������� ������ �������� ���� �߰�
        bool isInDetectionRange = distanceToPlayer <= currentDetectionRange;
        bool isInStopDistance = distanceToPlayer <= currentStopDistance;
        bool canAttack = Time.time >= lastAttackTime + currentAttackCooldown;

        //�� ���� �и� (����ȭ�� �Լ� ȣ��)
        ProcessMovementAndAttack(isInDetectionRange, isInStopDistance, canAttack, playerCenterPosition);
    }

    private bool HandlePlayerDeath()//�÷��̾� ��� ���� ó��
    {
        bool isPlayerDead = (playerScript != null && playerScript.IsDead);

        if(isPlayerDead)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("Move", false);

            if(!playerWasDead)
            {
                animator.SetTrigger("Idle");
                playerWasDead = true;
            }
            return true;//�÷��̾ �׾����� �� �̻� ����/���� ������ �������� ����
        }
        else playerWasDead = false; return false; 
    }


    //�÷��̾ ���� ���� ���� �ȿ� �������� �б��� ���� �Լ�, ������ ���� ������ �Լ� ȣ��
    private void ProcessMovementAndAttack(bool isInDetectionRange, bool isInStopDistance, bool canAttack, Vector3 playerCenterPosition)
    {
        if(!isInDetectionRange)//�÷��̾ ���� ���� �ۿ� ������ ��� ����
        {
            StopMovement();
            return;
        }

        if(!isInStopDistance)//�÷��̾ ���� ���� �ȿ� ������ ���� �Ÿ��� ���� ������ ����
        {
            //���� ���� ���¿��ٸ�, ��� ������ ����ϰ� ������
            if (animator.GetBool("Attack"))
            {
                animator.SetBool("Attack", false);
            }  //GetBool�̶�? �ִϸ����Ϳ� �ִ� Attack�̶�� �̸��� bool �Ķ������ ���� ���¸� ��������(Get) ������ ��
               //���� Attack �Ķ���Ͱ� true�� �����Ǿ� �ִٸ�, �� �Լ��� true ���� ��ȯ��
               //GetBool�� ����ؼ� "���� ���� �ִϸ����Ͱ� 'Attack' ���¿� �ִٸ�"�̶�� ������ ���� �� �־�
               //SetBool��? true��� �ִϸ����Ϳ��� 'Attack' ���·� ��ȯ�϶�� ���,
               //false�� �ִϸ����Ϳ��� 'Attack' ���¸� �����϶�� ��� �ϴ� ����
            MoveTowardsPlayer(playerCenterPosition);
        }
        else//�÷��̾ ���� �Ÿ��� ������ ���߰� ����
        {
            StopAndAttackPlayer(canAttack, playerCenterPosition);
        }
    }


    private void MoveTowardsPlayer(Vector3 targetPosition)//�÷��̾� ����/�̵� ����
    {
        Vector2 direction = (targetPosition - transform.position).normalized;
        rb.velocity = direction * currentMoveSpeed;

        animator.SetBool("Move", true);
        FlipSprite(direction.x);//�¿� ���� �Լ� ȣ��
    }


    //�÷��̾�� ���� �� ���� ����. ���� ����� �����ϰ� ���ִ� ����
    private void StopAndAttackPlayer(bool canAttack, Vector3 playerCenterPosition)
    {
        rb.velocity = Vector3.zero;
        animator.SetBool("Move", false);

        FlipSprite(playerCenterPosition.x - transform.position.x);//���缭 ������ ���� �÷��̾ �ٶ󺸰�

        if(canAttack)
        {
            //���Ͱ� ������ �����ϱ� ���� �÷��̾ ������ ���� ���� ���� �ִ��� �ٽ� Ȯ��
            float distanceToPlayer = Vector2.Distance(transform.position, playerCenterPosition);

            if (distanceToPlayer <= currentStopDistance + 0.1f)
            {
                animator.SetBool("Attack", true);
                lastAttackTime = Time.time;
            }
            else
            {
                //�÷��̾ �̹� ���� ������ ������� ������ �������� ����
                Debug.Log("�÷��̾ �ʹ� �ָ� �־� ������ ����߾�!");
            }
        }
    }
    public void OnAttackFinished()//���� �ִϸ��̼��� ���� �Ŀ� Attack Bool�� �ٽ� false�� �ٲ��ִ� �Լ�
    {                             //���ϸ��̼ǿ��� �̺�Ʈ�� �߰�����
        animator.SetBool("Attack", false);
    }

    private void StopMovement()//������ ���� ����
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("Move", false);
    }

    private void FlipSprite(float directionX)//���� ��������Ʈ �¿� ����
    {
        if(directionX > 0) 
            spriteRenderer.flipX = false;      
        else if (directionX < 0)
            spriteRenderer.flipX = true;

    }

    public void Attack()//�� �Լ��� ������ �������� �� ��������� ����� ������ ����
    {
        //Animation Eventó�� Ư�� ������Ʈ�� ���� ��ũ��Ʈ�� �پ����� ��,
        //�׸��� �Լ� �̸��� ���ų� �����ε� �Ǿ� �ִٸ� Unity �ý����� � ��ũ��Ʈ�� � �Լ��� ȣ���ؾ� ����
        //��ȣ������ ������ �߻��� �� �־�.

        //Attack �ִϸ��̼� �̺�Ʈ�� ȣ��� ��, �÷��̾���� �Ÿ��� �ٽ� Ȯ��
        float distanceToPlayer = Vector2.Distance(transform.position, playerScript.transform.position);

        if (distanceToPlayer <= currentStopDistance + 1f)//1f�� ������ �ֱ� ���� ��
        {
            if (playerScript == null || playerScript == null || playerScript.IsDead)
            {   //���Ͱ� ���� �ִϸ��̼� ���� �÷��̾ �װų� ������ ��츦 ����� �α�
                Debug.Log("�÷��̾ ���ų� ����Ͽ� �������� �� �� ����!");
                return;
            }

            PlayerShield playerShield = playerScript.GetComponent<PlayerShield>();
            if (playerShield != null)//�÷��̾�� PlayerShield ��ũ��Ʈ�� �ִٸ�, ���¿� ������ ����
            {
                playerShield.TakeShieldDamage(currentAttackDamage);
                Debug.Log("���Ͱ� �÷��̾��� ���¿� " + currentAttackDamage + " �������� ���!");
            }
            else//PlayerShield ��ũ��Ʈ�� ���ٸ�, ����ó�� PlayerHealth�� ���� ������ ����
            {   //���Ͱ� �÷��̾�� �������� �ִ� ����
                PlayerHealth playerhealth = playerScript.GetComponent<PlayerHealth>();
                if (playerhealth != null)
                {
                    playerhealth.TakeDamage(currentAttackDamage);
                    Debug.Log("�÷��̾ " + currentAttackDamage + " �������� �޾Ҵ�! ���� ü��: " + playerhealth.CurrentHealth);
                }
                else Debug.LogError("�÷��̾�� PlayerHealth ��ũ��Ʈ�� ����!");
            }
        }
        else
        {
            Debug.Log("���� ���� ���̶� �������� �� �� ����!");
        }
    }
    
    public void EnemyDie()//���Ͱ� ������  
    {       
        if (isDead) return;//�̹� ���� ���¶��, �� �̻� �ƹ��͵� ���� �ʰ� �Լ��� ����

        isDead = true;//��� ���·� ����, �� ���� ���𿡴� ���� ��� ����� �ϸ� �ȵǴ� false��
        Debug.Log("�� ���!");
       
        //���� ����� ��� ���� ���
        if (deathAudioSource != null && deathSound != null)
        {
            deathAudioSource.PlayOneShot(deathSound);//PlayOneShot�� �̸� �״�� ���� ��� ���� �ٸ� �Ҹ��� ���� �ʰ�, ���ο� �Ҹ��� �� ���� ����ϴ� �Լ���.
            //���� ���, ���Ͱ� ���� ���� ���ÿ� ���� �� ���� �״� �Ҹ��� ��� �鸮�� �Ϸ��� PlayOneShot�� ���� �� ����.
            //���� �׳� audioSource.Play()�� ��ٸ�, �ٸ� �Ҹ��� ����� ������ ���� �Ҹ��� ����� ��.
        }

        if (playerScript != null)//!= null�� Player��ũ��Ʈ�� null�� ���� ������?. �� ����� ��ũ��Ʈ�� ����� ����.
        {
            //Player.AddScore �Լ��� ȣ���Ͽ� ������ ���� ���� ����
            //Player.Instance.AddScore(currentScoreValue); ��� �� ���� ������,
            //currentScore�� static�̹Ƿ� Player.AddScore�� �ƴ϶� Player.currentScore += currentScoreValue;
            //�� ���� �����ϴ� �� ���� ���迡���� �� ������ �� �־�.
            //������ player ��ũ��Ʈ�� AddScore() �Լ��� ��������� �� �Լ��� ȣ���ϴ� �� �� ��ü�������̾�.
            if (playerScript != null)//playerScript�� Start���� �̹� ã�� ������ �̰� Ȱ��
            {
                playerScript.AddScore(currentScoreValue);
            }
        }

        if (EnemySpawner != null)
        {   //���� ������ Ÿ���� Strong �Ǵ� Elite���� Ȯ��
            bool isThisStrongOrElite = (enemyType == EnemyType.Strong || enemyType == EnemyType.Elite);

            //EnemySpawn ��ũ��Ʈ�� EnemyDied �Լ��� ȣ���Ͽ� ���Ͱ� �׾����� �˸�
            //�̶�, �� ���Ͱ� ����/����Ʈ �������� ���θ� �Բ� ����
            EnemySpawner.EnemyDied(isThisStrongOrElite);
        }

        if (animator != null)
        {
            animator.SetTrigger("Die");//Animator�� "Die" Ʈ���� �ߵ�

            float DieTime = 1.5f;//��� �� ~�� �Ŀ� �����
    
            Destroy(gameObject, DieTime);//Destroy�Լ��� ���� ������Ʈ�� ������Ʈ�� ���ӿ��� ����(�ı�)�� �� ���
            //�� Enemy ������Ʈ�� ����ϸ� DieTime�� ���� �ð� (��)�� ���� �� �����Ѵ�.
        }
    }

    //PlayerAttack ��ũ��Ʈ���� ȣ��� �˹� �Լ�
    public void TakeKnockback(Vector2 knockbackDirection, float knockbackForce, float duration)
    {
        if (isDead) return;//���� ���ʹ� �˹���� ����

        if (rb == null)
        {
            Debug.LogWarning(gameObject.name + "���� Rigidbody2D�� ��� �˹��� ���� �� ����!");
            return;
        }

        //�̹� ���� ���� �˹� �ڷ�ƾ�� �ִٸ� ���� (�ߺ� �˹� ����)
        StopAllCoroutines();//�� ��ũ��Ʈ�� ��� �ڷ�ƾ ���� (�ٸ� �߿��� �ڷ�ƾ�� �ִٸ� ����!)
                            //Ư�� �ڷ�ƾ�� �����ϰ� �ʹٸ� StopCoroutine(�ڷ�ƾ_����); ����ؾ� ��.

        isKnockedBack = true;//�˹� ���� �÷��� ����
        rb.velocity = Vector2.zero;//���� �ӵ� �ʱ�ȭ (���� ������ ���� ����)

        //�˹� �� ����
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        Debug.Log(gameObject.name + "���� �˹� " + knockbackForce + "��ŭ ����! ����: " + knockbackDirection);

        //�˹� ���� �ð���ŭ ��ٸ� �� �˹� ���� ����
        StartCoroutine(KnockbackRoutine(duration));
    }
    private IEnumerator KnockbackRoutine(float duration)//�˹� �ڷ�ƾ
    {
        yield return new WaitForSeconds(duration);

        //�˹� �ð� ���� �� �ӵ� �ʱ�ȭ (�з����� ���� ���߰� ��)
        rb.velocity = Vector2.zero;
        isKnockedBack = false;//�˹� ���� �÷��� ����
        Debug.Log(gameObject.name + " �˹� ����.");
    }
}


