using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;

    private EnemyStats _stats;
    private StageEnemySpawner _spawner;
    
    private float _randomStopDistance;
    private float _lastAttackTime;

    private void Awake()
    {
        _stats = GetComponent<EnemyStats>();
        _stats.OnDied += HandleDeath;
    }

    private void OnDestroy()
    {
        if (_stats != null)
        {
            _stats.OnDied -= HandleDeath;
        }
    }

    public void Init(EnemyStatsData data, StageEnemySpawner spawner)
    {
        _spawner = spawner;
        _stats.Init(data);

        if (spriteRenderer != null && data != null)
        {
            spriteRenderer.sprite = data.enemySprite;
            spriteRenderer.color = data.visualColor;
            transform.localScale = data.visualScale;
        }

        // 기본 사거리를 기준으로 ±20%의 난수를 주어 도착 위치를 분산 (겹침 효과 연출)
        float baseRange = _stats.AttackRange > 0f ? _stats.AttackRange : 1.5f;
        _randomStopDistance = Random.Range(baseRange * 0.8f, baseRange * 1.2f);
        
        // 스폰 시 공격 타이머 초기화 (스폰되자마자 바로 때리지 않도록 딜레이 추가 가능)
        _lastAttackTime = Time.time;
    }

    private void Update()
    {
        if (!_stats.IsAlive) return;
        if (PlayerRef.Instance == null) return;

        float playerX = PlayerRef.Instance.transform.position.x;

        // 플레이어와의 거리가 설정된 정지 거리보다 멀면 계속 전진
        if (transform.position.x > playerX + _randomStopDistance)
        {
            transform.Translate(Vector3.left * (_stats.MoveSpeed * Time.deltaTime));
        }
        else
        {
            // 도달 시 멈추고 쿨타임마다 공격
            if (Time.time - _lastAttackTime >= _stats.AttackInterval)
            {
                _lastAttackTime = Time.time;
                PerformAttack();
            }
        }

        // 예외 처리: 만약 플레이어를 지나쳐서 너무 멀리 갔다면 반환
        if (transform.position.x < playerX - 20f)
        {
            ReturnToPool();
        }
    }

    private void PerformAttack()
    {
        // PlayerRef를 통해 플레이어의 체력 시스템에 접근하여 데미지 적용
        if (PlayerRef.Instance.TryGetComponent(out HealthSystem playerHealth))
        {
            if (playerHealth.IsAlive)
            {
                playerHealth.TakeDamage(_stats.AttackPower);
                Debug.Log($"[Enemy] 플레이어에게 {_stats.AttackPower} 데미지 타격!");
            }
        }
    }

    private void HandleDeath()
    {
        ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (_spawner != null)
        {
            _spawner.ReturnToPool(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}