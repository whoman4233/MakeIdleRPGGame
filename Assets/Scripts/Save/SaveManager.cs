using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// JSON 기반 세이브/로드 시스템.
///
/// [저장 항목]
///   - 재화 (Data, Core)
///   - 인벤토리 (GraftData 목록)
///   - 장착 슬롯 5개 (Head / Core / ArmL / ArmR / Legs)
///   - 업그레이드 레벨 배열
///   - 현재 스테이지 인덱스
///
/// [사용법]
///   씬의 빈 오브젝트에 컴포넌트로 추가 후 Graft Registry 슬롯에 GraftDataRegistry SO 연결.
///   게임 저장 : SaveManager.Instance.SaveGame()
///   게임 로드 : 게임 시작 시 자동 로드 (loadOnStart = true)
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("필수 연결")]
    [Tooltip("프로젝트 내 모든 GraftData가 등록된 레지스트리 SO")]
    [SerializeField] private GraftDataRegistry graftRegistry;

    [Header("자동 로드 설정")]
    [Tooltip("게임 시작 시 저장 파일이 있으면 자동으로 로드")]
    [SerializeField] private bool loadOnStart = true;

    // 저장 파일 경로 (Android는 /data/data/..., Windows Editor는 AppData/LocalLow/...)
    private const string SaveKey = "anomaly_save.json";

    // 세이브 영속화 백엔드. 기본은 로컬 파일, 향후 클라우드 등으로 교체 가능.
    private ISaveStorage _storage = new LocalFileStorage();

    // --- 캐시된 컴포넌트 참조 (Start에서 찾아옴) ---
    private PlayerGraft _playerGraft;

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

    private IEnumerator Start()
    {
        // 한 프레임 대기 → 다른 매니저들의 Start() 완료 보장
        yield return null;

        _playerGraft = FindObjectOfType<PlayerGraft>();

        if (_playerGraft == null)
            GameLog.Warn("[SaveManager] PlayerGraft 컴포넌트를 찾을 수 없습니다.");

        if (loadOnStart)
            LoadGame();
    }

    // =========================================================
    //  저장
    // =========================================================

    [ContextMenu("게임 저장 (SaveGame)")]
    public void SaveGame()
    {
        try
        {
            var data = CollectSaveData();
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            _storage.Save(SaveKey, json);
            GameLog.Log($"[SaveManager] 저장 완료 → {SaveKey}");
        }
        catch (System.Exception e)
        {
            GameLog.Error($"[SaveManager] 저장 실패: {e.Message}");
        }
    }

    private SaveData CollectSaveData()
    {
        var data = new SaveData();

        // 재화
        if (CurrencyManager.Instance != null)
        {
            data.dataAmount = CurrencyManager.Instance.Data;
            data.coreAmount = CurrencyManager.Instance.Core;
        }

        // 인벤토리
        if (InventoryManager.Instance != null)
        {
            data.inventoryGraftIds = new List<string>();
            foreach (var graft in InventoryManager.Instance.Grafts)
            {
                if (graft != null)
                    data.inventoryGraftIds.Add(graft.name);
            }
        }

        // 장착 슬롯
        if (_playerGraft != null)
        {
            data.equippedHead  = GraftId(_playerGraft.headGraft);
            data.equippedCore  = GraftId(_playerGraft.coreGraft);
            data.equippedArmL  = GraftId(_playerGraft.armLGraft);
            data.equippedArmR  = GraftId(_playerGraft.armRGraft);
            data.equippedLegs  = GraftId(_playerGraft.legsGraft);
        }

        // 업그레이드 레벨
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.upgrades != null)
        {
            int len = UpgradeManager.Instance.upgrades.Length;
            data.upgradeLevels = new int[len];
            for (int i = 0; i < len; i++)
                data.upgradeLevels[i] = UpgradeManager.Instance.upgrades[i].level;
        }

        // 스테이지
        if (StageManager.Instance != null)
            data.currentStageIndex = StageManager.Instance.CurrentStageIndex;

        // 추출 횟수 (비용 곡선)
        if (ExtractionManager.Instance != null)
            data.basicExtractionCount = ExtractionManager.Instance.BasicExtractionCount;

        return data;
    }

    // =========================================================
    //  불러오기
    // =========================================================

    [ContextMenu("게임 로드 (LoadGame)")]
    public void LoadGame()
    {
        if (!_storage.Exists(SaveKey))
        {
            GameLog.Log("[SaveManager] 저장 파일 없음. 새 게임 시작.");
            return;
        }

        try
        {
            string json = _storage.Load(SaveKey);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                GameLog.Warn("[SaveManager] 파싱 실패. 새 게임으로 진행.");
                return;
            }
            ApplySaveData(data);
            GameLog.Log("[SaveManager] 로드 완료.");
        }
        catch (System.Exception e)
        {
            GameLog.Error($"[SaveManager] 로드 실패: {e.Message}. 새 게임으로 진행.");
        }
    }

    private void ApplySaveData(SaveData data)
    {
        // 재화
        CurrencyManager.Instance?.LoadValues(data.dataAmount, data.coreAmount);

        // 인벤토리
        if (InventoryManager.Instance != null)
        {
            var grafts = new List<GraftData>();
            foreach (var id in data.inventoryGraftIds)
            {
                var graft = graftRegistry.Find(id);
                if (graft != null)
                    grafts.Add(graft);
                else
                    GameLog.Warn($"[SaveManager] GraftData '{id}' 를 레지스트리에서 찾지 못했습니다.");
            }
            InventoryManager.Instance.LoadGrafts(grafts);
        }

        // 장착 슬롯
        _playerGraft?.LoadEquipped(
            graftRegistry.Find(data.equippedHead),
            graftRegistry.Find(data.equippedCore),
            graftRegistry.Find(data.equippedArmL),
            graftRegistry.Find(data.equippedArmR),
            graftRegistry.Find(data.equippedLegs)
        );

        // 업그레이드 레벨
        UpgradeManager.Instance?.LoadUpgradeLevels(data.upgradeLevels);

        // 스테이지 (SetStage는 StageManager 내부에서 초기화까지 처리)
        StageManager.Instance?.SetStage(data.currentStageIndex);

        // 추출 횟수 (비용 곡선)
        ExtractionManager.Instance?.LoadExtractionCount(data.basicExtractionCount);
    }

    // =========================================================
    //  더미 데이터 생성 (테스트용)
    // =========================================================

    /// <summary>
    /// 테스트용 더미 세이브 파일을 생성합니다.
    /// 인스펙터 우클릭 메뉴 또는 코드에서 호출.
    /// 레지스트리에 등록된 처음 3개의 GraftData를 인벤토리/장착에 사용합니다.
    /// </summary>
    [ContextMenu("더미 저장 데이터 생성 (테스트용)")]
    public void WriteDummySave()
    {
        if (graftRegistry == null)
        {
            GameLog.Error("[SaveManager] GraftDataRegistry가 연결되지 않았습니다.");
            return;
        }

        var data = new SaveData
        {
            dataAmount = 9999,
            coreAmount = 50,
            currentStageIndex = 2,
            inventoryGraftIds = new List<string>(),
            upgradeLevels = new int[0]
        };

        // 레지스트리에서 처음 5개 그래프트를 인벤토리에 추가
        var allGrafts = graftRegistry.AllGrafts;
        for (int i = 0; i < Mathf.Min(5, allGrafts.Count); i++)
        {
            if (allGrafts[i] != null)
                data.inventoryGraftIds.Add(allGrafts[i].name);
        }

        // 슬롯 타입별로 하나씩 자동 배정
        foreach (var graft in allGrafts)
        {
            if (graft == null) continue;
            switch (graft.slotType)
            {
                case GraftSlotType.Head  when string.IsNullOrEmpty(data.equippedHead):  data.equippedHead  = graft.name; break;
                case GraftSlotType.Core  when string.IsNullOrEmpty(data.equippedCore):  data.equippedCore  = graft.name; break;
                case GraftSlotType.ArmL  when string.IsNullOrEmpty(data.equippedArmL): data.equippedArmL  = graft.name; break;
                case GraftSlotType.ArmR  when string.IsNullOrEmpty(data.equippedArmR): data.equippedArmR  = graft.name; break;
                case GraftSlotType.Legs  when string.IsNullOrEmpty(data.equippedLegs): data.equippedLegs  = graft.name; break;
            }
        }

        // 업그레이드 레벨: 등록된 업그레이드 수만큼 [3,2,1,0,...] 패턴으로 채움
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.upgrades != null)
        {
            int len = UpgradeManager.Instance.upgrades.Length;
            data.upgradeLevels = new int[len];
            for (int i = 0; i < len; i++)
                data.upgradeLevels[i] = Mathf.Max(0, 3 - i);
        }

        string json = JsonUtility.ToJson(data, prettyPrint: true);
        _storage.Save(SaveKey, json);
        GameLog.Log($"[SaveManager] 더미 저장 데이터 생성 완료 → {SaveKey}\n{json}");
    }

    // =========================================================
    //  유틸리티
    // =========================================================

    public bool HasSaveFile() => _storage.Exists(SaveKey);

    [ContextMenu("저장 파일 삭제")]
    public void DeleteSave()
    {
        if (_storage.Exists(SaveKey))
        {
            _storage.Delete(SaveKey);
            GameLog.Log("[SaveManager] 저장 파일 삭제됨.");
        }
        else
        {
            GameLog.Log("[SaveManager] 삭제할 저장 파일이 없습니다.");
        }
    }

    public string GetSavePath() => SaveKey;

    private static string GraftId(GraftData graft) => graft != null ? graft.name : "";
}
