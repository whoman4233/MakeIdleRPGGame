public class MoveForwardState : IPlayerState
{
    private PlayerController _c;

    public void Enter(PlayerController controller)
    {
        _c = controller;
        // TODO: Run 애니메이션
    }

    public void Tick()
    {
        _c.MoveForward();

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
    }

    public void Exit() { }
}
