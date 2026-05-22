using UnityEngine;
using System.Collections;
using UnityEngine.Networking;//서버 통신을 위한 필수 네임스페이스

[System.Serializable]
public class PlayerGameData//1. 서버에 보낼 데이터 가방 (규격)
{
    public string playerName;
    public int finalScore;
    public int moveLevel;
    public int playTime;
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;//싱글톤: 어디서든 NetworkManager.Instance로 접근

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);//씬이 바뀌어도 파괴되지 않음
    }

    public void SendDataToServer(int score, int speedLevel, int time)//2. 외부(Player 등)에서 호출할 데이터 전송 함수
    {
        //데이터 가방에 값 채우기
        PlayerGameData data = new PlayerGameData();
        data.playerName = "Jaewoong_Player";//나중에 유저 이름 입력 기능 넣으면 연동
        data.finalScore = score;
        data.moveLevel = speedLevel;
        data.playTime = time;

        //3. 데이터 가공 (JSON으로 변환)
        //유니티 내장 기능을 써서 클래스를 텍스트(JSON)로 바꿈
        string jsonData = JsonUtility.ToJson(data);
        Debug.Log($"[서버 전송 준비] 생성된 JSON: {jsonData}");

        //4. 실제 전송 시작 (코루틴 호출)
        StartCoroutine(PostRequest("https://jsonplaceholder.typicode.com/posts", jsonData));
    }

    IEnumerator PostRequest(string url, string json)//5. 서버에 실제로 데이터를 보낼 함수 (비동기 처리)
    {
        //실제 서버가 없어도 테스트 가능한 가짜 주소(JSONPlaceholder) 사용
        using (UnityWebRequest request = UnityWebRequest.PostWwwForm(url, json))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();//서버 응답 올 때까지 대기 (비동기)

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(" 서버 전송 성공! 답장: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError(" 서버 전송 실패: " + request.error);
            }
        }
    }
}
