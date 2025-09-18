using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR//UnityEditor ���ӽ����̽��� ����ϰ� �ִ� ��ũ��Ʈ�� ���忡 ���Ե� �� �߻���.
using UnityEditor.UIElements;
//UnityEditor�� �����Ϳ����� �۵��ϴ� ����̶�, ���� ���� ���忡�� ���ԵǸ� �� �ǰŵ�.
#endif//#if UNITY_EDITOR ����� �ش� �ڵ带 ������ ���� ��ó���� ���ù����� ������.
using UnityEngine;
using UnityEngine.UI;
using TMPro;//TextMeshPro ��� �� using ���� TMPro�� �߰���


//�Լ��� ������� ������ ���� �Լ� �����!!
//���߿� if���� �������� bool�� �������� ª�� �ϱ�!!

//WeaponType�� PlayerAttack ��ũ��Ʈ���� �����ϴ°� �� ����������
//���� Item ��ũ��Ʈ���� Player.currentWeaponType�� �ٷ� �����ؼ� ���⸦ �����ϴ� ���̶��
//���⿡ ���ܵδ� �͵� ������ �ʾ�. (���û���)

public enum WeaponType
{
    None,
    Bow,
    Sword,
    Axe
    //���߿� �ٸ� ���� ������ �߰��� �� ���⿡ �� ��������
}

public class Player : MonoBehaviour
{
    [Header("�̵� ���� ����")]
    //�̵� ���� ����
    public float MoveSpeed = 5f;//�̵� �ӵ�     
    private float horizontalInput;
    private float verticalInput;
    private Vector2 movement;

    [Header("��� ���� ����")]
    //��� ���� ����
    public bool IsDead = false;//�÷��̾��� ��� ����,PlayerHealth��ũ��Ʈ�� ���� ���� public���� ����   
    private PlayerHealth playerHealth; // PlayerHealth ��ũ��Ʈ ����
    private GameOverManager gameOverManager;//GameOverManager ��ũ��Ʈ ����


    //���� óġ �� ���� ������ ������ ����
    //�� ���� ���� �ٷ� ���� ����, �� �� ������ �ٸ� ��ũ��Ʈ ��� ��� �ϴ� [HideInInspector]�� �ν����Ϳ� ������ �ʰ� �߾�
    [HideInInspector] public int CurrentScore = 0;//�÷��̾��� ���� ����(�ʱⰪ 0)

    [Header("ScoreText UI ����")]
    public TextMeshProUGUI ScoreTextUI;
    //TextMeshProUGUI: �ַ� UI Canvas ���� �ؽ�Ʈ�� ��ġ�� �� ����ϴ� ������Ʈ��.
    //UGUI(Unity UI) �ý����� �Ϻη�, ȭ�鿡 �����Ǿ� ���� �÷��̿� ������ ǥ�õǴ� �������̽� ��ҿ� �����.
    //���� ���, ���� ����, ü�� ��, �κ��丮 â, �޴� ��ư�� �ؽ�Ʈó�� ���� ȭ�� ���� �������̵Ǵ� �ؽ�Ʈ�� ����.

    //TextMeshPro: �ַ� 3D ������ �ؽ�Ʈ�� ��ġ�� �� ����ϴ� ������Ʈ��.
    //���� ������Ʈ�� �Ϻη� 3D ���� �ȿ� �ؽ�Ʈ�� ���� ���� �� �����.���� ���,
    //������ �̸�ǥ�� �Ӹ� ���� �ߴ� ������ ����, ����Ʈ ������Ʈ�� ���� �ؽ�Ʈ ���� ��쿡 ����.
    //�� �ؽ�Ʈ�� ����Ƽ ���� 3D ������ �����ϰ�, ī�޶� ���� ������ ��.

    //GetComponent<T>() vs [SerializeField] ����
    //GetComponent<T>(): �� �Լ��� �ڵ带 ���ؼ� ���� ������Ʈ�� ������Ʈ�� ������ �� ����ϴ� �ž�. �ַ� Start()�� Awake() �Լ����� �����.
    //[SerializeField]: �� Ű����� ������ �ν����Ϳ� ������Ѽ� �����ڰ� ���� ������Ʈ�� �����ϰ� ����� ������ ��.
    //GetComponent<T>()�� �� ��� �ϴ� �� �ƴϰ�, [SerializeField]�� ����� ������ �ν����Ϳ��� ���� �巡���ؼ� �����ϸ� ��
    //�׷��� GetComponent<AudioSource>()�� ���� �ʰ� [SerializeField]�� ����� �ν����Ϳ��� ����� ������Ʈ�� �׷��� �ؼ� �����߾�.
    //�� ������Ʈ�� ���� ���带 �������� [SerializeField] private�� �Ἥ �巡���ؼ� ���� ��������!

