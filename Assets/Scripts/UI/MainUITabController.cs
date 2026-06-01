using UnityEngine;
using UnityEngine.UI;

public class MainUITabController : MonoBehaviour
{
    [Header("Tab Panels")]
    public GameObject[] tabPanels;   // 0=Graft 1=Extraction 2=Synthesis 3=Gacha 4=Upgrade

    [Header("Tab Buttons")]
    public Button[] tabButtons;

    [Header("Synthesis Ref")]
    public SynthesisUI synthesisUI;

    private int _currentTab = -1;

    private void Start()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int idx = i;
            tabButtons[i].onClick.AddListener(() => SwitchTab(idx));
        }
        SwitchTab(0);
    }

    public void SwitchTab(int index)
    {
        SoundManager.Instance?.PlayButtonClick();
        _currentTab = index;

        for (int i = 0; i < tabPanels.Length; i++)
            if (tabPanels[i] != null)
                tabPanels[i].SetActive(i == index);

        // 합성 탭이면 인벤토리 선택 모드로 전환
        if (index == 2)
        {
            var invUI = tabPanels[2]?.GetComponentInChildren<InventoryUI>(true);
            if (invUI != null && synthesisUI != null)
                invUI.SetSelectionMode(synthesisUI.OnGraftReceived);
        }
        else
        {
            // Graft(0), Extraction(1) 탭은 장착 모드
            if (index <= 1)
            {
                var invUI = tabPanels[index]?.GetComponentInChildren<InventoryUI>(true);
                invUI?.SetEquipMode();
            }
        }
    }
}