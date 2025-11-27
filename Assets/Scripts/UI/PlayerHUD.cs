using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats playerStats;

    [Header("Bars (Filled Image)")]
    public Image hpFill;
    public Image expFill;

    [Header("Texts")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI hpText;    // 옵션
    public TextMeshProUGUI expText;   // 옵션
    public TextMeshProUGUI levelText; // 옵션

    [Header("Tween Settings")]
    [Tooltip("HP/EXP 바가 목표 값까지 도달하는 시간(초)")]
    public float tweenDuration = 0.25f;
    public bool useUnscaledTime = false;

    [Header("Debug")]
    public bool enableDebug = true;

    private Coroutine _hpTweenRoutine;
    private Coroutine _expTweenRoutine;

    private void Start()
    {
        // PlayerStats 자동 찾기
        if (playerStats == null && PlayerRef.Instance != null)
            playerStats = PlayerRef.Instance.Stats;

        if (playerStats != null)
        {
            playerStats.OnStatsChanged += RefreshStats;
            if (enableDebug)
                Debug.Log("[HUD] PlayerStats 구독 완료", this);
        }
        else
        {
            if (enableDebug)
                Debug.LogWarning("[HUD] PlayerStats가 할당되지 않았습니다.", this);
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += RefreshCurrency;
        }

        // 초기 값 한 번 반영
        RefreshStats();
        RefreshCurrency();
    }

    private void OnDestroy()
    {
        if (playerStats != null)
            playerStats.OnStatsChanged -= RefreshStats;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= RefreshCurrency;
    }

    // ====== Stats 갱신 ======
    private void RefreshStats()
    {
        if (playerStats == null)
            return;

        // HP
        if (hpFill != null)
        {
            float maxHp = Mathf.Max(1f, playerStats.MaxHP); // 0 나누기 방지
            float target = Mathf.Clamp01(playerStats.curHP / maxHp);

            if (enableDebug)
            {
                Debug.Log($"[HUD] HP 갱신 - curHP={playerStats.curHP}, maxHP={maxHp}, fillTarget={target}", this);
            }

            // Tween
            if (_hpTweenRoutine != null)
                StopCoroutine(_hpTweenRoutine);
            _hpTweenRoutine = StartCoroutine(TweenFillAmount(hpFill, target));
        }

        // EXP
        if (expFill != null)
        {
            float required = playerStats.RequiredExp;
            float target;

            if (required > 0f)
                target = Mathf.Clamp01(playerStats.curExp / required);
            else
                target = 1f;

            if (enableDebug)
            {
                Debug.Log($"[HUD] EXP 갱신 - curExp={playerStats.curExp}, required={required}, fillTarget={target}", this);
            }

            if (_expTweenRoutine != null)
                StopCoroutine(_expTweenRoutine);
            _expTweenRoutine = StartCoroutine(TweenFillAmount(expFill, target));
        }

        // 선택: 숫자 텍스트 표시
        if (hpText != null)
        {
            hpText.text = $"{Mathf.FloorToInt(playerStats.curHP)}/{Mathf.FloorToInt(playerStats.MaxHP)}";
        }

        if (expText != null)
        {
            float required = playerStats.RequiredExp;

            if (required > 0f)
                expText.text = $"{Mathf.FloorToInt(playerStats.curExp)}/{Mathf.FloorToInt(required)}";
            else
                expText.text = "MAX";
        }

        if (levelText != null)
        {
            levelText.text = $"Lv.{playerStats.level}";
        }
    }

    // ====== Currency 갱신 ======
    private void RefreshCurrency()
    {
        if (goldText != null && CurrencyManager.Instance != null)
        {
            goldText.text = CurrencyManager.Instance.Gold.ToString("N0");

            if (enableDebug)
                Debug.Log($"[HUD] Gold 갱신 - {CurrencyManager.Instance.Gold}", this);
        }
    }

    // ====== Tween 코루틴 ======
    private IEnumerator TweenFillAmount(Image image, float target)
    {
        if (image == null)
            yield break;

        // Image 설정 확인 로그
        if (enableDebug)
        {
            if (image.type != Image.Type.Filled)
            {
                Debug.LogWarning(
                    $"[HUD] {image.name} Image.type이 Filled가 아닙니다. 현재 값: {image.type}",
                    image
                );
            }
        }

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
