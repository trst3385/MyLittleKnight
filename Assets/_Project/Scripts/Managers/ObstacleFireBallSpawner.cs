using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;


public class ObstacleFireBallSpawner : MonoBehaviour
{
    [Header("발사체 프리팹, 콜라이더 연결")]
    public GameObject ObstacleFireBall;//발사체 프리팹 참조
    public BoxCollider2D SpawnAreaCollider;//생성할 타일맵 가장자리에 콜라이더를 생성해 이곳을 기준으로 발사체 생성

    private float spawnTimer;

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
        if (ObstacleDifficultyManager.Instance != null)//첫 생성 시간이 준비 안 됐어도 '다음 소환 예약'은 계속 돌아가야해
        {                                              //이 코드가 return 위에 있어야 5초 대기 중에도 다음 소환을 계속 예약함
            CancelInvoke("SpawnFireBall");//CancelInvoke: 기존 예약을 취소하고 새로운 주기로 갱신하기 위함
            float nextInterval = ObstacleDifficultyManager.Instance.GetCurrentSpawnInterval();

            //새로운 주기로 다시 예약(첫 발사 간격과 반복 간격을 모두 currentSpawnInterval로 설정)
            InvokeRepeating("SpawnFireBall", nextInterval, nextInterval);
        }

        //ObstacleDifficultyManager매니저에게 "지금 Fireball 소환해도 돼?"라고 물어보고,
        //아직 안 된다고 하면(5초 전) 생성 로직을 실행하지 않고 기다림
        if (ObstacleDifficultyManager.Instance == null || !ObstacleDifficultyManager.Instance.IsObstacleActionReady())
            return;

        //시간 정지 아이템 획득 시 InvokeRepeating은 돌지만, 여기서 즉시 리턴하여 생성을 막음
        if (TimeFreeze.Instance != null && TimeFreeze.Instance.IsTimeFrozen) return;


        // --- 실제 발사체 생성 로직 시작 ---
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