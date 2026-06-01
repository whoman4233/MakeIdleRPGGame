using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class HealthSystem : MonoBehaviour, IAttackable
{
    public event Action OnHealthChanged;
    public event Action OnDied;
    public event Action OnDamaged;

    private PlayerStats playerStats;
    private float currentHealth;
    private bool _isDead;

    [Header("Invincibility")]
    [SerializeField] private float invincibilityDuration = 0.3f;
    private float _invincibleUntil;

    public float InvincibilityDuration => invincibilityDuration;

    public float CurrentHealth => currentHealth;
    public bool IsAlive => !_isDead && currentHealth > 0f;
    public int TeamId => 0;
    public Transform Transform => transform;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        ReviveFull();
    }

    private void Update()
    {
        HandleHealthRegen();
    }

    private void HandleHealthRegen()
    {
        if (!IsAlive) return;
        float regen = playerStats.GetStatValue(StatType.HealthRegen);
        if (regen == 0f) return;
        float maxHealth = playerStats.GetStatValue(StatType.MaxHealth);
        float prev = currentHealth;
        currentHealth += regen * Time.deltaTime;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        if (Mathf.Abs(currentHealth - prev) > 0.01f)
            OnHealthChanged?.Invoke();
    }

    public void TakeDamage(float damageAmount)
    {
        if (!IsAlive) return;
        if (Time.time < _invincibleUntil) return;
        _invincibleUntil = Time.time + invincibilityDuration;
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0f, currentHealth);
        OnHealthChanged?.Invoke();
        SoundManager.Instance?.PlayEnemyHit();
        GameFeel.Instance?.Shake(0.12f, 0.18f);
        OnDamaged?.Invoke();
        if (currentHealth <= 0f) Kill();
    }

    public void ReviveFull()
    {
        _isDead = false;
        _invincibleUntil = 0f;
        float maxHealth = playerStats.GetStatValue(StatType.MaxHealth);
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
    }

    private void Kill()
    {
        if (_isDead) return;
        _isDead = true;
        OnDied?.Invoke();
    }
}