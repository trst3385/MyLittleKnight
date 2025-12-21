using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//씬 전환을 위해 추가


public class NextStagePortal : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    public string nextGameScene;//씬의 이름을 적어!(씬마다 다른 씬 이름을 적자)

    [Header("포탈 연출 설정")]
    public List<SpriteRenderer> runes;//빛날 룬들(상하좌우)을 인스펙터에서 넣어줘
    public float lerpSpeed = 5f;      //빛이 밝아지는 속도

    private Color targetColor;
    private Color curColor;

    private void Awake()
    {

        if (runes != null && runes.Count > 0)//시작할 때는 룬의 색 없음
        {
            targetColor = runes[0].color;
            targetColor.a = 0f;
            curColor = targetColor;

            foreach (var r in runes) r.color = curColor;//모든 룬의 초기 알파값을 0으로
        }
    }
    private void Update()
    {
        curColor = Color.Lerp(curColor, targetColor, lerpSpeed * Time.deltaTime);//curColor를 targetColor로 서서히 변경 (부드러운 연출)
        foreach (var r in runes) r.color = curColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))//플레이어 태그를 가진 오브젝트와 닿았을 때만 작동
        {
            targetColor.a = 1.0f;//룬의 목표 투명도를 1(불투명)로 변경하여 빛나게 함

            //씬 전환
            Debug.Log($"{nextGameScene}으로 이동합니다!");
            SceneManager.LoadScene(nextGameScene);
        }
    }

    private void OnTriggerExit2D(Collider2D other)//포탈 밖으로 나가면 룬 색이 꺼짐
    {
        if (other.CompareTag("Player")) targetColor.a = 0.0f;//나가면 불 끄기
    }
}
