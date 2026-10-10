using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeListUI : MonoBehaviour
{
    public UpgradeManager upgradeManager;
    public RectTransform contentRoot;
    public UpgradeItemUI itemPrefab;

    [Header("부위 강화 통합")]
    public PlayerGraft playerGraft;
    public GraftEnhanceItemUI graftItemPrefab;

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
        BuildList();
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

        if (upgradeManager.upgrades != null && upgradeManager.upgrades.Length > 0)
        {
            AddSectionHeader("전역 강화");
            count++;
        }

        for (int i = 0; i < upgradeManager.upgrades.Length; i++)
        {
            var data = upgradeManager.upgrades[i];
            if (data == null) continue;
            var item = Instantiate(itemPrefab, contentRoot);
            item.Init(upgradeManager, i);
            count++;
        }

        if (playerGraft != null && graftItemPrefab != null)
        {
            GraftSlotType[] _slots = { GraftSlotType.Head, GraftSlotType.Core, GraftSlotType.ArmL, GraftSlotType.ArmR, GraftSlotType.Legs };
            bool _headerAdded = false;
            foreach (var _st in _slots)
            {
                var _g = playerGraft.GetEquipped(_st);
                if (_g == null) continue;
                if (!_headerAdded) { AddSectionHeader("장착 부위 강화"); count++; _headerAdded = true; }
                var _gi = Instantiate(graftItemPrefab, contentRoot);
                _gi.Init(_g);
                count++;
            }
        }

        // Content 높이 계산 (행마다 높이 달라 실제 합산)
        float totalHeight = _padding * 2;
        int _n = contentRoot.childCount;
        for (int _i = 0; _i < _n; _i++)
        {
            var _le = contentRoot.GetChild(_i).GetComponent<LayoutElement>();
            totalHeight += (_le != null && _le.preferredHeight > 0f) ? _le.preferredHeight : _itemHeight;
        }
        if (_n > 1) totalHeight += (_n - 1) * _spacing;
        contentRoot.sizeDelta = new Vector2(contentRoot.sizeDelta.x, totalHeight);

        // 스크롤 맨 위로
        ResetScroll();
    }

    private void AddSectionHeader(string title)
    {
        var go = new GameObject("SectionHeader", typeof(RectTransform));
        go.transform.SetParent(contentRoot, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 46f;
        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.14f, 0.2f, 0.85f);

        var tgo = new GameObject("Label", typeof(RectTransform));
        tgo.transform.SetParent(go.transform, false);
        var trt = tgo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(18f, 0f); trt.offsetMax = new Vector2(-18f, 0f);
        var tmp = tgo.AddComponent<TMPro.TextMeshProUGUI>();
        var font = TMPro.TMP_Settings.defaultFontAsset;
        if (font != null) tmp.font = font;
        tmp.text = title;
        tmp.fontSize = 26f;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.color = new Color(0.85f, 0.85f, 0.92f, 1f);
        tmp.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
    }
}