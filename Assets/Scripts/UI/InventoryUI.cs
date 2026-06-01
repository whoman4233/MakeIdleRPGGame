using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform contentRoot;
    public InventoryItemUi itemPrefab;
    public ScrollRect scrollRect;

    [Header("Grid Settings")]
    public int   columns  = 4;
    public float cellSize = 150f;
    public float spacing  = 8f;
    public float padding  = 10f;

    private InventoryManager      _inventory;
    private PlayerGraft           _playerGraft;
    private Action<GraftData>     _onItemSelectedCallback;
    private List<InventoryItemUi> _pool = new List<InventoryItemUi>();
    private bool                  _initialized;

    private void Awake()
    {
        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>(true);
    }

    private void Start()
    {
        Init();
        Rebuild();
    }

    private void OnEnable()
    {
        // Start 이전에 OnEnable이 먼저 오는 경우 대비
        Init();
        Rebuild();
    }

    private void OnDestroy()
    {
        if (_inventory != null)
            _inventory.OnInventoryChanged -= Rebuild;
    }

    // ── 초기화 (중복 실행 방지) ───────────────────
    private void Init()
    {
        if (_initialized) return;

        _inventory = InventoryManager.Instance;
        if (_inventory == null) return; // 아직 Instance 없으면 패스 (다음 OnEnable에서 재시도)

        _inventory.OnInventoryChanged += Rebuild;

        if (PlayerRef.Instance != null)
            _playerGraft = PlayerRef.Instance.GetComponent<PlayerGraft>();

        _initialized = true;
    }

    // ── 모드 ─────────────────────────────────────
    public void SetEquipMode()     { _onItemSelectedCallback = null; }
    public void SetSelectionMode(Action<GraftData> cb) { _onItemSelectedCallback = cb; }

    // ── 리빌드 ────────────────────────────────────
    public void Rebuild()
    {
        // Instance가 아직 없으면 Init 재시도
        if (!_initialized) Init();
        if (_inventory == null) return;
        if (contentRoot == null || itemPrefab == null) return;

        var list = _inventory.Grafts;

        // 1. 풀 전체 숨기기
        foreach (var slot in _pool)
            slot.gameObject.SetActive(false);

        // 2. 데이터만큼 슬롯 활성화
        for (int i = 0; i < list.Count; i++)
        {
            if (i >= _pool.Count) _pool.Add(CreateSlot());
            if (list[i] == null) continue;
            _pool[i].gameObject.SetActive(true);
            _pool[i].Init(list[i], _playerGraft, OnItemClicked);
        }

        // 3. Content 높이 — ContentSizeFitter가 있으면 자동, 없으면 수동
        var csf = contentRoot.GetComponent<ContentSizeFitter>();
        if (csf == null)
        {
            int rows = Mathf.Max(1, Mathf.CeilToInt(list.Count / (float)columns));
            float h  = padding * 2 + rows * cellSize + Mathf.Max(0, rows - 1) * spacing;
            contentRoot.sizeDelta = new Vector2(contentRoot.sizeDelta.x, h);
        }

        if (gameObject.activeInHierarchy)
            StartCoroutine(ScrollToTop());
    }

    private InventoryItemUi CreateSlot()
    {
        var slot = Instantiate(itemPrefab, contentRoot);
        slot.gameObject.SetActive(false);
        return slot;
    }

    private IEnumerator ScrollToTop()
    {
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null)
            scrollRect.normalizedPosition = new Vector2(0f, 1f);
    }

    private void OnItemClicked(GraftData data)
    {
        SoundManager.Instance?.PlayEquip();
        if (_onItemSelectedCallback != null)
            _onItemSelectedCallback.Invoke(data);
        else
            _playerGraft?.Equip(data);
    }
}