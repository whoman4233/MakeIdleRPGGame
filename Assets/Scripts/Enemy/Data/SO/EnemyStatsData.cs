using UnityEngine;

public class EnemyStatsData : ScriptableObject
{
    [Header("Basic Info")]
    public string id = "enemy_default";
    public string displayName = "Slime";

    [Header("Stats")]
    public float maxHP = 50f;
    public float attackPower = 5f;
    public float moveSpeed = 0f;
    public float attackInterval = 1.5f;
    public float attackRange = 1.5f;

    [Header("Reward")]
    public int expReward = 10;
    public long goldReward = 5;

    [Header("Flags")]
    public bool isBoss = false; 
}
