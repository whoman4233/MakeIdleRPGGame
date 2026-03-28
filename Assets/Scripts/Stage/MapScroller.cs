using UnityEngine;

public class MapScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 2f;      // 배경이 지나가는 속도 (적의 이동 속도와 비슷하게 맞추면 좋습니다)
    public float bgWidth = 20f;         // 배경 조각 1개의 가로(X) 길이
    
    [Header("Background Segments")]
    public Transform[] backgrounds;     // 이어 붙일 배경 조각들 (2~3개 추천)

    private void Update()
    {
        // 플레이어가 죽었을 때는 스크롤(전진)을 멈춥니다.
        if (PlayerRef.Instance != null && PlayerRef.Instance.Stats != null)
        {
            if (PlayerRef.Instance.Health.CurrentHealth <= 0) return;
        }

        for (int i = 0; i < backgrounds.Length; i++)
        {
            Transform bg = backgrounds[i];
            
            // 왼쪽으로 이동
            bg.Translate(Vector3.left * (scrollSpeed * Time.deltaTime));

            // 카메라 왼쪽 화면 밖으로 완전히 벗어났다면
            if (bg.position.x <= -bgWidth)
            {
                // 맨 오른쪽 끝으로 순간이동 시켜서 무한 루프 형성
                Vector3 newPos = bg.position;
                newPos.x += bgWidth * backgrounds.Length;
                bg.position = newPos;
            }
        }
    }
}