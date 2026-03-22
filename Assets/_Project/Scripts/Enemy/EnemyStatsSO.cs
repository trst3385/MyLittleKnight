using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Enemy/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject//MonoBehaviour 대신 ScriptableObject으로 상속.
{
    [Header("기본 공통 스탯")]
    //몬스터의 각 스탯(인스펙터에서 설정)
    public float MaxHP;
    public float MoveSpeed;
    public float StopDistance;//플레이어와 이 거리에 닿으면 멈춤
    public float AttackCooldown;
    public float AttackDamage;
    public float DetectionRange;//몬스터가 플레이어를 감지하는 거리
    public float PlayerTargetOffsetY;//몬스터가 플레이어 오브젝트의 중앙으로 이동 Y축 오프셋 (pivot/center 보정)
    public Color SpriteColor;//몬스터 색상
    public int ScoreValue;//몬스터 처치 시 받을 점수

    [Header("보스 및 특수 몬스터 전용")]
    [Tooltip("원거리 탄막 공격 한 발당 데미지")]
    public float energyDamage;

    [Tooltip("대쉬(돌진) 시 이동 속도")]
    public float DashSpeed;

    [Tooltip("대쉬가 유지되는 시간 (초)")]
    public float DashDuration;

    [Tooltip("다음 대쉬까지 걸리는 재사용 대기시간")]
    public float DashCoolTime;

    [Tooltip("대쉬 직전 멈춰서 기를 모으는 시간")]
    public float DashReadyTime;
}
