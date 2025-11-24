using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerStats : MonoBehaviour, IAttackable
{
    [Header("Base Data (ScriptableObject)")]
    public PlayerStatsData data;

    [Header("Team")]
    [SerializeField] private int teamId = 0; // 0 = 플레이어

    [Header("Runtime Level & Exp")]
    public int level;
    public float curExp;
    public float expToNextLevel;

    [Header("Runtime Resources")]
    public float curHP;
    public float curMP;

    public event Action OnStatsChanged;
    public event Action OnLevelUp;
    public event Action OnDied;

    // IAttackable
    public Transform Transform => transform;
    public bool IsAlive => curHP > 0f;
    public int TeamId => teamId;

    public float MaxHP => data != null ? data.GetMaxHP(level) : 0f;
    public float MaxMP => data != null ? data.GetMaxMP(level) : 0f;
    public float Attack => data != null ? data.GetAttack(level) : 0f;
    public float Defense => data != null ? data.GetDefense(level) : 0f;
    public float MoveSpeed => data != null ? data.GetMoveSpeed() : 0f;
    public float AttackInterval => data != null ? data.GetAttackInterval() : 1f;

    private void Awake()
    {
        if (data == null)
            Debug.LogError("PlayerStats: PlayerStatsData가 비어있습니다.", this);
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
        InitFromData();
    }

    public void InitFromData()
    {
        if (data == null) return;

        level = Mathf.Max(data.baseLevel, 1);
        curHP = MaxHP;
        curMP = MaxMP;

        curExp = 0f;
        expToNextLevel = data.GetRequiredExp(level);

        OnStatsChanged?.Invoke();
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        float damage = Mathf.Max(amount - Defense, 1f);
        curHP = Mathf.Clamp(curHP - damage, 0f, MaxHP);
        OnStatsChanged?.Invoke();

        if (curHP <= 0f)
            Die();
    }

    public void HealHP(float amount)
    {
        curHP = Mathf.Clamp(curHP + amount, 0f, MaxHP);
        OnStatsChanged?.Invoke();
    }

    public void AddExp(float amount)
    {
        if (data == null || level >= data.maxLevel) return;

        curExp += amount;

        while (curExp >= expToNextLevel && level < data.maxLevel)
        {
            curExp -= expToNextLevel;
            LevelUpInternal();
        }

        OnStatsChanged?.Invoke();
    }

    private void LevelUpInternal()
    {
        level++;
        expToNextLevel = data.GetRequiredExp(level);
        curHP = MaxHP;
        curMP = MaxMP;
        OnLevelUp?.Invoke();
    }

    private void Die()
    {
        OnDied?.Invoke();
        // 죽음 애니메이션 / 리스폰은 PlayerController에서
    }
}
