using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform contentRoot;
    public InventoryItemUi itemPrefab;

    [Header("Pooling Settings")]
    public int initialPoolSize = 100; // 초기 생성 슬롯 수

    private InventoryManager _inventory;
    private PlayerGraft _playerGraft;
    private Action<GraftData> _onItemSelectedCallback;

    // 생성된 모든 UI 슬롯을 관리하는 리스트
    private List<InventoryItemUi> _slotPool = new List<InventoryItemUi>();

    private void Awake()
    {
        // 시작 시 풀에 슬롯 미리 생성 (Pre-warming)
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewSlot();
        }
    }

    private void Start()
    {
        _inventory = InventoryManager.Instance;
        if (PlayerRef.Instance != null)
            _playerGraft = PlayerRef.Instance.GetComponent<PlayerGraft>();

        if (_inventory != null)
            _inventory.OnInventoryChanged += Rebuild;

        OpenForEquip();
    }

    private void OnDestroy()
    {
        if (_inventory != null)
            _inventory.OnInventoryChanged -= Rebuild;
    }

    // 새 슬롯을 생성하여 풀에 추가하는 내부 메서드
    private InventoryItemUi CreateNewSlot()
    {
        var slotUI = Instantiate(itemPrefab, contentRoot);
        slotUI.gameObject.SetActive(false);
        _slotPool.Add(slotUI);
        return slotUI;
    }

    public void OpenForEquip()
    {
        _onItemSelectedCallback = null;
        gameObject.SetActive(true);
        Rebuild();
    }

    public void OpenForSelection(Action<GraftData> onSelected)
    {
        _onItemSelectedCallback = onSelected;
        gameObject.SetActive(true);
        Rebuild();
    }

    public void Rebuild()
    {
        if (_inventory == null || contentRoot == null || itemPrefab == null) return;

        var dataList = _inventory.Grafts;

        // 1. 모든 슬롯 일단 비활성화
        for (int i = 0; i < _slotPool.Count; i++)
        {
            _slotPool[i].gameObject.SetActive(false);
        }

        // 2. 데이터 개수만큼 슬롯 활성화 및 데이터 주입
        for (int i = 0; i < dataList.Count; i++)
        {
            // 풀이 모자라면 동적 생성
            if (i >= _slotPool.Count)
            {
                CreateNewSlot();
            }

            var graft = dataList[i];
            if (graft == null) continue;

            _slotPool[i].gameObject.SetActive(true);
            _slotPool[i].Init(graft, _playerGraft, HandleItemClicked);
        }
    }

    private void HandleItemClicked(GraftData clickedData)
    {
        if (_onItemSelectedCallback != null)
        {
            _onItemSelectedCallback.Invoke(clickedData);
        }
        else
        {
            if (_playerGraft != null)
            {
                _playerGraft.Equip(clickedData);
            }
        }
    }
}