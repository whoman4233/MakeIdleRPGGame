using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera _mainCam;

    private void Start()
    {
        _mainCam = Camera.main;
    }

    private void LateUpdate()
    {
        if (_mainCam == null) return;

        // 카메라가 바라보는 방향과 완벽히 똑같은 방향을 보게 만듭니다. (스프라이트 찌그러짐 방지)
        transform.forward = _mainCam.transform.forward;
        
        // 만약 좌우로만 카메라를 쳐다보고, 위아래로는 꼿꼿하게 서있게 하고 싶다면 아래 주석을 풀고 위 코드를 지우세요.
        // Vector3 camForward = _mainCam.transform.forward;
        // camForward.y = 0f; 
        // transform.forward = camForward;
    }
}