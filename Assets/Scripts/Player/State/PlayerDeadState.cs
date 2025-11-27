public class DeadState : IPlayerState
{
    private PlayerController _c;

    public void Enter(PlayerController controller)
    {
        _c = controller;
        // TODO: 죽음 애니메이션, 입력/AI 정지 등
    }

    public void Tick() { }

    public void Exit() { }
}
