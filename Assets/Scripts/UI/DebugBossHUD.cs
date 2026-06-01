using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 테스트용 보스 즉시 소환 HUD - F1 토글
/// StageManager.Instance 대신 FindObjectOfType으로 안전하게 참조
/// </summary>
public class DebugBossHUD : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button spawnBossButton;
    [SerializeField] private Button nextStageButton;
    [SerializeField] private Button resetPhaseButton;
    [SerializeField] private CanvasGroup canvasGroup;

    private bool _visible = true;
    private StageManager _sm;

    private void Start()
    {
        if (spawnBossButton)  spawnBossButton.onClick.AddListener(OnSpawnBoss);
        if (nextStageButton)  nextStageButton.onClick.AddListener(OnNextStage);
        if (resetPhaseButton) resetPhaseButton.onClick.AddListener(OnResetPhase);

        // Instance 패턴이 실패할 경우를 대비해 FindObjectOfType 사용
        _sm = StageManager.Instance ?? FindObjectOfType<StageManager>();

        if (_sm != null)
        {
            _sm.OnPhaseChanged     += HandlePhase;
            _sm.OnKillCountChanged += Refresh;
            _sm.OnStageChanged     += Refresh;
        }
        Refresh();
    }

    private void OnDestroy()
    {
        if (_sm != null)
        {
            _sm.OnPhaseChanged     -= HandlePhase;
            _sm.OnKillCountChanged -= Refresh;
            _sm.OnStageChanged     -= Refresh;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            _visible = !_visible;
            if (canvasGroup != null) canvasGroup.alpha = _visible ? 1f : 0f;
        }
    }

    private void HandlePhase(StagePhase p) { Refresh(); }

    private void OnSpawnBoss()
    {
        if (_sm == null) { GameLog.Warn("[DebugBossHUD] StageManager 없음"); return; }
        if (_sm.CurrentPhase == StagePhase.Boss) { GameLog.Log("[DebugBossHUD] 이미 Boss Phase"); return; }

        var m = typeof(StageManager).GetMethod("EnterBossPhase",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (m != null) { m.Invoke(_sm, null); GameLog.Log("[DebugBossHUD] 보스 소환!"); }
        else GameLog.Error("[DebugBossHUD] EnterBossPhase 없음");
    }

    private void OnNextStage()
    {
        if (_sm != null) _sm.GoToNextStage();
    }

    private void OnResetPhase()
    {
        if (_sm != null) _sm.OnPlayerDied();
    }

    private void Refresh()
    {
        if (statusText == null) return;
        if (_sm == null)
        {
            // 재시도
            _sm = StageManager.Instance ?? FindObjectOfType<StageManager>();
            if (_sm == null) { statusText.text = "StageManager 없음"; return; }
            _sm.OnPhaseChanged     += HandlePhase;
            _sm.OnKillCountChanged += Refresh;
            _sm.OnStageChanged     += Refresh;
        }

        string col;
        switch (_sm.CurrentPhase)
        {
            case StagePhase.Boss:    col = "#FF4444"; break;
            case StagePhase.Cleared: col = "#FFD700"; break;
            default:                 col = "#00FF88"; break;
        }
        string sn = _sm.CurrentStage != null ? _sm.CurrentStage.displayName : "없음";
        statusText.text = string.Format(
            "<b>[DEBUG HUD]</b>  F1 토글\nStage : {0} ({1})\nPhase : <color={2}>{3}</color>\nKills : {4}",
            _sm.CurrentStageIndex + 1, sn, col, _sm.CurrentPhase, _sm.GetCurrentKillCount());
    }
}