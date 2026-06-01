using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    public CanvasGroup panelGroup;
    public TextMeshProUGUI messageText;
    public Button continueButton;
    public string defaultMessage = "YOU DIED";

    private HealthSystem _healthSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private IEnumerator Start()
    {
        if (panelGroup != null) { panelGroup.alpha = 0f; panelGroup.gameObject.SetActive(false); }
        if (continueButton != null) continueButton.onClick.AddListener(OnClickContinue);

        yield return null;

        var playerRef = PlayerRef.Instance ?? FindObjectOfType<PlayerRef>();
        if (playerRef != null)
            _healthSystem = playerRef.Health ?? playerRef.GetComponent<HealthSystem>();

        if (_healthSystem != null)
            _healthSystem.OnDied += OnPlayerDied;
        else
            GameLog.Warn("[GameOverUI] HealthSystem 없음");
    }

    private void OnDestroy()
    {
        if (_healthSystem != null) _healthSystem.OnDied -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        // StageManager에 플레이어 사망 알림 (보스페이즈 → 노멀 복귀)
        StageManager.Instance?.OnPlayerDied();
        Show(defaultMessage);
    }

    public void Show(string msg)
    {
        if (panelGroup == null) return;
        if (messageText != null) messageText.text = msg;
        panelGroup.gameObject.SetActive(true);
        panelGroup.alpha = 1f;
    }

    public void Hide()
    {
        if (panelGroup == null) return;
        panelGroup.alpha = 0f;
        panelGroup.gameObject.SetActive(false);
    }

    private void OnClickContinue()
    {
        SoundManager.Instance?.PlayButtonClick();

        var ads = AdManager.Instance;
        if (ads != null && ads.IsAdReady())
        {
            // 광고 시청 완료 시에만 부활 (취소/실패 시 부활하지 않음)
            ads.ShowRewardedAd(onReward: Revive);
        }
        else
        {
            // 광고 미준비 시 폴백: 즉시 부활 (광고 없는 환경/에디터 대비)
            Revive();
        }
    }

    private void Revive()
    {
        // 체력 회복
        if (_healthSystem != null) _healthSystem.ReviveFull();
        // 플레이어 AI 재개
        var ctrl = PlayerRef.Instance?.Controller;
        if (ctrl != null) ctrl.ChangeState(PlayerStateType.MoveForward);
        Hide();
    }
}
