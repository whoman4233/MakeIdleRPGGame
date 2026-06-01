using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageEffect : MonoBehaviour
{
    [Header("피격 플래시 (화면 적색)")]
    [SerializeField] private Image damageImage;
    [SerializeField] private float flashPeakAlpha = 0.6f;
    [SerializeField] private float flashInTime    = 0.04f;
    [SerializeField] private float flashOutTime   = 0.18f;
    [SerializeField] private float glitchStrength = 0.55f;
    [SerializeField] private float glitchDuration = 0.3f;

    [Header("무적 깜빡임")]
    [Tooltip("무적 중 플레이어 스프라이트 깜빡임. 자동으로 Player 하위 SpriteRenderer 탐색.")]
    [SerializeField] private float blinkInterval  = 0.06f;  // 깜빡임 간격
    [SerializeField] private float blinkMinAlpha  = 0.2f;   // 최소 알파

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   damageClip;

    private HealthSystem    _healthSystem;
    private SpriteRenderer  _playerSprite;
    private Coroutine       _flashRoutine;
    private Coroutine       _blinkRoutine;

    private IEnumerator Start()
    {
        yield return null;

        _healthSystem = PlayerRef.Instance != null
            ? PlayerRef.Instance.GetComponent<HealthSystem>()
            : FindObjectOfType<HealthSystem>();

        if (_healthSystem != null)
        {
            _healthSystem.OnDamaged       += TriggerFlash;
            _healthSystem.OnHealthChanged += CheckInvincibility;
        }
        else GameLog.Warn("[PlayerDamageEffect] HealthSystem 없음");

        // Player 하위 SpriteRenderer 자동 탐색
        if (PlayerRef.Instance != null)
            _playerSprite = PlayerRef.Instance.GetComponentInChildren<SpriteRenderer>();

        if (damageImage != null)
            damageImage.color = new Color(1f, 0f, 0f, 0f);
    }

    private void OnDestroy()
    {
        if (_healthSystem != null)
        {
            _healthSystem.OnDamaged       -= TriggerFlash;
            _healthSystem.OnHealthChanged -= CheckInvincibility;
        }
    }

    // 피격 화면 플래시 + 글리치
    private void TriggerFlash()
    {
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashRoutine());
        CCTVEffectController.Instance?.TriggerGlitch(glitchDuration, glitchStrength);
        StartBlink();
    }

    // HP가 변할 때마다 무적 여부 체크 → 무적 종료 시 깜빡임 중단
    private void CheckInvincibility()
    {
        if (_healthSystem == null || _playerSprite == null) return;
        // 무적 종료 감지: 피격 이후 invincibleUntil 지났으면 알파 복원
        // 타이머 직접 노출 없이 IsAlive + 시간 흐름으로만 판단
    }

    private void StartBlink()
    {
        if (_playerSprite == null) return;
        if (_blinkRoutine != null) StopCoroutine(_blinkRoutine);
        _blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine()
    {
        if (_healthSystem == null || _playerSprite == null) yield break;

        // invincibilityDuration 동안 깜빡임
        // HealthSystem의 _invincibleUntil에 직접 접근 대신 public 프로퍼티 활용
        float endTime = Time.time + _healthSystem.InvincibilityDuration;
        bool keepRunning = true;
        while (keepRunning && Time.time < endTime)
        {
            SetSpriteAlpha(blinkMinAlpha);
            yield return new WaitForSeconds(blinkInterval);
            SetSpriteAlpha(1f);
            yield return new WaitForSeconds(blinkInterval);
            if (Time.time >= endTime) keepRunning = false;
        }
        SetSpriteAlpha(1f);  // 무적 종료 시 완전 불투명 복원
        _blinkRoutine = null;
    }

    private IEnumerator FlashRoutine()
    {
        if (audioSource != null && damageClip != null)
            audioSource.PlayOneShot(damageClip);

        float t = 0f;
        bool keepRunning = true;
        while (keepRunning)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(0f, flashPeakAlpha, t / flashInTime));
            if (t >= flashInTime) keepRunning = false;
            else yield return null;
        }
        t = 0f; keepRunning = true;
        while (keepRunning)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(flashPeakAlpha, 0f, t / flashOutTime));
            if (t >= flashOutTime) keepRunning = false;
            else yield return null;
        }
        SetAlpha(0f);
        _flashRoutine = null;
    }

    private void SetAlpha(float a)
    {
        if (damageImage == null) return;
        var c = damageImage.color; c.a = a; damageImage.color = c;
    }

    private void SetSpriteAlpha(float a)
    {
        if (_playerSprite == null) return;
        var c = _playerSprite.color; c.a = a; _playerSprite.color = c;
    }
}
