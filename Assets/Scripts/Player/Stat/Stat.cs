using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stat
{
    public StatType Type;
    public float BaseValue;
    
    private bool isDirty = true;
    private float _value;
    private float lastBaseValue;
    
    private readonly List<StatModifier> statModifiers;

    public float Value 
    {
        get 
        {
            if (isDirty || BaseValue != lastBaseValue) 
            {
                lastBaseValue = BaseValue;
                _value = CalculateFinalValue();
                isDirty = false;
            }
            return _value;
        }
    }

    public Stat(StatType type, float baseValue)
    {
        Type = type;
        BaseValue = baseValue;
        statModifiers = new List<StatModifier>();
    }

    public void AddModifier(StatModifier mod)
    {
        isDirty = true;
        statModifiers.Add(mod);
        statModifiers.Sort(CompareModifierOrder);
    }

    public void RemoveAllModifiersFromSource(object source)
    {
        isDirty = true;
        statModifiers.RemoveAll(mod => mod.source == source);
    }

    private int CompareModifierOrder(StatModifier a, StatModifier b)
    {
        if (a.type < b.type) return -1;
        if (a.type > b.type) return 1;
        return 0;
    }

    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        for (int i = 0; i < statModifiers.Count; i++)
        {
            StatModifier mod = statModifiers[i];

            if (mod.type == ModifierType.Flat)
            {
                finalValue += mod.value;
            }
            else if (mod.type == ModifierType.PercentAdd)
            {
                sumPercentAdd += mod.value;
                if (i + 1 >= statModifiers.Count || statModifiers[i + 1].type != ModifierType.PercentAdd)
                {
                    finalValue *= (1.0f + sumPercentAdd);
                    sumPercentAdd = 0;
                }
            }
            else if (mod.type == ModifierType.PercentMult)
            {
                finalValue *= (1.0f + mod.value);
            }
        }
        
        return (float)Math.Round(finalValue, 4);
    }
}