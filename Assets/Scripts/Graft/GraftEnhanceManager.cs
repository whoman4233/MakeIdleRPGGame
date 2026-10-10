using System;
using System.Collections.Generic;
using UnityEngine;

public class GraftEnhanceManager : MonoBehaviour
{
    public static GraftEnhanceManager Instance { get; private set; }

    [Header("강화 설정")]
    [Tooltip("강화 1레벨당 스탯 증가 비율 (0.08 = 8%)")]
    public float bonusPerLevel = 0.08f;
    [Tooltip("최대 강화 레벨 (0 이하면 무제한)")]
    public int maxLevel = 20;

    [Header("비용 곱선 (Data 소모)")]
    [Tooltip("첫 강화(0→1) 기준 비용")]
    public int baseCost = 200;
    [Tooltip("레벨마다 곱해지는 배율 (1.25 = 25% 증가)")]
    [Range(1.01f, 3f)]
    public float growthRate = 1.25f;

    // graft.name -> 강화 레벨
    private readonly Dictionary<string, int> _levels = new Dictionary<string, int>();

    private PlayerGraft _playerGraft;

    /// <summary>강화 상태 변경 시 (레벨업/로드). UI 갱신용.</summary>
    public event Action OnChanged;
    /// <summary>특정 부위 강화 성공 시 (연출용).</summary>
    public event Action<GraftData> OnEnhanced;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _playerGraft = FindObjectOfType<PlayerGraft>();
    }

    // =========================================================
    //  조회
    // =========================================================

    public int GetLevel(GraftData graft)
    {
        if (graft == null) return 0;
        return _levels.TryGetValue(graft.name, out int lv) ? lv : 0;
    }

    /// <summary>장착 스탯에 곱할 배율 (1 + 레벨 × 보너스).</summary>
    public float GetMultiplier(GraftData graft)
    {
        return 1f + GetLevel(graft) * bonusPerLevel;
    }

    public bool IsMaxLevel(GraftData graft)
    {
        return maxLevel > 0 && GetLevel(graft) >= maxLevel;
    }

    /// <summary>다음 레벨 강화 비용 (Data). 지수 곱선.</summary>
    public int GetCost(GraftData graft)
    {
        int lv = GetLevel(graft);
        return Mathf.RoundToInt(baseCost * Mathf.Pow(growthRate, lv));
    }

    public bool CanEnhance(GraftData graft)
    {
        if (graft == null || IsMaxLevel(graft)) return false;
        return CurrencyManager.Instance != null && CurrencyManager.Instance.Data >= GetCost(graft);
    }

    // =========================================================
    //  강화
    // =========================================================

    /// <summary>부위를 1레벨 강화. 성공 시 true.</summary>
    public bool Enhance(GraftData graft)
    {
        if (!CanEnhance(graft)) return false;

        int cost = GetCost(graft);
        CurrencyManager.Instance.AddData(-cost);

        int lv = GetLevel(graft) + 1;
        _levels[graft.name] = lv;

        if (_playerGraft == null) _playerGraft = FindObjectOfType<PlayerGraft>();
        if (_playerGraft != null)
            _playerGraft.ReapplyAllModifiers();

        OnEnhanced?.Invoke(graft);
        OnChanged?.Invoke();
        GameLog.Log("[GraftEnhance] " + graft.graftName + " 강화 → Lv." + lv + " (비용 " + cost + ")");
        return true;
    }

    // =========================================================
    //  Save / Load
    // =========================================================

    public void GetSaveData(List<string> ids, List<int> levels)
    {
        ids.Clear(); levels.Clear();
        foreach (var kv in _levels)
        {
            if (kv.Value <= 0) continue;
            ids.Add(kv.Key);
            levels.Add(kv.Value);
        }
    }

    public void LoadLevels(List<string> ids, List<int> levels)
    {
        _levels.Clear();
        if (ids != null && levels != null)
        {
            int n = Mathf.Min(ids.Count, levels.Count);
            for (int i = 0; i < n; i++)
                if (!string.IsNullOrEmpty(ids[i]) && levels[i] > 0)
                    _levels[ids[i]] = levels[i];
        }
        OnChanged?.Invoke();
    }
}
