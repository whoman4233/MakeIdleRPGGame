using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats playerStats;
    private HealthSystem healthSystem;

    [Header("Bars (Filled Image)")]
    public Image hpFill;

    [Header("Texts")]
    public TextMeshProUGUI dataText;
    public TextMeshProUGUI coreText;
    public TextMeshProUGUI hpText;

    [Header("Tween Settings")]
    [Tooltip("HP 바가 목표 수치로 이동하는 시간(초)")]
    public float tweenDuration = 0.25f;
    public bool useUnscaledTime = false;

    [Header("Debug")]
    public bool enableDebug = true;

    private Coroutine _hpTweenRoutine;

    private void Start()
    {
        if (PlayerRef.Instance != null)
        {
            if (playerStats == null) playerStats = PlayerRef.Instance.Stats;
            healthSystem = PlayerRef.Instance.Health;
        }

        if (playerStats != null)
        {
            playerStats.OnStatsChanged += RefreshStats; 
            if (enableDebug) Debug.Log("[HUD] PlayerStats 연결 완료", this);
        }

        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += RefreshStats; 
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += RefreshCurrency;
        }

        RefreshStats();
        RefreshCurrency();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged -= RefreshStats;

        if (healthSystem != null)
            healthSystem.OnHealthChanged -= RefreshStats;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= RefreshCurrency;
    }

    // ====== Stats 갱신 ======
    private void RefreshStats()
    {
        if (hpFill != null && healthSystem != null && playerStats != null)
        {
            float maxHp = Mathf.Max(1f, playerStats.GetStatValue(StatType.MaxHealth));
            float curHp = healthSystem.CurrentHealth; 
            float target = Mathf.Clamp01(curHp / maxHp);

            if (enableDebug)
            {
                Debug.Log($"[HUD] HP 갱신 - curHP={curHp}, maxHP={maxHp}, fillTarget={target}", this);
            }

            if (_hpTweenRoutine != null) StopCoroutine(_hpTweenRoutine);
            _hpTweenRoutine = StartCoroutine(TweenFillAmount(hpFill, target));

            if (hpText != null)
            {
                hpText.text = $"{Mathf.FloorToInt(curHp)}/{Mathf.FloorToInt(maxHp)}";
            }
        }
    }

    // ====== Currency 갱신 ======
    private void RefreshCurrency()
    {
        if (CurrencyManager.Instance != null)
        {
            if (dataText != null)
                dataText.text = CurrencyManager.Instance.Data.ToString("N0");

            if (coreText != null)
                coreText.text = CurrencyManager.Instance.Core.ToString("N0");

            if (enableDebug)
                Debug.Log($"[HUD] Data 갱신 - Data: {CurrencyManager.Instance.Data}, Core: {CurrencyManager.Instance.Core}", this);
        }
    }

    // ====== Tween 코루틴 ======
    private IEnumerator TweenFillAmount(Image image, float target)
    {
        if (image == null) yield break;

        float start = image.fillAmount;
        float time = 0f;

        if (tweenDuration <= 0f)
        {
            image.fillAmount = target;
            yield break;
        }

        while (time < tweenDuration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            time += dt;

            float t = Mathf.Clamp01(time / tweenDuration);
            float value = Mathf.Lerp(start, target, t);
            image.fillAmount = value;

            yield return null;
        }

        image.fillAmount = target;
    }
}