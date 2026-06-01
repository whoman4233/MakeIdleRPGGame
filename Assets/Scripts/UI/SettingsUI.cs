using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 환경설정 UI - BGM/SFX 볼륨 슬라이더
/// Canvas 내 SettingsPanel에 붙여서 사용
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("BGM")]
    public Slider bgmSlider;
    public TextMeshProUGUI bgmValueText;

    [Header("SFX")]
    public Slider sfxSlider;
    public TextMeshProUGUI sfxValueText;

    [Header("Panel")]
    public GameObject settingsPanel;

    private void Start()
    {
        // 현재 볼륨으로 슬라이더 초기화
        if (bgmSlider != null)
        {
            bgmSlider.minValue = 0f;
            bgmSlider.maxValue = 1f;
            bgmSlider.value = SoundManager.Instance != null ? SoundManager.Instance.bgmVolume : 0.4f;
            bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = SoundManager.Instance != null ? SoundManager.Instance.sfxVolume : 0.8f;
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }

        RefreshTexts();

        // 처음엔 닫혀있게
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        bgmSlider?.onValueChanged.RemoveListener(OnBGMChanged);
        sfxSlider?.onValueChanged.RemoveListener(OnSFXChanged);
    }

    public void ToggleSettings()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(!settingsPanel.activeSelf);
        SoundManager.Instance?.PlayButtonClick();
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnBGMChanged(float value)
    {
        SoundManager.Instance?.SetBGMVolume(value);
        RefreshTexts();
        // 볼륨 설정 저장
        PlayerPrefs.SetFloat("BGMVolume", value);
        PlayerPrefs.Save();
    }

    private void OnSFXChanged(float value)
    {
        SoundManager.Instance?.SetSFXVolume(value);
        RefreshTexts();
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }

    private void RefreshTexts()
    {
        if (bgmValueText != null && bgmSlider != null)
            bgmValueText.text = $"{Mathf.RoundToInt(bgmSlider.value * 100)}%";
        if (sfxValueText != null && sfxSlider != null)
            sfxValueText.text = $"{Mathf.RoundToInt(sfxSlider.value * 100)}%";
    }
}