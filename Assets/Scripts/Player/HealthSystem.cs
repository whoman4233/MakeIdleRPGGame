using System;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class HealthSystem : MonoBehaviour, IAttackable
{
    // 1. 체력 변경 시 UI 등에 알릴 이벤트
    public event Action OnHealthChanged;
    // 2. 사망 시 게임 오버 UI 등에 알릴 이벤트
    public event Action OnDied;

    private PlayerStats playerStats;
    private float currentHealth;

    // IAttackable 인터페이스 구현 및 외부 참조용 프로퍼티
    public float CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;
    public int TeamId => 0; 
    public Transform Transform => transform;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        // 시작 시 최대 체력으로 초기화
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
        if (regen != 0) 
        {
            float maxHealth = playerStats.GetStatValue(StatType.MaxHealth);
            currentHealth += regen * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            
            // 체력이 변했으므로 HUD 갱신을 위해 이벤트 호출
            OnHealthChanged?.Invoke();

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (!IsAlive) return;

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // 데미지를 입었으므로 HUD 갱신을 위해 이벤트 호출
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ReviveFull()
    {
        float maxHealth = playerStats.GetStatValue(StatType.MaxHealth);
        currentHealth = maxHealth;
        
        // 체력이 가득 찼으므로 HUD 갱신을 위해 이벤트 호출
        OnHealthChanged?.Invoke();
        Debug.Log("Player Health Restored.");
    }

    private void Die()
    {
        OnDied?.Invoke();
        Debug.Log("Player Dead.");
    }
}