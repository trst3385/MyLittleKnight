using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR//UnityEditor ���ӽ����̽��� ����ϰ� �ִ� ��ũ��Ʈ�� ���忡 ���Ե� �� �߻���.
using static UnityEditor.ShaderData;
#endif//UnityEditor�� �����Ϳ����� �۵��ϴ� ����̶�, ���� ���� ���忡�� ���ԵǸ� �� �ǰŵ�.
//#if UNITY_EDITOR ����� �ش� �ڵ带 ������ ���� ��ó���� ���ù����� ������.
using TMPro;//TextMeshPro UI�� ���� ��� �ϴϱ� �߰����ڱ�!

public class ObstacleDifficultyManager : MonoBehaviour
{
    //--- �ν����Ϳ��� ������ ���̵� ������ ---
    [Header("�߻�ü �ӵ� ����")]//����� �̰� ������ ����Ƽ �ν����� â�� �����ϰ� ���� ���� ����� ���� ����̾�
    public float InitialBoltSpeed = 5f;//�ʱ� �߻�ü �ӵ�. ������ ���۵� �� �߻�ü�� �����̴� �⺻ �ӵ�
    public float MaxBoltSpeed = 20f;//�ִ� �߻�ü �ӵ�
    public float SpeedIncreaseRate = 0.5f;//�ӵ� ������. �߻�ü�� �ӵ��� �� �� ������ ������ � �������� ���ϴ� ��
    public float SpeedIncreaseInterval = 20f;//�ӵ� ���� ����. n �ʸ��� �߻�ü�� �ӵ��� �� �ܰ辿 �ø��� ���ϴ� �ð� ����.
    [Header("�߻�ü ���� �ֱ� ����")]
    public float InitialSpawnInterval = 3f;//�ʱ� ���� �ֱ�. ������ ���۵� �� �߻�ü�� �����Ǵ� �⺻ �ð� ����.
    public float MinSpawnInterval = 1f;//�ִ� ���� �ֱ�. ���̵��� ��� �ö󰡵� �� �ð����� �� ª�������� �ʾ�. �ִ� �����Ǵ� �ð� ����.
    public float IntervalDecreaseRate = 0.5f;//�ֱ� ���ҷ�. �߻�ü ���� �ֱⰡ �� �� �پ�� ������ �� �ʾ� ������ ���ϴ� ���̾�.
    public float IntervalDecreaseInterval = 20f;//�ֱ� ���� ����. �� �ʸ��� ���� �ֱ⸦ �� �ܰ辿 ������ ���ϴ� �ð� ����. 10�ʷ� ����������, 10�ʸ��� intervalDecreaseRate��ŭ ���� �ֱⰡ ª��������.
    [Header("�߻�ü ������ ����")]
    public int InitialDamage = 5;//�ʱ� �߻�ü ������
    public int DamageIncrease = 2;//������ ������. �߻�ü�� �������� �� �� ������ ������ � �������� ���ϴ� ��
    public float DamageIncreaseInterval = 20f;//������ ���� ����. �� �ʸ��� �߻�ü�� �������� �� �ܰ辿 �ø��� ���ϴ� �ð� ����. 20�ʷ� ����������, 20�ʸ��� damageIncrease��ŭ �������� �����ϰ���.
    [Header("UI �˸�")]
    public TextMeshProUGUI ObstacleLevelText;//UI �ؽ�Ʈ�� ���� ����

    //--- ���ο��� ����� ������, ������ ���� ���µ��� ���� ������ ---
    private float timeSinceLastSpeedIncrease = 0f;
    private float timeSinceLastIntervalDecrease = 0f;
    private float timeSinceLastDamageIncrease = 0f;
    private float currentBoltSpeed;
    private float currentSpawnInterval;
    private int currentDamage;
    private int currentLevel = 0;//���� ���̵� ������ ������ ����


    // ��𼭵� �� ��ũ��Ʈ�� ������ �� �ְ� ���ִ� '�̱���' ����
    public static ObstacleDifficultyManager Instance { get; private set; }

    void Awake()//Start�Լ����� ���� ����
    {//�Լ� �ȿ� �ִ� if���� �� �� ���� ������ ���� ������. �� ��ũ��Ʈ�� ���� ������Ʈ�� ���ӿ� �� �ϳ��� �����ϵ��� �����ϴ� ��
        if (Instance != null && Instance != this)//���� �̹� '������ �ν��Ͻ�'�� �����ϰ�, �װ� ���� �� �ڽ��� �ƴ϶��
            Destroy(gameObject);//�� �ڵ尡 "�ߺ��Ǵ� ������Ʈ�� �ı��ϴ� ����"�� ��.
        else
        {
            Instance = this;//�̰� "��, ���� �ƹ��� ����? �׷� ���� �ٷ� �� '������ �ν��Ͻ�'�� �Ǿ�߰ڴ�!" ��� ���̾�.
            DontDestroyOnLoad(gameObject);//EnemyDifficulty ��ũ��Ʈ������ �ִ°ž�. "���� �ٲ� ���� �ı����� ��!" ��� ���̾�.
        }
    }
    void Start()
    {
        //�ʱⰪ ����
        //������ ���۵� ��, �ν����Ϳ� ������ �� �ʱⰪ(initial)��,
        //���� ���ӿ��� ����� ���� ��(current)�� �־��ִ� ������ ��.
        currentBoltSpeed = InitialBoltSpeed;
        currentSpawnInterval = InitialSpawnInterval;
        currentDamage = InitialDamage;

        //���� ���� �� UI �ؽ�Ʈ�� �ʱ� ������ ǥ��
        UpdateLevelText();
    }

