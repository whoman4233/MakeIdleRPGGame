using System.Collections.Generic;
using UnityEngine;

public class AttackableRegistry : MonoBehaviour
{
    public static AttackableRegistry Instance { get; private set; }

    private readonly HashSet<IAttackable>  _units      = new HashSet<IAttackable>();
    private readonly List<IAttackable>     _rangeCache = new List<IAttackable>();
    private readonly List<IAttackable>     _deadCache  = new List<IAttackable>();

    // 주기적으로 죽은/null 유닛 정리 (매 N초)
    private float _cleanupInterval = 5f;
    private float _nextCleanup;

    public IReadOnlyCollection<IAttackable> Units => _units;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Time.time >= _nextCleanup)
        {
            _nextCleanup = Time.time + _cleanupInterval;
            CleanupDeadUnits();
        }
    }

    public void Register(IAttackable unit)
    {
        if (unit != null) _units.Add(unit);
    }

    public void Unregister(IAttackable unit)
    {
        if (unit != null) _units.Remove(unit);
    }

    /// <summary>범위 내 살아있는 적 반환 (캐시 재사용)</summary>
    public List<IAttackable> GetEnemiesInRange(Vector3 origin, float range)
    {
        _rangeCache.Clear();
        foreach (var unit in _units)
        {
            if (unit == null || !unit.IsAlive) continue;
            if (unit.TeamId == 0) continue;
            if (Mathf.Abs(unit.Transform.position.x - origin.x) <= range)
                _rangeCache.Add(unit);
        }
        return _rangeCache;
    }

    /// <summary>5초마다 null/dead 유닛 일괄 제거 (메모리 누수 방지)</summary>
    private void CleanupDeadUnits()
    {
        _deadCache.Clear();
        foreach (var unit in _units)
            if (unit == null || !unit.IsAlive) _deadCache.Add(unit);
        foreach (var unit in _deadCache)
            _units.Remove(unit);
        _deadCache.Clear();
    }
}
