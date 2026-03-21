using System;
using UnityEngine;

// 기존 EquipmentSlotType을 대체할 괴이 육체 이식 부위
public enum GraftSlotType
{
    Head,
    Core,
    ArmL,
    ArmR,
    Legs
}

[RequireComponent(typeof(PlayerStats))]
public class PlayerGraft : MonoBehaviour
{
    public PlayerStats stats { get; private set; }

    [Header("Equipped Grafts")]
    public GraftData headGraft;
    public GraftData coreGraft;
    public GraftData armLGraft;
    public GraftData armRGraft;
    public GraftData legsGraft;

    public event Action OnGraftChanged;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        // 시작 시 장착된 모든 괴이 육체의 스탯 적용
        ReapplyAllModifiers();
    }

    public GraftData GetEquipped(GraftSlotType slot)
    {
        return slot switch
        {
            GraftSlotType.Head => headGraft,
            GraftSlotType.Core => coreGraft,
            GraftSlotType.ArmL => armLGraft,
            GraftSlotType.ArmR => armRGraft,
            GraftSlotType.Legs => legsGraft,
            _ => null
        };
    }

    public void Equip(GraftData newGraft)
    {
        if (newGraft == null) return;

        switch (newGraft.slotType) // GraftData에 public GraftSlotType slotType; 필드가 있어야 합니다.
        {
            case GraftSlotType.Head: ReplaceSlot(ref headGraft, newGraft); break;
            case GraftSlotType.Core: ReplaceSlot(ref coreGraft, newGraft); break;
            case GraftSlotType.ArmL: ReplaceSlot(ref armLGraft, newGraft); break;
            case GraftSlotType.ArmR: ReplaceSlot(ref armRGraft, newGraft); break;
            case GraftSlotType.Legs: ReplaceSlot(ref legsGraft, newGraft); break;
        }

        OnGraftChanged?.Invoke();
    }

    private void ReplaceSlot(ref GraftData slotRef, GraftData newGraft)
    {
        // 기존 이식물 적출 (스탯 제거)
        if (slotRef != null)
        {
            RemoveGraftModifiers(slotRef);
        }

        slotRef = newGraft;

        // 새 이식물 장착 (스탯 적용)
        if (slotRef != null)
        {
            ApplyGraftModifiers(slotRef);
        }
    }

    public void ReapplyAllModifiers()
    {
        // 세이브/로드 후 초기화 시 기존 스탯을 모두 날리고 새로 덮어씌움
        if (headGraft != null) ApplyGraftModifiers(headGraft);
        if (coreGraft != null) ApplyGraftModifiers(coreGraft);
        if (armLGraft != null) ApplyGraftModifiers(armLGraft);
        if (armRGraft != null) ApplyGraftModifiers(armRGraft);
        if (legsGraft != null) ApplyGraftModifiers(legsGraft);
    }

    // --- 새로 추가된 모디파이어 직접 제어 로직 ---

    private void ApplyGraftModifiers(GraftData graft)
    {
        if (graft.modifiers == null) return;

        foreach (var mod in graft.modifiers)
        {
            Stat targetStat = stats.GetStat(mod.statType);
            if (targetStat != null)
            {
                // source를 현재 graft로 지정하여 추가
                var newMod = new StatModifier(mod.statType, mod.value, mod.type, graft);
                targetStat.AddModifier(newMod);
            }
        }
    }

    private void RemoveGraftModifiers(GraftData graft)
    {
        // 모든 스탯 타입을 순회하며 해당 이식물(graft)이 소스인 모디파이어를 제거
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            Stat targetStat = stats.GetStat(type);
            if (targetStat != null)
            {
                targetStat.RemoveAllModifiersFromSource(graft);
            }
        }
    }
}