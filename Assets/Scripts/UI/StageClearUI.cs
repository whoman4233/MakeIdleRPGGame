using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 스테이지 클리어 및 엔드리스 루프 진입 시 배너 연출.
/// Canvas 하위 오브젝트에 추가 후 CanvasGroup / Text 연결.
/// StageManager의 OnStageChanged, OnEndlessLoopStarted 이벤트 구독.
/// </summary>
public class StageClearUI : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI subText;

    [Header("연출 설정")]
    [SerializeField] private float holdDuration  = 1.5f;
    [SerializeField] private float fadeDuration  = 0.4f;

    private StageManager _sm;
    private Coroutine    _routine;

    private void Start()
    {
        _sm = StageManager.Instance ?? FindObjectOfType<StageManager>();
        if (_sm != null)
        {
            _sm.OnStageChanged       += OnStageChanged;
            _sm.OnEndlessLoopStarted += OnEndlessLoop;
        }
        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (_sm != null)
        {
            _sm.OnStageChanged       -= OnStageChanged;
            _sm.OnEndlessLoopStarted -= OnEndlessLoop;
        }
    }

    // 일반 스테이지 클리어 → 다음 스테이지 이름 표시
    private void OnStageChanged()
    {
        if (_sm == null) return;
        var stage = _sm.CurrentStage;
        string header = "STAGE CLEAR";
        string sub    = stage != null ? stage.displayName + " 진입" : "";
        Show(header, sub);
    }

    // 엔드리스 루프 진입 → 루프 횟수 표시
    private void OnEndlessLoop(int loopCount)
    {
        Show("ENDLESS LOOP x" + loopCount, "난이도 상승 — 생존하라");
    }

    public void Show(string header, string sub)
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine(header, sub));
    }

    private IEnumerator ShowRoutine(string header, string sub)
    {
        if (canvasGroup == null) yield break;
        if (headerText != null) headerText.text = header;
        if (subText    != null) subText.text    = sub;

        // 페이드 인
        SetVisible(true);
        float t = 0f;
        bool keepRunning = true;
        while (keepRunning)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
            if (t >= fadeDuration) keepRunning = false;
            else yield return null;
        }
        canvasGroup.alpha = 1f;

        // 유지
        yield return new WaitForSeconds(holdDuration);

        // 페이드 아웃
        t = 0f; keepRunning = true;
        while (keepRunning)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            if (t >= fadeDuration) keepRunning = false;
            else yield return null;
        }
        SetVisible(false);
        _routine = null;
    }

    private void SetVisible(bool show)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha = show ? 1f : 0f;
        canvasGroup.gameObject.SetActive(show);
    }
}
