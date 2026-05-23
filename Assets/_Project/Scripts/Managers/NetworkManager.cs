using System.Collections;
using UnityEngine;
using UnityEngine.Networking;//서버 통신(HTTP)을 위한 필수 네임스페이스

[System.Serializable]
public class PlayerGameData//1. 서버 전송용 데이터 구조 정의(DTO: Data Transfer Object)
{
    public string playerName;
    public int finalScore;
    public int moveLevel;
    public int playTime;

    //◀만약? 실무 작업: 추가 기획 요구사항에 맞춰 변수 딱 한 줄만 추가!
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;//싱글톤: 어디서든 NetworkManager.Instance로 접근

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);// 씬 전환 시 오브젝트 파괴를 막아 연속적인 네트워크 팝업이나 대기 상태를 유지함
        }
        else
        {   //[방어 코드] 씬 재로드 등으로 인한 중복 생성 발생 시 가짜 복사본을 즉시 삭제해서 메모리 누수 방지
            Debug.LogWarning($"[방어 코드!] 중복된 NetworkManager 발견! 가짜 복사본을 파괴할게!");
            Destroy(gameObject);
            return;//삭제 예약 후 아래 코드가 실행되지 않도록 즉시 함수 종료
        }
    }

    //2. 외부 인게임 로직(Player 등)에서 최종 데이터를 넘겨받아 전송을 시작하는 진입점
    public void SendDataToServer(int score, int speedLevel, int time)
    {
        //데이터 객체 생성 및 인게임 변수 맵핑
        PlayerGameData data = new PlayerGameData();
        data.playerName = "Jaewoong_Player";//차후 로그인 시스템 구축 시 계정 ID와 연동 가능
        data.finalScore = score;
        data.moveLevel = speedLevel;
        data.playTime = time;

        //3. 직렬화(Serialization): C# 객체 데이터를 웹 표준 포맷인 JSON 텍스트 문자열로 변환
        //유니티 내장 기능을 써서 클래스를 텍스트(JSON)로 바꿈
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"[서버 전송 준비] 생성된 JSON: {jsonData}");

        //4. 비동기 웹 요청을 처리하는 코루틴 호출 (가짜 서버 주소 활용)
        StartCoroutine(PostRequest("https://jsonplaceholder.typicode.com/posts", jsonData));
    }

    IEnumerator PostRequest(string url, string json)//5. HTTP POST 방식으로 서버에 순수 문자열 데이터를 송신하는 비동기 통신 함수
    {
        //[테스트 안내] 상용 서버가 없어도 HTTP 응답 테스트가 가능한 가짜 온라인 서버(JSONPlaceholder) 주소 활용
        //using 문을 사용해서 통신이 끝나거나 에러가 나면 request 객체의 메모리(네트워크 자원)를 자동으로 해제(Dispose)함
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            //순수 JSON 텍스트 전송을 위해 데이터를 바이트 배열로 변환 후 업로드 핸들러에 장착
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            //헤더 설정: 본문 데이터가 HTML 폼 형식이 아닌 '순수 JSON 규격'임을 서버에 명시
            request.SetRequestHeader("Content-Type", "application/json");

            //[실무 필수 설정!, 네트워크 예외 대책] 5초 동안 서버 응답이 없으면 무한 대기를 막기 위해 통신을 강제 타임아웃(종료)시킴
            request.timeout = 5;

            yield return request.SendWebRequest();//서버 응답 올 때까지 대기 (비동기, 메인 스레드 멈춤 방지))


            //6. HTTP 통신 결과 검증 및 응답 처리
            if (request.result == UnityWebRequest.Result.Success)
            {   //가짜 서버 특성: 데이터가 실제 DB에 저장되지는 않지만, 수신 성공의 의미로 보낸 데이터를 규격화하여 다시 반환해줌
                Debug.Log($"<color=green><b>[서버 전송 성공]</b></color> 서버 수신 완료 답장: {request.downloadHandler.text}");
            }
            else//각 에러 상황을 세분화해서 클라이언트 로그에 기록(디버깅 용이성 확보)
            {
                if (request.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError("[네트워크 에러] 유저의 인터넷 연결이 끊겼어!");
                }
                else if (request.responseCode == 404)
                {
                    Debug.LogError("[서버 에러] 요청한 서버 주소(URL)를 찾을 수 없어!");
                }
                else
                {
                    Debug.LogError($"[기타 에러] 에러 내용: {request.error} | 응답코드: {request.responseCode}");
                }
            }
        }
    }
}
