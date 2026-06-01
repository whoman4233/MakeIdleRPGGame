using UnityEngine;

/// <summary>
/// Panel_Graft 안의 GraftSlotUI 5개를 PlayerGraft에 연결하는 초기화 컴포넌트.
/// Panel_Graft 또는 EquipSlotArea에 붙여두면 Start()에서 자동 처리.
/// </summary>
public class GraftSlotAreaInitializer : MonoBehaviour
{
    private void Start()
    {
        if (PlayerRef.Instance == null) return;
        var pg = PlayerRef.Instance.GetComponent<PlayerGraft>();
        if (pg == null) return;

        var slots = GetComponentsInChildren<GraftSlotUI>(true);
        foreach (var slot in slots)
            slot.Init(pg);
    }
}