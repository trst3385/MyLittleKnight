using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Item: MonoBehaviour
{
    //아이템 종류 정의
    public enum ItemType { None, ArrowPower, SwordPower, Heal, MoveSpeed, ShieldHeal, ChangeToBow, ChangeToSword }


    [Header("아이템 개별 획득 사운드")]
    public ItemType itemType;//이 아이템이 무엇인지 인스펙터에서 선택
    [SerializeField] private AudioClip itemSound;//이 아이템 전용 사운드 하나만 등록
   


    [Header("아이템의 타입, 각 효과들")]
    public float EffectDamage = 1f;//활,검 데미지
    public float AttackCooldown = 0.2f;//활 아이템 획득 시 공격속도 증가 수
    public float Speed = 1f;//이동 속도
    public float Healing = 5f;//체력 회복 효과 값
    public float ShieldAmount = 4f;//방어력 회복 값
    public float DespawnTime = 10f;//아이템이 생성된 후 자동으로 사라지는 시간 (초)

    private bool isUsed = false;//아이템을 획득 했는지 결정하는 bool 타입 변수

    void Start()
    {
        //ItemSpawner 참조를 찾을 필요 없어 (싱글톤 사용)
        //AudioSource 컴포넌트를 직접 쓸 필요 없어 (SoundManager 사용)
        Destroy(gameObject, DespawnTime);//일정 시간 후에 아이템이 사라지도록
    }

    private void OnTriggerEnter2D(Collider2D other)//어떤 콜라이더"(other)와 "접촉이 발생했을 때
    {
        if (!other.CompareTag("Player")) return;//닿은 오브젝트가 "Player" 태그를 가지고 있는지 확인
        if (isUsed) return;//이 함수가 시작되자마자 이 아이템이 이미 사용된 건지 확인해
                           //isUsed가 true면 이미 처리된 아이템이니 함수 종료(이미 한번 획득했으니)
        isUsed = true;//이 아이템은 이미 사용됨! 이라는 표시


        if (other.TryGetComponent<PlayerStatsEffects>(out var statsEffects))//플레이어 컴포넌트들을 안전하게 가져오기
        {
            AttackController attackController = other.GetComponent<AttackController>();
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            PlayerShield playerShield = other.GetComponent<PlayerShield>();

            //아이템 효과 적용
            UseItem(statsEffects, attackController, playerHealth, playerShield);
        }

        //싱글톤으로 스포너에 알림 (드래그 연결 필요 없음)
        if (ItemSpawner.Instance != null)
            ItemSpawner.Instance.ItemDestroyed();

        //인스펙터에 넣은 바로 그 사운드 재생
        if (itemSound != null && SoundManager.Instance != null)
            SoundManager.Instance.PlaySFX(itemSound);


        //아이템은 한번만 먹고 사라져야해, 이걸 지우면 아이템을 먹어도 사라지지 않아
        Destroy(gameObject);
    }

    void UseItem(PlayerStatsEffects statsEffects, AttackController attackController, PlayerHealth playerHealth, PlayerShield playerShield)
    {//실제 아이템 효과를 적용하는 함수. itemType에 따라 다른 효과를 줘.

        switch (itemType)//itemType(Inspector에서 설정한 값)에 따라 분기.
        {
            case ItemType.None:
                Debug.LogWarning("아이템 타입이 설정되지 않았어! Inspector를 확인해!");
                break;

            case ItemType.ArrowPower://활 데미지 증가
                if (statsEffects != null) statsEffects.ArrowDamageUp(EffectDamage, AttackCooldown);
                break;

            case ItemType.SwordPower://검 데미지 증가
                if (statsEffects != null) statsEffects.SwordDamageUp(EffectDamage);
                break;

            case ItemType.Heal://힐링
                if (statsEffects != null) statsEffects.Heal(Healing);
                break;
            
            case ItemType.ShieldHeal://방어력 회복
                if (statsEffects != null) playerShield.HealShield(ShieldAmount);//PlayerShield의 HealShield 함수 호출
                break;

            case ItemType.MoveSpeed://이속증가
                if (statsEffects != null) statsEffects.MoveSpeedUp(Speed);
                break;
        }
    }
}