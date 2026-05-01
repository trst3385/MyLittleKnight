using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/PlayerStats")]
public class PlayerStatsSO : ScriptableObject
{
    //------------이동------------------
    [Header("--- 이동 관련 데이터 ---")]
    public float moveSpeed = 8f;
    public int maxMoveSpeedLevel = 10;
    public AudioClip walkSound;//이동 사운드


    //------------무적------------------
    [Header("--- 무적 스킬 데이터 ---")]
    [Tooltip("무적 지속 시간")]
    public float invincibilityDuration = 3f;
    [Tooltip("무적 스킬 쿨타임")]
    public float invincibilityCooldown = 20f;
    [Tooltip("무적 상태 시 캐릭터 색상")]
    public Color invincibilityColor = new Color(1f, 1f, 0.5f, 0.8f);
    public AudioClip invincibilitySound;//무적 사운드
}
