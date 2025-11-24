public interface IPlayerState
{
    void Enter(PlayerController controller);
    void Tick();
    void Exit();
}
