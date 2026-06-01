using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemyStats : MonoBehaviour, IAttackable
{
    [Header("Data")]
    public EnemyStatsData data;

    [Header("Team")]
    [SerializeField] private int teamId = 1;

    [Header("Runtime")]
    public float curHP;
    private bool _isDead;

    public event Action OnStatsChanged;
    public event Action OnDied;
    public event Action OnDamaged;

    public Transform Transform => transform;
    public bool IsAlive => !_isDead && curHP > 0f;
    public int TeamId => teamId;

    private float Multiplier => StageManager.Instance != null ? StageManager.Instance.GetStatMultiplier() : 1f;

    public float MaxHP          => data != null ? data.MaxHealth      * Multiplier : 0f;
    public float AttackPower    => data != null ? data.AttackPower    * Multiplier : 0f;
    public float MoveSpeed      => data != null ? data.MoveSpeed      : 0f;
    public float AttackInterval => data != null ? data.AttackInterval : 1.5f;
    public float AttackRange    => data != null ? data.AttackRange    : 1.5f;
    public int   DropGold       => data != null ? Mathf.RoundToInt(data.DropGold * Multiplier) : 0;

    public void Init(EnemyStatsData newData)
    {
        data    = newData;
        _isDead = false;
        curHP   = MaxHP;
        OnStatsChanged?.Invoke();
    }

    private void OnEnable()  { AttackableRegistry.Instance?.Register(this); }
    private void OnDisable() { AttackableRegistry.Instance?.Unregister(this); }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;
        curHP = Mathf.Clamp(curHP - amount, 0f, MaxHP);
        OnStatsChanged?.Invoke();
        OnDamaged?.Invoke();
        if (curHP <= 0f) Die();
    }

    private void Die()
    {
        _isDead = true;
        OnDied?.Invoke();

        if (StageManager.Instance != null)
            StageManager.Instance.OnEnemyKilled(this);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddData(DropGold);

        TryDropGraft();

        // 주의: Destroy 호출 금지
        // IsAlive == false 가 되면 EnemyController가 ReturnToPool 처리함
    }

    private void TryDropGraft()
    {
        if (data == null) return;
        if (data.dropGrafts == null || data.dropGrafts.Count == 0) return;
        if (InventoryManager.Instance == null) return;

        if (UnityEngine.Random.value > data.graftDropChance) return;

        int idx = UnityEngine.Random.Range(0, data.dropGrafts.Count);
        var drop = data.dropGrafts[idx];
        if (drop == null) return;

        InventoryManager.Instance.AddGraft(drop);
        GameLog.Log($"[Drop] {data.enemyName} -> {drop.graftName} 드랍!");
    }
}