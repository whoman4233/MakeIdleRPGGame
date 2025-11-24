public class ChaseState : IPlayerState
{
    private PlayerController _c;

    public void Enter(PlayerController controller)
    {
        _c = controller;
        // TODO: Run 애니메이션 유지
    }

    public void Tick()
    {
        if (_c.CurrentTarget == null || !_c.CurrentTarget.IsAlive)
        {
            _c.ChangeState(PlayerStateType.MoveForward);
            return;
        }

        float dist = _c.DistanceToTarget();

        if (dist <= _c.attackRange)
        {
            _c.ChangeState(PlayerStateType.Attack);
        }
        else
        {
            _c.MoveTowards(_c.CurrentTarget.Transform.position);
        }
    }

    public void Exit() { }
}
