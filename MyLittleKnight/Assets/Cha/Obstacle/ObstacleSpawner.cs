using System.Collections;//�ڷ�ƾ�� ����ϱ� ���� �ʿ�
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

//08.03
//ObstacleDifficultyManager ��ũ��Ʈ�� ��������� ObstacleSpawner ��ũ��Ʈ�� ���� ���̵��� ���� �������� �ʰ�,
//ObstacleDifficultyManager ��ũ��Ʈ���� ���� ���� �ֱ�, �ӵ�, ������ ���� �޾ƿͼ� ����ϰ� �� �ž�.
// ��ֹ� ���� �ֱ� ������Ʈ
public class ObstacleSpawner : MonoBehaviour
{
    [Header("�߻�ü ������, �ݶ��̴� ����")]
    public GameObject ObstacleBoltPrefab;//�߻�ü ������ ����
    public BoxCollider2D SpawnAreaCollider;//������ Ÿ�ϸ� �����ڸ��� �ݶ��̴��� ������ �̰��� �������� �߻�ü ����

    void Start()
    {
        //���� ���� ��, ObstacleDifficultyManager ��ũ��Ʈ���� ������ �ð� ��������
        //"SpawnObstacle" �Լ��� �ݺ��ؼ� ȣ����.
        //InvokeRepeating �Լ��� �μ��� 3���� ����ؾ���!
        //ù ��° GetCurrentSpawnInterval()�� ���� ���� ���� �ð�,
        //�� ��° GetCurrentSpawnInterval()�� �ݺ� �����̾�.
        //InvokeRepeating �Լ��� Ư�� �Լ��� ������ �ð� �������� �ݺ��ؼ� �����ϴ� ����̾�.
        //���� ���� �� 3�� �ڿ� "SpawnObstacle" �Լ��� ó�� �����ϰ�,
        //�� ���Ŀ��� ObstacleDifficultyManager���� ������ �������� �ݺ���.
        InvokeRepeating("SpawnObstacle", 3f, ObstacleDifficultyManager.Instance.GetCurrentSpawnInterval());
    }

    private void SpawnObstacle()
    {
        CancelInvoke("SpawnObstacle");//�� �ڵ�� SpawnObstacle �Լ��� ���� ���� ���� ���� InvokeRepeating�� ���� ���ߴ� �ž�.
        InvokeRepeating("SpawnObstacle", ObstacleDifficultyManager.Instance.GetCurrentSpawnInterval(), ObstacleDifficultyManager.Instance.GetCurrentSpawnInterval());
        //InvokeRepeating(...): �׸��� ����, �ٷ� �Ʒ��� �ִ� �� �ڵ尡
        //ObstacleDifficultyManage ��ũ��Ʈ���� ��� ������ ���ο� �ð� �������� InvokeRepeating�� �ٽ� �����ϴ� ����.

        int spawnSide = Random.Range(0, 2);

        Vector2 spawnPosition = Vector2.zero;
        Vector2 moveDirection = Vector2.zero;

        if (spawnSide == 0)//���ʿ��� ����������
        {
            spawnPosition = new Vector2(SpawnAreaCollider.bounds.min.x, Random.Range(SpawnAreaCollider.bounds.min.y, SpawnAreaCollider.bounds.max.y));
            moveDirection = Vector2.right;
        }
        else if (spawnSide == 1)//�����ʿ��� ��������
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