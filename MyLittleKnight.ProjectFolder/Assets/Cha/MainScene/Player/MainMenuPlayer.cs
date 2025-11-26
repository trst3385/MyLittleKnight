using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPlayer : MonoBehaviour
{
    [HideInInspector]public float MoveSpeed;//MainMenuCutsceneManager 스크립트가 이 변수를 제어함
    public Animator animator;
    public int direction = -1;//1은 오른쪽, -1은 왼쪽

    [Header("활 공격 설정")]
    public GameObject ArrowPrefab;//인스펙터에 화살 프리팹을 연결할 변수
    public Transform ArrowSpawnPoint;//화살이 생성될 위치


    void Start()
    {
        transform.localScale = new Vector3(10, 10, 1);
        animator.SetBool("Move", true);
    }

    void Update()
    {
        //direction 값에 따라 이동 방향을 결정
        transform.Translate(Vector3.right * MoveSpeed * Time.deltaTime * direction);

        //방향이 바뀔 때 캐릭터 스프라이트를 반전시켜야 해
        if (direction == 1)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
    public void ShootArrow()
    {
        //화살 생성
        GameObject newArrow = Instantiate(ArrowPrefab, ArrowSpawnPoint.position, Quaternion.identity);
        //화살 스크립트 가져오기
        MainMenuArrow arrowScript = newArrow.GetComponent<MainMenuArrow>();

        if (arrowScript != null)
        {
            //화살의 방향 설정 (플레이어의 현재 방향과 동일하게 설정)
            //플레이어의 'direction' 변수를 사용
            arrowScript.direction = direction;

            //화살이 플레이어와 같은 방향을 바라보도록 회전
            if (direction == -1)//왼쪽을 볼 때
            {
                //Y축으로 180도 회전하여 화살 스프라이트도 왼쪽을 보게 함
                newArrow.transform.rotation = Quaternion.Euler(0, 180, 0);
            }
        }
    }
}
