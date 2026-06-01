using System.Collections;
using UnityEngine;

/// <summary>
/// 게임필(타격감) 전용 유틸리티 — 히트스톱 + 카메라 셰이크.
/// 타격/처치/보스등장 등에서 한 줄로 호출한다. 예) GameFeel.Instance.HitStop(0.05f);
/// 시간 정지는 unscaledTime 기반 코루틴으로 복구하므로 일시정지 로직과 충돌하지 않는다.
/// </summary>
public class GameFeel : MonoBehaviour
{
    public static GameFeel Instance { get; private set; }

    [Header("연출 강도")]
    [SerializeField] private float defaultHitStopScale = 0f;   // 정지 정도(0=완전정지)

    private Transform _camTf;
    private Vector3   _camBasePos;
    private Coroutine _hitStopCo;
    private Coroutine _shakeCo;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        var cam = Camera.main;
        if (cam != null) { _camTf = cam.transform; _camBasePos = _camTf.localPosition; }
    }

    // ── 히트스톱 ──────────────────────────────
    /// <summary>지정 시간(실시간 초) 동안 게임 시간을 거의 멈췄다가 복구.</summary>
    public void HitStop(float duration = 0.05f, float timeScale = -1f)
    {
        if (!isActiveAndEnabled) return;
        if (_hitStopCo != null) StopCoroutine(_hitStopCo);
        _hitStopCo = StartCoroutine(HitStopCo(duration, timeScale < 0 ? defaultHitStopScale : timeScale));
    }

    private IEnumerator HitStopCo(float duration, float scale)
    {
        Time.timeScale = scale;
        float t = 0f;
        while (t < duration) { t += Time.unscaledDeltaTime; yield return null; }
        Time.timeScale = 1f;
        _hitStopCo = null;
    }

    // ── 카메라 셰이크 ──────────────────────────
    /// <summary>카메라를 magnitude 크기로 duration(실시간 초) 동안 흔든다.</summary>
    public void Shake(float duration = 0.12f, float magnitude = 0.15f)
    {
        if (_camTf == null || !isActiveAndEnabled) return;
        if (_shakeCo != null) StopCoroutine(_shakeCo);
        _shakeCo = StartCoroutine(ShakeCo(duration, magnitude));
    }

    private IEnumerator ShakeCo(float duration, float magnitude)
    {
        float t = 0f;
        while (t < duration)
        {
            float damper = 1f - (t / duration);          // 점점 약해짐
            float x = (Random.value * 2f - 1f) * magnitude * damper;
            float y = (Random.value * 2f - 1f) * magnitude * damper;
            _camTf.localPosition = _camBasePos + new Vector3(x, y, 0f);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        _camTf.localPosition = _camBasePos;
        _shakeCo = null;
    }

    private void OnDisable()
    {
        if (Mathf.Approximately(Time.timeScale, 0f) == false && _hitStopCo != null)
            Time.timeScale = 1f;
        if (_camTf != null) _camTf.localPosition = _camBasePos;
    }
}