    void Update()
    {
        //�ð��� ������ ���� ���̵� ����
        timeSinceLastSpeedIncrease += Time.deltaTime;
        timeSinceLastIntervalDecrease += Time.deltaTime;
        timeSinceLastDamageIncrease += Time.deltaTime;

        //if���� Ư�� ������ ������ ���� �ȿ� �ִ� �ڵ带 �����ϰ� �ϴ°��ݾ�?
        //n�ʰ� �����ٸ� �� ������ '��(true)' �� ��.

        //�ӵ� ����
        if (timeSinceLastSpeedIncrease >= SpeedIncreaseInterval)
        {
            currentBoltSpeed = Mathf.Min(currentBoltSpeed + SpeedIncreaseRate, MaxBoltSpeed);
            timeSinceLastSpeedIncrease = 0f;
            Debug.Log($"�߻�ü �ӵ� ����! ���� �ӵ�: {currentBoltSpeed}");

            //���̵� ���� ���� �� UI ������Ʈ�� ���⿡�� �� ���� ȣ��
            currentLevel++;     //�ӵ� ���� if���� �־ �ٸ� if���� ���� ���� �ð��� ���� ���� ���ݾ�? �׷��� �� �� �ϳ����� �־����.
            UpdateLevelText();  //�� if�� ���� ������ �� ������ ���϶����� ����1���� �ִϱ� ���� n�ʸ��� ���� 1������ �ƴ϶� 3������ �ǹ�����!
                                //if������ ���ÿ� �۵��ؼ� ������ ������. ������! ��� ���� ��ȭ �ð��� �Ȱ��� �س�����!

            //���� if���� �ƴ� Update �Լ��� ������...������ n�ʸ��� ������ �� �ƴ϶�, �� �����Ӹ��� ����ؼ� �ö󰡰� �� �ž�,
            //Update �Լ��� ������ ����Ǵ� ���� �� �����Ӹ��� ȣ��Ǵ� �Լ��ϱ�. �׷��� if�� �ȿ� ���°ž�

            //currentLevel ������ Update �Լ� �ȿ� �ִ� ������ �ƴ϶� ObstacleDifficultyManager��� Ŭ���� ��ü�� ���� �ִ� �����ݾ�,
            //�׷��� �� Ŭ���� �ȿ� �ִ� � �Լ��� �� currentLevel ������ �����Ӱ� �����ϰ� ���� �ٲ� �� �־�.

            //1. currentLevel++; �� �ڵ尡 ����Ǹ�, ObstacleDifficultyManager ��ũ��Ʈ �ȿ� �ִ� currentLevel ������ ���� 1 �����ؼ� �����.
            //2. �״��� �ٿ� �ִ� UpdateLevelText(); �Լ��� ȣ���.
            //3. UpdateLevelText() �Լ� ���� �ڵ尡 ����Ǵµ�, �̶� {currentLevel} �κ��� ������,
            //��ũ��Ʈ �ȿ� ����� currentLevel ������ ���� ���� ���� �����ͼ� ����ϴ� ����.
        }

        //���� �ֱ� ����
        if (timeSinceLastIntervalDecrease >= IntervalDecreaseInterval)
        {
            currentSpawnInterval = Mathf.Max(MinSpawnInterval, currentSpawnInterval - IntervalDecreaseRate);
            timeSinceLastIntervalDecrease = 0f;
            Debug.Log($"�߻�ü ���� �ֱ� ����! ���� �ֱ�: {currentSpawnInterval}");
        }

        //������ ����
        if (timeSinceLastDamageIncrease >= DamageIncreaseInterval)
        {
            currentDamage += DamageIncrease;
            timeSinceLastDamageIncrease = 0f;
            Debug.Log($"�߻�ü ������ ����! ���� ������: {currentDamage}");
        }    
    }

    
    private void UpdateLevelText()//UI �ؽ�Ʈ�� ������Ʈ �Լ�
    {
        if (ObstacleLevelText != null)
        {
            ObstacleLevelText.text = $"��ֹ� Lv.{currentLevel}";
            Debug.Log($"�߻�ü ��ֹ��� ��ȭ�ƾ�!");
        }
    }

    //�ٸ� ��ũ��Ʈ���� ���� ���� ������ �� �ִ� �Լ���
    public float GetCurrentBoltSpeed()
    {
        return currentBoltSpeed;
    }

    public float GetCurrentSpawnInterval()
    {
        return currentSpawnInterval;
    }

    public int GetCurrentDamage()
    {
        return currentDamage;
    }
}

