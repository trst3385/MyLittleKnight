using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using TMPro;//TextMeshPro�� ����ϱ� ���� �߰�


//�� ��ũ��Ʈ(EnemyDifficulty)�� '���� ��ȭ �˸�' UI (DifficultyStatsText)�� ���� ����.
//����: Ư�� ���(���� ��ȭ)�� ���ӵ� ������ �˸��� ������ �����ϱ� �����մϴ�.
//����: ���̵� ��/�ƿ��� ���� �ð��� ȿ���� ������, ���� ������ �˸��� ���� �����ϱ� �����!
//(TextAlimManager�� ���� ���� ���� ��ũ��Ʈ�� ���غ�!)



public class EnemyDifficulty : MonoBehaviour
{
    //�ν����Ϳ��� ������ ������
    [Header("Normal ���� ���� �ֱ� ����")]//����� �̰� ������ ����Ƽ �ν����� â�� �����ϰ� ���� ���� ����� ���� ����̾�
    public float InitialNormalSpawnTime = 4f;//���� ���� �� Normal ������ ���� �ֱ� (��: 4��)
    public float MinNormalSpawnTime = 1f;//���� �ֱⰡ �ƹ��� �������� �� �� ���Ϸδ� �� ������ (��: 1��)
    public float SpawnTimeDecreaseRate = 0.1f;//�� �ʸ��� ���� �ֱ⸦ �󸶳� ������ (��: 0.1�ʾ� �پ��)
    public float DecreaseInterval = 10f;//���� �ֱⰡ �پ��� �ð� ���� (��: 10�ʸ��� �� ����)
    //decreaseInterval�� n�� ���� ������ ���� �ð��� spawnTimeDecreaseRate�� n�� ��ŭ �پ���.

    [Header("Normal ���� ���� ���� ���� ����")]
    public int InitialNormalSpawnCount = 1;//���� ���� �� Normal ���� ���� ���� ����
    public int SpawnCountIncreasePerLevel = 1;//���̵� �������� ���� ���� ���� ������
    public int MaxNormalSpawnCount = 5;//���� ���� ���� �ִ�ġ (�ʹ� �������� �� ����)

    //���� ���� ���̵� ���� ������
    [Header("���� ���� ���̵� ����")]
    [SerializeField] private float statInterval = 20f;//���� ������ �������� �ð� ���� (��)
    [SerializeField] private float atkIncrease = 0.2f;//���̵� �������� ���� ���ݷ� ���� ���� (20% = 0.2) -> 20%��
    [SerializeField] private float hpIncrease = 0.2f;//���̵� �������� ���� ü�� ���� ���� (20% = 0.2) -> 20%��
    [SerializeField] private float speedIncrease = 0.01f;//���̵� �������� ���� �̵� �ӵ� ���� ���� (1% = 0.01) -> 1%��

    
    [Header("UI, ������Ʈ ����")]//����� �̰� ������ ����Ƽ �ν����� â�� �����ϰ� ���� ���� ����� ���� ����̾�
    [SerializeField] private TextMeshProUGUI notificationText;//EnemyDifficultyStatsText UI�� ������!
    [SerializeField] private TextMeshProUGUI enemyLevelText;//EnemyDifficultyLevelText UI�� ������!!
    [SerializeField] private TextAlimManager textalimManager;//TextAlimManager UI�� ������!
    [SerializeField] private EnemySpawn enemySpawnRef;//EnemySpawn ������Ʈ ������!!


    private float gameTimer = 0f;//���� ���� �� �� ��� �ð�
    private int currentDifficultyLevel = 0;//���� ���� ���� ���̵� ����

    //FindObjectOfType
    //���ο��� ����� ������
    private float currentNormalSpawnTime;//���� Normal ���Ͱ� �����Ǵ� ���� �ֱ� (�� ���� ��� ���� �ž�)
    private float timeSinceLastDecrease;//���������� ���� �ֱ⸦ ���� �� ���� �ð�
    private int currentNormalSpawnCount;//���� ���� ���� ���� ������ ������ ����
    public static EnemyDifficulty Instance { get; private set; }
    //private set;�� �ǹ�:
    //public static EnemyDifficulty Instance : �ܺ�(�ٸ� ��ũ��Ʈ)���� EnemyDifficulty.Instance�� ���� �� �ְ� ���.
    //get; : ���� �������� �� ������ ����.
    //private set; : ���� �����ϴ� �� ���� �� EnemyDifficulty Ŭ���� ���ο����� ����.

