using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;//RawImage�� ����ϱ� ���� �ʿ���

public class BackgroundScroller : MonoBehaviour
{
    public float scrollSpeed = 1f;//���ȭ�� �����̴� �ӵ�, �ν����� â���� ��ũ�� �ӵ��� ����
    private RawImage rawImage;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
    }
    void Update()
    {   
        if (rawImage != null)
        {
            
            Rect uvRect = rawImage.uvRect;//Raw Image�� ���� �ִ� �̹����� ���� ������ uvRect��� ������ ����
            uvRect.x += scrollSpeed * Time.deltaTime;//x��(�¿�)���� �̵�,scrollSpeed ������ �ӵ��� ���� �� �����Ӹ���
            rawImage.uvRect = uvRect;//���� ���ο� ��ġ�� Raw Image�� �ٽ� ����
                                     //�� ������ �� �����Ӹ��� �ݺ��ϴϱ� �̹����� ��� �����̴� ��ó�� ���̴� �ž�.
        }
    }
}
