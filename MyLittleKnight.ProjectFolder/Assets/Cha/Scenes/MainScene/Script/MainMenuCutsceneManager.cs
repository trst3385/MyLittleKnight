using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.Android.Gradle;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

[Serializable]//이 구조체가 유니티 인스펙터에 보이도록 해줘
public struct CutsceneSpeedData
{
    public float PlayerSpeed;
    public float Monster1Speed;//Monster
    public float Monster2Speed;//Monster2
    public float EliteMonsterSpeed;//EliteMonster
}

public class MainMenuCutsceneManager : MonoBehaviour
{
    public enum CutsceneState//각 컷씬의 각 상태를 정의하는 enum
    {
        ChasePlayer,//1. 몬스터가 플레이어를 쫒아감
        PlayerAttack,//2. 플레이어가 검 공격 모션을 하며 역으로 몬스터를 쫒아감
        ChasePlayer2,//3. 여러 마리의 몬스터가 플레이어를 쫒아감
        PlayerAttack2//4. 플레이어가 활 공격 모션을 하며 역으로 몬스터를 쫒아감
    }

    public CutsceneState CurrentState;//현재 컷씬의 상태

    [Header("오브젝트 연결")]
    public MainMenuPlayer Player;
    public MainMenuEnemy Monster;
    public MainMenuEnemy Monster2;
    public MainMenuEnemy EliteMonster; 
    
    private List<MainMenuEnemy> allEnemies;//모든 몬스터 객체를 담을 리스트 (인스펙터에서 크기 설정)

    [Header("컷씬 속도")]
    public CutsceneSpeedData Cutscene1;//ChasePlayer 상태 속도
    public CutsceneSpeedData Cutscene2;//PlayerAttack 상태 속도
    public CutsceneSpeedData Cutscene3;//ChasePlayer2 상태 속도
    public CutsceneSpeedData Cutscene4;//PlayerAttack3 상태 속도

    [Header("컷씬 전환 값")]
    public float ChangeStateX = 20f;//컷씬 상태를 바꿀 X 좌표 (화면 오른쪽 끝) 인스펙터에서 설정해!
    public float ReturnPositionX = -20f;//캐릭터가 돌아올 X 좌표 (화면 왼쪽 끝)인스펙터에서 설정해!
    //ChangeStateX는 오른쪽 방향으로 갈때의 도달할 x값
    //ReturnPositionX는 반대로 왼쪽 방향으로 갈때의 도달할 x값

    void Start()
    {
        //게임 시작 시 첫 번째 상태로 설정하고 컷씬을 시작
        CurrentState = CutsceneState.ChasePlayer;
        allEnemies = new List<MainMenuEnemy> { Monster, Monster2, EliteMonster };//모든 몬스터를 리스트에 담아 관리 준비
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
                    CurrentState = CutsceneState.PlayerAttack2;
                    SetState();
                }
                break;

