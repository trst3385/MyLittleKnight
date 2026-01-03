using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//씬 전환을 위해 추가


public class NextStagePortal : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    public string nextGameScene;//씬의 이름을 적어!(씬마다 다른 씬 이름을 적자)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))//플레이어 태그를 가진 오브젝트와 닿았을 때만 작동
        {
            Debug.Log($"{nextGameScene}으로 이동!");//씬 전환
            SceneManager.LoadScene(nextGameScene);
        }
        else Debug.Log("아직 미션을 완료하지 못했어!");
    }
}
