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
    private bool _isDead; // 중복 사망 처리 방지

    public event Action OnStatsChanged;
    public event Action OnDied;

    public Transform Transform => transform;
    public bool IsAlive => !_isDead && curHP > 0f;
    public int TeamId => teamId;

    // StageManager의 배율을 가져와서 실시간으로 스탯 뻥튀기 적용
    private float Multiplier => StageManager.Instance != null ? StageManager.Instance.GetStatMultiplier() : 1f;

    public float MaxHP => data != null ? data.MaxHealth * Multiplier : 0f;
    public float AttackPower => data != null ? data.AttackPower * Multiplier : 0f;
    public float MoveSpeed => data != null ? data.MoveSpeed : 0f;
    public float AttackInterval => data != null ? data.AttackInterval : 1.5f;
    public float AttackRange => data != null ? data.AttackRange : 1.5f;
    
    // 재화 드랍량도 스테이지가 오를수록 증가
    public int DropGold => data != null ? Mathf.RoundToInt(data.DropGold * Multiplier) : 0;

    public void Init(EnemyStatsData newData)
    {
        data = newData;
        _isDead = false;
        curHP = MaxHP; // 스케일링이 적용된 MaxHP로 초기화
        OnStatsChanged?.Invoke();
    }

    private void OnEnable()
    {
        AttackableRegistry.Instance?.Register(this);
    }

    private void OnDisable()
    {
        AttackableRegistry.Instance?.Unregister(this);
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
        _isDead = true; // 플래그 설정
        OnDied?.Invoke();

        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnEnemyKilled(this);
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddData(DropGold);
        }

        // 주의: 여기서 Destroy(gameObject)를 호출하면 안 됩니다!
        // 사망 상태(!IsAlive)가 되면 EnemyController.Update()에서 ReturnToPool을 알아서 호출합니다.
    }
}