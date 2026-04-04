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

    private GraftData _data;
    private Action<GraftData> _onClickAction; // 클릭 이벤트를 외부로 위임하기 위한 델리게이트

    // Init 매개변수에 클릭 시 실행할 액션을 추가로 받습니다.
    public void Init(GraftData data, PlayerGraft playerGraft, Action<GraftData> onClickAction)
    {
        _data = data;
        _onClickAction = onClickAction;

        if (nameText != null)
            nameText.text = data != null ? data.graftName : "-";

        if (slotText != null)
            slotText.text = data != null ? data.slotType.ToString() : "";

        if (icon != null && data != null && data.icon != null)
            icon.sprite = data.icon;

        if (equipButton != null)
        {
            equipButton.onClick.RemoveAllListeners();
            equipButton.onClick.AddListener(OnClickItem);
        }
    }

    private void OnClickItem()
    {
        if (_data == null) return;
        
        // 아이템이 클릭되면 넘겨받은 외부 함수를 실행합니다.
        _onClickAction?.Invoke(_data);
    }
}