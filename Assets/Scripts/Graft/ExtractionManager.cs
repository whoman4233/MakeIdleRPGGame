using System;
using UnityEngine;

public class ExtractionManager : MonoBehaviour
{
    public static ExtractionManager Instance { get; private set; }

    // UI 및 다른 시스템에서 구독할 이벤트
    public event Action<GraftData> OnExtractionSuccess;
    public event Action OnExtractionFailed;

    [Header("Drop Tables")]
    public DropTable basicExtractionTable; // 관측 데이터용 테이블
    public DropTable deepExtractionTable;  // 이상 코어용 고급 테이블

    [Header("Costs")]
    public int basicExtractionCost = 100;
    public int deepExtractionCost = 10;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 관측 데이터를 사용한 기본 추출 (Basic)
    public void ExtractBasic()
    {
        if (CurrencyManager.Instance.Data >= basicExtractionCost)
        {
            CurrencyManager.Instance.AddData(-basicExtractionCost);
            PerformExtraction(basicExtractionTable);
        }
        else
        {
            OnExtractionFailed?.Invoke();
            Debug.Log("Not enough Data for extraction.");
        }
    }

    // 이상 코어를 사용한 심연 추출 (Deep)
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
            Debug.Log("Not enough Core for extraction.");
        }
    }

    // 합성 시스템 등에서 비용 없이 추출할 때 사용
    public void ExtractFree(DropTable specificTable)
    {
        PerformExtraction(specificTable);
    }

    private void PerformExtraction(DropTable table)
    {
        if (table == null) return;

        GraftData resultGraft = table.GetRandomDrop();
        
        if (resultGraft != null)
        {
            // 인벤토리 매니저에 실제 아이템 추가
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddGraft(resultGraft);
            }

            // 성공 이벤트 발생 및 획득 데이터 전달
            OnExtractionSuccess?.Invoke(resultGraft);
            Debug.Log($"Extraction Success: Acquired {resultGraft.graftName}");
        }
        else
        {
            // 테이블 설정 오류 등으로 드랍 실패 시
            OnExtractionFailed?.Invoke();
        }
    }
}