    [Header("Walk����")]//���� ���� ����, ����� �ν����Ϳ� �� ���̰� ����!
    [SerializeField] private AudioSource walkingaudioSource;
    [SerializeField] private AudioClip walkSound;
    //�Ҹ��� �� ������ ����ϰ� ������ AudioSource ������Ʈ�� Pitch �Ӽ��� �����ϸ� ��.
    //AudioSource ������Ʈ���� Play On Awake�̶�?
    //�Ѵ� ���: ���� ���� ��(������Ʈ�� ����/Ȱ��ȭ�� ��) �ڵ����� ��� ����ó�� �Ҹ��� ����ϰ� ���� �� �����.
    //���� ���: Ư�� �̺�Ʈ(��: �÷��̾ �ȱ� �����ϰų�, ���Ͱ� �װų�, ������ ��)�� ���缭 �Ҹ��� ����ϰ� ���� �� �����.
    //�� ĳ������ ���, isMoving ���¿� ���� �ȱ� �Ҹ��� ���/�����ϴ� ������ �ڵ�� ���� �����߱� ������,
    //Play On Awake�� ���� �־ ���� �� �Ҹ��� �鸮�� �ʴ� �ž�.�� �ڵ尡 ���� "�� �����̰� ������ �Ҹ��� ������� ��"��� ����ϱ� ����.

    //������Ʈ ����
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
        gameOverManager = FindObjectOfType<GameOverManager>();

