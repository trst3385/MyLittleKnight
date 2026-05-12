using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;//SceneManager를 쓰기 위해 선언
using System.Collections;
using System.Collections.Generic;



public class SceneName : MonoBehaviour
{
    //5.12 씬이 사실상 3개 뿐이고 내가 완전히 이해하지 못할 딕셔너리를 사용한것보단 조금 번거로워도 확실하게 아는 방식으로 바꿨어
    ////씬 파일 이름과 표시될 이름을 매핑
    //private Dictionary<string, string> sceneNames = new Dictionary<string, string>()
    //{
    //{ "GameScene1", "Stage 1" },
    //{ "GameScene2", "Stage 2" },
    //{ "GameScene3", "Stage 3 Challenge" },
    //{ "MainMenuScene", "메인 메뉴" }
    //};

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
        sceneText.color = titleColor;
        string currentSceneFile = SceneManager.GetActiveScene().name;

        //5.12 딕셔너리 대신 switch문으로 대체.
        //여기서 직접 게임씬 이름과 스테이지에서 보일 이름을 적어야해!
        switch (currentSceneFile)
        {   //여기서 일일히 씬에서 보일 텍스트를 적어야 하지만 대신, 내가 확실하게 알 수 있는 방식이야,
            //지금 규모에는 이 도구(switch)가 더 적당해! 라고 내가 판단해서 수정한 거니까
            case "GameScene1":
                sceneText.text = "Stage 1";
                break;
            case "GameScene2":
                sceneText.text = "Stage 2";
                break;
            case "GameScene3":
                sceneText.text = "Stage 3 Challenge";
                break;
            case "MainMenuScene"://메인 메뉴도 일단은 추가
                sceneText.text = "메인 메뉴";
                break;
            default:
                Debug.LogError($"[SceneName 에러] '{currentSceneFile}' 이름 설정 누락!");
                sceneText.text = "<color=red>ERR: MISSING NAME</color>";
                return;//텍스트 위치/크기가 변경되기 전에 여기서 멈춰
        }

        //성공했을 때만 실행되는 초기 위치/크기 세팅
        textRectTransform.anchoredPosition = startPosition;
        textRectTransform.localScale = Vector3.one * startScale;

        //-------------------------------------------------//

        //5.12 씬이 사실상 3개 뿐이고 내가 완전히 이해하지 못할 딕셔너리를 사용한것보단 조금 번거로워도 확실하게 아는 방식으로 바꿨어
        //sceneText.color = titleColor;//텍스트 색상을 인스펙터에서 설정한 색으로 바꿈
        //string currentSceneFile = SceneManager.GetActiveScene().name;//현재 활성화된 씬의 파일 이름을 가져옴

        ////TryGetValue의 결과(bool)를 직접 체크해.(TryGetValue는 성공하면 true, 실패하면 false 반환)
        ////딕셔너리에서 파일 이름(Key)에 맞는 스테이지 이름(Value)을 찾아서 UI에 스테이지 이름을 보내
        ////있으면 displayName지역변수에 담아(out)
        //if (sceneNames.TryGetValue(currentSceneFile, out string displayName))
        //{
        //    sceneText.text = displayName;//성공: 딕셔너리에 있는 이름을 할당
        //}
        //else
        //{
        //    //실패: 빨간색 에러 로그를 띄우고 함수를 즉시 종료(return)
        //    Debug.LogError($"[SceneName 에러!] '{currentSceneFile}' 씬이 딕셔너리에 등록되지 않았어!");
        //    sceneText.text = "<color=red>ERR: MISSING NAME</color>";//화면에도 에러임을 표시 (개발 중에 바로 알 수 있게)

        //    return;//틀리면 else에서 함수가 끝나기 때문에 아래 위치/크기 설정 코드는 실행 안 됨!
        //}
        ////성공했을 때만 실행되는 초기 위치/크기 세팅
        //textRectTransform.anchoredPosition = startPosition;
        //textRectTransform.localScale = Vector3.one * startScale;

        //-------------------------------------------------//
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
