using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;

    private EnemyStats        _stats;
    private StageEnemySpawner _spawner;
    private Transform         _playerTransform;
    private float             _randomStopDistance;
    private float             _lastAttackTime;
    private bool              _pendingReturn;
    private EnemyMotion       _motion;

    private void Awake()
    {
        _stats  = GetComponent<EnemyStats>();
        _motion = GetComponent<EnemyMotion>();
    }

    private void OnEnable()
    {
        _pendingReturn = false;
        if (_stats != null) _stats.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        if (_stats != null) _stats.OnDied -= HandleDeath;
    }

    private void OnDestroy()
    {
        if (_stats != null) _stats.OnDied -= HandleDeath;
    }

    public void Init(EnemyStatsData data, StageEnemySpawner spawner)
    {
        _spawner       = spawner;
        _pendingReturn = false;
        _stats.Init(data);

        if (spriteRenderer != null && data != null)
        {
            spriteRenderer.sprite = data.enemySprite;
            spriteRenderer.color  = data.visualColor;
            transform.localScale  = data.visualScale;
            var bgMat = Resources.Load<Material>("EnemyWhiteBG");
            if (bgMat != null) spriteRenderer.sharedMaterial = bgMat;
        }

        if (PlayerRef.Instance != null) _playerTransform = PlayerRef.Instance.transform;

        float baseRange = _stats.AttackRange > 0f ? _stats.AttackRange : 1.5f;
        _randomStopDistance = Random.Range(baseRange * 0.8f, baseRange * 1.2f);
        _lastAttackTime = Time.time;
    }

    private void Update()
    {
        if (_pendingReturn) return;
        if (!_stats.IsAlive) return;
        if (_playerTransform == null) return;

        float playerX = _playerTransform.position.x;

        if (transform.position.x > playerX + _randomStopDistance)
        {
            transform.Translate(Vector3.left * (_stats.MoveSpeed * Time.deltaTime));
        }
        else
        {
            if (Time.time - _lastAttackTime >= _stats.AttackInterval)
            {
                _lastAttackTime = Time.time;
                PerformAttack();
            }
        }

        if (transform.position.x < playerX - 25f)
            ReturnToPool();
    }

    private void PerformAttack()
    {
        _motion?.PlayAttack();
        var playerRef = PlayerRef.Instance;
        if (playerRef == null) return;
        if (!playerRef.TryGetComponent(out HealthSystem playerHealth)) return;
        if (!playerHealth.IsAlive) return;
        playerHealth.TakeDamage(_stats.AttackPower);
        SoundManager.Instance?.PlayPlayerHit();
    }

    private void HandleDeath()
    {
        if (_pendingReturn) return;
        _pendingReturn = true;
        SoundManager.Instance?.PlayEnemyDeath();

        // 보스 처치 순간에만 강한 타격감 연출 (일반 몹은 생략 — 과하면 촌스러움)
        if (_stats != null && _stats.IsBoss)
        {
            GameFeel.Instance?.HitStop(0.08f);
            GameFeel.Instance?.Shake(0.35f, 0.4f);
        }

        // EnemyMotion이 있으면 페이드아웃 완료 후 반환 콜백 위임
        // 없으면 즉시 반환
        if (_motion != null)
            _motion.PlayDeath(ReturnToPool);
        else
            ReturnToPool();
    }

    public void ReturnToPool()
    {
        if (_spawner != null)
            _spawner.ReturnToPool(this);
        else
            gameObject.SetActive(false);
    }
}