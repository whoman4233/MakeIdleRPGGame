using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GraftSlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image           icon;
    public TextMeshProUGUI slotLabel;
    public TextMeshProUGUI graftName;
    public Button          unequipButton;
    public Image           emptyOverlay;

    [Header("Settings")]
    public GraftSlotType slotType;

    private PlayerGraft _pg;

    private static readonly Color COL_HEAD = new Color(0.9f,0.5f,0.5f);
    private static readonly Color COL_CORE = new Color(0.9f,0.7f,0.3f);
    private static readonly Color COL_ARML = new Color(0.4f,0.8f,0.6f);
    private static readonly Color COL_ARMR = new Color(0.4f,0.7f,0.9f);
    private static readonly Color COL_LEGS = new Color(0.7f,0.5f,0.9f);

    public void Init(PlayerGraft pg)
    {
        _pg = pg;
        if (slotLabel != null) { slotLabel.text = GetLabel(); slotLabel.color = GetColor(); }
        if (unequipButton != null) { unequipButton.onClick.RemoveAllListeners(); unequipButton.onClick.AddListener(OnUnequip); }
        if (_pg != null) _pg.OnGraftChanged += DoRefresh;
        DoRefresh();
    }

    private void OnDestroy()
    {
        if (_pg != null) _pg.OnGraftChanged -= DoRefresh;
    }

    public void DoRefresh()
    {
        if (_pg == null) return;
        var eq  = _pg.GetEquipped(slotType);
        bool has = eq != null;
        if (icon != null)         { icon.enabled = has; if (has && eq.icon != null) icon.sprite = eq.icon; }
        if (graftName != null)    graftName.text = has ? eq.graftName : "비어있음";
        if (emptyOverlay != null) emptyOverlay.gameObject.SetActive(!has);
        if (unequipButton != null) unequipButton.gameObject.SetActive(has);
    }

    private void OnUnequip()
    {
        if (_pg == null) return;
        var cur = _pg.GetEquipped(slotType);
        if (cur == null) return;
        // 모디파이어 제거
        foreach (StatType t in Enum.GetValues(typeof(StatType)))
            _pg.stats.GetStat(t)?.RemoveAllModifiersFromSource(cur);
        // 슬롯 비우기
        switch(slotType) {
            case GraftSlotType.Head: _pg.headGraft = null; break;
            case GraftSlotType.Core: _pg.coreGraft = null; break;
            case GraftSlotType.ArmL: _pg.armLGraft = null; break;
            case GraftSlotType.ArmR: _pg.armRGraft = null; break;
            case GraftSlotType.Legs: _pg.legsGraft = null; break;
        }
        _pg.stats.NotifyStatsChanged();
        SoundManager.Instance?.PlayEquip();
        DoRefresh();
    }

    private string GetLabel() => slotType switch {
        GraftSlotType.Head=>"HEAD", GraftSlotType.Core=>"CORE",
        GraftSlotType.ArmL=>"ARM·L", GraftSlotType.ArmR=>"ARM·R",
        GraftSlotType.Legs=>"LEGS", _=>slotType.ToString()
    };
    private Color GetColor() => slotType switch {
        GraftSlotType.Head=>COL_HEAD, GraftSlotType.Core=>COL_CORE,
        GraftSlotType.ArmL=>COL_ARML, GraftSlotType.ArmR=>COL_ARMR,
        GraftSlotType.Legs=>COL_LEGS, _=>Color.white
    };
}