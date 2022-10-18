namespace Infrastructure
{
    public interface IUpdatableState : IState
    {
        void Update();
    }
}