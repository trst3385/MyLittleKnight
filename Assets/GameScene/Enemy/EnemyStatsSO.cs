using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Enemy/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject//MonoBehaviour 대신 ScriptableObject으로 상속.
{
    //몬스터의 각 스텟(인스펙터에서 설정)
    public float MoveSpeed;
    public float StopDistance;//플레이어와 이 거리에 닿으면 멈춤
    public float AttackCooldown;
    public float AttackDamage;
    public float DetectionRange;//몬스터가 플레이어를 감지하는 거리
    public Color SpriteColor;//색상
    public int ScoreValue;//몬스터 처치 시 받을 점수
}
