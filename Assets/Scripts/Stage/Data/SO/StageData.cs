using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Stage/StageData", fileName = "StageData")]
public class StageData : ScriptableObject
{
    [Header("Basic Info")]
    public string id          = "stage_1";
    public string displayName = "Stage 1";
    [TextArea] public string description;

    [Header("Background")]
    [Tooltip("배경 색상 (스카이박스 없을 때 Camera 배경색으로 사용)")]
    public Color bgColor = new Color(0.1f, 0.1f, 0.15f, 1f);
    [Tooltip("배경 스프라이트 (없으면 단색 처리)")]
    public Sprite bgSprite;

    [Header("Normal Enemy Settings")]
    [Tooltip("이 스테이지에서 등장하는 일반 몹 목록")]
    public List<EnemyStatsData> normalEnemies = new List<EnemyStatsData>();
    public int   maxAliveNormal    = 5;
    public float spawnIntervalNormal = 2f;

    [Header("Boss Settings")]
    [Tooltip("이 스테이지 보스")]
    public EnemyStatsData bossEnemy;
    public int normalKillToSummonBoss = 20;

    [Header("Rewards")]
    [Tooltip("스테이지 클리어(보스 처치) 시 추가 골드")]
    public int clearBonusGold = 100;
}
