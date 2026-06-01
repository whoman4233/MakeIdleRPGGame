using System.Collections;
using UnityEngine;

public class AttackState : IPlayerState
{
    private PlayerController _c;
    private MonoBehaviour    _host;
    private Coroutine        _attackRoutine;

    public void Enter(PlayerController controller)
    {
        _c    = controller;
        _host = controller;
        StartAttackLoop();
    }

    public void Tick()
    {
        // 매 틱마다 타겟 재탐색 — 보스 스폰 즉시 전환 대응
        var newTarget = _c.FindTarget();
        if (newTarget != null) _c.CurrentTarget = newTarget;

        if (_c.CurrentTarget == null || !_c.CurrentTarget.IsAlive)
        {
            _c.ChangeState(PlayerStateType.MoveForward);
            return;
        }
        if (_c.DistanceToTarget() > _c.attackRange * 1.2f)
            _c.ChangeState(PlayerStateType.MoveForward);
    }

    public void Exit()
    {
        if (_attackRoutine != null && _host != null)
        {
            _host.StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }
    }

    private void StartAttackLoop()
    {
        if (_host == null) return;
        if (_attackRoutine != null) _host.StopCoroutine(_attackRoutine);
        _attackRoutine = _host.StartCoroutine(AttackLoop());
    }

    private IEnumerator AttackLoop()
    {
        bool running = true;
        while (running)
        {
            if (_c.CurrentTarget == null || !_c.CurrentTarget.IsAlive)
            {
                _c.ChangeState(PlayerStateType.MoveForward);
                yield break;
            }

            Vector3 toTarget = _c.CurrentTarget.Transform.position - _c.transform.position;
            toTarget.y = 0f; toTarget.z = 0f;
            if (_c.modelRoot != null && toTarget != Vector3.zero)
                _c.modelRoot.rotation = Quaternion.LookRotation(toTarget);

            float baseDamage = Mathf.Max(1f, _c.Stats.GetStatValue(StatType.AttackPower));
            float critChance = _c.Stats.GetStatValue(StatType.CriticalChance);
            bool  isCrit     = UnityEngine.Random.value < critChance;
            float finalDmg   = isCrit ? baseDamage * 2f : baseDamage;

            var registry = AttackableRegistry.Instance;
            int hitCount = 0;

            if (registry != null)
            {
                var nearTargets = registry.GetEnemiesInRange(_c.transform.position, _c.attackRange);
                for (int i = 0; i < nearTargets.Count; i++)
                {
                    var t = nearTargets[i];
                    if (t == null || !t.IsAlive) continue;
                    t.TakeDamage(finalDmg);
                    hitCount++;
                }
                if (hitCount == 0 && _c.CurrentTarget != null)
                {
                    _c.CurrentTarget.TakeDamage(finalDmg);
                    hitCount++;
                }
            }
            else
            {
                if (_c.CurrentTarget != null)
                {
                    _c.CurrentTarget.TakeDamage(finalDmg);
                    hitCount++;
                }
            }

            if (hitCount > 0)
            {
                SoundManager.Instance?.PlayEnemyHit();
                if (_c.CurrentTarget != null)
                    DamagePopupManager.Instance?.Show(
                        _c.CurrentTarget.Transform.position, finalDmg, isCrit);
            }

            float spd      = _c.Stats.GetStatValue(StatType.AttackSpeed);
            float interval = 1f / Mathf.Max(0.1f, spd);
            yield return new WaitForSeconds(interval);
        }
    }
}