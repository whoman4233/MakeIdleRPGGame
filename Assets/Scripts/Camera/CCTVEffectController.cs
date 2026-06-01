using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CCTVEffectController : MonoBehaviour
{
    [Header("셰이더 연결")]
    [SerializeField] private Shader cctvShader;

    [Header("색수차 (Chromatic Aberration)")]
    [SerializeField, Range(0f, 0.03f)] private float chromaStrength  = 0.008f;

    [Header("비네트 (Vignette)")]
    [SerializeField, Range(0f, 3f)]    private float vignetteStrength = 1.5f;
    [SerializeField, Range(0.01f, 1f)] private float vignetteSoftness = 0.3f;

    [Header("스캔라인 (Scanlines)")]
    [SerializeField, Range(0f, 1f)]      private float scanlineStrength = 0.18f;
    [SerializeField, Range(100f, 2000f)] private float scanlineDensity  = 960f;

    [Header("필름 그레인 (Film Grain)")]
    [SerializeField, Range(0f, 0.5f)] private float grainStrength = 0.10f;
    [SerializeField, Range(1f, 10f)]  private float grainSize     = 2.0f;

    [Header("CCTV 녹색 틴트")]
    [SerializeField, Range(0f, 0.5f)] private float tintStrength = 0.08f;

    [Header("보스 페이즈 강화 배율")]
    [SerializeField] private float bossGrainMultiplier  = 3.5f;
    [SerializeField] private float bossChromaMultiplier = 3.0f;
    [SerializeField] private float bossTransitionTime   = 0.4f;

    [Header("글리치 설정")]
    [SerializeField] private float glitchDuration = 0.25f;
    [SerializeField, Range(0f, 1f)] private float glitchPeakStrength = 0.85f;

    private Material  _mat;
    private float     _baseGrain;
    private float     _baseChroma;
    private Coroutine _transRoutine;
    private Coroutine _glitchRoutine;
    private float     _curGrain;
    private float     _curChroma;
    private float     _curGlitch;

    private StageManager _sm;

    public static CCTVEffectController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        if (cctvShader == null) cctvShader = Shader.Find("Anomaly/CCTVEffect");
        if (cctvShader == null) { GameLog.Error("[CCTVEffect] 셰이더 없음"); enabled = false; return; }
        _mat = new Material(cctvShader) { hideFlags = HideFlags.HideAndDontSave };
        _baseGrain = grainStrength; _baseChroma = chromaStrength;
        _curGrain  = grainStrength; _curChroma  = chromaStrength;
        _curGlitch = 0f;
    }

    private void Start()
    {
        _sm = StageManager.Instance ?? FindObjectOfType<StageManager>();
        if (_sm != null) _sm.OnPhaseChanged += OnPhaseChanged;
        else GameLog.Warn("[CCTVEffect] StageManager 없음");
    }

    private void OnDestroy()
    {
        if (_mat != null) DestroyImmediate(_mat);
        if (_sm  != null) _sm.OnPhaseChanged -= OnPhaseChanged;
        Instance = null;
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (_mat == null) { Graphics.Blit(src, dest); return; }
        _mat.SetFloat("_ChromaStrength",   _curChroma);
        _mat.SetFloat("_VignetteStrength", vignetteStrength);
        _mat.SetFloat("_VignetteSoft",     vignetteSoftness);
        _mat.SetFloat("_ScanStrength",     scanlineStrength);
        _mat.SetFloat("_ScanDensity",      scanlineDensity);
        _mat.SetFloat("_GrainStrength",    _curGrain);
        _mat.SetFloat("_GrainSize",        grainSize);
        _mat.SetFloat("_GlitchStrength",   _curGlitch);
        _mat.SetFloat("_GlitchTime",       Time.time);
        _mat.SetFloat("_TintStrength",     tintStrength);
        Graphics.Blit(src, dest, _mat);
    }

    // ── 보스 페이즈 전환 ──────────────────────────────────────
    private void OnPhaseChanged(StagePhase phase)
    {
        float tGrain  = phase == StagePhase.Boss ? _baseGrain  * bossGrainMultiplier  : _baseGrain;
        float tChroma = phase == StagePhase.Boss ? _baseChroma * bossChromaMultiplier : _baseChroma;
        if (_transRoutine != null) StopCoroutine(_transRoutine);
        _transRoutine = StartCoroutine(TransitionRoutine(tGrain, tChroma));

        // 보스 등장 시 글리치 폭발
        if (phase == StagePhase.Boss) TriggerGlitch(0.6f, 1.0f);
    }

    private IEnumerator TransitionRoutine(float tGrain, float tChroma)
    {
        float sGrain = _curGrain, sChroma = _curChroma, t = 0f;
        while (t < bossTransitionTime)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / bossTransitionTime);
            _curGrain  = Mathf.Lerp(sGrain,  tGrain,  n);
            _curChroma = Mathf.Lerp(sChroma, tChroma, n);
            yield return null;
        }
        _curGrain = tGrain; _curChroma = tChroma;
    }

    // ── 글리치 공개 API ───────────────────────────────────────
    /// <summary>피격 등 이벤트 시 글리치 발동. duration(초), strength(0~1)</summary>
    public void TriggerGlitch(float duration = -1f, float strength = -1f)
    {
        float d = duration < 0f ? glitchDuration     : duration;
        float s = strength < 0f ? glitchPeakStrength : strength;
        if (_glitchRoutine != null) StopCoroutine(_glitchRoutine);
        _glitchRoutine = StartCoroutine(GlitchRoutine(d, s));
    }

    private IEnumerator GlitchRoutine(float duration, float peak)
    {
        float half = duration * 0.35f;
        // 점등
        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            _curGlitch = Mathf.Lerp(0f, peak, t / half);
            yield return null;
        }
        _curGlitch = peak;
        // 소멸
        t = 0f;
        float fade = duration - half;
        while (t < fade)
        {
            t += Time.deltaTime;
            _curGlitch = Mathf.Lerp(peak, 0f, t / fade);
            yield return null;
        }
        _curGlitch = 0f;
        _glitchRoutine = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _baseGrain = grainStrength;
        _baseChroma = chromaStrength;
    }
#endif
}