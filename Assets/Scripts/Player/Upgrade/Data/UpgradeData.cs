using UnityEngine;

[CreateAssetMenu(menuName = "Data/Upgrade/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public StatModifier modifier;

    public int level = 0;
    [Tooltip("0 = 무제한")]
    public int maxLevel = 0;
    public int baseCost = 10;
    public float costMultiplier = 1.5f;

    public bool IsMaxLevel => maxLevel > 0 && level >= maxLevel;

    public int GetCurrentCost()
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, level));
    }
}
