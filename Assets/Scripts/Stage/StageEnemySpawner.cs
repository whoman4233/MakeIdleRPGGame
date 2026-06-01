using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEnemySpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    public EnemyController masterEnemyPrefab;

    [Header("Spawn Rules")]
    public float spawnOffsetX  = 15f;

    [Header("Pooling Settings")]
    public int initialPoolSize = 10;

    public event Action<EnemyStats> OnBossSpawned;

    private Coroutine _spawnRoutine;
    private readonly Queue<EnemyController> _enemyPool = new Queue<EnemyController>();
    private float _spawnInterval = 3f;

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            var e = Instantiate(masterEnemyPrefab, transform);
            e.gameObject.SetActive(false);
            _enemyPool.Enqueue(e);
        }
    }

    private void Start()
    {
        var sm = StageManager.Instance ?? FindObjectOfType<StageManager>();
        if (sm != null)
        {
            sm.OnPhaseChanged += HandlePhaseChanged;
            sm.OnStageChanged += HandleStageChanged;
        }
        HandleStageChanged(); // 초기 스폰 간격 적용
        StartSpawning();
    }

    private void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
            StageManager.Instance.OnStageChanged -= HandleStageChanged;
        }
    }

    private void HandleStageChanged()
    {
        var stage = StageManager.Instance?.CurrentStage;
        if (stage != null) _spawnInterval = stage.spawnIntervalNormal;
    }

    private void HandlePhaseChanged(StagePhase newPhase)
    {
        if (newPhase == StagePhase.Boss)        { StopSpawning(); SpawnBoss(); }
        else if (newPhase == StagePhase.Normal) { StartSpawning(); }
    }

    public void StartSpawning()
    {
        if (_spawnRoutine == null)
            _spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null) { StopCoroutine(_spawnRoutine); _spawnRoutine = null; }
    }

    private IEnumerator SpawnRoutine()
    {
        bool keepGoing = true;
        while (keepGoing)
        {
            var stage = StageManager.Instance?.CurrentStage;
            if (stage != null && stage.normalEnemies != null && stage.normalEnemies.Count > 0)
                SpawnEnemy(false);
            else
                GameLog.Warn("[StageEnemySpawner] 현재 스테이지에 normalEnemies 없음");
            yield return new WaitForSeconds(_spawnInterval);
        }
    }

    private void SpawnBoss()
    {
        var stage = StageManager.Instance?.CurrentStage;
        if (stage == null || stage.bossEnemy == null)
        { GameLog.Warn("[Spawner] 보스 데이터 없음!"); return; }
        SpawnEnemy(true, stage.bossEnemy);
    }

    private void SpawnEnemy(bool isBoss, EnemyStatsData overrideData = null)
    {
        if (masterEnemyPrefab == null || PlayerRef.Instance == null) return;

        Vector3 spawnPos = PlayerRef.Instance.transform.position;
        spawnPos.x += spawnOffsetX; spawnPos.y = 0f; spawnPos.z = 0f;

        EnemyController target = _enemyPool.Count > 0
            ? _enemyPool.Dequeue()
            : Instantiate(masterEnemyPrefab, spawnPos, Quaternion.identity, transform);

        target.transform.position = spawnPos;
        target.gameObject.SetActive(true);

        EnemyStatsData data = overrideData;
        if (data == null)
        {
            var stage = StageManager.Instance?.CurrentStage;
            if (stage != null && stage.normalEnemies.Count > 0)
                data = stage.normalEnemies[UnityEngine.Random.Range(0, stage.normalEnemies.Count)];
        }
        if (data == null) return;

        target.Init(data, this);

        if (isBoss)
        {
            var stats = target.GetComponent<EnemyStats>();
            OnBossSpawned?.Invoke(stats);
        }
    }

    private const int MAX_POOL_SIZE = 30;

    public void ReturnToPool(EnemyController enemy)
    {
        enemy.gameObject.SetActive(false);
        if (_enemyPool.Count < MAX_POOL_SIZE)
            _enemyPool.Enqueue(enemy);
        else
            Destroy(enemy.gameObject);
    }
}
