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

        Debug.Log($"[StageManager] Stage 변경 → {CurrentStage.displayName}", this);
    }

    private void SetPhaseInternal(StagePhase phase, bool invokeStageChanged = false)
    {
        currentPhase = phase;

        if (invokeStageChanged)
            OnStageChanged?.Invoke();

        OnPhaseChanged?.Invoke(currentPhase);
        OnKillCountChanged?.Invoke();
    }

    // ====== Enemy 죽었을 때 호출 ======
    public void OnEnemyKilled(EnemyStats enemy)
    {
        var stage = CurrentStage;

        bool isBoss = enemy != null && enemy.data != null && enemy.data.isBoss;
        Debug.Log($"[StageManager] Enemy Killed. killCount(before)={currentKillCount}, isBoss={isBoss}");

        currentKillCount++;
        OnKillCountChanged?.Invoke();

        Debug.Log($"[StageManager] Enemy Killed. killCount(after)={currentKillCount}, phase={currentPhase}, stage={stage?.displayName}");

        if (stage == null)
            return;

        if (isBoss)
        {
            Debug.Log("[StageManager] Boss Killed → 다음 스테이지로 이동");
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
            // 마지막 스테이지 반복하고 싶으면:
            // nextIndex = stages.Length - 1;
            return;
        }

        Debug.Log($"[StageManager] 다음 스테이지로 이동: {stages[nextIndex].displayName}");
        SetStage(nextIndex);
    }

    // 플레이어 죽었을 때 호출 (보스 실패 -> 다시 노가다)
    public void OnPlayerDied()
    {
        var stage = CurrentStage;
        Debug.Log($"[StageManager] 플레이어 사망. 현재 페이즈: {currentPhase}");

        if (currentPhase == StagePhase.Boss)
        {
            // 보스 실패 → 같은 스테이지 Normal Phase로 리셋
            currentKillCount = 0;
            SetPhaseInternal(StagePhase.Normal, invokeStageChanged: false);
            Debug.Log("[StageManager] 보스 실패 → Normal Phase로 돌아감 (노가다)");
        }
        else
        {
            // Normal Phase에서 죽었으면 그냥 유지 (원하면 KillCount 유지 or 리셋 선택 가능)
            // currentKillCount = 0; // 리셋하고 싶으면 활용
            OnKillCountChanged?.Invoke();
        }
    }

    public int GetCurrentKillCount() => currentKillCount;
}
