using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats playerStats;
    private HealthSystem healthSystem;

    [Header("Bars")]
    public Image hpFill;

    [Header("Texts")]
    public TextMeshProUGUI dataText;
    public TextMeshProUGUI coreText;
    public TextMeshProUGUI hpText;

    [Header("Tween")]
    public float tweenDuration  = 0.25f;
    public bool  useUnscaledTime = false;

    private Coroutine _hpTweenRoutine;

    private void Start()
    {
        if (PlayerRef.Instance != null)
        {
            if (playerStats == null) playerStats = PlayerRef.Instance.Stats;
            healthSystem = PlayerRef.Instance.Health;
        }

        if (playerStats  != null) playerStats.OnStatsChanged   += RefreshStats;
        if (healthSystem != null) healthSystem.OnHealthChanged  += RefreshStats;
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged += RefreshCurrency;

        RefreshStats();
        RefreshCurrency();
    }

    private void OnDestroy()
    {
        if (playerStats  != null) playerStats.OnStatsChanged   -= RefreshStats;
        if (healthSystem != null) healthSystem.OnHealthChanged  -= RefreshStats;
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= RefreshCurrency;
    }

    private void RefreshStats()
    {
        if (hpFill == null || healthSystem == null || playerStats == null) return;
        float maxHp  = Mathf.Max(1f, playerStats.GetStatValue(StatType.MaxHealth));
        float curHp  = healthSystem.CurrentHealth;
        float target = Mathf.Clamp01(curHp / maxHp);

        if (_hpTweenRoutine != null) StopCoroutine(_hpTweenRoutine);
        _hpTweenRoutine = StartCoroutine(TweenFillAmount(hpFill, target));

        if (hpText != null)
            hpText.text = Mathf.FloorToInt(curHp) + "/" + Mathf.FloorToInt(maxHp);
    }

    private void RefreshCurrency()
    {
        if (CurrencyManager.Instance == null) return;
        if (dataText != null) dataText.text = CurrencyManager.Instance.Data.ToString("N0");
        if (coreText != null) coreText.text = CurrencyManager.Instance.Core.ToString("N0");
    }

    private IEnumerator TweenFillAmount(Image image, float target)
    {
        if (image == null) yield break;
        float start = image.fillAmount;
        float time  = 0f;
        if (tweenDuration <= 0f) { image.fillAmount = target; yield break; }
        bool keepRunning = true;
        while (keepRunning)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            time += dt;
            image.fillAmount = Mathf.Lerp(start, target, Mathf.Clamp01(time / tweenDuration));
            if (time >= tweenDuration) keepRunning = false;
            else yield return null;
        }
        image.fillAmount = target;
    }
}
