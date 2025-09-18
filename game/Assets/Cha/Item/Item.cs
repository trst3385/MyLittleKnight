using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Item: MonoBehaviour
{
    [Header("������ ȹ�� ����")]
    //[SerializeField] private AudioSource audioSource; �̰� ���� �Ƚ�!
    //soundObject��� ���ο� ���� ������Ʈ�� ����� �ű⿡ AudioSource ������Ʈ�� �������� �߰��ؼ� ���� ������,
    //AudioSource ������ �ʿ� ��������
    [SerializeField] private AudioClip bowSound;
    [SerializeField] private AudioClip swordSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip moveSpeedSound;
    [SerializeField] private AudioClip shieldSound;
    [SerializeField] private AudioClip weaponChangeCtbSound;
    [SerializeField] private AudioClip weaponChangeCtsSound;
    public enum ItemType//������ ������ ���� �����ϱ� ���� ����������
    {//enum �ȿ� ���ǵ� �� �׸���� ����(integer) ���� ����. ���� ���� �ִ� �׸��� 0, �� ������ 1,
     //�̷� ������ �ڵ����� ���ڰ� �Ű�����. ���� �װ� ���� ���ڸ� �������� ���� �־�.

        None,//0 �ƹ��͵� �ƴ� (�⺻��, �Ǽ� ������)
        ArrowPower,//1 Ȱ ���ݷ� ����
        SwordPower,//2 �� ������ ����
        Heal,//3 ü�� ȸ��
        MoveSpeed,//4 �̵��ӵ� ����
        ShieldHeal,//5 ���� ȸ��


        ChangeToBow,//Ȱ�� ���� ���� ������ �߰� ����
        ChangeToSword,//������ ���� ���� ������ �߰� ����

    }

    [Header("�������� Ÿ��, �� ȿ���� ����")]
    public ItemType itemType;//enum�� �̸��� ItemType �빮�ڸ� ��� ������ ������ itemType�� �ҹ��ڷ� �߾�
    public float EffectDamage = 1f;//Ȱ,�� ������
    public float AttackCooldown = 0.2f;//Ȱ ������ ȹ�� �� ���ݼӵ� ���� ��
    public float Speed = 1f;//�̵� �ӵ�
    public float Healing = 5f;//ü�� ȸ�� ȿ�� ��
    public float ShieldAmount = 4f;//���� ȸ�� ��
    public float DespawnTime = 10f;//�������� ������ �� �ڵ����� ������� �ð� (��)
    private ItemSpawner itemSpawner;//ItemSpawner ��ũ��Ʈ ����

    void Start()
    {
        //ItemSpawner�� ������ ã�Ƽ� �Ҵ�
        itemSpawner = FindObjectOfType<ItemSpawner>();
        if (itemSpawner == null)
            Debug.LogWarning("ItemSpawner�� ������ ã�� �� ����! ������ ī��Ʈ�� ������Ʈ���� ���� �ž�.");

        //���� �ð� �Ŀ� �������� ���������
        Destroy(gameObject, DespawnTime);
    }

    private void OnTriggerEnter2D(Collider2D other)//� �ݶ��̴�"(other)�� "������ �߻����� ��
    {   //�� �Լ��� �� �������� Collider2D (Is Trigger�� üũ��)�� �ٸ� Collider2D�� ����� �� ȣ��.

        //���� ������Ʈ�� "Player" �±׸� ������ �ִ��� Ȯ��
        if (!other.CompareTag("Player")) return;
        
        PlayerStatsEffects playerStatsEffects = other.GetComponent<PlayerStatsEffects>();
        AttackController attackController = other.GetComponent<AttackController>();
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        PlayerShield playerShield = other.GetComponent<PlayerShield>();

        //�� �߿� �ϳ��� ��ȿ�ϸ� ������ ��� �õ�
        if(playerStatsEffects != null || attackController != null || playerHealth != null )
            UseItem(playerStatsEffects, attackController, playerHealth, playerShield);
        else
            Debug.LogWarning("�������� ȹ�������� �÷��̾�� �ʿ��� ������Ʈ(PlayerStatsEffects, AttackController, PlayerHealth)�� ����!");



        if (itemSpawner != null)//ItemSpawner ��ũ��Ʈ���� �˸��� �ڵ�
            itemSpawner.ItemDestroyed();


        //UseItem()���� ������ Ÿ�Կ� ���� ���� Ŭ���� �����ϰ�, ���⼭ ���.
        AudioClip soundToPlay = null;
        switch (itemType)
        {
            case ItemType.ArrowPower: soundToPlay = bowSound; break;
            case ItemType.SwordPower: soundToPlay = swordSound; break;
            case ItemType.Heal: soundToPlay = healSound; break;
            case ItemType.MoveSpeed: soundToPlay = moveSpeedSound; break;
            case ItemType.ShieldHeal: soundToPlay = shieldSound; break;
            case ItemType.ChangeToBow: soundToPlay = weaponChangeCtbSound; break; 
            case ItemType.ChangeToSword: soundToPlay = weaponChangeCtsSound; break;
        }
        if (soundToPlay != null)//������ ������Ʈ�� ��������� ����� �鸮��
        {        
            GameObject soundObject = new GameObject("OneShotAudio");//�ڵ尡 ����� ������ �����Ǵ� ������Ʈ��,
                                                                    //�ڵ� �ȿ��� ������� ������Ʈ��
            //�� �ڵ尡 ����Ǹ� ���� "OneShotAudio"��� �̸��� �� ������Ʈ�� �ϳ� �������.
            //�� ������Ʈ�� �Ҹ��� ����ϴ� ���Ҹ� �ϰ�, �Ҹ� ����� ������ �ڵ����� �����.

            soundObject.transform.position = transform.position;//soundObject�� ��ġ�� �������� �ִ� ��ġ(transform.position)�� ���� ������.
                                                                //�̷��� �ϸ� �Ҹ��� �������� �ִ� ������ ���� ��ó�� ���.
            AudioSource tempAudioSource = soundObject.AddComponent<AudioSource>();//soundObject�� AudioSource ������Ʈ�� �߰�
            tempAudioSource.clip = soundToPlay;//���� ���� AudioSource ������Ʈ�� ����� ���� ����(soundToPlay)�� �Ҵ���.
            tempAudioSource.volume = 1f;//�Ҹ��� ������ �ִ�� ����. �̷��� �ϸ� ������ �����ϰ� ������.
            tempAudioSource.spatialBlend = 0;//spatialBlend�� 0�� 2D ����, 1�� 3D ����
            //2D ����� ī�޶���� �Ÿ��� ������� �׻� ���� ũ��� �鸮�� ������, �������� ���� ������ �Ҹ� ũ�Ⱑ �޶����� ������ �ذ�
            tempAudioSource.Play();
            
            //�Ҹ� ����� ���� �ڿ� �ڵ����� �ı��ǵ���
            //�Ҹ��� ����Ǵ� �ð���ŭ ��ٷȴٰ�, �Ҹ� ����� ������ soundObject�� �ڵ����� �ı�
            Destroy(soundObject, soundToPlay.length);
        }
        //�������� �ѹ��� �԰� ���������, �̰� ����� �������� �Ծ ������� �ʾ�.
        Destroy(gameObject);     
    }
    
    void UseItem(PlayerStatsEffects statsEffects, AttackController attackController, PlayerHealth playerHealth, PlayerShield playerShield)
    //���� ������ ȿ���� �����ϴ� �Լ�. itemType�� ���� �ٸ� ȿ���� ��.
    {
        switch(itemType)//itemType(Inspector���� ������ ��)�� ���� �б��Ѵ�.
        {
            case ItemType.None:
                Debug.LogWarning("������ Ÿ���� �������� �ʾҽ��ϴ�! Inspector�� Ȯ���ϼ���!");
                break;

            case ItemType.ArrowPower://Ȱ ������ ����
                if(statsEffects != null)
                    statsEffects.ArrowDamageUp(EffectDamage, AttackCooldown);
                break;

            case ItemType.SwordPower://�� ������ ����
                if (statsEffects != null)
                    statsEffects.SwordDamageUp(EffectDamage);
                break;

            case ItemType.Heal://����
                if (statsEffects != null)
                    statsEffects.Heal(Healing);
                break;

            case ItemType.ShieldHeal://���� ȸ��
                if(statsEffects != null)
                    playerShield.HealShield(ShieldAmount);//PlayerShield�� HealShield �Լ� ȣ��
                break;

            case ItemType.MoveSpeed://�̼�����
                if (statsEffects != null)
                    statsEffects.MoveSpeedUp(Speed);//player ��� statsEffects ���, useThis ��� effectAmount ���
                break;
        }
    }
}
