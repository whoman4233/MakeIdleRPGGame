using System.Collections.Generic;
using UnityEngine;

public class SynthesisManager : MonoBehaviour
{
    public static SynthesisManager Instance { get; private set; }

    [Header("Synthesis Settings")]
    public DropTable synthesisResultTable; // 합성 성공 시 보상으로 줄 드랍 테이블

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // UI에서 선택된 3개의 육체 데이터를 전달받음
    public bool TrySynthesis(List<GraftData> selectedGrafts)
    {
        if (selectedGrafts == null || selectedGrafts.Count != 3)
        {
            Debug.Log("Synthesis requires exactly 3 grafts.");
            return false;
        }

        // 1. 인벤토리에서 재료 아이템 제거 (InventoryManager의 실제 메서드명에 맞춰 수정 필요)
        foreach (var graft in selectedGrafts)
        {
            // 예: InventoryManager.Instance.RemoveItem(graft);
        }

        // 2. 무료 1회 추출 진행
        ExtractionManager.Instance.ExtractFree(synthesisResultTable);
        
        Debug.Log("Synthesis Complete.");
        return true;
    }
}