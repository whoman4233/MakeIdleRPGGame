using UnityEngine;

[CreateAssetMenu(menuName = "Data/Player/Stats", fileName = "PlayerStatsData")]
public class PlayerStatsData : ScriptableObject
{
    [Header("Basic Info")]
    public string id = "player_default";
    public string displayName = "Knight";

    [Header("Base Stats (Lv1 기준)")]
    public int baseLevel = 1;
    public float baseMaxHP = 100f;
    public float baseMaxMP = 50f;
    public float baseAttack = 10f;
    public float baseDefense = 5f;
    public float baseMoveSpeed = 3f;
    public float baseAttackInterval = 1.0f;

    [Header("Level Up Growth (레벨당 증가량)")]
    public float hpPerLevel = 20f;
    public float mpPerLevel = 10f;
    public float attackPerLevel = 2f;
    public float defensePerLevel = 1f;

    [Header("Exp Curve")]
    public AnimationCurve expCurve = AnimationCurve.Linear(1, 100, 50, 5000);
    public int maxLevel = 50;

    public float GetMaxHP(int level) => baseMaxHP + hpPerLevel * (Mathf.Max(1, level) - 1);
    public float GetMaxMP(int level) => baseMaxMP + mpPerLevel * (Mathf.Max(1, level) - 1);
    public float GetAttack(int level) => baseAttack + attackPerLevel * (Mathf.Max(1, level) - 1);
    public float GetDefense(int level) => baseDefense + defensePerLevel * (Mathf.Max(1, level) - 1);
    public float GetMoveSpeed() => baseMoveSpeed;
    public float GetAttackInterval() => baseAttackInterval;

    public float GetRequiredExp(int level)
    {
        if (level >= maxLevel) return 0f;
        return expCurve.Evaluate(level);
    }

    public int TeamID;
}
