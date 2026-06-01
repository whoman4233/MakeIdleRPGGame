using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyHitFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor    = Color.white;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private float glitchStrength = 0.3f;

    private SpriteRenderer _spriteRenderer;
    private EnemyStats     _stats;
    private Color          _baseColor;
    private Coroutine      _flashRoutine;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _stats          = GetComponent<EnemyStats>();
    }

    private void Start()
    {
        _stats.OnStatsChanged += RefreshBaseColor;
        RefreshBaseColor();
    }

    private void OnEnable()  { if (_stats != null) _stats.OnDamaged += TriggerFlash; }
    private void OnDisable() { if (_stats != null) _stats.OnDamaged -= TriggerFlash; StopFlash(); }
    private void OnDestroy() { if (_stats != null) _stats.OnStatsChanged -= RefreshBaseColor; }

    private void TriggerFlash()
    {
        if (_flashRoutine != null) StopCoroutine(_flashRoutine);
        _flashRoutine = StartCoroutine(FlashRoutine());

        // 보스 피격 시 글리치 강하게, 일반 적은 약하게
        bool isBoss = _stats.data != null && _stats.data.isBoss;
        float strength = isBoss ? glitchStrength * 2f : glitchStrength;
        float duration = isBoss ? 0.35f : 0.18f;
        CCTVEffectController.Instance?.TriggerGlitch(duration, strength);
    }

    private IEnumerator FlashRoutine()
    {
        _spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        _spriteRenderer.color = _baseColor;
        _flashRoutine = null;
    }

    private void StopFlash()
    {
        if (_flashRoutine != null) { StopCoroutine(_flashRoutine); _flashRoutine = null; }
        if (_spriteRenderer != null) _spriteRenderer.color = _baseColor;
    }

    private void RefreshBaseColor()
    {
        _baseColor = (_stats != null && _stats.data != null) ? _stats.data.visualColor : Color.white;
    }
}