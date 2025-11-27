public class IdleState : IPlayerState
{
    private PlayerController _c;

    public void Enter(PlayerController controller)
    {
        _c = controller;
        // TODO: Idle 애니메이션
    }

    public void Tick()
    {
        var target = _c.FindTarget();
        if (target != null)
        {
            _c.CurrentTarget = target;
            float dist = _c.DistanceToTarget();

            if (dist <= _c.attackRange)
                _c.ChangeState(PlayerStateType.Attack);
            else
                _c.ChangeState(PlayerStateType.Chase);
        }
        else
        {
            _c.MoveForward();
        }
    }

    public void Exit() { }
}
