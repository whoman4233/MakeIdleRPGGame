using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public EnemyStats enemyPrefab;

    public int maxAlive = 5;
    public float spawnInterval = 3f;
    public Vector3 spawnAreaSize = new Vector3(5, 0, 5);

    private float _timer;

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < spawnInterval)
            return;

        _timer = 0f;

        // 현재 살아있는 EnemyStats 개수 확인
        int aliveCount = 0;

        if (AttackableRegistry.Instance != null)
        {
            foreach (var unit in AttackableRegistry.Instance.Units)
            {
                if (unit is EnemyStats enemy && enemy.IsAlive)
                    aliveCount++;
            }
        }

        if (aliveCount >= maxAlive)
            return;

        SpawnOne();
    }

    private void SpawnOne()
    {
        Vector3 offset = new Vector3(
            Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f),
            0f,
            Random.Range(-spawnAreaSize.z * 0.5f, spawnAreaSize.z * 0.5f)
        );

        Vector3 pos = transform.position + offset;
        Instantiate(enemyPrefab, pos, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.1f, spawnAreaSize);
    }
}
