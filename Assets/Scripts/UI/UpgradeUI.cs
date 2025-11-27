using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeUI : MonoBehaviour
{
    public int upgradeIndex;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI costText;
    public TextMeshProUGUI levelText;
    public Button upgradeButton;

    private UpgradeManager mgr;

    private void Start()
    {
        mgr = UpgradeManager.Instance;

        upgradeButton.onClick.AddListener(OnClick);

        RefreshUI();
        CurrencyManager.Instance.OnCurrencyChanged += RefreshUI;
    }

    private void OnDestroy()
    {
        CurrencyManager.Instance.OnCurrencyChanged -= RefreshUI;
    }

    private void OnClick()
    {
        if (mgr.TryUpgrade(upgradeIndex))
            RefreshUI();
    }

    private void RefreshUI()
    {
        var data = mgr.upgrades[upgradeIndex];

        titleText.text = data.upgradeName;
        levelText.text = $"Lv. {data.level}";
        costText.text = $"{data.GetCurrentCost()} G";

        upgradeButton.interactable = (CurrencyManager.Instance.Gold >= data.GetCurrentCost());
    }
}
