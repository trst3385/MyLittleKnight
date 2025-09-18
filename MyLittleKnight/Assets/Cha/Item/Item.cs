using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Item: MonoBehaviour
{
    [Header("아이템 획득 사운드")]
    //[SerializeField] private AudioSource audioSource; 이건 이제 안써!
    //soundObject라는 새로운 게임 오브젝트를 만들고 거기에 AudioSource 컴포넌트를 동적으로 추가해서 쓰기 때문에,
    //AudioSource 변수가 필요 없어졌어
    [SerializeField] private AudioClip bowSound;
    [SerializeField] private AudioClip swordSound;
    [SerializeField] private AudioClip healSound;
    [SerializeField] private AudioClip moveSpeedSound;
    [SerializeField] private AudioClip shieldSound;
    [SerializeField] private AudioClip weaponChangeCtbSound;
    [SerializeField] private AudioClip weaponChangeCtsSound;
    public enum ItemType//아이템 종류를 쉽게 구분하기 위해 열거형으로
    {//enum 안에 정의된 각 항목들은 정수(integer) 값을 가져. 가장 위에 있는 항목은 0, 그 다음은 1,
     //이런 식으로 자동으로 숫자가 매겨지지. 물론 네가 직접 숫자를 지정해줄 수도 있어.

        None,//0 아무것도 아님 (기본값, 실수 방지용)
        ArrowPower,//1 활 공격력 증가
        SwordPower,//2 검 데미지 증가
        Heal,//3 체력 회복
        MoveSpeed,//4 이동속도 증가
        ShieldHeal,//5 방어력 회복


        ChangeToBow,//활로 무기 변경 아이템 추가 예시
        ChangeToSword,//검으로 무기 변경 아이템 추가 예시

    }

    [Header("아이템의 타입, 각 효과들 변수")]
    public ItemType itemType;//enum의 이름을 ItemType 대문자를 적어서 참조할 변수인 itemType은 소문자로 했어
    public float EffectDamage = 1f;//활,검 데미지
    public float AttackCooldown = 0.2f;//활 아이템 획득 시 공격속도 증가 수
    public float Speed = 1f;//이동 속도
    public float Healing = 5f;//체력 회복 효과 값
    public float ShieldAmount = 4f;//방어력 회복 값
    public float DespawnTime = 10f;//아이템이 생성된 후 자동으로 사라지는 시간 (초)
    private ItemSpawner itemSpawner;//ItemSpawner 스크립트 참조

    void Start()
    {
        //ItemSpawner를 씬에서 찾아서 할당
        itemSpawner = FindObjectOfType<ItemSpawner>();
        if (itemSpawner == null)
            Debug.LogWarning("ItemSpawner를 씬에서 찾을 수 없어! 아이템 카운트가 업데이트되지 않을 거야.");

        //일정 시간 후에 아이템이 사라지도록
        Destroy(gameObject, DespawnTime);
    }

    private void OnTriggerEnter2D(Collider2D other)//어떤 콜라이더"(other)와 "접촉이 발생했을 때
    {   //이 함수는 이 아이템의 Collider2D (Is Trigger가 체크된)가 다른 Collider2D와 닿았을 때 호출.

        //닿은 오브젝트가 "Player" 태그를 가지고 있는지 확인
        if (!other.CompareTag("Player")) return;
        
        PlayerStatsEffects playerStatsEffects = other.GetComponent<PlayerStatsEffects>();
        AttackController attackController = other.GetComponent<AttackController>();
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        PlayerShield playerShield = other.GetComponent<PlayerShield>();

        //셋 중에 하나라도 유효하면 아이템 사용 시도
        if(playerStatsEffects != null || attackController != null || playerHealth != null )
            UseItem(playerStatsEffects, attackController, playerHealth, playerShield);
        else
            Debug.LogWarning("아이템을 획득했지만 플레이어에게 필요한 컴포넌트(PlayerStatsEffects, AttackController, PlayerHealth)가 없어!");



        if (itemSpawner != null)//ItemSpawner 스크립트에게 알리는 코드
            itemSpawner.ItemDestroyed();


        //UseItem()에서 아이템 타입에 따라 사운드 클립을 결정하고, 여기서 재생.
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
        if (soundToPlay != null)//아이템 오브젝트가 사라지더라도 사운드는 들리게
        {        
            GameObject soundObject = new GameObject("OneShotAudio");//코드가 실행될 때마다 생성되는 오브젝트야,
                                                                    //코드 안에서 만들어진 오브젝트지
            //이 코드가 실행되면 씬에 "OneShotAudio"라는 이름의 빈 오브젝트가 하나 만들어져.
            //이 오브젝트는 소리를 재생하는 역할만 하고, 소리 재생이 끝나면 자동으로 사라져.

            soundObject.transform.position = transform.position;//soundObject의 위치를 아이템이 있던 위치(transform.position)와 같게 설정해.
                                                                //이렇게 하면 소리가 아이템이 있던 곳에서 나는 것처럼 들려.
            AudioSource tempAudioSource = soundObject.AddComponent<AudioSource>();//soundObject에 AudioSource 컴포넌트를 추가
            tempAudioSource.clip = soundToPlay;//새로 만든 AudioSource 컴포넌트에 재생할 사운드 파일(soundToPlay)을 할당해.
            tempAudioSource.volume = 1f;//소리의 볼륨을 최대로 설정. 이렇게 하면 볼륨이 일정하게 유지돼.
            tempAudioSource.spatialBlend = 0;//spatialBlend의 0이 2D 사운드, 1이 3D 사운드
            //2D 사운드는 카메라와의 거리에 상관없이 항상 같은 크기로 들리기 때문에, 아이템을 먹을 때마다 소리 크기가 달라지는 문제를 해결
            tempAudioSource.Play();
            
            //소리 재생이 끝난 뒤에 자동으로 파괴되도록
            //소리가 재생되는 시간만큼 기다렸다가, 소리 재생이 끝나면 soundObject를 자동으로 파괴
            Destroy(soundObject, soundToPlay.length);
        }
        //아이템은 한번만 먹고 사라져야해, 이걸 지우면 아이템을 먹어도 사라지지 않아.
        Destroy(gameObject);     
    }
    
    void UseItem(PlayerStatsEffects statsEffects, AttackController attackController, PlayerHealth playerHealth, PlayerShield playerShield)
    //실제 아이템 효과를 적용하는 함수. itemType에 따라 다른 효과를 줘.
    {
        switch(itemType)//itemType(Inspector에서 설정한 값)에 따라 분기한다.
        {
            case ItemType.None:
                Debug.LogWarning("아이템 타입이 설정되지 않았습니다! Inspector를 확인하세영!");
                break;

            case ItemType.ArrowPower://활 데미지 증가
                if(statsEffects != null)
                    statsEffects.ArrowDamageUp(EffectDamage, AttackCooldown);
                break;

            case ItemType.SwordPower://검 데미지 증가
                if (statsEffects != null)
                    statsEffects.SwordDamageUp(EffectDamage);
                break;

            case ItemType.Heal://힐링
                if (statsEffects != null)
                    statsEffects.Heal(Healing);
                break;

            case ItemType.ShieldHeal://방어력 회복
                if(statsEffects != null)
                    playerShield.HealShield(ShieldAmount);//PlayerShield의 HealShield 함수 호출
                break;

            case ItemType.MoveSpeed://이속증가
                if (statsEffects != null)
                    statsEffects.MoveSpeedUp(Speed);//player 대신 statsEffects 사용, useThis 대신 effectAmount 사용
                break;
        }
    }
}
