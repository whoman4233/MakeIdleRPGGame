using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUi : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI slotText;   // 이식 부위(Head/Core/Arm 등) 표시용
    public Button equipButton;

    private GraftData _data;
    private PlayerGraft _playerGraft;

    public void Init(GraftData data, PlayerGraft playerGraft)
    {
        _data = data;
        _playerGraft = playerGraft;

        if (nameText != null)
            nameText.text = data != null ? data.graftName : "-"; // displayName을 graftName으로 변경

        if (slotText != null)
            slotText.text = data != null ? data.slotType.ToString() : "";

        // 주의: GraftData 스크립트에 'public Sprite icon;' 과 'public GraftSlotType slotType;' 변수가 선언되어 있어야 합니다.
        if (icon != null && data != null && data.icon != null)
            icon.sprite = data.icon;

        if (equipButton != null)
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(OnClickEquip);
        }
    }

    private void OnClickEquip()
    {
        if (_data == null || _playerGraft == null)
            return;

        _playerGraft.Equip(_data);
        
        // 깨진 한글 로그를 세계관에 맞는 연출 텍스트로 변경
        Debug.Log($"[InventoryUI] 괴이 육체 이식 완료: {_data.graftName} -> {_data.slotType} 슬롯");
    }
}