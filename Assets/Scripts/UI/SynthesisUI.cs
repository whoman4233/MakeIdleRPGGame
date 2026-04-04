using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SynthesisUI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] slotImages = new Image[3]; 
    public Button synthesizeButton;

    [Header("Settings")]
    public Sprite defaultSlotSprite; 

    private List<GraftData> _selectedGrafts = new List<GraftData>();

    private void Start()
    {
        if (synthesizeButton != null)
        {
            synthesizeButton.onClick.AddListener(OnSynthesizeClicked);
        }
        UpdateSlotsUI();
    }

    // ★ 에러 해결의 핵심: private을 public으로 변경했습니다.
    public void OnGraftReceived(GraftData selectedGraft)
    {
        ToggleGraftSelection(selectedGraft);
    }

    public void ToggleGraftSelection(GraftData graft)
    {
        if (graft == null) return;

        if (_selectedGrafts.Contains(graft))
        {
            _selectedGrafts.Remove(graft);
        }
        else 
        {
            if (_selectedGrafts.Count >= 3)
            {
                Debug.Log("[SynthesisUI] 이미 3개의 육체를 선택했습니다.");
                return;
            }
            _selectedGrafts.Add(graft);
        }
        UpdateSlotsUI();
    }

    private void UpdateSlotsUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] == null) continue;

            if (i < _selectedGrafts.Count)
            {
                // 주의: GraftData에 정의된 아이콘 변수명(예: icon 또는 graftIcon)에 맞춰주세요.
                slotImages[i].sprite = _selectedGrafts[i].icon; 
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].sprite = defaultSlotSprite;
                slotImages[i].color = defaultSlotSprite == null ? new Color(0, 0, 0, 0) : Color.white;
            }
        }

        if (synthesizeButton != null)
        {
            synthesizeButton.interactable = (_selectedGrafts.Count == 3);
        }
    }

    private void OnSynthesizeClicked()
    {
        if (_selectedGrafts.Count != 3) return;

        bool isSuccess = SynthesisManager.Instance.TrySynthesis(_selectedGrafts);
        
        if (isSuccess)
        {
            _selectedGrafts.Clear();
            UpdateSlotsUI();
        }
    }
}