using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Item: MonoBehaviour
{
    //아이템 종류 정의
    public enum ItemType { None, ArrowPower, SwordPower, Heal, MoveSpeed, ShieldHeal}


    [Header("아이템 데이터 (ScriptableObject)")]
    public ItemDataSO data;//인스펙터에서 생성한 .asset 파일을 여기에 연결


    private bool isUsed = false;//아이템을 획득 했는지 결정하는 bool 타입 변수

    void Start()
    {
        //SO에 설정된 despawnTime 사용
        if (data != null)
        {
            Destroy(gameObject, data.despawnTime);
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: ItemDataSO가 연결되지 않았어!");
        } 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isUsed)//닿은 오브젝트가 Player 태그를 가지고 있는지 확인,
        {                                         //이미 획득했으면 함수 종료(중복 획득 방지)
            return;
        }

        isUsed = true;//획득한 아이템은 이 아이템은 이미 사용됐으니 중복 사용 방지

        UseItem(other);//플레이어 본체(other)를 UseItem()에 넘겨줌

        if (ItemSpawner.Instance != null)//싱글톤 스포너에 알림

        {
            ItemSpawner.Instance.ItemDestroyed();
        }
        if (data != null && data.itemSound != null && SoundManager.Instance != null)//SO에 저장된 전용 사운드 재생
        {
            SoundManager.Instance.PlaySFX(data.itemSound);
        }

        Destroy(gameObject);//아이템은 한번만 먹고 사라져야해
    }

    void UseItem(Collider2D player)
    {
        if (data == null)
        {
            Debug.LogError($"{gameObject.name}: ItemDataSO(data)가 연결되지 않았어!");
            return;
        }

        switch (data.itemType)
        {
            case ItemType.ArrowPower:
                if (player.TryGetComponent<BowWeapon>(out var bow))//player 오브젝트에서 바로 BowWeapon을 찾아, 검 아이템도 동일
                {    
                    bow.UpgradeBow(data.effectValue, data.attackCooldown);
                }
                break;

            case ItemType.SwordPower:
                if (player.TryGetComponent<SwordWeapon>(out var sword))
                {
                    sword.UpgradeSword(data.effectValue, data.swordCooldownDecrease);
                }
                break;

            case ItemType.Heal:
                if (player.TryGetComponent<PlayerHealth>(out var health))
                {
                    health.Heal(data.effectValue);
                }
                break;

            case ItemType.ShieldHeal:
                if (player.TryGetComponent<PlayerShield>(out var shield))
                {
                    shield.HealShield(data.effectValue);
                }
                break;

            case ItemType.MoveSpeed:
                if (player.TryGetComponent<Player>(out var p))
                {
                    p.UpgradeMoveSpeed(data.effectValue);
                }
                else
                {
                    Debug.LogWarning("플레이어 본체 스크립트(Player)를 찾을 수 없어!");
                }
                break;

            case ItemType.None:
                Debug.LogWarning($"{gameObject.name}: 아이템 타입이 None으로 설정되어 있어!");
                break;
        }      
    }
}