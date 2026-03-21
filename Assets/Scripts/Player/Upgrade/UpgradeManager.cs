using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }

    public UpgradeData[] upgrades;

    private void Awake()
    {
        Instance = this;
    }

    public bool TryUpgrade(int index)
    {
        if (index < 0 || index >= upgrades.Length)
            return false;

        UpgradeData data = upgrades[index];

        int cost = data.GetCurrentCost();

        if (CurrencyManager.Instance.Data < cost)
            return false;

        CurrencyManager.Instance.AddData(-cost);

        data.level++;

        // 변경된 구조에 맞게 PlayerStats에서 해당 스탯 객체를 직접 가져와 Modifier를 추가합니다.
        Stat targetStat = PlayerRef.Instance.Stats.GetStat(data.modifier.statType);
        if (targetStat != null)
        {
            targetStat.AddModifier(data.modifier);
        }

        return true;
    }
}