        if (walkingaudioSource == null) 
            Debug.LogError("Player: AudioSource ������Ʈ�� ã�� �� ����! �ν����� ����� Ȯ�� �߾� ���߾�?!");
        if (animator == null) 
            Debug.LogError("Start: Animator ������Ʈ�� ã�� �� ����! �÷��̾� ������Ʈ�� Animator ������Ʈ�� Ȯ����!"); 
        if (playerHealth == null) 
            Debug.LogError("Start: PlayerHealth ������Ʈ�� ã�� �� ����! �÷��̾� ������Ʈ�� PlayerHealth ������Ʈ�� Ȯ����!");
        if (gameOverManager == null) 
            Debug.LogError("Player ��ũ��Ʈ���� GameOverManager�� ã�� �� �����ϴ�!");
        //== null�� �� ������Ʈ�� ������Ʈ�� null����. �� ���ų� ����ִ� �����϶� ���� ����� ����
        //!=�� �ݴ�� null���°� �ƴ� ����
        if (ScoreTextUI != null) 
            ScoreTextUI.text = "Score: " + CurrentScore.ToString();
        //���� ���� �� ScoreText UI�� �ʱ� ���� ǥ��
        //currentScore int�� ������ ToString�� ��� �����Ϸ��� �ڵ����� ���ڿ��� ��ȯ.
        //������ ToString()�� ����ϴ� ���� �ڵ��� ��Ȯ��,������,�̷��� �� ������ ���� ������ �ʿ��� ���� ����� ���� �����̾�
        
    }
    void FixedUpdate() //�������궧�� FixedUpdate.�ð��� ������� ��Ȯ�ϰ� �ϰ��� ����� �ʿ��� ���� �ùķ��̼�.
    {//�������궧�� �� �Լ���!!   
       
        if (IsDead)//��� ���¸� �������� �ʰ��ϱ�
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = movement * MoveSpeed;
        //Rigidbody�� �̿��� �̵�, velocity�� Rigidbody�� ���� �ӵ��� ��Ÿ��
        //����2�� movement���� �� �ɸ����� �̵��ӵ�(Move)�� ����� Rigidbody�� ����
    }
    public void PlayerDie()//�÷��̾� ����� ȣ��� �Լ�
    {
        if (IsDead) 
            return;

        IsDead = true;//��� ���·� ����, �� ���� ���𿡴� ���� ��� ����� �ϸ� �ȵǴ� false��
        Debug.Log("�÷��̾� ���!");

        if (animator != null)
        {
            animator.SetTrigger("Die");//Animator�� "Die" Ʈ���� �ߵ�

            float DieTime = 1.5f; //��� �� ������� �ð�
            Destroy(gameObject, DieTime);

            //1.5�� �ڿ� CallGameOverManager�Լ��� ȣ��, �÷��̾ ������ �� UI
            Invoke("CallGameOverManager", 1f);
        }
        //����ÿ��� �̵� �Է� ���� �ʱ�
        horizontalInput = 0;
        verticalInput = 0;
        movement = Vector2.zero;

        if (rb != null) 
            rb.simulated = false;//���� �ùķ��̼� ����  
    }
    private void CallGameOverManager()//�÷��̾ ������ GameOverManager UI ȣ��     
    {//Invoke�� ���� string���� �� �Լ� �̸��� �޾Ƽ� ȣ���� �� �ְŵ�,
     //PlayerDie() �Լ� �ȿ� �ִ� if�� �ڵ� ����� Invoke�� ���� ������ �� ���� �����̾�,
     //�׷��� if���� CallGameOverManager��� �Լ� �ȿ� �ְ�, Invoke�� �� �Լ� �̸�,
     //("CallGameOverManager")�� ȣ���ϰ� ���� ����, �̷��� �ϸ� Invoke�� ��Ģ�� ��Ű�鼭 �װ� ���ϴ� ����� ������ �� �־�.
     //CallGameOverManager �Լ��� Invoke�� ���� ������� '���� ����� �Լ�' ��� ���� ��.
        if (gameOverManager != null) 
            gameOverManager.OnGameOver();//gameOverManager ��ũ��Ʈ�� OnGameOver�Լ� ȣ��
        
    }   

    void Update()//�����Ӵ� ������Ʈ ����
    {      
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        movement = new Vector2(horizontalInput, verticalInput).normalized;//normalized�� �̵� �ӵ��� �����ϰ� ����

        //�ȱ� ���� ��� ���� ȣ��
        HandleWalkingSound();

        if (IsDead)//�׾����� �ƹ� �Էµ� ���� �ʰ� �������� ����
        {
            horizontalInput = 0;
            verticalInput = 0;
            movement = Vector2.zero;
            return;//�׾����� ���⼭ �Լ� ����
        }


        if (animator != null) 
            animator.SetBool("Move", movement.magnitude > 0);

        // �ɸ��� ���� ��ȯ(�¿����)
        //<0 = Horizontal���� 0���� �۴�, -1�� �Ʊ⿡ �������� �̵�
        //>0 = �ݴ�� Horizontal���� 0���� ũ�⿡ ���������� �̵�
        if (horizontalInput < 0) 
            spriteRenderer.flipX = true;//�������� �̵�
        else if (horizontalInput > 0) 
            spriteRenderer.flipX = false;//���������� �̵�
    }

    public Vector3 GetCenterPosition()//���͵��� �÷��̾��� '�߾�'�̶�� �ν��ϰ� ����/������ ��ġ
    {
        //BoxCollider2D�� �ִٸ� �ݶ��̴��� �߽� ���
        Collider2D playerCollider = GetComponent<Collider2D>();

        if (playerCollider != null)
        {
            return playerCollider.bounds.center;//True�϶� 
        }
        return transform.position;//false�϶�
    }

    void HandleWalkingSound()
    {
        //ĳ���Ͱ� �����̴��� Ȯ��
        bool isMoving = (horizontalInput != 0 || verticalInput != 0);

        if(isMoving)
        {
            //�Ҹ��� ��� ���� �ƴ� ���� ���
            //����� ������Ʈ�� �Ҹ� ������ ��� ����Ǿ� �ְ�, ���� �ȱ� �Ҹ��� ��� ���� �ƴ϶��, �Ҹ��� �����! ��� �ǹ̾�.
            if (walkingaudioSource != null && walkSound != null && !walkingaudioSource.isPlaying)
            {
                walkingaudioSource.clip = walkSound;
                walkingaudioSource.loop = true;//�ݺ� ���
                walkingaudioSource.Play();//���� ���·� ���� �÷��� ����!
            }                             //true �����϶� ��� �Ҹ��� �鸮�°ž�!
        }
        else
        {
            //�������� ���� �� �Ҹ� ����
            if (walkingaudioSource != null && walkingaudioSource.isPlaying) 
                walkingaudioSource.Stop();
        }
    }

    public void AddScore(int amount)//������ �߰��ϴ� �Լ�
    {
        CurrentScore += amount;//���޹��� amount(����)��ŭ ������ ������
        Debug.Log("���� ����: " + CurrentScore);

        //UI �ؽ�Ʈ�� ����Ǿ� �ִ��� Ȯ��
        if (ScoreTextUI != null) 
            ScoreTextUI.text = "Score: " + CurrentScore.ToString();
                                 //ToString()�� int ���� ���ڿ��� �ٲ��ִ� �Լ���
    }
    public void ResetScore()//������ �ʱ�ȭ�ϴ� �Լ�
    {
        CurrentScore = 0;
        //�ΰ��� ���� UI�� �ʱ�ȭ�ϰ� �ʹٸ� �� �ٵ� �߰�
        if (ScoreTextUI != null) 
            ScoreTextUI.text = "Score: 0";
    }
}
