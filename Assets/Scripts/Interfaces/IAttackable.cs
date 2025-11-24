using UnityEngine;

public interface IAttackable
{
    Transform Transform { get; }
    bool IsAlive { get; }
    int TeamId { get; }

    void TakeDamage(float amount);
}
