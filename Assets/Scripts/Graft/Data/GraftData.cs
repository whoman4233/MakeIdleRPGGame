using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGraftData", menuName = "Anomaly/Graft Data")]
public class GraftData : ScriptableObject
{
    [Header("Basic Info")]
    public string graftName;
    [TextArea] public string description;
    public GraftSlotType slotType;

    [Header("UI Icon")]
    [Tooltip("인벤토리 UI에 표시되는 아이콘")]
    public Sprite icon;

    [Header("Visual Appearance")]
    [Tooltip("장착 시 해당 신체 부위 SpriteRenderer에 표시되는 스프라이트")]
    public Sprite appearanceSprite;
    [Tooltip("appearanceSprite 색상 오버라이드")]
    public Color  appearanceColor = Color.white;

    [Header("Core Skill")]
    [Tooltip("Core 슬롯 전용. 장착 시 사용 가능한 액티브 스킬.")]
    public CoreSkillData coreSkill;

    [Header("Stats")]
    public List<StatModifier> modifiers;

    /// <summary>외형 스프라이트 (appearanceSprite 우선, 없으면 icon 폴백)</summary>
    public Sprite EffectiveAppearanceSprite =>
        appearanceSprite != null ? appearanceSprite : icon;
}
