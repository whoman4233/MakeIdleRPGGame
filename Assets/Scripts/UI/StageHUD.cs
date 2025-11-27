using UnityEngine;
using TMPro;

public class StageHUD : MonoBehaviour
{
    public TextMeshProUGUI stageNameText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI killCountText;

    private StageManager _stageManager;

    private void Start()
    {
        _stageManager = StageManager.Instance;
        if (_stageManager == null)
        {
            Debug.LogWarning("[StageHUD] StageManager 인스턴스를 찾지 못했습니다.");
            return;
        }

        // 여기서 이벤트 구독
        _stageManager.OnStageChanged += RefreshAll;
        _stageManager.OnKillCountChanged += RefreshKillCount;
        _stageManager.OnPhaseChanged += OnPhaseChanged;

        // 초기 UI 갱신
        RefreshAll();
    }

    private void OnDestroy()
    {
        if (_stageManager == null) return;

        _stageManager.OnStageChanged -= RefreshAll;
        _stageManager.OnKillCountChanged -= RefreshKillCount;
        _stageManager.OnPhaseChanged -= OnPhaseChanged;
    }

    private void RefreshAll()
    {
        RefreshStageName();
        RefreshPhase();
        RefreshKillCount();
    }

    private void RefreshStageName()
    {
        if (stageNameText == null || _stageManager == null) return;

        var stage = _stageManager.CurrentStage;
        stageNameText.text = stage != null ? stage.displayName : "No Stage";
    }

    private void RefreshPhase()
    {
        if (phaseText == null || _stageManager == null) return;

        phaseText.text = _stageManager.CurrentPhase.ToString();
    }

    private void RefreshKillCount()
    {
        if (killCountText == null || _stageManager == null) return;

        var stage = _stageManager.CurrentStage;
        if (stage == null)
        {
            killCountText.text = "-";
            return;
        }

        int cur = _stageManager.GetCurrentKillCount();
        int need = stage.normalKillToSummonBoss;

        // 디버그용 로그 한 번
        // Debug.Log($"[StageHUD] RefreshKillCount: {cur}/{need}, phase={_stageManager.CurrentPhase}");

        if (_stageManager.CurrentPhase == StagePhase.Boss || need <= 0)
        {
            killCountText.text = $"{cur}";
        }
        else
        {
            killCountText.text = $"{cur} / {need}";
        }
    }

    private void OnPhaseChanged(StagePhase phase)
    {
        RefreshPhase();
        RefreshKillCount();
    }
}
