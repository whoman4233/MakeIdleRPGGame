using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DepthRecordUI : MonoBehaviour
{
    [Header("References")]
    public DepthMilestoneManager manager;
    public RectTransform contentRoot;
    public DepthMilestoneItemUI itemPrefab;

    [Header("Header Texts")]
    public TextMeshProUGUI currentDepthText;
    public TextMeshProUGUI bestDepthText;
    public TextMeshProUGUI claimableText;

    [Header("Buttons")]
    public Button claimAllButton;

    private void Start()
    {
        if (manager == null) manager = DepthMilestoneManager.Instance;

        if (claimAllButton != null)
        {
            claimAllButton.onClick.RemoveAllListeners();
            claimAllButton.onClick.AddListener(OnClaimAll);
        }

        BuildList();
        RefreshHeader();
    }

    private void OnEnable()
    {
        if (manager == null) manager = DepthMilestoneManager.Instance;
        Subscribe(true);

        if (contentRoot != null && contentRoot.childCount == 0)
            BuildList();

        RefreshHeader();
        RefreshItems();
    }

    private void OnDisable()
    {
        Subscribe(false);
    }

    private void Subscribe(bool on)
    {
        if (manager != null)
        {
            if (on) manager.OnMilestonesChanged += OnChanged;
            else    manager.OnMilestonesChanged -= OnChanged;
        }
        if (StageManager.Instance != null)
        {
            if (on) StageManager.Instance.OnStageChanged += OnChanged;
            else    StageManager.Instance.OnStageChanged -= OnChanged;
        }
    }

    private void OnChanged()
    {
        RefreshHeader();
        RefreshItems();
    }

    private void OnClaimAll()
    {
        if (manager == null) return;
        int n = manager.ClaimAll();
        if (n > 0) SoundManager.Instance?.PlayButtonClick();
    }

    private void RefreshHeader()
    {
        var sm = StageManager.Instance;

        if (currentDepthText != null)
            currentDepthText.text = sm != null ? ("현재 심층  " + sm.CurrentDepth) : "현재 심층  -";

        if (bestDepthText != null)
            bestDepthText.text = sm != null ? ("최고 기록  " + sm.BestDepth) : "최고 기록  -";

        int claimable = manager != null ? manager.ClaimableCount : 0;

        if (claimableText != null)
            claimableText.text = claimable > 0 ? ("수령 가능 " + claimable) : "";

        if (claimAllButton != null)
            claimAllButton.interactable = claimable > 0;
    }

    private void BuildList()
    {
        if (manager == null || contentRoot == null || itemPrefab == null)
        {
            GameLog.Warn("[DepthRecordUI] 참조 미연결");
            return;
        }

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        foreach (var m in manager.Milestones)
        {
            if (m == null) continue;
            var item = Instantiate(itemPrefab, contentRoot);
            item.Init(manager, m);
        }
    }

    private void RefreshItems()
    {
        if (contentRoot == null) return;
        var items = contentRoot.GetComponentsInChildren<DepthMilestoneItemUI>(true);
        foreach (var it in items)
            it.Refresh();
    }
}
