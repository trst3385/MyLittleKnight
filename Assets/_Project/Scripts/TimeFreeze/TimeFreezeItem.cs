using UnityEngine;

public class TimeFreezeItem : MonoBehaviour
{
    [Header("시간 정지 지속 시간")]
    [SerializeField] private float freezeDuration = 3.0f;//시간이 멈췄을때 지속시간

    [Header("사운드 설정")]
    [SerializeField] private AudioClip pickupSound;//아이템 획득 시 들릴 사운드


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (TimeFreeze.Instance != null)
            {
                TimeFreeze.Instance.ActivateTimeFreeze(freezeDuration);//시간 정지 기능 실행

                //획득 사운드 재생 로직, SoundManager에게 이 사운드를 한 번 재생해 달라고 요청
                if (SoundManager.Instance != null && pickupSound != null) SoundManager.Instance.PlaySFX(pickupSound);

                if (TimeFreezeItemSpawner.Instance != null)//TimeFreezeitemSpawner에게 획득 알림 (싱글톤 Instance 사용)
                    TimeFreezeItemSpawner.Instance.OnItemPickedUp();
            }
        }
    }
}
