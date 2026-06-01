using System;
using System.Collections.Generic;
using UnityEngine;

// 1. 순수하게 스탯 데이터만 관리하는 클래스로 축소
public class PlayerStats : MonoBehaviour
{
    // 스탯 변동 시 UI 등에 알리기 위한 이벤트
    public event Action OnStatsChanged;

    private Dictionary<StatType, Stat> stats;

private void Awake() 
    {
        stats = new Dictionary<StatType, Stat>
        {
            { StatType.AttackPower,    new Stat(StatType.AttackPower,    20f) },
            { StatType.AttackSpeed,    new Stat(StatType.AttackSpeed,    1.2f)},
            { StatType.MaxHealth,      new Stat(StatType.MaxHealth,      300f)},
            { StatType.HealthRegen,    new Stat(StatType.HealthRegen,    0f)  },
            { StatType.CriticalChance, new Stat(StatType.CriticalChance, 0f)  },
        };
    }

    // 최종 스탯 값 반환
public float GetStatValue(StatType type)
    {
        if (stats == null) return 0f;
        if (!stats.ContainsKey(type))
            stats[type] = new Stat(type, 0f);
        return stats[type].Value;
    }

    // 모디파이어 추가/제거를 위해 Stat 객체 자체를 반환
public Stat GetStat(StatType type)
    {
        if (stats == null) return null;
        if (!stats.ContainsKey(type))
        {
            GameLog.Warn($"[PlayerStats] StatType '{type}' 딕셔너리에 없음. 자동 추가.");
            stats[type] = new Stat(type, 0f);
        }
        return stats[type];
    }

    // 외부(예: UpgradeManager)에서 스탯 모디파이어를 추가/제거한 직후 호출
    public void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }
}