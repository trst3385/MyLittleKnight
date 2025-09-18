using System.Collections;//코루틴을 사용하기 위해 필요
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

//08.03
//ObstacleDifficultyManager 스크립트를 만들었으니 ObstacleSpawner 스크립트는 이제 난이도를 직접 조절하지 않고,
//ObstacleDifficultyManager 스크립트에게 현재 스폰 주기, 속도, 데미지 값을 받아와서 사용하게 될 거야.
// 장애물 생성 주기 업데이트
public class ObstacleSpawner : MonoBehaviour
{
    [Header("발사체 프리팹, 콜라이더 연결")]
    public GameObject ObstacleBoltPrefab;//발사체 프리팹 참조
    public BoxCollider2D SpawnAreaCollider;//생성할 타일맵 가장자리에 콜라이더를 생성해 이곳을 기준으로 발사체 생성

    void Start()
    {
        //게임 시작 후, ObstacleDifficultyManager 스크립트에서 설정한 시간 간격으로
        //"SpawnObstacle" 함수를 반복해서 호출해.
        //InvokeRepeating 함수는 인수를 3개만 사용해야해!
        //첫 번째 GetCurrentSpawnInterval()은 최초 실행 지연 시간,
        //두 번째 GetCurrentSpawnInterval()은 반복 간격이야.
        //InvokeRepeating 함수는 특정 함수를 일정한 시간 간격으로 반복해서 실행하는 기능이야.
        //게임 시작 후 3초 뒤에 "SpawnObstacle" 함수를 처음 실행하고,
        //그 이후에는 ObstacleDifficultyManager에서 설정한 간격으로 반복해.
        InvokeRepeating("SpawnObstacle", 3f, ObstacleDifficultyManager.Instance.GetCurrentSpawnInterval());
    }

    private void SpawnObstacle()
    {
        CancelInvoke("SpawnObstacle");//이 코드는 SpawnObstacle 함수에 대해 현재 실행 중인 InvokeRepeating을 먼저 멈추는 거야.
        InvokeRepeating("SpawnObstacle", ObstacleDifficultyManager.Instance.GetCurrentSpawnInterval(), ObstacleDifficultyManager.Instance.GetCurrentSpawnInterval());
        //InvokeRepeating(...): 그리고 나서, 바로 아래에 있는 이 코드가
        //ObstacleDifficultyManage 스크립트에서 방금 가져온 새로운 시간 간격으로 InvokeRepeating을 다시 시작하는 거지.

        int spawnSide = Random.Range(0, 2);

        Vector2 spawnPosition = Vector2.zero;
        Vector2 moveDirection = Vector2.zero;

        if (spawnSide == 0)//왼쪽에서 오른쪽으로
        {
            spawnPosition = new Vector2(SpawnAreaCollider.bounds.min.x, Random.Range(SpawnAreaCollider.bounds.min.y, SpawnAreaCollider.bounds.max.y));
            moveDirection = Vector2.right;
        }
        else if (spawnSide == 1)//오른쪽에서 왼쪽으로
        {
            spawnPosition = new Vector2(SpawnAreaCollider.bounds.max.x, Random.Range(SpawnAreaCollider.bounds.min.y, SpawnAreaCollider.bounds.max.y));
            moveDirection = Vector2.left;
        }

        GameObject bolt = Instantiate(ObstacleBoltPrefab, spawnPosition, Quaternion.identity);
        ObstacleBolt boltScript = bolt.GetComponent<ObstacleBolt>();

        boltScript.MoveSpeed = ObstacleDifficultyManager.Instance.GetCurrentBoltSpeed();
        boltScript.Damage = ObstacleDifficultyManager.Instance.GetCurrentDamage();
        boltScript.MoveDirection = moveDirection;
    }
}