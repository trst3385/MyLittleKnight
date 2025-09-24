using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//RawImage를 사용하기 위해 필요해

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 1f;//배경화면 움직이는 속도, 인스펙터 창에서 스크롤 속도를 조절
    private RawImage rawImage;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
    }
    void Update()
    {   
        if (rawImage != null)
        {
            
            Rect uvRect = rawImage.uvRect;//Raw Image가 보고 있는 이미지의 영역 정보를 uvRect라는 변수에 저장
            uvRect.x += scrollSpeed * Time.deltaTime;//x축(좌우)으로 이동,scrollSpeed 변수의 속도에 따라 매 프레임마다
            rawImage.uvRect = uvRect;//계산된 새로운 위치를 Raw Image에 다시 적용
                                     //이 과정을 매 프레임마다 반복하니까 이미지가 계속 움직이는 것처럼 보이는 거야.
        }
    }
}
