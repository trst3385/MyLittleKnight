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

    //------------검--------------------
    [Header("--- 검(Sword) 관련 데이터 ---")]
    public GameObject swordEnergyPrefab;   //검기 프리팹
    public AudioClip swordAttackSound;     //공격 사운드
    public string enemyTag = "Enemy";      //검 공격 범위 콜라이더 안에 있는 오브젝트 태그(적 몬스터의 Tag면 데미지를 줘)

    public float baseSwordDamage = 2f;      //기본 검 데미지
    public float knockbackForce = 10f;      //넉백 강도
    public float knockbackDuration = 0.5f;  //넉백 지속 시간
    public float baseSwordEnergyDamage = 5f;//검기 에너지 공격력
    public float swordEnergySpeed = 15f;    //검기 에너지 발사 속도
    public float baseSwordSkillCooldown = 10f;//검 스킬 기본 쿨타임
    public float enhancedCooldownDecrease = 2f;//강화 시 줄일 쿨타임
    public float minSwordCooldown = 5f;    //최소 쿨타임 제한

    public int maxSwordLevel = 10;         //검 최대 레벨
    public int swordEnhanceStackLimit = 3; //강화 스택 기준 (3회)


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
