using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class TimeFreezeItem : MonoBehaviour
{
    [SerializeField] private TimeFreezeData stats;//TimeFreezeData SO 인스펙터에서 연결

    void OnTriggerEnter2D(Collider2D other)
    {
        if (stats == null)//stats (SO에셋)가 연결되어 있는지 확인하는 안전장치
        {
            Debug.LogWarning("TimeFreezeItem: Stats(SO)가 연결되지 않았어!");
            return;
        }

        if (other.CompareTag("Player") && TimeFreeze.Instance != null)
        {
            //직접 숫자를 쓰는 대신 이젠 stats(SO)에서 가져옴
            TimeFreeze.Instance.ActivateTimeFreeze(stats.freezeDuration);

            //사운드도 SO에서 가져옴, (아이템은 획득 시 사라지니 획득 시 사운드는 SoundManager에서 관리)
            if (SoundManager.Instance != null && stats.pickupSound != null)
            {
                SoundManager.Instance.PlaySFX(stats.pickupSound);
            }

            if (TimeFreezeItemSpawner.Instance != null)
            {
                TimeFreezeItemSpawner.Instance.OnItemPickedUp();
            }
        }
    }
}
