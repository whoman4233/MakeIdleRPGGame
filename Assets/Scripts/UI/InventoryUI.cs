using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public RectTransform contentRoot;       // ScrollView/Viewport/Content
    public InventoryItemUi itemPrefab;

    private InventoryManager _inventory;
    private PlayerGraft _playerGraft;       // PlayerEquipment ➔ PlayerGraft로 변경

    private void Start()
    {
        _inventory = InventoryManager.Instance;

        if (PlayerRef.Instance != null)    // (주의) PlayerRef 인지 PlayerRefs 인지 확인 필요
            _playerGraft = PlayerRef.Instance.GetComponent<PlayerGraft>();

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

        // 기존 생성된 슬롯들 삭제 (초기화)
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }

        var list = _inventory.Grafts;       // Equipments ➔ Grafts로 변경
        for (int i = 0; i < list.Count; i++)
        {
            var graft = list[i];
            if (graft == null) continue;

            var slotUI = Instantiate(itemPrefab, contentRoot);
            slotUI.Init(graft, _playerGraft); // 이 부분 때문에 InventoryItemUi.cs 에서도 에러가 날 것입니다!
        }
    }
}