using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

//08.03
//ObstacleDifficultyManager 스크립트를 만들었으니 ObstacleSpawner 스크립트는 이제 난이도를 직접 조절하지 않고,
//ObstacleDifficultyManager 스크립트에게 현재 스폰 주기, 속도, 데미지 값을 받아와서 사용하게 될 거야.
//장애물 생성 주기 업데이트
public class ObstacleFireBallSpawner : MonoBehaviour
{
    [Header("발사체 프리팹, 콜라이더 연결")]
    public GameObject ObstacleFireBall;//발사체 프리팹 참조
    public BoxCollider2D SpawnAreaCollider;//생성할 타일맵 가장자리에 콜라이더를 생성해 이곳을 기준으로 발사체 생성

    void Start()
    {
        float interval = 3f;//기본값, 만약 ObstacleDifficultyManager이 null 상태. 즉 연결이 안됐을때 최소한의 기능.
                            //3초 간격 생성 을 보장하는 안전 장치(Fallback) 역할
        if (ObstacleDifficultyManager.Instance != null)
            interval = ObstacleDifficultyManager.Instance.GetCurrentSpawnInterval();

        InvokeRepeating("SpawnFireBall", interval, interval);
        //interval이 두개인 이유?
        //첫 발사까지 걸리는 시간과 이후 발사가 반복되는 간격을 모두,
        //ObstacleDifficultyManager가 결정한 interval 값으로 사용하기 위해
    }

    private void SpawnFireBall()
    {
        //InvokeRepeating은 계속 타이머를 돌리지만, 여기서 즉시 리턴하여 생성을 막기
        if (TimeFreeze.Instance != null && TimeFreeze.Instance.IsTimeFrozen) return;
    

        CancelInvoke("SpawnFireBall");
        float newInterval = ObstacleDifficultyManager.Instance.GetCurrentSpawnInterval();
        InvokeRepeating("SpawnFireBall", ObstacleDifficultyManager.Instance.GetCurrentSpawnInterval(), ObstacleDifficultyManager.Instance.GetCurrentSpawnInterval());
     

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
        GameObject FireBall = Instantiate(ObstacleFireBall, spawnPosition, Quaternion.identity);
        ObstacleFireBall FireBallScript = FireBall.GetComponent<ObstacleFireBall>();
        FireBallScript.MoveDirection = moveDirection;
    }
}