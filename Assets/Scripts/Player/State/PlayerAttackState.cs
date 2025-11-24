using System.Collections;
using UnityEngine;

public class AttackState : IPlayerState
{
    private PlayerController _c;
    private MonoBehaviour _host;
    private Coroutine _attackRoutine;

    public void Enter(PlayerController controller)
    {
        _c = controller;
        _host = controller;
        StartAttackLoop();
    }

    public void Tick()
    {
        if (_c.CurrentTarget == null || !_c.CurrentTarget.IsAlive)
        {
            _c.ChangeState(PlayerStateType.MoveForward);
            return;
        }

        float dist = _c.DistanceToTarget();
        if (dist > _c.attackRange * 1.2f)
        {
            _c.ChangeState(PlayerStateType.Chase);
        }
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

        if (_attackRoutine != null)
            _host.StopCoroutine(_attackRoutine);

        _attackRoutine = _host.StartCoroutine(AttackLoop());
    }

    private IEnumerator AttackLoop()
    {
        while (true)
        {
            if (_c.CurrentTarget == null || !_c.CurrentTarget.IsAlive)
            {
                _c.ChangeState(PlayerStateType.MoveForward);
                yield break;
            }

            Vector3 toTarget = _c.CurrentTarget.Transform.position - _c.transform.position;
            toTarget.y = 0f;
            if (_c.modelRoot != null && toTarget != Vector3.zero)
                _c.modelRoot.rotation = Quaternion.LookRotation(toTarget);

            _c.CurrentTarget.TakeDamage(_c.Stats.Attack);

            yield return new WaitForSeconds(_c.Stats.AttackInterval);
        }
    }
}
