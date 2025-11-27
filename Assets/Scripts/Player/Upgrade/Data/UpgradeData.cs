using UnityEngine;

[CreateAssetMenu(menuName = "Data/Upgrade/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public StatModifier modifier;

    public int level = 0;
    public int baseCost = 10;
    public float costMultiplier = 1.5f;

    public int GetCurrentCost()
    {
        return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, level));
    }
}
