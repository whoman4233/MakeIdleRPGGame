using System;
using UnityEngine;

public class ExtractionManager : MonoBehaviour
{
    public static ExtractionManager Instance { get; private set; }

    // UI 및 다른 시스템에서 구독할 이벤트
    public event Action<GraftData> OnExtractionSuccess;
    public event Action            OnExtractionFailed;
    public event Action            OnCostChanged;      // 비용 변경 시 UI에 알림

    [Header("Drop Tables")]
    public DropTable basicExtractionTable; // 관측 데이터용 테이블
    public DropTable deepExtractionTable;  // 이상 코어용 고급 테이블

    [Header("Basic 추출 비용 곡선")]
    [Tooltip("첫 번째 추출 기준 비용")]
    public int   basicBaseCost    = 100;
    [Tooltip("추출마다 곱해지는 배율 (1.15 = 15% 증가)")]
    [Range(1.01f, 2f)]
    public float basicGrowthRate  = 1.15f;
    [Tooltip("한 번에 증가하는 추출 횟수 단위 (1 = 매번, 5 = 5번마다 한 번 증가)")]
    public int   costStepSize     = 1;

    [Header("Deep 추출 비용 (고정)")]
    public int deepExtractionCost = 10;

    // 현재 기본 추출 누적 횟수 (세이브 대상)
    private int _basicExtractionCount;

    /// <summary>현재 기본 추출 비용 (지수 증가 곡선)</summary>
    public int CurrentBasicCost
    {
        get
        {
            int steps = _basicExtractionCount / Mathf.Max(1, costStepSize);
            return Mathf.RoundToInt(basicBaseCost * Mathf.Pow(basicGrowthRate, steps));
        }
    }

    public int BasicExtractionCount => _basicExtractionCount;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // =========================================================
    //  추출 공개 메서드
    // =========================================================

    /// <summary>관측 데이터를 사용한 기본 추출</summary>
    public void ExtractBasic(bool free = false)
    {
        if (free) { PerformExtraction(basicExtractionTable); return; }

        int cost = CurrentBasicCost;

        if (CurrencyManager.Instance.Data >= cost)
        {
            CurrencyManager.Instance.AddData(-cost);
            _basicExtractionCount++;
            OnCostChanged?.Invoke();
            PerformExtraction(basicExtractionTable);
        }
        else
        {
            OnExtractionFailed?.Invoke();
            GameLog.Log($"[Extraction] 데이터 부족. 필요: {cost}, 보유: {CurrencyManager.Instance.Data}");
        }
    }

    /// <summary>이상 코어를 사용한 심연 추출</summary>
    public void ExtractDeep()
    {
        if (CurrencyManager.Instance.Core >= deepExtractionCost)
        {
            CurrencyManager.Instance.AddCore(-deepExtractionCost);
            PerformExtraction(deepExtractionTable);
        }
        else
        {
            OnExtractionFailed?.Invoke();
            GameLog.Log("Not enough Core for extraction.");
        }
    }

    /// <summary>합성 시스템 등에서 비용 없이 추출</summary>
    public void ExtractFree(DropTable specificTable)
    {
        PerformExtraction(specificTable);
    }

    // =========================================================
    //  Save / Load
    // =========================================================

    public void LoadExtractionCount(int savedCount)
    {
        _basicExtractionCount = Mathf.Max(0, savedCount);
        OnCostChanged?.Invoke();
    }

    // =========================================================
    //  내부 처리
    // =========================================================

    private void PerformExtraction(DropTable table)
    {
        if (table == null) return;

        GraftData resultGraft = table.GetRandomDrop();

        if (resultGraft != null)
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.AddGraft(resultGraft);

            OnExtractionSuccess?.Invoke(resultGraft);
            GameLog.Log($"[Extraction] 성공: {resultGraft.graftName}  (다음 비용: {CurrentBasicCost})");
        }
        else
        {
            OnExtractionFailed?.Invoke();
        }
    }
}