using UnityEngine;
using UnityEngine.UI;

public class MainUITabController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject[] tabPanels; 

    [Header("Tab Buttons")]
    public Button[] tabButtons;

    [Header("Inventory Refs")]
    public InventoryUI inventoryUI;         // 하단 고정 인벤토리 스크립트
    public GameObject inventoryPanelObject; // 인벤토리 전체를 담고 있는 부모 오브젝트
    public SynthesisUI synthesisUI;         // 합성 로직 참조용

    private void Start()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i; 
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }

        SwitchTab(0);
    }

    public void SwitchTab(int tabIndex)
    {
        // 1. 상단 메인 패널들 교체
        for (int i = 0; i < tabPanels.Length; i++)
        {
            if (tabPanels[i] != null)
            {
                tabPanels[i].SetActive(i == tabIndex);
            }
        }

        // 2. 인벤토리 표시 및 모드 결정 로직
        // 인덱스 가정: 0:장비, 1:합성, 2:추출 (이 3개일 때만 인벤토리 표시)
        bool useInventory = (tabIndex >= 0 && tabIndex <= 2);
        
        if (inventoryPanelObject != null)
        {
            inventoryPanelObject.SetActive(useInventory);
        }

        if (useInventory && inventoryUI != null)
        {
            if (tabIndex == 1) // 합성 탭일 때
            {
                // 인벤토리를 '합성 재료 선택' 모드로 열기
                inventoryUI.OpenForSelection(synthesisUI.OnGraftReceived);
            }
            else // 장비나 추출 탭일 때
            {
                // 인벤토리를 '일반 장착' 모드로 열기
                inventoryUI.OpenForEquip();
            }
        }
    }
}