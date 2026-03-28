using UnityEngine;

[RequireComponent(typeof(EnemyStats))]
public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    private EnemyStats _stats;
    private IAttackable _player;
    private float _lastAttackTime;

    private void Awake()
    {
        _stats = GetComponent<EnemyStats>();
    }

    // 스포너가 적을 생성할 때 호출합니다.
    public void Init(EnemyStatsData data)
    {
        _stats.Init(data); // 스탯 초기화 넘겨주기

        // 2.5D 비주얼 세팅
        if (spriteRenderer != null && data.enemySprite != null)
        {
            spriteRenderer.sprite = data.enemySprite;
            spriteRenderer.color = data.visualColor;
            spriteRenderer.transform.localScale = data.visualScale;
        }

        gameObject.name = $"Enemy_{data.enemyName}";
    }

    private void Start()
    {
        if (PlayerRef.Instance != null)
            _player = PlayerRef.Instance.GetComponent<IAttackable>();
    }

    private void Update()
    {
        if (!_stats.IsAlive || _player == null || !_player.IsAlive) return;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.Transform.position);

        if (distanceToPlayer > _stats.AttackRange)
        {
            // 무한 직진 (런닝머신)
            Vector3 direction = (_player.Transform.position - transform.position).normalized;
            direction.y = 0f; direction.z = 0f; 
            transform.position += direction * (_stats.MoveSpeed * Time.deltaTime);
        }
        else
        {
            AttackPlayer();
        }
    }

    private void AttackPlayer()
    {
        if (Time.time - _lastAttackTime >= _stats.AttackInterval)
        {
            _lastAttackTime = Time.time;
            _player.TakeDamage(_stats.AttackPower);
        }
    }
}