using System.Collections.Generic;
using UnityEngine;

// 3. 육체 이식(장착) 및 모디파이어 적용을 전담하는 클래스
[RequireComponent(typeof(PlayerStats))]
public class GraftManager : MonoBehaviour
{
    private PlayerStats playerStats;
    
    // 현재 이식된 육체들을 추적하기 위한 리스트
    private List<GraftData> equippedGrafts = new List<GraftData>();

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    // 육체 이식 로직
    public void EquipGraft(GraftData graft)
    {
        if (equippedGrafts.Contains(graft)) return;

        equippedGrafts.Add(graft);

        foreach (var mod in graft.modifiers)
        {
            // source를 현재 graft로 지정하여 추가
            var newMod = new StatModifier(mod.statType, mod.value, mod.type, graft);
            playerStats.GetStat(mod.statType).AddModifier(newMod);
        }
    }

    // 육체 적출 로직
    public void UnequipGraft(GraftData graft)
    {
        if (!equippedGrafts.Contains(graft)) return;

        equippedGrafts.Remove(graft);

        // PlayerStats에서 모든 스탯을 순회하며 해당 graft가 소스인 모디파이어 제거
        foreach (StatType type in System.Enum.GetValues(typeof(StatType)))
        {
            // Enum에 정의되었지만 초기화되지 않은 스탯이 있을 수 있으므로 예외 처리 필요 시 추가
            Stat targetStat = playerStats.GetStat(type);
            if (targetStat != null)
            {
                targetStat.RemoveAllModifiersFromSource(graft);
            }
        }
    }
}