    //��𼭵� �� ��ũ��Ʈ�� ���� ������ �� �ְ� ���ִ� '�̱���' ����
    //GameManageró�� ���ӿ� �ϳ��� �ְ� �ٸ� ��ũ��Ʈ���� �� ���� ������ ��� �� �� ���� ���
    //Instance�� Ư���� Ű���尡 �ƴ� ������ �̸��� ���̾�, �����ڵ��� �̱��� ������ ���� �� ���������� ����ϴ� �̸��̾�.
    //�ٽ��� Instance��� �̸��� �ƴ϶�, �� �տ� ���� public static�̾�.
    //public static�� �پ��� ������, Instance�� '� ������Ʈ���� ������ �ʰ�', Ŭ���� ��ü�� �� �ϳ��� �����ϴ� Ư���� ������ �Ǵ� ����.
    //Awake() �Լ����� Instance = this;��� �ڵ带 ¥��, �� ������ ���ӿ��� ���� ���� ������� �� �ν��Ͻ��� ����Ű���� ��Ģ�� ������ �ž�.
    //DifficultyStatsText UI�� DifficultyManager ������Ʈ���� �� ��ũ��Ʈ�� ���� �̱��� �������� �������
    //�� EnemyDifficulty ��ũ��Ʈ�� DifficultyManager ������Ʈ�� �پ� ���� ��ü�� ���̵��� �Ѱ��ϸ�,
    //DifficultyStatsText UI �˸� ����� ���� ����� ���� �� ���� �ٸ� ��ũ��Ʈ���� �����ϰ� ����ϱ� ���� �̱��� �������� �������.

    //���� Ÿ���� �����ϱ� ���� Enum
    public enum StatType
    {
        AttackDamage,
        Health,
        MoveSpeed
    }

    void Awake()
    {
        //���� ���� �� �� ��ũ��Ʈ�� ������ �ν��Ͻ��� ������.
        if(Instance != null && Instance != this)
            Destroy(gameObject);//�̹� �ν��Ͻ��� ������ �ڽ��� �ı�(�ߺ� ����)
        else
        {
            Instance = this as EnemyDifficulty;//�ڽ��� ������ �ν��Ͻ��� ��
            DontDestroyOnLoad(gameObject);//���� �ٲ� �ı����� �ʰ� (���� ��ü ���̵� ������)
            //DontDestroyOnLoad�� ����Ƽ���� Ư�� ���� ������Ʈ�� ���� ���� �ƴ�
            //���� ������ �Ѿ ���� �ı����� �ʰ� �����ǵ��� �� �� ����ϴ� �Լ�
        }
    }
    void Start()
    {
        currentNormalSpawnTime = InitialNormalSpawnTime;//���� ���� �� �ʱ� ���� �ֱ�� ����
        timeSinceLastDecrease = 0f;//�ð� �ʱ�ȭ
        gameTimer = 0f;//���� Ÿ�̸� �ʱ�ȭ
        currentDifficultyLevel = 0;//���̵� ���� �ʱ�ȭ
        currentNormalSpawnCount = InitialNormalSpawnCount;//���� �� ���� ���� ���� �ʱ�ȭ   
        UpdateMonsterLevelText();


        //EnemySpawn �ν��Ͻ� ã�Ƽ� ����
        if (enemySpawnRef == null)
            Debug.LogError("EnemyDifficulty: EnemySpawn ��ũ��Ʈ�� ������ ã�� �� ����!");
        else
            //EnemySpawn ��ũ��Ʈ�� �ʱ� ���� ���� ���� ����
            enemySpawnRef.SetNormalSpawnCount(currentNormalSpawnCount);


        if(textalimManager == null)
            Debug.LogError("EnemyDifficulty: TextAlimManager ��ũ��Ʈ�� ������ ã�� �� ����!");


        if (notificationText != null)//���� ���� �� UI �ؽ�Ʈ�� ���
            notificationText.text = "";
    }
    
