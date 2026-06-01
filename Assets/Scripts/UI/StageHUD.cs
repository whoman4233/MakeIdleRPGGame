using UnityEngine;
using TMPro;

public class StageHUD : MonoBehaviour
{
    public TextMeshProUGUI stageNameText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI killCountText;

    private StageManager _sm;

    private void Start()
    {
        _sm = StageManager.Instance ?? FindObjectOfType<StageManager>();
        if (_sm == null) { GameLog.Warn("[StageHUD] StageManager 없음"); return; }

        _sm.OnStageChanged    += RefreshAll;
        _sm.OnKillCountChanged += RefreshKillCount;
        _sm.OnPhaseChanged    += OnPhaseChanged;
        RefreshAll();
    }

    private void OnDestroy()
    {
        if (_sm == null) return;
        _sm.OnStageChanged    -= RefreshAll;
        _sm.OnKillCountChanged -= RefreshKillCount;
        _sm.OnPhaseChanged    -= OnPhaseChanged;
    }

    private void RefreshAll() { RefreshStageName(); RefreshPhase(); RefreshKillCount(); }

    private void RefreshStageName()
    {
        if (stageNameText == null || _sm == null) return;
        var stage = _sm.CurrentStage;
        stageNameText.text = stage != null ? stage.displayName : "No Stage";
    }

    private void RefreshPhase()
    {
        if (phaseText == null || _sm == null) return;
        phaseText.text = _sm.CurrentPhase.ToString();
    }

    private void RefreshKillCount()
    {
        if (killCountText == null || _sm == null) return;
        var stage = _sm.CurrentStage;
        if (stage == null) { killCountText.text = "-"; return; }
        int cur  = _sm.GetCurrentKillCount();
        int need = stage.normalKillToSummonBoss;
        killCountText.text = (_sm.CurrentPhase == StagePhase.Boss || need <= 0)
            ? cur.ToString()
            : cur + " / " + need;
    }

    private void OnPhaseChanged(StagePhase phase) { RefreshPhase(); RefreshKillCount(); }
}