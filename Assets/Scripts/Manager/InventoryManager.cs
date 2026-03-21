using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField]
    private List<GraftData> grafts = new List<GraftData>();

    public IReadOnlyList<GraftData> Grafts => grafts;

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

    public void AddGraft(GraftData data)
    {
        if (data == null) return;

        grafts.Add(data);
        OnInventoryChanged?.Invoke();

        // 기존의 깨진 한글 로그를 알아보기 쉽게 변경
        Debug.Log($"[Inventory] 괴이 육체 추가됨: {data.graftName}");
    }

    public void RemoveGraft(GraftData data)
    {
        if (data == null) return;

        if (grafts.Remove(data))
        {
            OnInventoryChanged?.Invoke();
            Debug.Log($"[Inventory] 괴이 육체 제거됨: {data.graftName}");
        }
    }
}