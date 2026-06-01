using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 첫 실행 튜토리얼 - PlayerPrefs로 표시 여부 관리
/// 처음 실행 시 순서대로 안내 텍스트 표시
/// </summary>
public class TutorialManager : MonoBehaviour
{
    [Header("튜토리얼 패널")]
    public GameObject tutorialPanel;
    public TextMeshProUGUI tutorialText;
    public TextMeshProUGUI stepIndicatorText;
    public Button nextButton;
    public Button skipButton;

    private static readonly string[] Steps = {
        "[시스템 부팅]\n격리 구역 내 미확인 표본 다수 감지.\n자동 방어 프로토콜을 가동합니다.",
        "[전투 안내]\n표본들이 자동으로 접근합니다.\n범위 내 모든 표본에 자동 공격이 적용됩니다.",
        "[육체 이식 시스템]\n하단 [이식] 탭에서 수집한 표본 부위를\n신체 각 부위에 장착하면 능력치가 상승합니다.",
        "[표본 추출]\n[추출] 탭에서 관측 데이터를 소모해\n새로운 표본 부위를 획득할 수 있습니다.",
        "[생체 연성기]\n[합성] 탭에서 표본 3개를 갈아\n더 강력한 표본으로 재조합할 수 있습니다.",
        "[프로토콜 개시]\n격리 프로토콜을 시작합니다.\n모든 표본을 격리하십시오."
    };

    private int _currentStep = 0;
    private const string PREF_KEY = "TutorialDone";

    private void Start()
    {
        if (PlayerPrefs.GetInt(PREF_KEY, 0) == 1)
        {
            // 이미 본 튜토리얼 → 즉시 숨김
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            return;
        }
        StartTutorial();
    }

    private void StartTutorial()
    {
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        _currentStep = 0;
        ShowStep(_currentStep);

        if (nextButton != null) nextButton.onClick.AddListener(OnNext);
        if (skipButton != null) skipButton.onClick.AddListener(SkipTutorial);
    }

    private void ShowStep(int step)
    {
        if (tutorialText != null)
            tutorialText.text = Steps[step];
        if (stepIndicatorText != null)
            stepIndicatorText.text = $"{step + 1} / {Steps.Length}";
        if (nextButton != null)
        {
            var btnText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = step >= Steps.Length - 1 ? "[프로토콜 개시]" : "다음 >>";
        }
    }

    private void OnNext()
    {
        SoundManager.Instance?.PlayButtonClick();
        _currentStep++;
        if (_currentStep >= Steps.Length)
        {
            CompleteTutorial();
        }
        else
        {
            ShowStep(_currentStep);
        }
    }

    private void SkipTutorial()
    {
        SoundManager.Instance?.PlayButtonClick();
        CompleteTutorial();
    }

    private void CompleteTutorial()
    {
        PlayerPrefs.SetInt(PREF_KEY, 1);
        PlayerPrefs.Save();
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }

    // 에디터 테스트용: 튜토리얼 초기화
    [ContextMenu("튜토리얼 초기화 (재실행)")]
    public void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(PREF_KEY);
        PlayerPrefs.Save();
    }
}