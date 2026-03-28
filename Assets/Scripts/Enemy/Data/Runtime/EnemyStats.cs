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

    // 새로운 EnemyStatsData.cs 변수명에 맞게 매핑
    public float MaxHP => data != null ? data.MaxHealth : 0f;
    public float AttackPower => data != null ? data.AttackPower : 0f;
    public float MoveSpeed => data != null ? data.MoveSpeed : 0f;
    public float AttackInterval => data != null ? data.AttackInterval : 1.5f;
    public float AttackRange => data != null ? data.AttackRange : 1.5f;
    
    // EXP 관련 코드는 삭제하고, 골드는 관측 데이터(DropGold)로 변경
    public int DropGold => data != null ? data.DropGold : 0;

    private void Awake()
    {
        // 스포너가 데이터를 나중에 주입할 수도 있으므로 여기서 에러를 띄우지 않습니다.
        // if (data == null) Debug.LogWarning("EnemyStats: 데이터 대기 중...");
    }

    // ★ 스포너에서 껍데기에 영혼(데이터)을 주입할 때 호출할 함수
    public void Init(EnemyStatsData newData)
    {
        data = newData;
        curHP = MaxHP;
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

    private void Start()
    {
        if (data != null)
        {
            curHP = MaxHP;
            OnStatsChanged?.Invoke();
        }
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        float before = curHP;
        curHP = Mathf.Clamp(curHP - amount, 0f, MaxHP);

        // 연출을 위한 이벤트 호출
        OnStatsChanged?.Invoke();

        if (curHP <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDied?.Invoke();

        // 스테이지 매니저에게 격리(처치) 완료 보고 -> 게이지 상승
        if (StageManager.Instance != null)
        {
            StageManager.Instance.OnEnemyKilled(this);
        }

        // 재화 획득 (CurrencyManager.Instance.AddGold 혹은 AddData 함수명에 맞춰주세요)
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddData(DropGold);
        }

        Destroy(gameObject);
    }
}