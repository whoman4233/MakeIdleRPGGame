using System;
using UnityEngine;

public enum GraftSlotType { Head, Core, ArmL, ArmR, Legs }

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
        switch (newGraft.slotType)
        {
            case GraftSlotType.Head: ReplaceSlot(ref headGraft, newGraft); break;
            case GraftSlotType.Core: ReplaceSlot(ref coreGraft, newGraft); break;
            case GraftSlotType.ArmL: ReplaceSlot(ref armLGraft, newGraft); break;
            case GraftSlotType.ArmR: ReplaceSlot(ref armRGraft, newGraft); break;
            case GraftSlotType.Legs: ReplaceSlot(ref legsGraft, newGraft); break;
        }
        stats.NotifyStatsChanged();
        OnGraftChanged?.Invoke();
    }

    private void ReplaceSlot(ref GraftData slotRef, GraftData newGraft)
    {
        if (slotRef != null) RemoveGraftModifiers(slotRef);
        slotRef = newGraft;
        if (slotRef != null) ApplyGraftModifiers(slotRef);
    }

    // 모디파이어 제거만 — 슬롯 참조는 건드리지 않음 (버그 수정 핵심)
    public void RemoveAllModifiers()
    {
        if (headGraft != null) RemoveGraftModifiers(headGraft);
        if (coreGraft != null) RemoveGraftModifiers(coreGraft);
        if (armLGraft != null) RemoveGraftModifiers(armLGraft);
        if (armRGraft != null) RemoveGraftModifiers(armRGraft);
        if (legsGraft != null) RemoveGraftModifiers(legsGraft);
    }

    // 슬롯 참조를 보존한 채 모디파이어 재적용
    public void ReapplyAllModifiers()
    {
        RemoveAllModifiers();
        if (headGraft != null) ApplyGraftModifiers(headGraft);
        if (coreGraft != null) ApplyGraftModifiers(coreGraft);
        if (armLGraft != null) ApplyGraftModifiers(armLGraft);
        if (armRGraft != null) ApplyGraftModifiers(armRGraft);
        if (legsGraft != null) ApplyGraftModifiers(legsGraft);
    }

    // 세이브 로드 전용 — 슬롯 교체 후 재적용
    public void LoadEquipped(GraftData head, GraftData core, GraftData armL, GraftData armR, GraftData legs)
    {
        RemoveAllModifiers();   // 기존 모디파이어 해제 (슬롯은 유지)
        headGraft = head;       // 슬롯 교체
        coreGraft = core;
        armLGraft = armL;
        armRGraft = armR;
        legsGraft = legs;
        ReapplyAllModifiers(); // 새 슬롯 기준으로 재적용
        stats.NotifyStatsChanged();
        OnGraftChanged?.Invoke();
    }

    private void ApplyGraftModifiers(GraftData graft)
    {
        if (graft == null || graft.modifiers == null) return;
        foreach (var mod in graft.modifiers)
        {
            Stat targetStat = stats.GetStat(mod.statType);
            if (targetStat != null)
                targetStat.AddModifier(new StatModifier(mod.statType, mod.value, mod.type, graft));
        }
    }

    private void RemoveGraftModifiers(GraftData graft)
    {
        if (graft == null) return;
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            Stat targetStat = stats.GetStat(type);
            targetStat?.RemoveAllModifiersFromSource(graft);
        }
    }
}
