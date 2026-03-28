using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageEnemySpawner : MonoBehaviour
{
    [Header("Spawn Setup")]
    public EnemyController masterEnemyPrefab;      // 우리가 만든 '단 하나의 껍데기 프리팹'
    public List<EnemyStatsData> spawnableEnemies;  // 이번 스테이지에 나올 적 데이터(SO) 리스트
    
    [Header("Spawn Rules")]
    public float spawnInterval = 3f;               // 스폰 주기
    public float spawnOffsetX = 15f;               // 플레이어 기준 오른쪽으로 얼마나 멀리서 생성할지

    private Coroutine _spawnRoutine;

    private void Start()
    {
        StartSpawning();
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
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (masterEnemyPrefab == null || spawnableEnemies == null || spawnableEnemies.Count == 0) return;
        if (PlayerRef.Instance == null) return;

        // ★ 런닝머신 전용 고정 스폰 위치 (플레이어 우측 +15)
        Vector3 spawnPos = PlayerRef.Instance.transform.position;
        spawnPos.x += spawnOffsetX;
        spawnPos.y = 0f;
        spawnPos.z = 0f;

        // 껍데기 생성
        EnemyController newEnemy = Instantiate(masterEnemyPrefab, spawnPos, Quaternion.identity);
        
        // 랜덤 데이터 주입 (변신!)
        EnemyStatsData randomData = spawnableEnemies[Random.Range(0, spawnableEnemies.Count)];
        newEnemy.Init(randomData);
    }
}