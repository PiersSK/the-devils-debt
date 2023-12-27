public abstract class BaseState
{
    public Enemy enemy;
    public EnemyStateMachine stateMachine;

    public abstract void Enter();
    public abstract void Perform();
    public abstract void Exit();
}