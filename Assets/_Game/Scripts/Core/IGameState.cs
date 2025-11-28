namespace LavaQuest.Core
{
    public interface IGameState
    {
        void Initialize(GameStateMachine stateMachine);
        void Enter();
        void Exit();
        void Tick();
    }
}