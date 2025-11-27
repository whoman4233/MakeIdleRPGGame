using UnityEngine;

public enum ShopItemType
{
    Weapon,
    Armor,
    Accessory
}

[System.Serializable]
public class ShopSlot
{
    public ShopItemType type;
    public long cost = 500;
    public EquipmentData[] candidates;   // 이 중에서 랜덤 뽑기
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Shop Slots")]
    public ShopSlot[] slots;

    private PlayerEquipment _playerEquipment;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (PlayerRef.Instance != null)
            _playerEquipment = PlayerRef.Instance.GetComponent<PlayerEquipment>();
    }

    public bool TryBuy(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        if (CurrencyManager.Instance == null) return false;
        if (_playerEquipment == null) return false;

        var slot = slots[slotIndex];
        long cost = slot.cost;

        if (CurrencyManager.Instance.Gold < cost)
        {
            Debug.Log("[Shop] 골드 부족");
            return false;
        }

        var equip = GetRandomEquip(slot);
        if (equip == null)
        {
            Debug.LogWarning("[Shop] 뽑을 장비가 없습니다.");
            return false;
        }

        // 결제
        CurrencyManager.Instance.AddGold(-cost);

        // 장비 지급 + 자동 장착
        _playerEquipment.Equip(equip);

        Debug.Log($"[Shop] {slot.type} 뽑기 성공: {equip.displayName}");

        // 추후: 팝업 / 토스트 UI로 알림 띄우기
        return true;
    }

    private EquipmentData GetRandomEquip(ShopSlot slot)
    {
        if (slot.candidates == null || slot.candidates.Length == 0)
            return null;

        int idx = Random.Range(0, slot.candidates.Length);
        return slot.candidates[idx];
    }
}
