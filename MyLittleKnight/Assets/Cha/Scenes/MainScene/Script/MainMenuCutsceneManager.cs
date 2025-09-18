using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class MainMenuCutsceneManager : MonoBehaviour
{
    public enum CutsceneState//각 컷씬의 각 상태를 정의하는 enum
    {
        ChasePlayer, //1. 몬스터가 플레이어를 쫒아감
        PlayerAttack,//2. 플레이어가 검 공격 모션을 하며 역으로 몬스터를 쫒아감
        ChasePlayer2//3. 여러 마리의 몬스터가 플레이어를 쫒아감
    }

    public CutsceneState CurrentState;//현재 컷씬의 상태

    [Header("오브젝트 연결")]
    public MainMenuPlayer Player;
    public MainMenuEnemy Monster;
    public MainMenuEnemy Monster2;
    public MainMenuEnemy EliteMonster;

    [Header("속도 설정")]
    //컷씬 전환에 필요한 변수들
    public float Cutscene1pSpeed = 2f;
    public float Cutscene1mSpeed = 3f;
    public float Cutscene1m1Speed = 3f;

    public float Cutscene2pSpeed = 3f;
    public float Cutscene2mSpeed = 1f;
    public float Cutscene2m1Speed = 3f;

    public float Cutscene3pSpeed = 3f;
    public float Cutscene3mSpeed = 3f;
    public float Cutscene3m1Speed = 3f;
    public float Cutscene3emSpeed = 3f;

    [Header("컷씬 전환 값")]
    public float ChangeStateX = 20f;//컷씬 상태를 바꿀 X 좌표 (화면 오른쪽 끝) 인스펙터에서 설정해!
    public float ReturnPositionX = -20f;//캐릭터가 돌아올 X 좌표 (화면 왼쪽 끝)인스펙터에서 설정해!
    //ChangeStateX는 오른쪽 방향으로 갈때의 도달할 x값
    //ReturnPositionX는 반대로 왼쪽 방향으로 갈때의 도달할 x값

    void Start()
    {
        //게임 시작 시 첫 번째 상태로 설정하고 컷씬을 시작
        CurrentState = CutsceneState.ChasePlayer;
        SetState();
    }

    void Update()
    {
        //매 프레임마다 현재 상태를 체크하고 로직을 실행
        switch (CurrentState)
        {
            case CutsceneState.ChasePlayer://가장 뒤에 있는 몬스터2의 x값을 기준으로 x값이 도달하면 컷씬 전환(왼쪽->오른쪽)
                if (Monster2.transform.position.x > ChangeStateX)
                {
                    CurrentState = CutsceneState.PlayerAttack;
                    SetState();
                }
                break;

            case CutsceneState.PlayerAttack://가장 뒤에 있는 Player의 x값을 기준으로 x값이 도달하면 컷씬 전환(오른쪽<-왼쪽)
                if (Player.transform.position.x < ReturnPositionX)
                {
                    CurrentState = CutsceneState.ChasePlayer2;
                    SetState();
                }
                break;

            case CutsceneState.ChasePlayer2://가장 뒤에 있는 Monster2의 x값을 기준으로 x값이 도달하면 컷씬 전환(왼쪽->오른쪽)
                if (Monster2.transform.position.x > ChangeStateX)
                {
                    CurrentState = CutsceneState.ChasePlayer;
                    SetState();
                }
                break;
        }
    }

    void SetState()//상태가 바뀔 때, 각 캐릭터와 몬스터의 속도와 애니메이션을 설정
    {
        switch (CurrentState)
        {
            case CutsceneState.ChasePlayer:

                Monster.gameObject.SetActive(true);
                Monster2.gameObject.SetActive(true);
                EliteMonster.gameObject.SetActive(false);//EliteMonster는 아직 사용 안하니 비활성화

                //이 코드는 플레이어를 화면 왼쪽 끝(returnPositionX)으로 이동시키는 역할이야.
                //몬스터에게 쫓기는 첫 번째 컷씬이 시작될 때 플레이어를 화면 밖에서 등장시키는 데 사용해.
                Player.transform.position = new Vector3(ReturnPositionX, Player.transform.position.y, Player.transform.position.z);
                 
                //이 코드는 몬스터를 플레이어보다 -3만큼 더 왼쪽에 위치시키는 역할이야.
                //이렇게 하면 플레이어와 몬스터 사이에 간격이 생겨서, 몬스터가 플레이어를 쫓는 것처럼 보이게 돼.
                Monster.transform.position = new Vector3(ReturnPositionX - 3, Monster.transform.position.y, Monster.transform.position.z);
                Monster2.transform.position = new Vector3(ReturnPositionX - 6, Monster2.transform.position.y, Monster2.transform.position.z); // <-- Monster2를 더 뒤에 위치시켜


                //방향 오른쪽으로
                Player.direction = 1;
                Monster.direction = 1;
                Monster2.direction = 1;
                
                //플레이어와 몬스터의 속도 설정
                Player.MoveSpeed = Cutscene1pSpeed;
                Monster.MoveSpeed = Cutscene1mSpeed;
                Monster2.MoveSpeed = Cutscene1m1Speed;
                //애니메이션 설정
                Player.animator.SetBool("Move", true);
                Player.animator.SetBool("Attack", false);

                Monster.animator.SetBool("Move", true);
                Monster.animator.SetBool("Attack", true);
                Monster2.animator.SetBool("Move", true);
                Monster2.animator.SetBool("Attack", true);
                break;

            case CutsceneState.PlayerAttack:

                Monster.gameObject.SetActive(true);//일반 몬스터 활성화
                Monster2.gameObject.SetActive(true);
                EliteMonster.gameObject.SetActive(false);//EliteMonster는 아직 사용 안하니 비활성화

                //이 코드는 1, 3번과 반대로 플레이어를 화면 왼쪽 끝(returnPositionX)으로 이동시키는 역할이야
                //몬스터에게 쫓기는 첫 번째 컷씬이 시작될 때 플레이어를 화면 밖에서 등장시키는 데 사용해
                Player.transform.position = new Vector3(ChangeStateX + 8, Player.transform.position.y, Player.transform.position.z);

                //이 코드는 몬스터를 플레이어보다 -3만큼 더 왼쪽에 위치시키는 역할이야
                //이렇게 하면 플레이어와 몬스터 사이에 간격이 생겨서, 몬스터가 플레이어를 쫓는 것처럼 보이게 돼.
                Monster.transform.position = new Vector3(ChangeStateX + 3, Monster.transform.position.y, Monster.transform.position.z);
                Monster2.transform.position = new Vector3(ChangeStateX + 6, Monster2.transform.position.y, Monster2.transform.position.z);

                //플레이어가 몬스터를 쫓아 왼쪽으로 이동
                Player.direction = -1;
                Monster.direction = -1;
                Monster2.direction = -1;

                //속도
                Player.MoveSpeed = Cutscene2pSpeed;
                Monster.MoveSpeed = Cutscene2mSpeed;
                Monster2.MoveSpeed = Cutscene2m1Speed;

                //애니메이션 설정
                Player.animator.SetBool("Move", true);
                Player.animator.SetBool("Attack", true);//공격 애니메이션 시작
                
                //몬스터는 도망치는 모션을 계속 유지
                Monster.animator.SetBool("Move", true);
                Monster.animator.SetBool("Attack", false);
                Monster2.animator.SetBool("Move", true);
                Monster2.animator.SetBool("Attack", false);
                break;

            case CutsceneState.ChasePlayer2:

                
                Monster.gameObject.SetActive(true);
                Monster2.gameObject.SetActive(true);
                EliteMonster.gameObject.SetActive(true);//여기서 Elite 몬스터를 써야하니 활성화

                //이 코드는 플레이어를 화면 오른쪽 끝(changeStateX)으로 이동시키는 역할이야.
                //몬스터를 쫓는 두 번째 컷씬이 시작될 때 플레이어를 화면 밖에서 등장시키는 데 사용해.
                Player.transform.position = new Vector3(ReturnPositionX, Player.transform.position.y, Player.transform.position.z);

                //이 코드는 몬스터를 플레이어보다 3만큼 더 왼쪽에 위치시켜.
                //이렇게 하면 플레이어와 몬스터 사이에 간격이 생겨서, 플레이어가 몬스터를 쫓는 것처럼 보이게 돼.
                EliteMonster.transform.position = new Vector3(ReturnPositionX - 4, EliteMonster.transform.position.y, EliteMonster.transform.position.z);
                Monster.transform.position = new Vector3(ReturnPositionX - 6, Monster.transform.position.y, Monster.transform.position.z);
                Monster2.transform.position = new Vector3(ReturnPositionX - 8, Monster2.transform.position.y, Monster2.transform.position.z);

                //방향 오른쪽으로
                Player.direction = 1;
                EliteMonster.direction = 1;
                Monster.direction = 1;
                Monster2.direction = 1;

                //속도와 애니메이션 설정
                EliteMonster.MoveSpeed = Cutscene3emSpeed;
                Player.MoveSpeed = Cutscene3pSpeed;
                Monster.MoveSpeed = Cutscene3mSpeed;
                Monster2.MoveSpeed = Cutscene3m1Speed;

                //각 이동, 공격 에니메이션
                Player.animator.SetBool("Attack", false);
                EliteMonster.animator.SetBool("Move", true);
                EliteMonster.animator.SetBool("Attack", true);
                Monster.animator.SetBool("Move", true);
                Monster.animator.SetBool("Attack", false);
                Monster2.animator.SetBool("Move", true);
                Monster2.animator.SetBool("Attack", false);
                break;
        }
    }
    
}
