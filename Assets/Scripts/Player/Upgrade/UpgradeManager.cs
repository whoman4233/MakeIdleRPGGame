using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    public UpgradeData[] upgrades;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public bool TryUpgrade(int index)
    {
        if (index < 0 || index >= upgrades.Length) return false;
        UpgradeData data = upgrades[index];
        int cost = data.GetCurrentCost();
        if (CurrencyManager.Instance.Data < cost) return false;
        CurrencyManager.Instance.AddData(-cost);
        data.level++;
        var mod = new StatModifier(data.modifier.statType, data.modifier.value, data.modifier.type, data);
        Stat targetStat = PlayerRef.Instance.Stats.GetStat(data.modifier.statType);
        targetStat?.AddModifier(mod);
        PlayerRef.Instance.Stats.NotifyStatsChanged();
        return true;
    }

    public void LoadUpgradeLevels(int[] savedLevels)
    {
        if (savedLevels == null || upgrades == null) return;
        var playerStats = PlayerRef.Instance?.Stats;
        if (playerStats == null) { GameLog.Warn("[UpgradeManager] PlayerRef.Stats 없음"); return; }
        foreach (var upg in upgrades)
        {
            if (upg == null) continue;
            playerStats.GetStat(upg.modifier.statType)?.RemoveAllModifiersFromSource(upg);
        }
        int len = Mathf.Min(savedLevels.Length, upgrades.Length);
        for (int i = 0; i < len; i++)
        {
            UpgradeData data = upgrades[i];
            if (data == null) continue;
            data.level = savedLevels[i];
            Stat targetStat = playerStats.GetStat(data.modifier.statType);
            if (targetStat == null) continue;
            for (int j = 0; j < data.level; j++)
            {
                var mod = new StatModifier(data.modifier.statType, data.modifier.value, data.modifier.type, data);
                targetStat.AddModifier(mod);
            }
        }
        playerStats.NotifyStatsChanged();
    }
}
