using System;
using UnityEngine;

public enum StagePhase { Normal, Boss, Cleared }

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    [Header("Stage List")]
    public StageData[] stages;

    [Header("Endless Mode")]
    [Tooltip("마지막 스테이지 클리어 후 계속 반복 (배율 누적)")]
    public bool endlessMode = true;
    [Tooltip("엔드리스 반복당 추가 배율 (0.1 = 10%씩 증가)")]
    public float endlessExtraMultiplier = 0.15f;

    [Header("Runtime")]
    [SerializeField] private int currentStageIndex;
    [SerializeField] private int currentKillCount;
    [SerializeField] private StagePhase currentPhase = StagePhase.Normal;
    [SerializeField] private int endlessLoopCount;   // 몇 번 반복했는지

    public StageData CurrentStage =>
        (stages != null && stages.Length > 0 &&
         currentStageIndex >= 0 && currentStageIndex < stages.Length)
            ? stages[currentStageIndex] : null;

    public StagePhase CurrentPhase  => currentPhase;
    public int CurrentStageIndex    => currentStageIndex;
    public int EndlessLoopCount     => endlessLoopCount;

    public event Action              OnStageChanged;
    public event Action              OnKillCountChanged;
    public event Action<StagePhase>  OnPhaseChanged;
    /// <summary>엔드리스 루프 진입 시 발행 (루프 횟수 포함)</summary>
    public event Action<int>         OnEndlessLoopStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 기본값 0 세팅. SaveManager.LoadGame()이 이후 덮어씀 (한 프레임 지연 로드).
        SetStage(0);
    }

    
    /// <summary>스테이지 레벨 + 엔드리스 루프 배율 합산</summary>
    public float GetStatMultiplier()
    {
        float stageBonus   = currentStageIndex * 0.2f;
        float endlessBonus = endlessLoopCount  * endlessExtraMultiplier;
        return 1f + stageBonus + endlessBonus;
    }

    public void SetStage(int index)
    {
        if (stages == null || stages.Length == 0)
        {
            GameLog.Warn("[StageManager] 스테이지 데이터가 없습니다.", this);
            return;
        }
        index = Mathf.Clamp(index, 0, stages.Length - 1);
        currentStageIndex = index;
        currentKillCount  = 0;
        SetPhaseInternal(StagePhase.Normal, invokeStageChanged: true);
    }

    private void SetPhaseInternal(StagePhase phase, bool invokeStageChanged = false)
    {
        currentPhase = phase;
        if (invokeStageChanged) OnStageChanged?.Invoke();
        OnPhaseChanged?.Invoke(currentPhase);
        OnKillCountChanged?.Invoke();
    }

    public void OnEnemyKilled(EnemyStats enemy)
    {
        bool isBoss = enemy != null && enemy.data != null && enemy.data.isBoss;
        currentKillCount++;
        OnKillCountChanged?.Invoke();

        var stage = CurrentStage;
        if (stage == null) return;

        if (isBoss) { GoToNextStage(); return; }

        if (currentPhase == StagePhase.Normal &&
            stage.normalKillToSummonBoss > 0 &&
            currentKillCount >= stage.normalKillToSummonBoss)
            EnterBossPhase();
    }

    private void EnterBossPhase()
    {
        SoundManager.Instance?.PlayBossWarning();
        SetPhaseInternal(StagePhase.Boss);
    }

    public void GoToNextStage()
    {
        if (stages == null || stages.Length == 0) return;

        int nextIndex = currentStageIndex + 1;

        if (nextIndex >= stages.Length)
        {
            if (endlessMode)
            {
                endlessLoopCount++;
                SoundManager.Instance?.PlayStageUp();
                SetStage(0);                          // 처음 스테이지로 루프
                OnEndlessLoopStarted?.Invoke(endlessLoopCount);
            }
            else
            {
                SetPhaseInternal(StagePhase.Cleared, invokeStageChanged: true);
            }
            return;
        }

        SoundManager.Instance?.PlayStageUp();
        SetStage(nextIndex);
    }

    public void OnPlayerDied()
    {
        if (currentPhase == StagePhase.Boss)
        {
            currentKillCount = 0;
            SetPhaseInternal(StagePhase.Normal);
        }
        else
        {
            OnKillCountChanged?.Invoke();
        }
    }

    public int GetCurrentKillCount() => currentKillCount;

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveManager.Instance?.SaveGame();
    }
}
