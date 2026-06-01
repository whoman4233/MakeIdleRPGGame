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
        GameLog.Log($"[Inventory] 괴이 육체 추가됨: {data.graftName}");
    }

    public void RemoveGraft(GraftData data)
    {
        if (data == null) return;

        if (grafts.Remove(data))
        {
            OnInventoryChanged?.Invoke();
            GameLog.Log($"[Inventory] 괴이 육체 제거됨: {data.graftName}");
        }
    }

    /// <summary>세이브 데이터에서 인벤토리 전체를 교체합니다.</summary>
    public void LoadGrafts(List<GraftData> loadedGrafts)
    {
        grafts.Clear();
        if (loadedGrafts != null)
            grafts.AddRange(loadedGrafts);

        OnInventoryChanged?.Invoke();
        GameLog.Log($"[Inventory] 로드 완료 — 보유 육체: {grafts.Count}개");
    }
}