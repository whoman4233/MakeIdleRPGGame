using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    Attack,
    MaxHP,
    AttackSpeed,
    ExpGain,
    SkillDamage,   
    GoldGain       
}

[System.Serializable]
public struct StatModifier
{
    public StatType type;
    public float flatValue;
    public float percentValue;

    public StatModifier(StatType type, float flatValue = 0f, float percentValue = 0f)
    {
        this.type = type;
        this.flatValue = flatValue;
        this.percentValue = percentValue;
    }
}