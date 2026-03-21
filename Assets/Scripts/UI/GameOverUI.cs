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

        // Player ���� �̺�Ʈ ����
        if (PlayerRef.Instance != null && PlayerRef.Instance.Stats != null)
        {
            PlayerRef.Instance.Health.OnDied += OnPlayerDied;
        }
        else
        {
            Debug.LogWarning("[GameOverUI] PlayerRef Ǵ PlayerStats ã ߽ϴ.");
        }
    }

    private void OnDestroy()
    {
        if (PlayerRef.Instance != null && PlayerRef.Instance.Stats != null)
        {
            PlayerRef.Instance.Health.OnDied -= OnPlayerDied;
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

        // ���� �Ͻ����� �ϰ� ������:
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
        // Time.timeScale = 1f; // 일시정지를 사용했다면 해제

        // 플레이어 부활 로직 호출
        if (PlayerRef.Instance != null && PlayerRef.Instance.Health != null)
        {
            // Stats가 아닌 Health 컴포넌트의 ReviveFull을 호출합니다.
            PlayerRef.Instance.Health.ReviveFull();
            
            // 만약 플레이어의 상태를 다시 MoveForward 등으로 바꿔야 한다면:
            // PlayerRef.Instance.Controller.ChangeState(PlayerStateType.MoveForward);
        }

        Hide();
    }
}
