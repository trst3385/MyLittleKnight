using UnityEngine;

[CreateAssetMenu(fileName = "NewTimeFreezeData", menuName = "Item/TimeFreezeData")]
public class TimeFreezeData : ScriptableObject
{
    [Header("시간 정지 설정")]
    public float freezeDuration = 5f;//시간정지 지속 시간

    [Header("사운드 설정")]
    public AudioClip pickupSound;//아이템 획득 시 사운드
}
