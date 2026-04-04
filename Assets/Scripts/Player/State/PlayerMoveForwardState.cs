using UnityEngine;

public class MoveForwardState : IPlayerState
{
    private PlayerController _c;

    public void Enter(PlayerController controller)
    {
        _c = controller;
    }

    public void Tick()
    {
        _c.CurrentTarget = _c.FindTarget();
        _c.MoveForward();

        if (_c.CurrentTarget != null)
        {
            float dist = _c.DistanceToTarget();

            if (dist <= _c.attackRange)
            {
                _c.ChangeState(PlayerStateType.Attack);
            }
        }
    }

    public void Exit()
    {
    }
}