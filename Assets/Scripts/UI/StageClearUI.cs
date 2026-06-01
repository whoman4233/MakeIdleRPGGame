using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

    [Header("광고 보상 — 데이터 2배")]
    [SerializeField] private Button doubleRewardButton;   // 인스펙터에서 연결
    [SerializeField] private long   clearBonusData = 100; // 스테이지 클리어 기본 보너스 데이터

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
        if (doubleRewardButton != null)
            doubleRewardButton.onClick.AddListener(OnClickDoubleReward);

        SetVisible(false);
    }

    // 스테이지 클리어 시 광고 시청 → 보너스 데이터 2배 지급
    private void OnClickDoubleReward()
    {
        SoundManager.Instance?.PlayButtonClick();
        var ads = AdManager.Instance;
        if (ads == null || !ads.IsAdReady()) return;

        ads.ShowRewardedAd(onReward: () =>
        {
            // 현재 스테이지 배율을 반영해 보너스 지급 (2배 = 보너스를 그대로 한 번 더)
            CurrencyManager.Instance?.AddData(clearBonusData * 2);
            if (doubleRewardButton != null) doubleRewardButton.interactable = false;
        });
    }

    private void OnDestroy()
    {
        if (doubleRewardButton != null)
            doubleRewardButton.onClick.RemoveListener(OnClickDoubleReward);

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

        if (doubleRewardButton != null)
        {
            bool adReady = AdManager.Instance != null && AdManager.Instance.IsAdReady();
            doubleRewardButton.gameObject.SetActive(show && adReady);
            doubleRewardButton.interactable = show && adReady;
        }
    }
}
