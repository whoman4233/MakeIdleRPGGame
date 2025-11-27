using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public RectTransform contentRoot;       // ScrollView/Viewport/Content
    public InventoryItemUI itemPrefab;

    private InventoryManager _inventory;
    private PlayerEquipment _playerEquipment;

    private void Start()
    {
        _inventory = InventoryManager.Instance;

        if (PlayerRef.Instance != null)
            _playerEquipment = PlayerRef.Instance.GetComponent<PlayerEquipment>();

        if (_inventory != null)
            _inventory.OnInventoryChanged += Rebuild;

        Rebuild();
    }

    private void OnDestroy()
    {
        if (_inventory != null)
            _inventory.OnInventoryChanged -= Rebuild;
    }

    public void Rebuild()
    {
        if (_inventory == null || contentRoot == null || itemPrefab == null)
            return;

        // 기존 아이템들 삭제
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }

        var list = _inventory.Equipments;
        for (int i = 0; i < list.Count; i++)
        {
            var equip = list[i];
            if (equip == null) continue;

            var slotUI = Instantiate(itemPrefab, contentRoot);
            slotUI.Init(equip, _playerEquipment);
        }
    }
}
