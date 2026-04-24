using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;



public class SceneName : MonoBehaviour
{
    private Dictionary<string, string> sceneNames = new Dictionary<string, string>()
    {
    { "GameScene1", "Stage 1" },
    { "GameScene2", "Stage 2" },
    { "GameScene3", "Stage 3 Challenge" },
    { "MainMenuScene", "메인 메뉴" }
    };

    private const string TARGET_UI_NAME = "SceneNameText";//관리할 UI 이름을 딱 한 곳에서 정의

    [Tooltip("코드 내 자동으로 연결 상태")]
    [SerializeField] private TextMeshProUGUI sceneText;//연결할 UI
    private RectTransform textRectTransform;

    [Header("이름 UI 설정")]
    public Vector2 startPosition = Vector2.zero;        //화면 중앙
    public Vector2 endPosition = new Vector2(-750, 450);//구석 위치 (해상도에 따라 조절)
    public float startScale = 2.0f;                     //처음 크기
    public float endScale = 1.0f;                       //나중 크기
    public float moveDuration = 1f;                     //이동하는 시간
    public Color titleColor = Color.white;              //기본값은 흰색

    void Awake()
    {
        if (sceneText == null)//이미 인스펙터에 연결되어 있는지 확인하고, 없으면 자동으로 찾기
        {
            GameObject textObj = GameObject.Find(TARGET_UI_NAME);

            if (textObj != null)
            {
                sceneText = textObj.GetComponent<TextMeshProUGUI>();
                textRectTransform = textObj.GetComponent<RectTransform>();
                Debug.Log($"<color=green>{TARGET_UI_NAME}를 자동으로 찾아서 연결했어!</color>");
            }
            else
            {
                Debug.LogError($"{TARGET_UI_NAME} 이름을 가진 오브젝트가 씬에 없어! 이름을 확인해");
            }
        }
        else//인스펙터에 미리 드래그해뒀다면 바로 RectTransform 가져오기
        {
            textRectTransform = sceneText.GetComponent<RectTransform>();
        }
    }

    void Start()
    {
        if (sceneText != null)
        {
            sceneText.color = titleColor;//인스펙터에 설정된 텍스트 색상

            //1. 현재 실행 중인 씬의 파일 이름을 가져옴 (예: "GameScene1")
            string currentSceneFile = SceneManager.GetActiveScene().name;

            //2. 딕셔너리에 그 이름이 등록되어 있는지 확인
            if (sceneNames.ContainsKey(currentSceneFile))
            {
                //등록되어 있다면 그 이름으로 출력
                sceneText.text = sceneNames[currentSceneFile];
            }
            else
            {
                //만약 딕셔너리에 없다면 그냥 씬 이름을 출력 (안전장치)
                sceneText.text = currentSceneFile;
            }

            StartCoroutine(AnimateSceneName());
        }
    }
    IEnumerator AnimateSceneName()
    {
        float timer = 0f;

        //초기 세팅
        textRectTransform.anchoredPosition = startPosition;
        textRectTransform.localScale = Vector3.one * startScale;

        yield return new WaitForSeconds(1f);//1초 대기

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = timer / moveDuration;
            t = t * t * (3f - 2f * t);//부드러운 움직임

            textRectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
            textRectTransform.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one * endScale, t);

            yield return null;
        }

        //최종 위치 고정
        textRectTransform.anchoredPosition = endPosition;
        textRectTransform.localScale = Vector3.one * endScale;
    }
}
