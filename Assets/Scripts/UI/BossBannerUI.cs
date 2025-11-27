using UnityEngine;
using TMPro;
using System.Collections;

public class BossBannerUI : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI bannerText;
    public string message = "BOSS 등장!";
    public float showDuration = 1.0f;
    public float fadeDuration = 0.5f;

    private Coroutine _routine;

    private void OnEnable()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnPhaseChanged += OnPhaseChanged;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnPhaseChanged -= OnPhaseChanged;
    }

    private void OnPhaseChanged(StagePhase phase)
    {
        if (phase == StagePhase.Boss)
        {
            ShowBanner();
        }
    }

    public void ShowBanner()
    {
        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(BannerRoutine());
    }

    private IEnumerator BannerRoutine()
    {
        if (canvasGroup == null) yield break;

        if (bannerText != null)
            bannerText.text = message;

        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;

        // 잠깐 유지
        yield return new WaitForSeconds(showDuration);

        // 페이드 아웃
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            canvasGroup.alpha = a;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(false);
        _routine = null;
    }
}
