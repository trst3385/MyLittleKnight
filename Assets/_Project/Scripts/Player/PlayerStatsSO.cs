using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/PlayerStats")]
public class PlayerStatsSO : ScriptableObject
{
    //------------이동------------------
    [Header("--- 이동 관련 데이터 ---")]
    public float moveSpeed = 8f;           //이동속도
    public int maxMoveSpeedLevel = 10;     //최대 이속 에벨
    public AudioClip walkSound;            //이동 사운드

    //------------활--------------------
    [Header("--- 활(Bow) 관련 데이터 ---")]
    public GameObject arrowPrefab;         //일반 화살 프리팹
    public GameObject enhancedArrowPrefab; //강화 화살 프리팹
    public AudioClip bowAttackSound;       //발사 사운드

    public float arrowSpeed = 10f;         //발사속도
    public float baseArrowDamage = 1f;     //데미지
    public float baseArrowCooldown = 2f;   //최소 쿨타임
    public int numberOfArrows360 = 8;      //360도 방향 발사
    public float slowFactor = 0.5f;        //강화 화살 슬로우 비율

    public int maxBowLevel = 10;           //활 최대 강화 레벨
    public int maxEnhanceStacks = 3;       //활 아이템 3회 획득시 강화


    //------------무적------------------
    [Header("--- 무적 스킬 데이터 ---")]
    [Tooltip("무적 지속 시간")]
    public float invincibilityDuration = 3f; //무적 시간
    [Tooltip("무적 스킬 쿨타임")]
    public float invincibilityCooldown = 20f;//쿨타임
    [Tooltip("무적 상태 시 캐릭터 색상")]
    public Color invincibilityColor = new Color(1f, 1f, 0.5f, 0.8f);//무적 시 색상
    public AudioClip invincibilitySound;//무적 사운드
}
