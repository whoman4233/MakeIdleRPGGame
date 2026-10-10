using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무한 심층 깊이 마일스톤 1개. 특정 깊이에 처음 도달하면 1회 보상을 받는다.
/// </summary>
[Serializable]
public class DepthMilestone
{
    [Tooltip("이 보상을 받기 위해 도달해야 하는 심층 깊이 (StageManager.CurrentDepth 기준)")]
    public int depth;

    [Tooltip("UI 표시용 라벨. 비우면 '심층 {depth}' 로 표시.")]
    public string label;

    [Header("보상")]
    [Tooltip("지급할 관측 데이터")]
    public long dataReward;
    [Tooltip("지급할 이상 코어")]
    public int coreReward;

    public string DisplayLabel =>
        string.IsNullOrEmpty(label) ? $"심층 {depth}" : label;
}

/// <summary>
/// 깊이 마일스톤 보상 테이블 (데이터 드리븐). 깊이 오름차순으로 입력하는 것을 권장.
/// 밸런스 조정 시 코드 수정 없이 이 SO만 편집하면 된다.
/// </summary>
[CreateAssetMenu(fileName = "DepthMilestoneTable", menuName = "Anomaly/Depth Milestone Table")]
public class DepthMilestoneTable : ScriptableObject
{
    public List<DepthMilestone> milestones = new List<DepthMilestone>();
}
