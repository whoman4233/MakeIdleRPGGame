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

    private UpgradeManager _mgr;

    private void Start()
    {
        _mgr = UpgradeManager.Instance;
        if (_mgr == null)
        {
            GameLog.Warn("[UpgradeUI] UpgradeManager 없음", this);
            if (upgradeButton != null) upgradeButton.interactable = false;
            return;
        }
        if (_mgr.upgrades == null || upgradeIndex >= _mgr.upgrades.Length)
        {
            GameLog.Warn($"[UpgradeUI] upgradeIndex {upgradeIndex} 범위 초과", this);
            gameObject.SetActive(false);
            return;
        }

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnClick);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged += RefreshUI;

        // UpgradeManager에서 스탯 변경 알림도 구독 (업그레이드 후 즉시 반영)
        if (PlayerRef.Instance?.Stats != null)
            PlayerRef.Instance.Stats.OnStatsChanged += RefreshUI;

        RefreshUI();
    }

    private void OnDestroy()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= RefreshUI;
        if (PlayerRef.Instance?.Stats != null)
            PlayerRef.Instance.Stats.OnStatsChanged -= RefreshUI;
    }

    private void OnClick()
    {
        if (_mgr == null) return;
        SoundManager.Instance?.PlayButtonClick();
        if (_mgr.TryUpgrade(upgradeIndex))
            RefreshUI();
    }

    private void RefreshUI()
    {
        if (_mgr == null || _mgr.upgrades == null) return;
        if (upgradeIndex >= _mgr.upgrades.Length) return;

        var data = _mgr.upgrades[upgradeIndex];
        if (data == null) return;

        bool isMaxLevel = data.IsMaxLevel;

        if (titleText  != null) titleText.text  = data.upgradeName;
        if (levelText  != null) levelText.text  = isMaxLevel ? "MAX" : "Lv. " + data.level;
        if (costText   != null) costText.text   = isMaxLevel ? "-" : data.GetCurrentCost() + " Data";

        if (upgradeButton != null)
        {
            upgradeButton.interactable = !isMaxLevel
                && CurrencyManager.Instance != null
                && CurrencyManager.Instance.Data >= data.GetCurrentCost();
        }
    }
}
