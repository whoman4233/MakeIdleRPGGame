using UnityEngine;

[CreateAssetMenu(menuName = "Data/Stage/StageData", fileName = "StageData")]
public class StageData : ScriptableObject
{
    [Header("Basic Info")]
    public string id = "stage_1";
    public string displayName = "Stage 1";
    [TextArea] public string description;

    [Header("Normal Enemy Settings")]
    public EnemyStats normalEnemyPrefab;
    public int maxAliveNormal = 5;
    public float spawnIntervalNormal = 2f;

    [Header("Boss Settings")]
    public EnemyStats bossEnemyPrefab;
    public int normalKillToSummonBoss = 20;

    [Header("(옵션) 스테이지 클리어 조건")]
    public int killTarget = 0;
}
