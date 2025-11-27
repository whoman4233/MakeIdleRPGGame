using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public Button upgradeButton;

    private int _index;
    private UpgradeManager _manager;

    public void Init(UpgradeManager manager, int index)
    {
        _manager = manager;
        _index = index;

        Refresh();

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(OnClick);
    }

    private void OnEnable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged += Refresh;
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= Refresh;
    }

    private void OnClick()
    {
        if (_manager == null) return;

        if (_manager.TryUpgrade(_index))
        {
            Refresh();
        }
    }

    private void Refresh()
    {
        if (_manager == null) return;

        var data = _manager.upgrades[_index];
        titleText.text = data.upgradeName;
        levelText.text = $"Lv. {data.level}";
        costText.text = data.GetCurrentCost().ToString("N0");

        bool canBuy = CurrencyManager.Instance != null &&
                      CurrencyManager.Instance.Gold >= data.GetCurrentCost();

        upgradeButton.interactable = canBuy;
    }
}
