using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;



public class SceneName : MonoBehaviour
{
    //씬 파일 이름과 표시될 이름을 매핑
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
    public float delayBeforeStart = 1f;                 //이동 전 대기 시간
    public Color titleColor = Color.white;              //기본값은 흰색

    void Awake()
    {
        if (sceneText == null)//텍스트 컴포넌트 자동 연결, 이미 인스펙터에 연결되어 있는지 확인하고, 없으면 자동으로 찾기
        {
            GameObject textObj = GameObject.Find(TARGET_UI_NAME);

            if (textObj != null)
            {
                sceneText = textObj.GetComponent<TextMeshProUGUI>();
                textRectTransform = textObj.GetComponent<RectTransform>();
            }
            else
            {
                Debug.LogError($"{TARGET_UI_NAME} 이름을 가진 오브젝트가 씬에 없어! 이름을 확인해");
            }
        }

        if (sceneText != null)//RectTransform 가져오기
        {
            textRectTransform = sceneText.GetComponent<RectTransform>();
        }
        else
        {
            Debug.LogError($"{TARGET_UI_NAME}를 찾을 수 없어 빌드 후 UI가 표시되지 않을 수 있어!");
        }
    }

    void Start()
    {
        if (sceneText != null && textRectTransform != null)
        {
            SetupInitialUI();
            StartCoroutine(AnimateSceneName());
        }
    }

    private void SetupInitialUI()
    {
        sceneText.color = titleColor;//텍스트 색상을 인스펙터에서 설정한 색으로 바꿈
        string currentSceneFile = SceneManager.GetActiveScene().name;//현재 활성화된 씬의 파일 이름을 가져옴

        //딕셔너리에서 파일 이름(Key)에 맞는 스테이지 이름(Value)을 찾어,
        //TryGetValue는 값이 없어도 에러를 내지 않고 안전하게 확인해줘
        sceneNames.TryGetValue(currentSceneFile, out string displayName);

        //만약 찾은 이름이 비어있다면 파일 이름을 그대로 쓰고, 있다면 스테이지 이름을 표시함
        sceneText.text = string.IsNullOrEmpty(displayName) ? currentSceneFile : displayName;

        //애니메이션이 시작되기 전, 텍스트의 초기 위치(중앙)와 크기를 설정함
        textRectTransform.anchoredPosition = startPosition;
        textRectTransform.localScale = Vector3.one * startScale;
    }

    IEnumerator AnimateSceneName()
    {
        yield return new WaitForSeconds(delayBeforeStart);//1초(delayBeforeStart 변수) 대기

        float timer = 0f;
        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            float t = timer / moveDuration;

            //Smoother Step 적용 (t * t * (3f - 2f * t)), 시작과 끝을 부드럽게 만드는 가속/감속 로직
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            //Lerp: smoothT(0~1) 비율에 따라 위치와 크기를 실시간 계산
            textRectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, smoothT);//위치 이동
            textRectTransform.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one * endScale, smoothT);//크기 조절

            yield return null;
        }

        //최종 위치에 고정
        textRectTransform.anchoredPosition = endPosition;
        textRectTransform.localScale = Vector3.one * endScale;
    }
}
