using UnityEngine;

[CreateAssetMenu(menuName = "Data/Enemy/Stats", fileName = "EnemyStatsData")]
public class EnemyStatsData : ScriptableObject
{
    [Header("Basic Info")]
    public string id = "enemy_default";
    public string displayName = "Slime";

    [Header("Stats")]
    public float maxHP = 50f;
    public float attackPower = 5f;
    public float moveSpeed = 0f;       // 필요하면 사용, 아니면 0
    public float attackInterval = 1.5f;
    public float attackRange = 1.5f;

    [Header("Reward")]
    public int expReward = 10;
    public long goldReward = 5;
}
