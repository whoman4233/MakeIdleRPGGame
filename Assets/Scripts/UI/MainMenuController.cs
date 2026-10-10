using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 메인(타이틀) 화면 컨트롤러. 게임 시작 / 종료, 시작 시 페이드 전환.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Tooltip("시작 시 로드할 본 게임 씬 이름")]
    public string gameSceneName = "MainScene";
    [Tooltip("페이드용 검은 오버레이 (CanvasGroup)")]
    public CanvasGroup fadeOverlay;
    public float fadeDuration = 0.5f;

    private bool _loading;

    public void LoadGame()
    {
        if (_loading) return;
        _loading = true;
        SoundManager.Instance?.PlayButtonClick();
        if (fadeOverlay != null) StartCoroutine(FadeAndLoad());
        else SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator FadeAndLoad()
    {
        fadeOverlay.blocksRaycasts = true;
        float tmr = 0f;
        while (tmr < fadeDuration)
        {
            tmr += Time.unscaledDeltaTime;
            fadeOverlay.alpha = Mathf.Clamp01(tmr / fadeDuration);
            yield return null;
        }
        fadeOverlay.alpha = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        SoundManager.Instance?.PlayButtonClick();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
