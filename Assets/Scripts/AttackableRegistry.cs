using System.Collections.Generic;
using UnityEngine;

public class AttackableRegistry : MonoBehaviour
{
    public static AttackableRegistry Instance { get; private set; }

    private readonly HashSet<IAttackable> _units = new HashSet<IAttackable>();
    public IReadOnlyCollection<IAttackable> Units => _units;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Register(IAttackable unit)
    {
        if (unit != null)
            _units.Add(unit);
    }

    public void Unregister(IAttackable unit)
    {
        if (unit != null)
            _units.Remove(unit);
    }
}
