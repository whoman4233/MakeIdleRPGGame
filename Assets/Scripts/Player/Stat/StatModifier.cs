using System;
using System.Collections.Generic;
using UnityEngine;

// 스탯의 종류
public enum StatType 
{ 
    AttackPower, 
    AttackSpeed, 
    MaxHealth, 
    HealthRegen, // 음수가 되면 체력이 깎임 (패널티)
    CriticalChance 
}

// 연산 우선순위 (합연산 후 곱연산 진행)
public enum ModifierType 
{ 
    Flat = 100, 
    PercentAdd = 200, 
    PercentMult = 300 
}

[Serializable]
public class StatModifier
{
    public StatType statType;
    public float value;
    public ModifierType type;
    public object source; // 어떤 아이템(육체)에서 온 스탯인지 추적하기 위함

    // SO에서 입력하기 위한 생성자 오버로딩
    public StatModifier(StatType statType, float value, ModifierType type, object source = null)
    {
        this.statType = statType;
        this.value = value;
        this.type = type;
        this.source = source;
    }
}