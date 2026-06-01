using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventoryItemUi : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI slotText;
    public Button equipButton;

    private GraftData         _data;
    private Action<GraftData> _onClickAction;

    // 슬롯 타입별 색상
    private static readonly Color COL_HEAD = new Color(0.9f, 0.5f, 0.5f);
    private static readonly Color COL_CORE = new Color(0.9f, 0.7f, 0.3f);
    private static readonly Color COL_ARML = new Color(0.4f, 0.8f, 0.6f);
    private static readonly Color COL_ARMR = new Color(0.4f, 0.7f, 0.9f);
    private static readonly Color COL_LEGS = new Color(0.7f, 0.5f, 0.9f);

    public void Init(GraftData data, PlayerGraft playerGraft, Action<GraftData> onClickAction)
    {
        _data          = data;
        _onClickAction = onClickAction;

        if (nameText != null)
            nameText.text = data != null ? data.graftName : "-";

        if (slotText != null && data != null)
        {
            slotText.text  = SlotLabel(data.slotType);
            slotText.color = SlotColor(data.slotType);
        }

        if (icon != null)
        {
            if (data != null && data.icon != null)
            {
                icon.sprite  = data.icon;
                icon.color   = Color.white;
                icon.enabled = true;
            }
            else
            {
                // 아이콘 없으면 슬롯 컬러 블록으로 대체
                icon.sprite  = null;
                icon.color   = data != null
                    ? SlotColor(data.slotType) * 0.4f + new Color(0,0,0,1f) * 0.6f
                    : new Color(0.2f, 0.2f, 0.2f, 1f);
                icon.enabled = true;
            }
        }

        if (equipButton != null)
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(OnClickItem);
        }
    }

    private void OnClickItem()
    {
        if (_data == null) return;
        _onClickAction?.Invoke(_data);
    }

    private static string SlotLabel(GraftSlotType t) => t switch
    {
        GraftSlotType.Head => "HEAD",
        GraftSlotType.Core => "CORE",
        GraftSlotType.ArmL => "ARM·L",
        GraftSlotType.ArmR => "ARM·R",
        GraftSlotType.Legs => "LEGS",
        _ => t.ToString()
    };

    private static Color SlotColor(GraftSlotType t) => t switch
    {
        GraftSlotType.Head => COL_HEAD,
        GraftSlotType.Core => COL_CORE,
        GraftSlotType.ArmL => COL_ARML,
        GraftSlotType.ArmR => COL_ARMR,
        GraftSlotType.Legs => COL_LEGS,
        _ => Color.white
    };
}