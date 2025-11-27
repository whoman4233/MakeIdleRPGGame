using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField]
    private List<EquipmentData> equipments = new List<EquipmentData>();

    public IReadOnlyList<EquipmentData> Equipments => equipments;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddEquipment(EquipmentData data)
    {
        if (data == null) return;

        equipments.Add(data);
        OnInventoryChanged?.Invoke();

        Debug.Log($"[Inventory] 장비 추가: {data.displayName}");
    }

    public void RemoveEquipment(EquipmentData data)
    {
        if (data == null) return;

        if (equipments.Remove(data))
        {
            OnInventoryChanged?.Invoke();
            Debug.Log($"[Inventory] 장비 제거: {data.displayName}");
        }
    }
}
