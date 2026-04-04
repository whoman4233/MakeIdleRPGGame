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

        // 1. 인벤토리에서 재료 아이템 3개 제거
        foreach (var graft in selectedGrafts)
        {
            InventoryManager.Instance.RemoveGraft(graft);
        }

        // 2. 무료 1회 추출 진행 (결과물 뽑기 및 연출은 ExtractionManager에 위임)
        if (ExtractionManager.Instance != null)
        {
            ExtractionManager.Instance.ExtractFree(synthesisResultTable);
        }
        else
        {
            // 만약 ExtractionManager가 씬에 없다면, 백업으로 직접 드랍테이블을 돌려 인벤토리에 넣습니다.
            GraftData newGraft = synthesisResultTable.GetRandomDrop();
            if (newGraft != null)
            {
                InventoryManager.Instance.AddGraft(newGraft);
            }
        }
        
        Debug.Log("Synthesis Complete.");
        return true;
    }
}