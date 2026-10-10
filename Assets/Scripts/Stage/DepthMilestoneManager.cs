using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 무한 심층 깊이 마일스톤 보상 관리자.
///
/// - 특정 깊이에 처음 도달하면 해당 마일스톤이 '수령 가능' 상태가 된다 (BestDepth 기준).
/// - 플레이어가 Claim 하면 보상을 지급하고 '수령 완료'로 기록한다 (세이브 대상).
/// - 자동 지급이 아닌 수동 수령 방식 → UI에서 뱃지/버튼으로 노출하기 위함.
///
/// [필요 연결] 인스펙터에 DepthMilestoneTable SO 연결.
/// </summary>
public class DepthMilestoneManager : MonoBehaviour
{
    public static DepthMilestoneManager Instance { get; private set; }

    [Header("필수 연결")]
    [Tooltip("깊이별 보상이 정의된 테이블 SO")]
    [SerializeField] private DepthMilestoneTable table;

    // 수령 완료한 깊이 집합 (세이브 대상)
    private readonly HashSet<int> _claimed = new HashSet<int>();

    /// <summary>마일스톤 상태가 바뀌어 UI 갱신이 필요할 때 (도달/수령/로드).</summary>
    public event Action OnMilestonesChanged;
    /// <summary>개별 마일스톤 수령 성공 시 (연출/팝업용).</summary>
    public event Action<DepthMilestone> OnMilestoneClaimed;

    public IReadOnlyList<DepthMilestone> Milestones =>
        table != null ? table.milestones : System.Array.Empty<DepthMilestone>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private IEnumerator Start()
    {
        // StageManager가 Instance를 세팅할 때까지 한 프레임 대기
        yield return null;
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged       += HandleDepthChanged;
            StageManager.Instance.OnEndlessLoopStarted += HandleLoopChanged;
        }
    }

    private void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged       -= HandleDepthChanged;
            StageManager.Instance.OnEndlessLoopStarted -= HandleLoopChanged;
        }
    }

    private void HandleDepthChanged()     => OnMilestonesChanged?.Invoke();
    private void HandleLoopChanged(int _) => OnMilestonesChanged?.Invoke();

    // =========================================================
    //  상태 조회
    // =========================================================

    private int BestDepth => StageManager.Instance != null ? StageManager.Instance.BestDepth : 0;

    public bool IsClaimed(int depth)          => _claimed.Contains(depth);
    public bool IsReached(DepthMilestone m)   => m != null && BestDepth >= m.depth;
    public bool IsClaimable(DepthMilestone m) => m != null && !IsClaimed(m.depth) && BestDepth >= m.depth;

    /// <summary>수령 가능한 마일스톤 개수 (UI 뱃지용).</summary>
    public int ClaimableCount
    {
        get
        {
            if (table == null) return 0;
            int n = 0;
            foreach (var m in table.milestones)
                if (IsClaimable(m)) n++;
            return n;
        }
    }

    // =========================================================
    //  수령
    // =========================================================

    /// <summary>특정 깊이 마일스톤을 수령한다. 성공 시 true.</summary>
    public bool Claim(int depth)
    {
        if (table == null) return false;

        DepthMilestone target = null;
        foreach (var m in table.milestones)
            if (m != null && m.depth == depth) { target = m; break; }

        if (target == null || !IsClaimable(target)) return false;

        GrantReward(target);
        _claimed.Add(depth);
        OnMilestoneClaimed?.Invoke(target);
        OnMilestonesChanged?.Invoke();
        return true;
    }

    /// <summary>수령 가능한 모든 마일스톤을 일괄 수령한다. 수령한 개수 반환.</summary>
    public int ClaimAll()
    {
        if (table == null) return 0;

        int count = 0;
        foreach (var m in table.milestones)
        {
            if (m == null || !IsClaimable(m)) continue;
            GrantReward(m);
            _claimed.Add(m.depth);
            OnMilestoneClaimed?.Invoke(m);
            count++;
        }
        if (count > 0) OnMilestonesChanged?.Invoke();
        return count;
    }

    private void GrantReward(DepthMilestone m)
    {
        if (CurrencyManager.Instance == null) return;
        if (m.dataReward != 0) CurrencyManager.Instance.AddData(m.dataReward);
        if (m.coreReward != 0) CurrencyManager.Instance.AddCore(m.coreReward);
        GameLog.Log($"[DepthMilestone] 수령: {m.DisplayLabel} (Data +{m.dataReward}, Core +{m.coreReward})");
    }

    // =========================================================
    //  Save / Load
    // =========================================================

    public List<int> GetClaimedDepths() => new List<int>(_claimed);

    public void LoadClaimed(List<int> saved)
    {
        _claimed.Clear();
        if (saved != null)
            foreach (var d in saved) _claimed.Add(d);
        OnMilestonesChanged?.Invoke();
    }
}
