using UnityEngine;

//프로젝트창 우클릭으로 Create->Item->ItemDataSO에셋을 생성할때 꼭 필요한거야,이게 없으면 만들 수 없어
[CreateAssetMenu(fileName = "ItemDataSO", menuName = "Item/ItemData")]
public class ItemDataSO : ScriptableObject
{
    public Item.ItemType itemType; //아이템 종류
    public AudioClip itemSound;    //전용 사운드
    public float despawnTime = 10f;//등장 후 사라지는 시간

    //모든 수치를 여기에 다 때려넣는 게 아니라, 
    //아이템 스크립트에서 공통으로 쓸 변수 이름만 남기는 거야
    public float effectValue;//데미지, 힐량, 방어량, 이속 등 (범용)
    public float attackCooldown;//활 전용 공속
    public float swordCooldownDecrease;//검 강화 시 줄어들 쿨타임 수치 (3회 획득 시 2초 감소, 총 5초)
}
