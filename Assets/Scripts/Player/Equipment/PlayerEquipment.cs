using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerEquipment : MonoBehaviour
{
    public PlayerStats stats { get; private set; }

    [Header("Equipped")]
    public EquipmentData weapon;
    public EquipmentData armor;
    public EquipmentData accessory;

    public event Action OnEquipmentChanged;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        // 시작 시 장비에 맞는 Modifier 적용
        ReapplyAllModifiers();
    }

    public EquipmentData GetEquipped(EquipmentSlotType slot)
    {
        return slot switch
        {
            EquipmentSlotType.Weapon => weapon,
            EquipmentSlotType.Armor => armor,
            EquipmentSlotType.Accessory => accessory,
            _ => null
        };
    }

    public void Equip(EquipmentData newEquip)
    {
        if (newEquip == null) return;

        switch (newEquip.slotType)
        {
            case EquipmentSlotType.Weapon:
                ReplaceSlot(ref weapon, newEquip);
                break;
            case EquipmentSlotType.Armor:
                ReplaceSlot(ref armor, newEquip);
                break;
            case EquipmentSlotType.Accessory:
                ReplaceSlot(ref accessory, newEquip);
                break;
        }

        OnEquipmentChanged?.Invoke();
    }

    private void ReplaceSlot(ref EquipmentData slotRef, EquipmentData newEquip)
    {
        // 기존 장비 Modifier 제거
        if (slotRef != null && slotRef.modifiers != null)
        {
            foreach (var mod in slotRef.modifiers)
            {
                stats.RemoveEquipmentModifier(mod);
            }
        }

        slotRef = newEquip;

        // 새 장비 Modifier 적용
        if (slotRef != null && slotRef.modifiers != null)
        {
            foreach (var mod in slotRef.modifiers)
            {
                stats.AddEquipmentModifier(mod);
            }
        }
    }

    public void ReapplyAllModifiers()
    {
        // 싹 빼고 다시 적용 – 세이브/로드 후 초기화 용
        // 일단 간단히: 장비 전부 제거 후 다시 끼운다고 생각
        if (weapon != null && weapon.modifiers != null)
            foreach (var m in weapon.modifiers)
                stats.AddEquipmentModifier(m);

        if (armor != null && armor.modifiers != null)
            foreach (var m in armor.modifiers)
                stats.AddEquipmentModifier(m);

        if (accessory != null && accessory.modifiers != null)
            foreach (var m in accessory.modifiers)
                stats.AddEquipmentModifier(m);
    }
}
