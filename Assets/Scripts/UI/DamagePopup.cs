using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 데미지 숫자 팝업 단일 인스턴스.
/// DamagePopupManager에서 생성/풀링됨.
/// </summary>
[RequireComponent(typeof(TextMeshPro))]
public class DamagePopup : MonoBehaviour
{
    [SerializeField] private float riseSpeed    = 2f;
    [SerializeField] private float lifetime     = 0.7f;
    [SerializeField] private float fadeStart    = 0.4f;  // lifetime의 몇 초부터 페이드
    [SerializeField] private Color normalColor  = Color.white;
    [SerializeField] private Color critColor    = new Color(1f, 0.85f, 0f);  // 황금색
    [SerializeField] private float critScale    = 1.4f;

    private TextMeshPro _tmp;
    private Coroutine   _anim;

    private void Awake() { _tmp = GetComponent<TextMeshPro>(); }

    public void Spawn(Vector3 worldPos, float damage, bool isCrit)
    {
        transform.position = worldPos + new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, 0f);
        _tmp.text      = isCrit ? "CRIT " + Mathf.RoundToInt(damage).ToString() : Mathf.RoundToInt(damage).ToString();
        _tmp.color     = isCrit ? critColor : normalColor;
        _tmp.fontSize  = isCrit ? 4f * critScale : 4f;
        transform.localScale = Vector3.zero;   // 스케일 팝 연출 시작점
        gameObject.SetActive(true);

        // 크리티컬은 묵직하게 — 아주 약한 셰이크 연동
        if (isCrit) GameFeel.Instance?.Shake(0.08f, 0.08f);

        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(AnimRoutine());
    }

    private IEnumerator AnimRoutine()
    {
        // 등장 스케일 팝: 0 → 1.2 → 1.0 (빠르게 튀어오름)
        float popDur = 0.12f, pt = 0f;
        while (pt < popDur)
        {
            pt += Time.deltaTime;
            float k = pt / popDur;
            float s = (k < 0.6f) ? Mathf.Lerp(0f, 1.2f, k / 0.6f)
                                 : Mathf.Lerp(1.2f, 1.0f, (k - 0.6f) / 0.4f);
            transform.localScale = Vector3.one * s;
            transform.position += Vector3.up * riseSpeed * 0.5f * Time.deltaTime;
            yield return null;
        }
        transform.localScale = Vector3.one;

        float elapsed = 0f;
        bool keepRunning = true;
        while (keepRunning)
        {
            elapsed += Time.deltaTime;
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            // fadeStart 이후 알파 감소
            if (elapsed > fadeStart)
            {
                float t = (elapsed - fadeStart) / (lifetime - fadeStart);
                var c = _tmp.color; c.a = Mathf.Lerp(1f, 0f, t); _tmp.color = c;
            }

            if (elapsed >= lifetime) keepRunning = false;
            else yield return null;
        }

        gameObject.SetActive(false);
        DamagePopupManager.Instance?.ReturnToPool(this);
        _anim = null;
    }
}
