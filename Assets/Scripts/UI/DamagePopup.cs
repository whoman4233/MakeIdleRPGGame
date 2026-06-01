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
        gameObject.SetActive(true);

        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(AnimRoutine());
    }

    private IEnumerator AnimRoutine()
    {
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
