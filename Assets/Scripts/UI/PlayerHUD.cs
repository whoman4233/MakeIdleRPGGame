using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats playerStats;
    private HealthSystem healthSystem; // HealthSystem 참조 추가

    [Header("Bars (Filled Image)")]
    public Image hpFill;
    public Image expFill;

    [Header("Texts")]
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI hpText;    // 옵션
    public TextMeshProUGUI expText;   // 옵션
    public TextMeshProUGUI levelText; // 옵션

    [Header("Tween Settings")]
    [Tooltip("HP/EXP 바가 목표 수치로 이동하는 시간(초)")]
    public float tweenDuration = 0.25f;
    public bool useUnscaledTime = false;

    [Header("Debug")]
    public bool enableDebug = true;

    private Coroutine _hpTweenRoutine;
    private Coroutine _expTweenRoutine;

    private void Start()
    {
        // PlayerRef를 통해 Stats와 HealthSystem 캐싱
        if (PlayerRef.Instance != null)
        {
            if (playerStats == null) playerStats = PlayerRef.Instance.Stats;
            healthSystem = PlayerRef.Instance.Health;
        }

        // 스탯 변동 이벤트 구독 (EXP, 레벨업 등)
        if (playerStats != null)
        {
            // 주의: PlayerStats 내부에 OnStatsChanged 이벤트가 정의되어 있어야 합니다.
            // playerStats.OnStatsChanged += RefreshStats; 
            if (enableDebug) Debug.Log("[HUD] PlayerStats 연결 완료", this);
        }

        // 체력 변동 이벤트 구독
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += RefreshStats; 
        }

        // 재화 변동 이벤트 구독
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += RefreshCurrency;
        }

        // 초기 HUD 세팅
        RefreshStats();
        RefreshCurrency();
    }

    private void OnDestroy()
    {
        /* if (playerStats != null)
            playerStats.OnStatsChanged -= RefreshStats;
        */

        if (healthSystem != null)
            healthSystem.OnHealthChanged -= RefreshStats;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= RefreshCurrency;
    }

    // ====== Stats 갱신 ======
    private void RefreshStats()
    {
        // HP 갱신 로직 (HealthSystem + PlayerStats 조합)
        if (hpFill != null && healthSystem != null && playerStats != null)
        {
            float maxHp = Mathf.Max(1f, playerStats.GetStatValue(StatType.MaxHealth));
            float curHp = healthSystem.CurrentHealth; // HealthSystem에서 현재 체력 가져오기
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
        if (goldText != null && CurrencyManager.Instance != null)
        {
            // Gold 대신 Data(관측 데이터) 표시
            goldText.text = CurrencyManager.Instance.Data.ToString("N0");

            if (enableDebug)
                Debug.Log($"[HUD] Data 갱신 - {CurrencyManager.Instance.Data}", this);
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