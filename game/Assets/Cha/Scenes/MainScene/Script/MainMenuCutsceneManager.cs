using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class MainMenuCutsceneManager : MonoBehaviour
{
    public enum CutsceneState//�� �ƾ��� �� ���¸� �����ϴ� enum
    {
        ChasePlayer, //1. ���Ͱ� �÷��̾ �i�ư�
        PlayerAttack,//2. �÷��̾ �� ���� ����� �ϸ� ������ ���͸� �i�ư�
        ChasePlayer2//3. ���� ������ ���Ͱ� �÷��̾ �i�ư�
    }

    public CutsceneState CurrentState;//���� �ƾ��� ����

    [Header("������Ʈ ����")]
    public MainMenuPlayer Player;
    public MainMenuEnemy Monster;
    public MainMenuEnemy Monster2;
    public MainMenuEnemy EliteMonster;

    [Header("�ӵ� ����")]
    //�ƾ� ��ȯ�� �ʿ��� ������
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

    [Header("�ƾ� ��ȯ ��")]
    public float ChangeStateX = 20f;//�ƾ� ���¸� �ٲ� X ��ǥ (ȭ�� ������ ��) �ν����Ϳ��� ������!
    public float ReturnPositionX = -20f;//ĳ���Ͱ� ���ƿ� X ��ǥ (ȭ�� ���� ��)�ν����Ϳ��� ������!
    //ChangeStateX�� ������ �������� ������ ������ x��
    //ReturnPositionX�� �ݴ�� ���� �������� ������ ������ x��

    void Start()
    {
        //���� ���� �� ù ��° ���·� �����ϰ� �ƾ��� ����
        CurrentState = CutsceneState.ChasePlayer;
        SetState();
    }

    void Update()
    {
        //�� �����Ӹ��� ���� ���¸� üũ�ϰ� ������ ����
        switch (CurrentState)
        {
            case CutsceneState.ChasePlayer://���� �ڿ� �ִ� ����2�� x���� �������� x���� �����ϸ� �ƾ� ��ȯ(����->������)
                if (Monster2.transform.position.x > ChangeStateX)
                {
                    CurrentState = CutsceneState.PlayerAttack;
                    SetState();
                }
                break;

            case CutsceneState.PlayerAttack://���� �ڿ� �ִ� Player�� x���� �������� x���� �����ϸ� �ƾ� ��ȯ(������<-����)
                if (Player.transform.position.x < ReturnPositionX)
                {
                    CurrentState = CutsceneState.ChasePlayer2;
                    SetState();
                }
                break;

            case CutsceneState.ChasePlayer2://���� �ڿ� �ִ� Monster2�� x���� �������� x���� �����ϸ� �ƾ� ��ȯ(����->������)
                if (Monster2.transform.position.x > ChangeStateX)
                {
                    CurrentState = CutsceneState.ChasePlayer;
                    SetState();
                }
                break;
        }
    }

    void SetState()//���°� �ٲ� ��, �� ĳ���Ϳ� ������ �ӵ��� �ִϸ��̼��� ����
    {
        switch (CurrentState)
        {
            case CutsceneState.ChasePlayer:

                Monster.gameObject.SetActive(true);
                Monster2.gameObject.SetActive(true);
                EliteMonster.gameObject.SetActive(false);//EliteMonster�� ���� ��� ���ϴ� ��Ȱ��ȭ

                //�� �ڵ�� �÷��̾ ȭ�� ���� ��(returnPositionX)���� �̵���Ű�� �����̾�.
                //���Ϳ��� �ѱ�� ù ��° �ƾ��� ���۵� �� �÷��̾ ȭ�� �ۿ��� �����Ű�� �� �����.
                Player.transform.position = new Vector3(ReturnPositionX, Player.transform.position.y, Player.transform.position.z);
                 
                //�� �ڵ�� ���͸� �÷��̾�� -3��ŭ �� ���ʿ� ��ġ��Ű�� �����̾�.
                //�̷��� �ϸ� �÷��̾�� ���� ���̿� ������ ���ܼ�, ���Ͱ� �÷��̾ �Ѵ� ��ó�� ���̰� ��.
                Monster.transform.position = new Vector3(ReturnPositionX - 3, Monster.transform.position.y, Monster.transform.position.z);
                Monster2.transform.position = new Vector3(ReturnPositionX - 6, Monster2.transform.position.y, Monster2.transform.position.z); // <-- Monster2�� �� �ڿ� ��ġ����


                //���� ����������
                Player.direction = 1;
                Monster.direction = 1;
                Monster2.direction = 1;
                
                //�÷��̾�� ������ �ӵ� ����
                Player.MoveSpeed = Cutscene1pSpeed;
                Monster.MoveSpeed = Cutscene1mSpeed;
                Monster2.MoveSpeed = Cutscene1m1Speed;
                //�ִϸ��̼� ����
                Player.animator.SetBool("Move", true);
                Player.animator.SetBool("Attack", false);

                Monster.animator.SetBool("Move", true);
                Monster.animator.SetBool("Attack", true);
                Monster2.animator.SetBool("Move", true);
                Monster2.animator.SetBool("Attack", true);
                break;

            case CutsceneState.PlayerAttack:

                Monster.gameObject.SetActive(true);//�Ϲ� ���� Ȱ��ȭ
                Monster2.gameObject.SetActive(true);
                EliteMonster.gameObject.SetActive(false);//EliteMonster�� ���� ��� ���ϴ� ��Ȱ��ȭ

                //�� �ڵ�� 1, 3���� �ݴ�� �÷��̾ ȭ�� ���� ��(returnPositionX)���� �̵���Ű�� �����̾�
                //���Ϳ��� �ѱ�� ù ��° �ƾ��� ���۵� �� �÷��̾ ȭ�� �ۿ��� �����Ű�� �� �����
                Player.transform.position = new Vector3(ChangeStateX + 8, Player.transform.position.y, Player.transform.position.z);

                //�� �ڵ�� ���͸� �÷��̾�� -3��ŭ �� ���ʿ� ��ġ��Ű�� �����̾�
                //�̷��� �ϸ� �÷��̾�� ���� ���̿� ������ ���ܼ�, ���Ͱ� �÷��̾ �Ѵ� ��ó�� ���̰� ��.
                Monster.transform.position = new Vector3(ChangeStateX + 3, Monster.transform.position.y, Monster.transform.position.z);
                Monster2.transform.position = new Vector3(ChangeStateX + 6, Monster2.transform.position.y, Monster2.transform.position.z);

                //�÷��̾ ���͸� �Ѿ� �������� �̵�
                Player.direction = -1;
                Monster.direction = -1;
                Monster2.direction = -1;

                //�ӵ�
                Player.MoveSpeed = Cutscene2pSpeed;
                Monster.MoveSpeed = Cutscene2mSpeed;
                Monster2.MoveSpeed = Cutscene2m1Speed;

                //�ִϸ��̼� ����
                Player.animator.SetBool("Move", true);
                Player.animator.SetBool("Attack", true);//���� �ִϸ��̼� ����
                
                //���ʹ� ����ġ�� ����� ��� ����
                Monster.animator.SetBool("Move", true);
                Monster.animator.SetBool("Attack", false);
                Monster2.animator.SetBool("Move", true);
                Monster2.animator.SetBool("Attack", false);
                break;

            case CutsceneState.ChasePlayer2:

                
                Monster.gameObject.SetActive(true);
                Monster2.gameObject.SetActive(true);
                EliteMonster.gameObject.SetActive(true);//���⼭ Elite ���͸� ����ϴ� Ȱ��ȭ

                //�� �ڵ�� �÷��̾ ȭ�� ������ ��(changeStateX)���� �̵���Ű�� �����̾�.
                //���͸� �Ѵ� �� ��° �ƾ��� ���۵� �� �÷��̾ ȭ�� �ۿ��� �����Ű�� �� �����.
                Player.transform.position = new Vector3(ReturnPositionX, Player.transform.position.y, Player.transform.position.z);

                //�� �ڵ�� ���͸� �÷��̾�� 3��ŭ �� ���ʿ� ��ġ����.
                //�̷��� �ϸ� �÷��̾�� ���� ���̿� ������ ���ܼ�, �÷��̾ ���͸� �Ѵ� ��ó�� ���̰� ��.
                EliteMonster.transform.position = new Vector3(ReturnPositionX - 4, EliteMonster.transform.position.y, EliteMonster.transform.position.z);
                Monster.transform.position = new Vector3(ReturnPositionX - 6, Monster.transform.position.y, Monster.transform.position.z);
                Monster2.transform.position = new Vector3(ReturnPositionX - 8, Monster2.transform.position.y, Monster2.transform.position.z);

                //���� ����������
                Player.direction = 1;
                EliteMonster.direction = 1;
                Monster.direction = 1;
                Monster2.direction = 1;

                //�ӵ��� �ִϸ��̼� ����
                EliteMonster.MoveSpeed = Cutscene3emSpeed;
                Player.MoveSpeed = Cutscene3pSpeed;
                Monster.MoveSpeed = Cutscene3mSpeed;
                Monster2.MoveSpeed = Cutscene3m1Speed;

                //�� �̵�, ���� ���ϸ��̼�
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
