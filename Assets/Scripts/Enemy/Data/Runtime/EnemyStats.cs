using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyStats : MonoBehaviour, IAttackable
{
    [Header("Data")]
    public EnemyStatsData data;

    [Header("Team")]
    [SerializeField] private int teamId = 1;   // 0 = 플레이어, 1 = 적

    [Header("Runtime")]
    public float curHP;

    public event Action OnStatsChanged;
    public event Action OnDied;

    // IAttackable
    public Transform Transform => transform;
    public bool IsAlive => curHP > 0f;
    public int TeamId => teamId;

    public float MaxHP => data != null ? data.maxHP : 0f;
    public float AttackPower => data != null ? data.attackPower : 0f;
    public float MoveSpeed => data != null ? data.moveSpeed : 0f;
    public float AttackInterval => data != null ? data.attackInterval : 1.5f;
    public float AttackRange => data != null ? data.attackRange : 1.5f;
    public int ExpReward => data != null ? data.expReward : 0;
    public long GoldReward => data != null ? data.goldReward : 0;

    private void Awake()
    {
        if (data == null)
            Debug.LogError("EnemyStats: EnemyStatsData가 비어있습니다.", this);
    }

    private void OnEnable()
    {
        Debug.Log($"[EnemyStats] Register: {name}");
        AttackableRegistry.Instance?.Register(this);
    }

    private void OnDisable()
    {
        Debug.Log($"[EnemyStats] Unregister: {name}");
        AttackableRegistry.Instance?.Unregister(this);
    }

    private void Start()
    {
        curHP = MaxHP;
        OnStatsChanged?.Invoke();
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        float before = curHP;
        curHP = Mathf.Clamp(curHP - amount, 0f, MaxHP);

        Debug.Log($"[EnemyStats] {name} TakeDamage {amount}, HP {before} -> {curHP}");

        OnStatsChanged?.Invoke();

        if (curHP <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"[EnemyStats] {name} Die() 호출됨");

        OnDied?.Invoke();

        // 여기서 StageManager에 꼭 알려줘야 함
        if (StageManager.Instance != null)
        {
            Debug.Log($"[EnemyStats] {name} StageManager.OnEnemyKilled 호출");
            StageManager.Instance.OnEnemyKilled(this);
        }
        else
        {
            Debug.LogWarning("[EnemyStats] StageManager.Instance == null 이라 OnEnemyKilled 못 부름");
        }

        // ✅ 보상 지급
        if (PlayerRef.Instance != null)
        {
            PlayerRef.Instance.Stats.AddExp(ExpReward);
        }

        if (CurrencyManager.Instance != null)
        {
            float finalGold = GoldReward * PlayerRef.Instance.Stats.GoldGainBonus;
            CurrencyManager.Instance.AddGold((long)finalGold);
        }

        Destroy(gameObject);
    }
}
