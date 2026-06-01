using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BossHPBarUI : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image       hpFill;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI hpValueText;

    private StageManager       _sm;
    private StageEnemySpawner  _spawner;
    private EnemyStats         _bossStats;

    private void Start()
    {
        _sm = StageManager.Instance ?? FindObjectOfType<StageManager>();
        if (_sm != null) _sm.OnPhaseChanged += OnPhaseChanged;

        _spawner = FindObjectOfType<StageEnemySpawner>();
        if (_spawner != null) _spawner.OnBossSpawned += OnBossSpawned;
        else GameLog.Warn("[BossHPBarUI] StageEnemySpawner 없음");

        SetVisible(false);
    }

    private void OnDestroy()
    {
        if (_sm      != null) _sm.OnPhaseChanged    -= OnPhaseChanged;
        if (_spawner != null) _spawner.OnBossSpawned -= OnBossSpawned;
        UnsubscribeBoss();
    }

    private void OnPhaseChanged(StagePhase phase)
    {
        if (phase != StagePhase.Boss) { UnsubscribeBoss(); SetVisible(false); }
    }

    // 스폰 완료 이벤트로 보스 즉시 연결
    private void OnBossSpawned(EnemyStats bossStats)
    {
        if (bossStats == null) return;
        UnsubscribeBoss();
        _bossStats = bossStats;
        _bossStats.OnStatsChanged += RefreshHP;
        _bossStats.OnDied         += OnBossDied;

        if (bossNameText != null && _bossStats.data != null)
            bossNameText.text = _bossStats.data.enemyName;

        SetVisible(true);
        RefreshHP();
    }

    private void RefreshHP()
    {
        if (_bossStats == null) return;
        float ratio = _bossStats.MaxHP > 0f ? _bossStats.curHP / _bossStats.MaxHP : 0f;
        if (hpFill     != null) hpFill.fillAmount = Mathf.Clamp01(ratio);
        if (hpValueText != null) hpValueText.text =
            Mathf.CeilToInt(_bossStats.curHP) + " / " + Mathf.CeilToInt(_bossStats.MaxHP);
    }

    private void OnBossDied() { UnsubscribeBoss(); SetVisible(false); }

    private void UnsubscribeBoss()
    {
        if (_bossStats == null) return;
        _bossStats.OnStatsChanged -= RefreshHP;
        _bossStats.OnDied         -= OnBossDied;
        _bossStats = null;
    }

    private void SetVisible(bool show)
    {
        if (canvasGroup == null) return;
        canvasGroup.alpha          = show ? 1f : 0f;
        canvasGroup.interactable   = show;
        canvasGroup.blocksRaycasts = false;
    }
}