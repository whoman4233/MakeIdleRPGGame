using UnityEngine;

public class TogglePanel : MonoBehaviour
{
    public CanvasGroup panel;

    public void Toggle()
    {
        if (panel == null) return;

        bool show = panel.alpha <= 0f;
        panel.alpha = show ? 1f : 0f;
        panel.interactable = show;
        panel.blocksRaycasts = show;
    }
}
