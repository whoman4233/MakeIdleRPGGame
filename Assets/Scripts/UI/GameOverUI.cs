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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
            panelGroup.gameObject.SetActive(false);
        }

        if (continueButton != null)
            continueButton.onClick.AddListener(OnClickContinue);

        // Player 죽음 이벤트 구독
        if (PlayerRef.Instance != null && PlayerRef.Instance.Stats != null)
        {
            PlayerRef.Instance.Stats.OnDied += OnPlayerDied;
        }
        else
        {
            Debug.LogWarning("[GameOverUI] PlayerRef 또는 PlayerStats를 찾지 못했습니다.");
        }
    }

    private void OnDestroy()
    {
        if (PlayerRef.Instance != null && PlayerRef.Instance.Stats != null)
        {
            PlayerRef.Instance.Stats.OnDied -= OnPlayerDied;
        }
    }

    private void OnPlayerDied()
    {
        Show(defaultMessage);
    }

    public void Show(string msg)
    {
        if (panelGroup == null) return;

        if (messageText != null)
            messageText.text = msg;

        panelGroup.gameObject.SetActive(true);
        panelGroup.alpha = 1f;

        // 게임 일시정지 하고 싶으면:
        // Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (panelGroup == null) return;

        panelGroup.alpha = 0f;
        panelGroup.gameObject.SetActive(false);
    }

    private void OnClickContinue()
    {
        // Time.timeScale = 1f; // 일시정지 썼으면 해제

        // 플레이어 부활
        if (PlayerRef.Instance != null && PlayerRef.Instance.Stats != null)
        {
            PlayerRef.Instance.Stats.ReviveFull();
        }

        Hide();
    }
}
