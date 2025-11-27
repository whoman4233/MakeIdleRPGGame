using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour, IAttackable
{
    public PlayerStatsData baseData;

    public float curHP;
    public float curExp;
    public int level = 1;

    private List<StatModifier> upgradeModifiers = new List<StatModifier>();
    private List<StatModifier> equipmentModifiers = new List<StatModifier>();
    private List<StatModifier> buffModifiers = new List<StatModifier>();

    public event Action OnStatsChanged;
    public event Action OnDied;


    private void Start()
    {
        level = baseData.baseLevel;
        curHP = MaxHP;
        curExp = 0;
        RecalculateStats();
    }

    // ★ 공통 Stat 계산 로직
    private float CalculateStat(StatType type, float baseValue)
    {
        float flat = 0f;
        float percent = 0f;

        void Accumulate(List<StatModifier> list)
        {
            foreach (var mod in list)
            {
                if (mod.type != type)
                    continue;

                flat += mod.flatValue;
                percent += mod.percentValue;
            }
        }

        Accumulate(upgradeModifiers);
        Accumulate(equipmentModifiers);
        Accumulate(buffModifiers);

        return (baseValue + flat) * (1f + percent);
    }

    // === 최종 스탯 ===
    public float Attack => CalculateStat(StatType.Attack, baseData.GetAttack(level));
    public float MaxHP => CalculateStat(StatType.MaxHP, baseData.GetMaxHP(level));
    public float AttackSpeed => CalculateStat(StatType.AttackSpeed, 1f / baseData.GetAttackInterval());
    public float ExpGainBonus => CalculateStat(StatType.ExpGain, 1f);

    public float SkillDamageBonus => CalculateStat(StatType.SkillDamage, 1f);
    public float GoldGainBonus => CalculateStat(StatType.GoldGain, 1f);

    public float AttackInterval => baseData.GetAttackInterval() / AttackSpeed;

    public float RequiredExp => baseData.GetRequiredExp(level);

    public Transform Transform => this.gameObject.transform;

    public bool IsAlive => curHP > 0;

    public int TeamId => baseData.TeamID;

    // === Modifier 추가/제거 ===
    public void AddUpgradeModifier(StatModifier mod)
    {
        upgradeModifiers.Add(mod);
        RecalculateStats();
    }

    public void AddEquipmentModifier(StatModifier mod)
    {
        equipmentModifiers.Add(mod);
        RecalculateStats();
    }

    public void AddBuffModifier(StatModifier mod)
    {
        buffModifiers.Add(mod);
        RecalculateStats();
    }

    public void RemoveEquipmentModifier(StatModifier mod)
    {
        equipmentModifiers.Remove(mod);
        RecalculateStats();
    }

    public void RemoveBuffModifier(StatModifier mod)
    {
        buffModifiers.Remove(mod);
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        float oldMax = MaxHP;
        float rate = curHP / oldMax;
        curHP = Mathf.Clamp(MaxHP * rate, 1, MaxHP);

        OnStatsChanged?.Invoke();
    }

    // === Exp / Level ===
    public void AddExp(float amount)
    {
        amount *= ExpGainBonus;
        curExp += amount;

        while (curExp >= RequiredExp && RequiredExp > 0)
        {
            curExp -= RequiredExp;
            level++;

            if (level > baseData.maxLevel)
            {
                level = baseData.maxLevel;
                curExp = 0;
                break;
            }

            RecalculateStats();
        }

        OnStatsChanged?.Invoke();
    }

    // === Damage ===
    public void TakeDamage(float dmg)
    {
        curHP -= dmg;
        if (curHP <= 0)
        {
            curHP = 0;
            OnDied?.Invoke();
        }

        OnStatsChanged?.Invoke();
    }

    public void ReviveFull()
    {
        curHP = MaxHP;
        OnStatsChanged?.Invoke();
    }
}
