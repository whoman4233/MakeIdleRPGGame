using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI slotText;   // Weapon/Armor/Accessory Ç¥±â¿ë
    public Button equipButton;

    private EquipmentData _data;
    private PlayerEquipment _playerEquipment;

    public void Init(EquipmentData data, PlayerEquipment playerEquipment)
    {
        _data = data;
        _playerEquipment = playerEquipment;

        if (nameText != null)
            nameText.text = data != null ? data.displayName : "-";

        if (slotText != null)
            slotText.text = data != null ? data.slotType.ToString() : "";

        if (icon != null && data != null)
            icon.sprite = data.icon;

        if (equipButton != null)
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(OnClickEquip);
        }
    }

    private void OnClickEquip()
    {
        if (_data == null || _playerEquipment == null)
            return;

        _playerEquipment.Equip(_data);
        Debug.Log($"[InventoryUI] ÀåÂø: {_data.displayName}");
    }
}
