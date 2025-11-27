using UnityEngine;

public class UpgradeListUI : MonoBehaviour
{
    public UpgradeManager upgradeManager;
    public RectTransform contentRoot;     // ScrollView/Viewport/Content
    public UpgradeItemUI itemPrefab;      // 위에서 만든 프리팹

    private void Start()
    {
        if (upgradeManager == null)
            upgradeManager = UpgradeManager.Instance;

        BuildList();
    }

    private void BuildList()
    {
        if (upgradeManager == null || contentRoot == null || itemPrefab == null)
        {
            Debug.LogWarning("[UpgradeListUI] 세팅이 안 되어 있음");
            return;
        }

        // 기존 자식들 정리
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }

        // 업그레이드 데이터 만큼 생성
        for (int i = 0; i < upgradeManager.upgrades.Length; i++)
        {
            var data = upgradeManager.upgrades[i];
            if (data == null) continue;

            var item = Instantiate(itemPrefab, contentRoot);
            item.Init(upgradeManager, i);
        }
    }
}
