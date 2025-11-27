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

        if (CurrencyManager.Instance.Gold < cost)
            return false;

        CurrencyManager.Instance.AddGold(-cost);

        data.level++;

        // Modifier Àû¿ë
        PlayerRef.Instance.Stats.AddUpgradeModifier(data.modifier);

        return true;
    }
}
