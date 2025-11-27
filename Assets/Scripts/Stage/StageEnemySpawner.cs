using UnityEngine;

public class StageEnemySpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    public Vector3 spawnAreaSize = new Vector3(5, 0, 5);

    [Header("Boss Spawn")]
    public Transform bossSpawnPoint; // 비워두면 이 스포너 위치 사용

    private float _timer;
    private bool _bossSpawned;

    private void OnEnable()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged += OnStageChanged;
            StageManager.Instance.OnPhaseChanged += OnPhaseChanged;
        }
    }

    private void OnDisable()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnStageChanged -= OnStageChanged;
            StageManager.Instance.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    private void Start()
    {
        ResetForCurrentStage();
    }

    private void Update()
    {
        var stageMgr = StageManager.Instance;
        if (stageMgr == null)
            return;

        var stage = stageMgr.CurrentStage;
        if (stage == null)
            return;

        var phase = stageMgr.CurrentPhase;

        if (phase == StagePhase.Normal)
        {
            UpdateNormalPhase(stage);
        }
        else if (phase == StagePhase.Boss)
        {
            UpdateBossPhase(stage);
        }
        else
        {
            // Cleared 등 다른 상태에서는 아무 것도 안 함
        }
    }

    private void UpdateNormalPhase(StageData stage)
    {
        if (stage.normalEnemyPrefab == null)
            return;

        _timer += Time.deltaTime;
        if (_timer < stage.spawnIntervalNormal)
            return;

        _timer = 0f;

        int aliveNormal = 0;

        if (AttackableRegistry.Instance != null)
        {
            foreach (var unit in AttackableRegistry.Instance.Units)
            {
                if (unit is EnemyStats enemy && enemy.IsAlive && enemy.data != null && !enemy.data.isBoss)
                {
                    aliveNormal++;
                }
            }
        }

        if (aliveNormal >= stage.maxAliveNormal)
            return;

        SpawnNormal(stage.normalEnemyPrefab);
    }

    private void UpdateBossPhase(StageData stage)
    {
        if (_bossSpawned)
            return;

        if (stage.bossEnemyPrefab == null)
            return;

        SpawnBoss(stage.bossEnemyPrefab);
        _bossSpawned = true;
    }

    private void SpawnNormal(EnemyStats prefab)
    {
        Vector3 offset = new Vector3(
            Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f),
            0f,
            Random.Range(-spawnAreaSize.z * 0.5f, spawnAreaSize.z * 0.5f)
        );

        Vector3 pos = transform.position + offset;
        Instantiate(prefab, pos, Quaternion.identity);
    }

    private void SpawnBoss(EnemyStats prefab)
    {
        Vector3 pos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        Instantiate(prefab, pos, Quaternion.identity);
        Debug.Log("[StageEnemySpawner] Boss Spawned");
    }

    private void OnStageChanged()
    {
        ResetForCurrentStage();
    }

    private void OnPhaseChanged(StagePhase phase)
    {
        if (phase == StagePhase.Normal)
        {
            _bossSpawned = false;
            _timer = 0f;
        }
    }

    private void ResetForCurrentStage()
    {
        _bossSpawned = false;
        _timer = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.1f, spawnAreaSize);

        if (bossSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bossSpawnPoint.position, 0.5f);
        }
    }
}
