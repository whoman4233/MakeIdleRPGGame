using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossWarningEffect : MonoBehaviour
{
    [Header("연결")]
    [SerializeField] private Image warningImage;
    [Header("섬광 설정")]
    [SerializeField] private int   flashCount   = 3;
    [SerializeField] private float flashOnAlpha = 0.55f;
    [SerializeField] private float flashOnTime  = 0.07f;
    [SerializeField] private float flashOffTime = 0.10f;
    [Header("맥동 설정")]
    [SerializeField] private float pulseMaxAlpha = 0.25f;
    [SerializeField] private float pulsePeriod   = 1.6f;
    [Header("사운드 훅 (선택)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   bossWarningClip;

    private StageManager _sm;
    private Coroutine    _effectRoutine;

    private void Start()
    {
        _sm = StageManager.Instance ?? FindObjectOfType<StageManager>();
        if (_sm != null) _sm.OnPhaseChanged += OnPhaseChanged;
        else GameLog.Warn("[BossWarningEffect] StageManager 없음");
        if (warningImage != null) warningImage.color = new Color(1f,0f,0f,0f);
    }

    private void OnDestroy()
    {
        if (_sm != null) _sm.OnPhaseChanged -= OnPhaseChanged;
        StopEffect();
    }

    private void OnPhaseChanged(StagePhase phase)
    {
        if (phase == StagePhase.Boss) StartEffect();
        else StopEffect();
    }

    public void StartEffect() { StopEffect(); _effectRoutine = StartCoroutine(WarningRoutine()); }

    public void StopEffect()
    {
        if (_effectRoutine != null) { StopCoroutine(_effectRoutine); _effectRoutine = null; }
        SetAlpha(0f);
    }

    private System.Collections.IEnumerator WarningRoutine()
    {
        if (audioSource != null && bossWarningClip != null) audioSource.PlayOneShot(bossWarningClip);
        for (int i = 0; i < flashCount; i++)
        {
            SetAlpha(flashOnAlpha); yield return new WaitForSeconds(flashOnTime);
            SetAlpha(0f);          yield return new WaitForSeconds(flashOffTime);
        }
        bool running = true;
        while (running)
        {
            float t = 0f;
            while (t < pulsePeriod)
            {
                SetAlpha(Mathf.Sin((t / pulsePeriod) * Mathf.PI) * pulseMaxAlpha);
                t += Time.deltaTime; yield return null;
            }
            SetAlpha(0f);
        }
    }

    private void SetAlpha(float a)
    {
        if (warningImage == null) return;
        var c = warningImage.color; c.a = a; warningImage.color = c;
    }
}
