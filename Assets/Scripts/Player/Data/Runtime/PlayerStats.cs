using System.Collections.Generic;
using UnityEngine;

// 1. 순수하게 스탯 데이터만 관리하는 클래스로 축소
public class PlayerStats : MonoBehaviour
{
    private Dictionary<StatType, Stat> stats;

    private void Awake() 
    {
        // 초기 기본 스탯 세팅
        stats = new Dictionary<StatType, Stat>
        {
            { StatType.AttackPower, new Stat(StatType.AttackPower, 10f) },
            { StatType.AttackSpeed, new Stat(StatType.AttackSpeed, 1f) },
            { StatType.MaxHealth, new Stat(StatType.MaxHealth, 100f) },
            { StatType.HealthRegen, new Stat(StatType.HealthRegen, 0f) }
        };
    }

    // 최종 스탯 값 반환
    public float GetStatValue(StatType type) => stats[type].Value;

    // 모디파이어 추가/제거를 위해 Stat 객체 자체를 반환
    public Stat GetStat(StatType type) => stats[type];
}