    void Update()
    {
        //�÷��̾ �׾��� ���� ���̵� ������ ���ߵ��� ���⿡ ���ǹ��� �߰��ؾ� ��.
        //���� ���, if (Player.Instance != null && !Player.Instance.isDead) { ... }
        //Player ��ũ��Ʈ���� �̱��� ������ �����ߴٸ� �̷��� ������ �� �־�.

        //���� �ð� ��� �� ���� ���̵� ���� ���� ����
        gameTimer += Time.deltaTime;
        if(gameTimer >= (currentDifficultyLevel + 1) * statInterval)
        {
            currentDifficultyLevel++;
            Debug.Log($"���� ���� ���̵� ���� ����! ���� ����: {currentDifficultyLevel}, �� ��� �ð�: {gameTimer:F2}s");
            //F2�� C# ���ڿ� �����ÿ��� �ε��Ҽ���(Float) ���ڸ� �Ҽ��� ��° �ڸ����� ǥ���϶�� �ǹ�
            //gameTimer ���� 123.45678f ��� ����) F0�̸� 123, F1�̸� 123.5, F2�̸� 123.46


            UpdateMonsterLevelText();//���� ������ ������ ����, EnemyDifficultyLevelText UI�� ������Ʈ�� �Լ��� ȣ����
                                     //UpdateMonsterLevelText() �Լ��� currentDifficultyLevel ���� ���� ������ ȣ��Ǿ�� ��
                                     //�׷���! currentDifficultyLevel++ �ڵ� �ٷ� �Ʒ��� �߰��ϴ� �� ���� �����Ѱž�.
                                     //���� if������ currentDifficultyLevel�� �����ϸ� �װ� ���� ��ũ��Ʈ�� �۵��� �Ǵ��� Ȯ�� �� ���� �����ϴ°ž�


            //���� ���̵� ���� ���� �� ���� ���� ���� ������Ʈ
            currentNormalSpawnCount = Mathf.Min(InitialNormalSpawnCount + (currentDifficultyLevel * SpawnCountIncreasePerLevel));
            Debug.Log($"Normal ���� ���� ���� ���� ����! ���� ����: {currentNormalSpawnCount}����");
            if (enemySpawnRef != null)
                enemySpawnRef.SetNormalSpawnCount(currentNormalSpawnCount);//EnemySpawn�� ����� ���� ����


            if (notificationText != null)//EnemyDifficultyStatsText UI�� ����
            {
                notificationText.text = $"<color=red>���Ͱ� �� ���������ϴ�! (���� {currentDifficultyLevel})</color>";
                Invoke("ClearNotification", 3f);
            }
        }
        //�ð��� �帧�� ���� ���� �ֱ⸦ ���̴� ����
        timeSinceLastDecrease += Time.deltaTime;//������ ���� �� �ð��� ��� ����.

        if (timeSinceLastDecrease >= DecreaseInterval)//������ ���� ������ ��������
        {
            //currentNormalSpawnTime�� ���ҽ�Ű��, minNormalSpawnTime ���Ϸδ� �������� �ʰ� ��
            currentNormalSpawnTime = Mathf.Max(MinNormalSpawnTime, currentNormalSpawnTime - SpawnTimeDecreaseRate);
            timeSinceLastDecrease = 0f;//�ð� �ʱ�ȭ (���� ���� ������ ����)
            Debug.Log($"Normal ���� ���� �ֱ� ����! ���� �ֱ�: {currentNormalSpawnTime}s");

            if (enemySpawnRef != null)//EnemySpawn���� ���ο� ���� �ֱ⸦ �˷���!
                enemySpawnRef.SetNormalSpawnTime(currentNormalSpawnTime);
                //EnemySpawn ��ũ��Ʈ�� SetNormalSpawnTime() �Լ�
        }
    }

    private void UpdateMonsterLevelText()//EnemyDifficultyLevelText UI�� ���� �Լ�
    {                                    //notificationText UI�� �ٸ��� Lv.0 ~ 1 ~ 2 �����ϰ� �Ұž�
        if (enemyLevelText != null)
            enemyLevelText.text = $"���� Lv.{currentDifficultyLevel}";
    }


    private void ClearNotification()//�� �Լ��� notificationText UI �˸��� ȭ�鿡�� �����ִ� ����
    {                               //�� �Լ��� ���� Invoke("ClearNotification", 3f);ó�� ���� �ð� �ڿ� �ڵ����� ȣ��ǵ��� �ؼ�,
                                    //"���Ͱ� ���������ϴ�!" ���� �˸��� 3�� �Ŀ� ������� ����� �뵵�� ����.
        if (notificationText != null)
            notificationText.text = "";
    }


    public float GetSpawnTime()//EnemySpawn ��ũ��Ʈ�� �� ���� ������ �� �ֵ��� �Լ� ����
    {                          //�� �Լ��� '���� ���Ͱ� �����Ǵ� �ֱ�(�ð�)' ���� �ٸ� ��ũ��Ʈ���� �˷��ִ� ����
                               //return currentNormalSpawnTime; �� �ڵ�� EnemyDifficulty ��ũ��Ʈ ���ο��� �����ϰ� �ִ�,
                               //currentNormalSpawnTime�̶�� ���� ���� �״�� ��ȯ�� ��.
        return currentNormalSpawnTime;//���� ���� ���� �ֱ⸦ ������.
    }

    public int GetSpawnCount()
    {//�� �Լ��� '���� �� ���� �����Ǵ� ���� ������' ���� �ٸ� ��ũ��Ʈ���� �˷��ִ� ����
     //return currentNormalSpawnCount; �� �ڵ�� EnemyDifficulty ��ũ��Ʈ ������ currentNormalSpawnCount ���� ���� �״�� ��ȯ�� ��.
     //GetCurrentNormalSpawnTime() �Լ��� ������ �Ȱ���. ���� float Ÿ���� ���� �ֱ� ���,
     //int Ÿ���� ���� ������ ������ ��
        return currentNormalSpawnCount;
    }

    public float GetAdjustedMonsterStat(float baseStat, StatType statType)//���� ������ ���� ���̵��� ���� �����Ͽ� ��ȯ�ϴ� �Լ�
    {       
        float increaseRatio = 0f;

        switch (statType)
        {
            case StatType.AttackDamage:
                increaseRatio = atkIncrease;
                break;
            case StatType.Health:
                increaseRatio = hpIncrease;
                break;
            case StatType.MoveSpeed:
                increaseRatio = speedIncrease;
                break;
            default:
                Debug.LogWarning($"EnemyDifficulty: �� �� ���� ���� Ÿ�� ��û�� - {statType}");
                break;
        }
        float adjustedStat = baseStat * (1f + increaseRatio * currentDifficultyLevel);
        return adjustedStat;
    }
}
