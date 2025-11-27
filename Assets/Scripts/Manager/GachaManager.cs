using UnityEngine;
using System;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance { get; private set; }

    [Header("Settings")]
    public long cost = 500;                 // 한 번 뽑기 비용
    public EquipmentData[] pool;            // 여기서 랜덤으로 하나

    private PlayerEquipment _playerEquipment;

    public event Action<EquipmentData> OnGachaSuccess;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (PlayerRef.Instance != null)
            _playerEquipment = PlayerRef.Instance.GetComponent<PlayerEquipment>();
    }

    /// <summary>
    /// 뽑기 시도. 성공 시 true, 실패 시 false
    /// </summary>
    public bool TryGacha(out EquipmentData reward)
    {
        reward = null;

        if (CurrencyManager.Instance == null)
        {
            Debug.LogWarning("[Gacha] CurrencyManager 없음");
            return false;
        }

        if (_playerEquipment == null)
        {
            Debug.LogWarning("[Gacha] PlayerEquipment 없음");
            return false;
        }

        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning("[Gacha] 풀에 아이템이 없음");
            return false;
        }

        if (CurrencyManager.Instance.Gold < cost)
        {
            Debug.Log("[Gacha] 골드 부족");
            return false;
        }

        // === 재화 차감 ===
        CurrencyManager.Instance.AddGold(-cost);

        // === 랜덤 장비 선택 ===
        int idx = UnityEngine.Random.Range(0, pool.Length);
        reward = pool[idx];

        // === 랜덤 장비 선택 ===
        idx = UnityEngine.Random.Range(0, pool.Length);
        reward = pool[idx];

        // === 인벤토리에 추가 ===
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddEquipment(reward);
        }

        // === 자동 장착 (원하면 유지) ===
        _playerEquipment.Equip(reward);

        Debug.Log($"[Gacha] 뽑기 성공: {reward.displayName}");

        OnGachaSuccess?.Invoke(reward);
        return true;
    }
}