            case CutsceneState.PlayerAttack2://가장 뒤에 있는 Player의 x값을 기준으로 x값이 도달하면 컷씬 전환(오른쪽<-왼쪽)
                if (Player.transform.position.x < ReturnPositionX)
                {
                    //컷신 1번으로 돌아가며 사이클 종료(1번부터 다시 시작)
                    CurrentState = CutsceneState.ChasePlayer;
                    SetState();
                }
                break;
        }
    }

    void SetState()//상태가 바뀔 때, 각 캐릭터와 몬스터의 속도와 애니메이션을 설정
    {
        //현재 컷신에 맞는 속도 데이터 구조체를 가져와
        CutsceneSpeedData currentSpeedData;

        switch (CurrentState)
        {
            case CutsceneState.ChasePlayer://1
                currentSpeedData = Cutscene1;

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
                Player.MoveSpeed = currentSpeedData.PlayerSpeed;
                Monster.MoveSpeed = currentSpeedData.Monster1Speed;
                Monster2.MoveSpeed = currentSpeedData.Monster2Speed;

                //플레이어는 이동모션만, 몬스터는 공격, 이동 모션
                Player.animator.SetBool("Move", true);
                Player.animator.SetBool("Attack", false);
                Player.animator.SetBool("Attack(Bow)", false);

                Monster.animator.SetBool("Move", true);
                Monster.animator.SetBool("Attack", true);
                Monster2.animator.SetBool("Move", true);
                Monster2.animator.SetBool("Attack", true);
                break;

            case CutsceneState.PlayerAttack://2
                currentSpeedData = Cutscene2;

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

                //플레이어와 몬스터의 속도 설정
                Player.MoveSpeed = currentSpeedData.PlayerSpeed;
                Monster.MoveSpeed = currentSpeedData.Monster1Speed;
                Monster2.MoveSpeed = currentSpeedData.Monster2Speed;

                //애니메이션 설정
                Player.animator.SetBool("Move", true);
                Player.animator.SetBool("Attack", true);
                Player.animator.SetBool("Attack(Bow)", false);
                //플레이어는 검공격과 이동, 몬스터는 이동만
                Monster.animator.SetBool("Move", true);
                Monster.animator.SetBool("Attack", false);
                Monster2.animator.SetBool("Move", true);
                Monster2.animator.SetBool("Attack", false);
                break;

            case CutsceneState.ChasePlayer2://3
                currentSpeedData = Cutscene3;

                Monster.gameObject.SetActive(true);
                Monster2.gameObject.SetActive(true);
                EliteMonster.gameObject.SetActive(true);//여기서부터 4번까지 Elite 몬스터를 써야하니 활성화

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

                //플레이어와 몬스터의 속도 설정
                EliteMonster.MoveSpeed = currentSpeedData.EliteMonsterSpeed;
                Player.MoveSpeed = currentSpeedData.PlayerSpeed;
                Monster.MoveSpeed = currentSpeedData.Monster1Speed;
                Monster2.MoveSpeed = currentSpeedData.Monster2Speed;

                //플레이어와 일반 몬스터는 이동, 엘리트 몬스터만 공격과 이동
                Player.animator.SetBool("Attack", false);
                Player.animator.SetBool("Attack(Bow)", false);
                EliteMonster.animator.SetBool("Move", true);
                EliteMonster.animator.SetBool("Attack", true);
                Monster.animator.SetBool("Move", true);
                Monster.animator.SetBool("Attack", false);
                Monster2.animator.SetBool("Move", true);
                Monster2.animator.SetBool("Attack", false);
                break;

            case CutsceneState.PlayerAttack2://4
                currentSpeedData = Cutscene4;

                Monster.gameObject.SetActive(true);
                Monster2.gameObject.SetActive(true);
                EliteMonster.gameObject.SetActive(true);

                //초기 위치 설정 (화면 오른쪽 밖에서 시작하도록)
                Player.transform.position = new Vector3(ChangeStateX + 4, Player.transform.position.y, Player.transform.position.z);

                //몬스터들은 플레이어 뒤에서 쫓아오도록 설정 (혹은 그 자리에 멈춰있도록 설정)
                EliteMonster.transform.position = new Vector3(ChangeStateX, EliteMonster.transform.position.y, EliteMonster.transform.position.z);
                Monster.transform.position = new Vector3(ChangeStateX - 2, Monster.transform.position.y, Monster.transform.position.z);
                Monster2.transform.position = new Vector3(ChangeStateX - 4, Monster2.transform.position.y, Monster2.transform.position.z);

                //방향 왼쪽으로 이동
                Player.direction = -1;
                EliteMonster.direction = -1;
                Monster.direction = -1;
                Monster2.direction = -1;

                //플레이어, 몬스터 속도
                Player.MoveSpeed = currentSpeedData.PlayerSpeed;
                EliteMonster.MoveSpeed = currentSpeedData.EliteMonsterSpeed;
                Monster.MoveSpeed = currentSpeedData.Monster1Speed;
                Monster2.MoveSpeed = currentSpeedData.Monster2Speed;

                //플레이어는 활공격, 모든 몬스터는 이동
                Player.animator.SetBool("Attack(Bow)", true);
                EliteMonster.animator.SetBool("Move", true);
                EliteMonster.animator.SetBool("Attack", false);
                Monster.animator.SetBool("Move", true);
                Monster.animator.SetBool("Attack", false);
                Monster2.animator.SetBool("Move", true);
                Monster2.animator.SetBool("Attack", false);
                break;
        }
    } 
}
