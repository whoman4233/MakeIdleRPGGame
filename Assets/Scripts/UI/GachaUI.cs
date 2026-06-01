using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GachaUI : MonoBehaviour
{
    public ExtractionManager extractionManager;

    [Header("UI")]
    public TextMeshProUGUI costText;
    public TextMeshProUGUI resultText;
    public Button gachaButton;
    public Button adGachaButton; // 광고 시청 → 1회 무료 추출

    private void Start()
    {
        if (extractionManager == null)
            extractionManager = ExtractionManager.Instance;

        if (gachaButton != null)
            gachaButton.onClick.AddListener(OnClickExtract);

        if (adGachaButton != null)
            adGachaButton.onClick.AddListener(OnClickAdExtract);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged += RefreshInteractable;

        if (extractionManager != null)
        {
            extractionManager.OnExtractionSuccess += HandleExtractionSuccess;
            extractionManager.OnExtractionFailed  += HandleExtractionFailed;
            extractionManager.OnCostChanged        += RefreshCostText;
        }

        RefreshCostText();
        RefreshInteractable();
    }

    private void OnDestroy()
    {
        if (gachaButton != null)
            gachaButton.onClick.RemoveListener(OnClickExtract);

        if (adGachaButton != null)
            adGachaButton.onClick.RemoveListener(OnClickAdExtract);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= RefreshInteractable;

        if (extractionManager != null)
        {
            extractionManager.OnExtractionSuccess -= HandleExtractionSuccess;
            extractionManager.OnExtractionFailed  -= HandleExtractionFailed;
            extractionManager.OnCostChanged        -= RefreshCostText;
        }
    }

    private void OnClickAdExtract()
    {
        SoundManager.Instance?.PlayButtonClick();
        AdManager.Instance?.ShowRewardedAd(
            onReward: () =>
            {
                // 광고 시청 완료 → 비용 없이 1회 추출
                extractionManager?.ExtractBasic(free: true);
                SoundManager.Instance?.PlayGacha();
            }
        );
    }

    private void OnClickExtract()
    {
        if (extractionManager == null) return;
        
        // 수정된 메서드 호출
        extractionManager.ExtractBasic();
    }

    private void HandleExtractionSuccess(GraftData reward)
    {
        SoundManager.Instance?.PlayGacha();
        if (resultText != null && reward != null)
            resultText.text = $"[경고] 미확인 표본 추출됨:\n{reward.graftName}";

        RefreshCostText();
        RefreshInteractable();
    }

    private void HandleExtractionFailed()
    {
        SoundManager.Instance?.PlayError();
        if (resultText != null)
            resultText.text = "데이터 부족. 추출 프로토콜 거부됨.";

        RefreshInteractable();
    }

    private void RefreshCostText()
    {
        if (costText == null || extractionManager == null) return;
        costText.text = $"{extractionManager.CurrentBasicCost:N0} Data";
    }

    private void RefreshInteractable()
    {
        if (gachaButton == null) return;

        bool can = CurrencyManager.Instance != null && extractionManager != null
                   && CurrencyManager.Instance.Data >= extractionManager.CurrentBasicCost;

        gachaButton.interactable = can;

        if (adGachaButton != null)
            adGachaButton.interactable = AdManager.Instance != null && AdManager.Instance.IsAdReady();
    }
}