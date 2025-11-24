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

    // IAttackable 구현
    public Transform Transform => transform;
    public bool IsAlive => curHP > 0f;
    public int TeamId => teamId;

    // 읽기용 프로퍼티
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
        AttackableRegistry.Instance?.Register(this);
    }

    private void OnDisable()
    {
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

        curHP = Mathf.Clamp(curHP - amount, 0f, MaxHP);
        OnStatsChanged?.Invoke();

        if (curHP <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDied?.Invoke();

        // TODO: 여기서 골드/EXP 지급 연결 (나중에 CurrencyManager, PlayerStats.AddExp 등)
        // 예시:
        // PlayerRef.Instance.Stats.AddExp(ExpReward);
        // CurrencyManager.Instance.AddGold(GoldReward);

        // TODO: 죽음 이펙트/애니메이션 필요하면 추가
        Destroy(gameObject);
    }
}
