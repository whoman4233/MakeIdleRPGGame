using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraftEnhanceItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI costText;
    public Button upgradeButton;

    private GraftData _graft;

    public void Init(GraftData graft)
    {
        _graft = graft;
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnClick);
        }
        Refresh();
    }

    private void OnEnable()
    {
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged += Refresh;
        if (GraftEnhanceManager.Instance != null) GraftEnhanceManager.Instance.OnChanged += Refresh;
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null) CurrencyManager.Instance.OnCurrencyChanged -= Refresh;
        if (GraftEnhanceManager.Instance != null) GraftEnhanceManager.Instance.OnChanged -= Refresh;
    }

    private void OnClick()
    {
        var mgr = GraftEnhanceManager.Instance;
        if (mgr == null || _graft == null) return;
        if (mgr.Enhance(_graft))
            SoundManager.Instance?.PlayButtonClick();
    }

    private void Refresh()
    {
        var mgr = GraftEnhanceManager.Instance;
        if (_graft == null) return;

        if (titleText != null) titleText.text = _graft.graftName;

        int lv = mgr != null ? mgr.GetLevel(_graft) : 0;
        if (levelText != null) levelText.text = "Lv. " + lv;

        bool max = mgr != null && mgr.IsMaxLevel(_graft);
        if (costText != null) costText.text = max ? "MAX" : (mgr != null ? mgr.GetCost(_graft).ToString("N0") : "-");

        if (upgradeButton != null) upgradeButton.interactable = mgr != null && mgr.CanEnhance(_graft);
    }
}
