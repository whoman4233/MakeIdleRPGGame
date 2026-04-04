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

    // 외부(예: UpgradeManager)에서 스탯 모디파이어를 추가/제거한 직후 호출
    public void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }
}