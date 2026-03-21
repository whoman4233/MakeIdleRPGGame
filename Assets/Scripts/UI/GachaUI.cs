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

    private void Start()
    {
        if (extractionManager == null)
            extractionManager = ExtractionManager.Instance;

        if (gachaButton != null)
            gachaButton.onClick.AddListener(OnClickExtract);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged += RefreshInteractable;

        if (extractionManager != null)
        {
            extractionManager.OnExtractionSuccess += HandleExtractionSuccess;
            extractionManager.OnExtractionFailed += HandleExtractionFailed;
            
            if (costText != null)
                costText.text = $"{extractionManager.basicExtractionCost:N0} Data";
        }

        RefreshInteractable();
    }

    private void OnDestroy()
    {
        if (gachaButton != null)
            gachaButton.onClick.RemoveListener(OnClickExtract);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= RefreshInteractable;

        if (extractionManager != null)
        {
            extractionManager.OnExtractionSuccess -= HandleExtractionSuccess;
            extractionManager.OnExtractionFailed -= HandleExtractionFailed;
        }
    }

    private void OnClickExtract()
    {
        if (extractionManager == null) return;
        
        // 수정된 메서드 호출
        extractionManager.ExtractBasic();
    }

    private void HandleExtractionSuccess(GraftData reward)
    {
        if (resultText != null && reward != null)
        {
            resultText.text = $"[경고] 미확인 표본 추출됨:\n{reward.graftName}"; 
        }
        RefreshInteractable();
    }

    private void HandleExtractionFailed()
    {
        if (resultText != null)
        {
            resultText.text = "데이터 부족. 추출 프로토콜 거부됨.";
        }
        RefreshInteractable();
    }

    private void RefreshInteractable()
    {
        if (gachaButton == null) return;

        bool can = false;
        if (CurrencyManager.Instance != null && extractionManager != null)
        {
            // Gold -> Data 로 변경 적용
            can = CurrencyManager.Instance.Data >= extractionManager.basicExtractionCost;
        }

        gachaButton.interactable = can;
    }
}