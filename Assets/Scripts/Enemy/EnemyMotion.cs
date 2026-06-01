using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyMotion : MonoBehaviour
{
    [Header("Walk Sway")]
    [SerializeField] private float swayAmplitude  = 0.06f;
    [SerializeField] private float swayFrequency  = 6f;
    [SerializeField] private float tiltAngle      = 8f;
    [SerializeField] private float squashAmount   = 0.04f;

    [Header("Attack")]
    [SerializeField] private float attackDashDist = 0.35f;
    [SerializeField] private float attackDashTime = 0.08f;
    [SerializeField] private float attackBounce   = 0.12f;

    [Header("Death")]
    [SerializeField] private float deathSinkDist  = 0.5f;
    [SerializeField] private float deathFadeTime  = 0.45f;

    [Header("Boss Multiplier")]
    [SerializeField] private float bossSwayMult   = 1.6f;
    [SerializeField] private float bossTiltMult   = 0.5f;

    private EnemyStats      _stats;
    private SpriteRenderer  _sr;
    private EnemyController _ctrl;

    private float   _yOffset;
    private float   _baseY;
    private float   _appliedXOffset;  // 지난 프레임에 X에 얹은 넉백/대시 오프셋 (전진과 분리)
    private Vector3 _baseScale;
    private float   _swayTimer;
    private bool    _isDead;
    private bool    _scaleCaptured;
    private float   _xOffset;

    private Coroutine _atkCo;
    private Coroutine _dieCo;
    private Coroutine _hitCo;

    private void Awake()
    {
        _stats = GetComponent<EnemyStats>();
        _sr    = GetComponent<SpriteRenderer>();
        _ctrl  = GetComponent<EnemyController>();
    }

    private void OnEnable()
    {
        _isDead        = false;
        _scaleCaptured = false;
        _swayTimer     = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        _baseY         = transform.localPosition.y;
        _appliedXOffset = 0f;
        _baseScale     = transform.localScale;
        _yOffset       = 0f;
        _xOffset       = 0f;

        _stats.OnDamaged += OnHit;
    }

    private void OnDisable()
    {
        _stats.OnDamaged -= OnHit;
        StopAllCoroutines();
        _atkCo = null;
        _dieCo = null;
        _hitCo = null;
        DoReset();
    }

    private void LateUpdate()
    {
        if (_isDead) return;
        if (!_scaleCaptured) { _baseScale = transform.localScale; _scaleCaptured = true; }

        bool  boss = _stats.data != null && _stats.data.isBoss;
        float amp  = swayAmplitude * (boss ? bossSwayMult : 1f);
        float freq = swayFrequency * (boss ? 1.3f : 1f);
        float tilt = tiltAngle     * (boss ? bossTiltMult : 1f);

        bool moving = _ctrl != null && _stats.IsAlive
                   && PlayerRef.Instance != null
                   && transform.position.x > PlayerRef.Instance.transform.position.x + _stats.AttackRange * 0.8f;

        if (_atkCo == null)
        {
            _swayTimer += Time.deltaTime * (moving ? freq : freq * 0.4f);
            float sinVal = Mathf.Sin(_swayTimer);

            if (moving)
            {
                _yOffset = sinVal * amp;
                float sqY = 1f - squashAmount * Mathf.Sin(_swayTimer * 2f);
                float sqX = 1f + squashAmount * Mathf.Sin(_swayTimer * 2f) * 0.5f;
                transform.localScale    = new Vector3(_baseScale.x * sqX, _baseScale.y * sqY, _baseScale.z);
                transform.localRotation = Quaternion.Euler(0f, 0f, -tilt * Mathf.Cos(_swayTimer));
            }
            else
            {
                _yOffset = sinVal * amp * 0.3f;
                transform.localScale    = _baseScale;
                transform.localRotation = Quaternion.identity;
            }

            // 전진(Translate)이 옮긴 현재 X는 보존하고, 넉백/대시 오프셋만 델타로 얹는다.
            // (지난 프레임 오프셋을 빼고 이번 프레임 오프셋을 더해 매 프레임 누적되지 않게 함)
            float curX = transform.localPosition.x - _appliedXOffset;
            transform.localPosition = new Vector3(curX + _xOffset, _baseY + _yOffset, transform.localPosition.z);
            _appliedXOffset = _xOffset;
        }
    }

    // ── 공격 ─────────────────────────────────────
    public void PlayAttack()
    {
        if (_isDead) return;
        DoKill(ref _atkCo);
        _atkCo = StartCoroutine(AtkCo());
    }

    private IEnumerator AtkCo()
    {
        float startX = 0f;
        float dashX  = -attackDashDist;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / attackDashTime;
            _xOffset = Mathf.Lerp(startX, dashX, Mathf.SmoothStep(0f, 1f, t));
            transform.localScale = new Vector3(_baseScale.x * 1.15f, _baseScale.y * 0.88f, _baseScale.z);
            yield return null;
        }
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / attackBounce;
            _xOffset = Mathf.Lerp(dashX, startX, Mathf.SmoothStep(0f, 1f, t));
            transform.localScale = Vector3.Lerp(
                new Vector3(_baseScale.x * 1.15f, _baseScale.y * 0.88f, _baseScale.z), _baseScale, t);
            yield return null;
        }
        _xOffset = 0f;
        transform.localScale = _baseScale;
        _atkCo = null;
    }

    // ── 피격 ─────────────────────────────────────
    private void OnHit()
    {
        if (_isDead) return;
        DoKill(ref _atkCo);
        DoKill(ref _hitCo);
        _xOffset = 0f;
        _hitCo = StartCoroutine(HitCo());
    }

    private IEnumerator HitCo()
    {
        float t = 0f;
        while (t < 1f) { t += Time.deltaTime / 0.05f; _xOffset = Mathf.Lerp(0f, 0.18f, t); yield return null; }
        t = 0f;
        while (t < 1f) { t += Time.deltaTime / 0.08f; _xOffset = Mathf.Lerp(0.18f, 0f, t); yield return null; }
        _xOffset = 0f;
        _hitCo = null;
    }

    // ── 사망 (Controller가 직접 호출, 완료 후 콜백) ──
    public void PlayDeath(Action onComplete)
    {
        _isDead = true;
        DoKill(ref _atkCo);
        if (_dieCo != null) StopCoroutine(_dieCo);
        _dieCo = StartCoroutine(DieCo(onComplete));
    }

    private IEnumerator DieCo(Action onComplete)
    {
        Color   c      = _sr.color;
        Vector3 ss     = transform.localScale;
        float   startY = transform.localPosition.y;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / deathFadeTime;
            float e = Mathf.SmoothStep(0f, 1f, t);
            var p = transform.localPosition;
            transform.localPosition = new Vector3(p.x, Mathf.Lerp(startY, startY - deathSinkDist, e), p.z);
            transform.localScale    = Vector3.Lerp(ss, ss * 0.6f, e);
            _sr.color = new Color(c.r, c.g, c.b, Mathf.Lerp(1f, 0f, e));
            yield return null;
        }
        _dieCo = null;
        onComplete?.Invoke();
    }

    // ── 유틸 ─────────────────────────────────────
    private void DoReset()
    {
        _scaleCaptured          = false;
        _yOffset                = 0f;
        _xOffset                = 0f;
        _appliedXOffset         = 0f;
        transform.localRotation = Quaternion.identity;
        if (_sr != null) _sr.color = Color.white;
    }

    private void DoKill(ref Coroutine co)
    {
        if (co == null) return;
        StopCoroutine(co);
        co = null;
    }
}