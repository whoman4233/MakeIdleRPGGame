using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 심층 마일스톤 한 줄. 라벨/보상/상태(미도달·수령 가능·완료)를 표시하고 수령 버튼을 제공한다.
/// (UpgradeItemUI 패턴)
/// </summary>
public class DepthMilestoneItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI statusText;
    public Button claimButton;

    private DepthMilestone _milestone;
    private DepthMilestoneManager _manager;

    public void Init(DepthMilestoneManager manager, DepthMilestone milestone)
    {
        _manager = manager;
        _milestone = milestone;

        if (claimButton != null)
        {
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(OnClick);
        }
        Refresh();
    }

    private void OnClick()
    {
        if (_manager == null || _milestone == null) return;
        if (_manager.Claim(_milestone.depth))
            SoundManager.Instance?.PlayButtonClick();
    }

    public void Refresh()
    {
        if (_manager == null || _milestone == null) return;

        if (titleText != null)
            titleText.text = _milestone.DisplayLabel;

        if (rewardText != null)
        {
            string reward = "";
            if (_milestone.dataReward != 0)
                reward += "데이터 " + _milestone.dataReward.ToString("N0");
            if (_milestone.coreReward != 0)
            {
                if (reward.Length > 0) reward += "   ";
                reward += "코어 " + _milestone.coreReward.ToString("N0");
            }
            rewardText.text = reward;
        }

        bool claimed   = _manager.IsClaimed(_milestone.depth);
        bool claimable = _manager.IsClaimable(_milestone);

        if (statusText != null)
            statusText.text = claimed ? "수령 완료" : (claimable ? "수령 가능" : "미도달");

        if (claimButton != null)
            claimButton.interactable = claimable;
    }
}
