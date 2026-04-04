using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEnemySpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    public EnemyController masterEnemyPrefab;
    public List<EnemyStatsData> spawnableEnemies;
    
    // 이번에 테스트를 위해 추가된 보스 데이터 연결 슬롯
    [Header("Boss Setup")]
    public EnemyStatsData testBossData; 

    [Header("Spawn Rules")]
    public float spawnInterval = 3f;
    public float spawnOffsetX = 15f;

    [Header("Pooling Settings")]
    public int initialPoolSize = 10;

    private Coroutine _spawnRoutine;
    private Queue<EnemyController> _enemyPool = new Queue<EnemyController>();

    private void Awake()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            EnemyController enemy = Instantiate(masterEnemyPrefab, transform);
            enemy.gameObject.SetActive(false);
            _enemyPool.Enqueue(enemy);
        }
    }

    private void Start()
    {
        if (StageManager.Instance != null)
        {
            // 스테이지 페이즈(보스 등장 등) 변경 이벤트 구독
            StageManager.Instance.OnPhaseChanged += HandlePhaseChanged;
        }

        StartSpawning();
    }

    private void OnDestroy()
    {
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    // 페이즈가 변경될 때 스폰 로직을 제어합니다.
    private void HandlePhaseChanged(StagePhase newPhase)
    {
        if (newPhase == StagePhase.Boss)
        {
            StopSpawning(); // 일반 스폰 정지
            SpawnBoss();    // 보스 1마리 소환
        }
        else if (newPhase == StagePhase.Normal)
        {
            StartSpawning(); // 일반 스폰 재개
        }
    }

    public void StartSpawning()
    {
        if (_spawnRoutine == null)
            _spawnRoutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (_spawnRoutine != null)
        {
            StopCoroutine(_spawnRoutine);
            _spawnRoutine = null;
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            SpawnEnemy(false); // 일반 몬스터 스폰
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnBoss()
    {
        if (testBossData == null)
        {
            Debug.LogWarning("[Spawner] 보스 데이터가 연결되지 않았습니다!");
            return;
        }
        SpawnEnemy(true); // 보스 스폰
    }

    // isBoss 플래그에 따라 주입할 데이터를 결정합니다.
    private void SpawnEnemy(bool isBoss)
    {
        if (masterEnemyPrefab == null) return;
        if (PlayerRef.Instance == null) return;

        Vector3 spawnPos = PlayerRef.Instance.transform.position;
        spawnPos.x += spawnOffsetX;
        spawnPos.y = 0f;
        spawnPos.z = 0f;

        EnemyController spawnTarget = null;

        if (_enemyPool.Count > 0)
        {
            spawnTarget = _enemyPool.Dequeue();
            spawnTarget.transform.position = spawnPos;
            spawnTarget.gameObject.SetActive(true);
        }
        else
        {
            spawnTarget = Instantiate(masterEnemyPrefab, spawnPos, Quaternion.identity, transform);
        }
        
        // 보스면 보스 데이터, 아니면 리스트에서 랜덤 데이터
        EnemyStatsData dataToInject = isBoss ? testBossData : spawnableEnemies[Random.Range(0, spawnableEnemies.Count)];
        
        spawnTarget.Init(dataToInject, this);
    }

    public void ReturnToPool(EnemyController enemy)
    {
        enemy.gameObject.SetActive(false);
        _enemyPool.Enqueue(enemy);
    }
}