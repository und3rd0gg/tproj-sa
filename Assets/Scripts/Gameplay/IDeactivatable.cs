namespace Gameplay
{
    public interface IDeactivatable
    {
        public bool enabled { get; }

        public void Activate();
        public void Deactivate();
    }
}