using UnityEngine;

public enum EquipmentSlotType
{
    Weapon,
    Armor,
    Accessory
}

[CreateAssetMenu(menuName = "Data/Item/Equipment", fileName = "Equip_")]
public class EquipmentData : ScriptableObject
{
    [Header("Basic Info")]
    public string id = "equip_default";
    public string displayName = "New Equipment";
    public Sprite icon;

    [Header("Slot")]
    public EquipmentSlotType slotType;

    [Header("Stat Modifiers")]
    public StatModifier[] modifiers;

    [Header("Meta")]
    public int rarity = 1;
    public long basePrice = 100;
}
