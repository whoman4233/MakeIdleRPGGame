using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GachaUI : MonoBehaviour
{
    public GachaManager gachaManager;

    [Header("UI")]
    public TextMeshProUGUI costText;
    public TextMeshProUGUI resultText;   // "¡Û¡Û »ÌÀ½!" ÀÌ·± °Å ¶ç¿ì±â¿ë (¼±ÅÃ)
    public Button gachaButton;

    private void Start()
    {
        if (gachaManager == null)
            gachaManager = GachaManager.Instance;

        if (gachaButton != null)
            gachaButton.onClick.AddListener(OnClickGacha);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged += RefreshInteractable;

        if (gachaManager != null && costText != null)
            costText.text = $"{gachaManager.cost:N0} G";

        RefreshInteractable();
    }

    private void OnDestroy()
    {
        if (gachaButton != null)
            gachaButton.onClick.RemoveListener(OnClickGacha);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= RefreshInteractable;
    }

    private void OnClickGacha()
    {
        if (gachaManager == null) return;

        if (gachaManager.TryGacha(out var reward))
        {
            if (resultText != null && reward != null)
                resultText.text = $"{reward.displayName} È¹µæ!";
        }
        else
        {
            if (resultText != null)
                resultText.text = "»Ì±â ½ÇÆÐ (°ñµå ºÎÁ·?)";
        }

        RefreshInteractable();
    }

    private void RefreshInteractable()
    {
        if (gachaButton == null) return;

        bool can = false;
        if (CurrencyManager.Instance != null && gachaManager != null)
        {
            can = CurrencyManager.Instance.Gold >= gachaManager.cost;
        }

        gachaButton.interactable = can;
    }
}
