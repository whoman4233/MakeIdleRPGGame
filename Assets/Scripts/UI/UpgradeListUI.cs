using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeListUI : MonoBehaviour
{
    public UpgradeManager upgradeManager;
    public RectTransform contentRoot;
    public UpgradeItemUI itemPrefab;

    private float _itemHeight = 120f;
    private float _spacing = 5f;
    private float _padding = 20f;

    private void Start()
    {
        if (upgradeManager == null)
            upgradeManager = UpgradeManager.Instance;
        BuildList();
    }

    private void OnEnable()
    {
        if (upgradeManager == null)
            upgradeManager = UpgradeManager.Instance;
        if (contentRoot != null && contentRoot.childCount == 0)
            BuildList();
        else
            ResetScroll();
    }

    private void ResetScroll()
    {
        var sr = GetComponentInChildren<ScrollRect>();
        if (sr != null) sr.normalizedPosition = new Vector2(0f, 1f);
    }

    private void BuildList()
    {
        if (upgradeManager == null || contentRoot == null || itemPrefab == null)
        {
            GameLog.Warn("[UpgradeListUI] 참조 미연결");
            return;
        }

        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        int count = 0;
        for (int i = 0; i < upgradeManager.upgrades.Length; i++)
        {
            var data = upgradeManager.upgrades[i];
            if (data == null) continue;
            var item = Instantiate(itemPrefab, contentRoot);
            item.Init(upgradeManager, i);
            count++;
        }

        // Content 높이 수동 계산 및 설정
        float totalHeight = _padding * 2 + count * _itemHeight + (count - 1) * _spacing;
        contentRoot.sizeDelta = new Vector2(contentRoot.sizeDelta.x, totalHeight);

        // 스크롤 맨 위로
        ResetScroll();
    }
}