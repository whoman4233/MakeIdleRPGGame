using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데미지 팝업 오브젝트 풀 관리자.
/// Managers 하위에 추가, prefab 연결 필수.
/// AttackState에서 DamagePopupManager.Instance?.Show() 호출.
/// </summary>
public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [SerializeField] private DamagePopup popupPrefab;
    [SerializeField] private int         initialPoolSize = 20;

    private readonly Queue<DamagePopup> _pool = new Queue<DamagePopup>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (popupPrefab == null) { GameLog.Warn("[DamagePopupManager] prefab 미연결"); return; }
        for (int i = 0; i < initialPoolSize; i++)
        {
            var p = Instantiate(popupPrefab, transform);
            p.gameObject.SetActive(false);
            _pool.Enqueue(p);
        }
    }

    /// <summary>월드 좌표에 데미지 팝업 표시</summary>
    public void Show(Vector3 worldPos, float damage, bool isCrit = false)
    {
        if (popupPrefab == null) return;
        DamagePopup popup = _pool.Count > 0
            ? _pool.Dequeue()
            : Instantiate(popupPrefab, transform);
        popup.Spawn(worldPos, damage, isCrit);
    }

    public void ReturnToPool(DamagePopup popup)
    {
        if (popup == null) return;
        _pool.Enqueue(popup);
    }
}
