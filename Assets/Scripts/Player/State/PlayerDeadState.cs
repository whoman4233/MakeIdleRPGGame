public class DeadState : IPlayerState
{
    private PlayerController _c;

    public void Enter(PlayerController controller)
    {
        _c = controller;
        // 현재 타겟 클리어 — 사망 중 공격 루틴이 타겟 참조하지 못하도록
        _c.CurrentTarget = null;
    }

    // Dead 상태에서는 Tick 완전 차단
    public void Tick() { }

    public void Exit() { }
}
