using System;
using UnityEngine;

public enum StagePhase
{
    Normal,
    Boss,
    Cleared
}

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Stage List")]
    public StageData[] stages;

    [Header("Runtime")]
    [SerializeField] private int currentStageIndex;
    [SerializeField] private int currentKillCount;
    [SerializeField] private StagePhase currentPhase = StagePhase.Normal;

    public StageData CurrentStage =>
        (stages != null && stages.Length > 0 && currentStageIndex >= 0 && currentStageIndex < stages.Length)
            ? stages[currentStageIndex]
            : null;

    public StagePhase CurrentPhase => currentPhase;

    public event Action OnStageChanged;
    public event Action OnKillCountChanged;
    public event Action<StagePhase> OnPhaseChanged;

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

    private void Start()
    {
        SetStage(0);
    }

    // 스테이지 레벨에 따른 스탯 배율 반환 (예: 1스테이지=1.0, 2스테이지=1.2, 3스테이지=1.4 ...)
    public float GetStatMultiplier()
    {
        return 1f + (currentStageIndex * 0.2f); // 스테이지당 20% 증가
    }

    public void SetStage(int index)
    {
        if (stages == null || stages.Length == 0)
        {
            Debug.LogWarning("[StageManager] 스테이지 데이터가 없습니다.", this);
            return;
        }

        index = Mathf.Clamp(index, 0, stages.Length - 1);
        currentStageIndex = index;
        currentKillCount = 0;
        SetPhaseInternal(StagePhase.Normal, invokeStageChanged: true);

        Debug.Log($"[StageManager] Stage 세팅 됨 {CurrentStage.displayName}", this);
    }

    private void SetPhaseInternal(StagePhase phase, bool invokeStageChanged = false)
    {
        currentPhase = phase;

        if (invokeStageChanged)
            OnStageChanged?.Invoke();

        OnPhaseChanged?.Invoke(currentPhase);
        OnKillCountChanged?.Invoke();
    }

    public void OnEnemyKilled(EnemyStats enemy)
    {
        var stage = CurrentStage;

        bool isBoss = enemy != null && enemy.data != null && enemy.data.isBoss;

        currentKillCount++;
        OnKillCountChanged?.Invoke();

        if (stage == null)
            return;

        if (isBoss)
        {
            Debug.Log("[StageManager] Boss Killed -> 다음 스테이지로 이동");
            GoToNextStage();
            return;
        }

        if (currentPhase == StagePhase.Normal &&
            stage.normalKillToSummonBoss > 0 &&
            currentKillCount >= stage.normalKillToSummonBoss)
        {
            EnterBossPhase();
        }
    }

    private void EnterBossPhase()
    {
        Debug.Log("[StageManager] Boss Phase 진입");
        SetPhaseInternal(StagePhase.Boss, invokeStageChanged: false);
    }

    public void GoToNextStage()
    {
        int nextIndex = currentStageIndex + 1;

        if (stages == null || stages.Length == 0)
            return;

        if (nextIndex >= stages.Length)
        {
            Debug.Log("[StageManager] 마지막 스테이지 클리어. 더 이상 진행할 스테이지 없음.");
            return;
        }

        SetStage(nextIndex);
    }

    public void OnPlayerDied()
    {
        if (currentPhase == StagePhase.Boss)
        {
            currentKillCount = 0;
            SetPhaseInternal(StagePhase.Normal, invokeStageChanged: false);
            Debug.Log("[StageManager] 플레이어 사망. Boss에서 Normal Phase로 복귀");
        }
        else
        {
            OnKillCountChanged?.Invoke();
        }
    }

    public int GetCurrentKillCount() => currentKillCount;
}