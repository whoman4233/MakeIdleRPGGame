using UnityEngine;
using TMPro;
using System.Collections;

public class BossBannerUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI bannerText;
    public string message = "BOSS 출현!";
    public float showDuration = 1.0f;
    public float fadeDuration = 0.5f;

    private StageManager _sm;
    private Coroutine    _routine;

    private void Start()
    {
        _sm = StageManager.Instance ?? FindObjectOfType<StageManager>();
        if (_sm != null) _sm.OnPhaseChanged += OnPhaseChanged;
        else GameLog.Warn("[BossBannerUI] StageManager 없음");
        if (canvasGroup != null) { canvasGroup.alpha = 0f; canvasGroup.gameObject.SetActive(false); }
    }

    private void OnDestroy() { if (_sm != null) _sm.OnPhaseChanged -= OnPhaseChanged; }

    private void OnPhaseChanged(StagePhase phase) { if (phase == StagePhase.Boss) ShowBanner(); }

    public void ShowBanner() { if (_routine != null) StopCoroutine(_routine); _routine = StartCoroutine(BannerRoutine()); }

    private System.Collections.IEnumerator BannerRoutine()
    {
        if (canvasGroup == null) yield break;
        if (bannerText != null) bannerText.text = message;
        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(showDuration);
        float t = 0f;
        while (t < fadeDuration) { t += Time.deltaTime; canvasGroup.alpha = Mathf.Lerp(1f,0f,t/fadeDuration); yield return null; }
        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false);
        _routine = null;
    }
}
