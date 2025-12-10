using UnityEngine;

public class TimeFreezeItem : MonoBehaviour
{
    [Header("시간 정지 지속 시간")]
    [SerializeField] private float freezeDuration = 3.0f;//시간이 멈췄을때 지속시간

    [Header("사운드 설정")]
    [SerializeField] private AudioClip pickupSound;//아이템 획득 시 들릴 사운드


    private TimeFreezeItemSpawner TimeFreezeitemSpawner;//TimeFreezeItemSpawner 스크립트 참조

    void Start()//내부 컴포넌트는 Awake에, 외부 스크립트는 Start에
    {
        //TimeFreezeItemSpawner를 찾아서 연결
        TimeFreezeitemSpawner = FindFirstObjectByType<TimeFreezeItemSpawner>();
        if (TimeFreezeitemSpawner == null)
            Debug.LogWarning("TimeFreezeItem: TimeFreezeItemSpawner를 찾을 수 없어! 스폰 카운트가 제대로 작동하지 않을 수 있어!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (TimeFreeze.Instance != null)
            {
                TimeFreeze.Instance.ActivateTimeFreeze(freezeDuration);

                //획득 사운드 재생 로직, SoundManager에게 이 사운드를 한 번 재생해 달라고 요청
                if (SoundManager.Instance != null && pickupSound != null) SoundManager.Instance.PlaySFX(pickupSound);


                //TimeFreezeitemSpawner에게 파괴 알림
                if (TimeFreezeitemSpawner != null) TimeFreezeitemSpawner.ItemDestroyed();//인수를 넘길 필요가 없어짐

                Destroy(gameObject);
            }
        }
    }
}
