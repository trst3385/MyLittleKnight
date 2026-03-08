using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStats", menuName = "Enemy/Enemy Stats")]
public class EnemyStatsSO : ScriptableObject//MonoBehaviour 대신 ScriptableObject으로 상속.
{
    //몬스터의 각 스텟(인스펙터에서 설정)
    public float MaxHP;
    public float MoveSpeed;
    public float StopDistance;//플레이어와 이 거리에 닿으면 멈춤
    public float AttackCooldown;
    public float AttackDamage;
    public float DetectionRange;//몬스터가 플레이어를 감지하는 거리
    public float PlayerTargetOffsetY;//몬스터가 플레이어 오브젝트의 중앙으로 이동 Y축 오프셋 (pivot/center 보정)
    public Color SpriteColor;//색상
    public int ScoreValue;//몬스터 처치 시 받을 점수
    //다음엔 몬스터가 플레이어 중앙에 도착하는 변수도 여기에 추가하자 (현재 Enemy스크립트에 있어)
}
