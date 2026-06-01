using System.Collections;
using UnityEngine;

/// <summary>
/// 괴이 격리 프로토콜 - 사운드 매니저
/// BGM(앰비언트 드론) + SFX(UI/전투/이벤트) 통합 관리
/// Assets/Audio/ 폴더에 파일 넣고 Inspector에서 슬롯에 할당하면 즉시 작동
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("BGM - Assets/Audio/BGM/")]
    public AudioClip bgmAmbient;
    public AudioClip bgmBossAlert;
    [Range(0f, 1f)] public float bgmVolume = 0.4f;
    public float bgmFadeDuration = 1.5f;

    [Header("SFX UI - Assets/Audio/SFX/UI/")]
    public AudioClip sfxButtonClick;
    public AudioClip sfxGacha;
    public AudioClip sfxSynthesis;
    public AudioClip sfxError;
    public AudioClip sfxEquip;
    public AudioClip sfxUnequip;

    [Header("SFX Combat - Assets/Audio/SFX/Combat/")]
    public AudioClip sfxEnemyHit;
    public AudioClip sfxEnemyDeath;
    public AudioClip sfxPlayerHit;

    [Header("SFX Event - Assets/Audio/SFX/Event/")]
    public AudioClip sfxBossWarning;
    public AudioClip sfxStageUp;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    private AudioSource _bgmSource;
    private AudioSource _sfxSource;
    private Coroutine _fadeRoutine;
    private float _lastEnemyHitTime; // AOE 동시 다발 SFX 포화 방지

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.volume = bgmVolume;
        _bgmSource.spatialBlend = 0f;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
        _sfxSource.volume = sfxVolume;
        _sfxSource.spatialBlend = 0f;
    }

    private void Start()
    {
        // 저장된 볼륨 설정 불러오기
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", bgmVolume);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", sfxVolume);
        PlayBGM(bgmAmbient);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (_bgmSource.clip == clip && _bgmSource.isPlaying) return;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(CrossfadeBGM(clip));
    }

    public void StopBGM()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeOut());
    }

    public void SetBGMVolume(float vol) { bgmVolume = vol; _bgmSource.volume = vol; }
    public void SetSFXVolume(float vol) { sfxVolume = vol; _sfxSource.volume = vol; }

    private IEnumerator CrossfadeBGM(AudioClip next)
    {
        float half = bgmFadeDuration * 0.5f;
        if (_bgmSource.isPlaying)
        {
            float start = _bgmSource.volume;
            for (float t = 0; t < half; t += Time.deltaTime)
            {
                _bgmSource.volume = Mathf.Lerp(start, 0f, t / half);
                yield return null;
            }
        }
        _bgmSource.clip = next;
        _bgmSource.Play();
        for (float t = 0; t < half; t += Time.deltaTime)
        {
            _bgmSource.volume = Mathf.Lerp(0f, bgmVolume, t / half);
            yield return null;
        }
        _bgmSource.volume = bgmVolume;
    }

    private IEnumerator FadeOut()
    {
        float start = _bgmSource.volume;
        for (float t = 0; t < bgmFadeDuration; t += Time.deltaTime)
        {
            _bgmSource.volume = Mathf.Lerp(start, 0f, t / bgmFadeDuration);
            yield return null;
        }
        _bgmSource.Stop();
        _bgmSource.volume = bgmVolume;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
#if UNITY_EDITOR
            // 에디터에서만 경고 — 릴리즈 빌드에서는 조용히 스킵
            // GameLog.Warn("[SoundManager] AudioClip 미연결 — Assets/Audio/ 폴더에 파일 추가 필요");
#endif
            return;
        }
        _sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlayButtonClick() => PlaySFX(sfxButtonClick);
    public void PlayGacha()       => PlaySFX(sfxGacha);
    public void PlaySynthesis()   => PlaySFX(sfxSynthesis);
    public void PlayError()       => PlaySFX(sfxError);
    public void PlayEquip()       => PlaySFX(sfxEquip);
    public void PlayUnequip()     => PlaySFX(sfxUnequip);
    public void PlayEnemyHit()
    {
        // AOE 다중 적중 시 0.05초 이내 중복 재생 방지
        if (Time.time - _lastEnemyHitTime < 0.05f) return;
        _lastEnemyHitTime = Time.time;
        PlaySFX(sfxEnemyHit);
    }
    public void PlayEnemyDeath()  => PlaySFX(sfxEnemyDeath);
    public void PlayPlayerHit()   => PlaySFX(sfxPlayerHit);
    public void PlayBossWarning() { PlaySFX(sfxBossWarning); PlayBGM(bgmBossAlert); }
    public void PlayStageUp()     => PlaySFX(sfxStageUp);
}