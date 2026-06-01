using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 코어 스킬 UI.
/// 원형 쿨다운 게이지 + 아이콘 + 남은시간 텍스트 + 탭 버튼.
/// TopGameView_Area 하위에 배치.
/// </summary>
public class CoreSkillUI : MonoBehaviour
{
    [Header("References")]
    public Button           skillButton;
    [Tooltip("스킬 아이콘 Image")]
    public Image            iconImage;
    [Tooltip("쿨다운 원형 오버레이 Image (fillMethod=Radial360, fillOrigin=Top)")]
    public Image            cooldownFill;
    [Tooltip("남은 쿨다운 초 텍스트 (없어도 됨)")]
    public TextMeshProUGUI  cooldownText;
    [Tooltip("스킬 없을 때 표시할 잠금 오브젝트 (없어도 됨)")]
    public GameObject       lockedOverlay;

    private CoreSkillHandler _handler;

    private void Start()
    {
        // CoreSkillHandler 탐색
        if (PlayerRef.Instance != null)
            _handler = PlayerRef.Instance.GetComponent<CoreSkillHandler>();

        if (_handler == null)
        {
            GameLog.Warn("[CoreSkillUI] CoreSkillHandler 없음");
            return;
        }

        _handler.OnSkillChanged  += RefreshSkill;
        _handler.OnCooldownTick  += RefreshCooldown;

        if (skillButton != null)
            skillButton.onClick.AddListener(OnButtonClick);

        RefreshSkill();
        RefreshCooldown();
    }

    private void OnDestroy()
    {
        if (_handler == null) return;
        _handler.OnSkillChanged  -= RefreshSkill;
        _handler.OnCooldownTick  -= RefreshCooldown;
    }

    // ── 스킬 교체 시 아이콘/잠금 갱신 ──────────────────
    private void RefreshSkill()
    {
        bool hasSkill = (_handler != null && _handler.CurrentSkill != null);

        if (iconImage != null)
        {
            iconImage.enabled = hasSkill;
            if (hasSkill && _handler.CurrentSkill.icon != null)
                iconImage.sprite = _handler.CurrentSkill.icon;
        }

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!hasSkill);

        if (skillButton != null)
            skillButton.interactable = hasSkill;

        RefreshCooldown();
    }

    // ── 매 프레임 쿨다운 게이지 갱신 ───────────────────
    private void RefreshCooldown()
    {
        if (_handler == null) return;
        float ratio = _handler.CooldownRatio;   // 1=방금 사용, 0=사용 가능

        // 원형 오버레이: ratio가 클수록 많이 채워져 있음
        if (cooldownFill != null)
            cooldownFill.fillAmount = ratio;

        // 텍스트
        if (cooldownText != null)
        {
            if (ratio > 0f)
            {
                float remain = _handler.RemainSeconds;
                cooldownText.text    = remain >= 1f
                    ? Mathf.CeilToInt(remain).ToString()
                    : remain.ToString("F1");
                cooldownText.enabled = true;
            }
            else
            {
                cooldownText.enabled = false;
            }
        }

        // 버튼 밝기: 준비됨=밝음, 쿨다운 중=어두움
        if (skillButton != null)
        {
            var colors = skillButton.colors;
            colors.normalColor = ratio <= 0f
                ? Color.white
                : new Color(0.5f, 0.5f, 0.5f, 1f);
            skillButton.colors = colors;
        }
    }

    // ── 버튼 탭 ────────────────────────────────────────
    private void OnButtonClick()
    {
        if (_handler == null || !_handler.IsReady) return;
        _handler.Use();
    }
}
