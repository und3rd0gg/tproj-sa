using DG.Tweening;

namespace Infrastructure
{
    public class BootstrapState : IState
    {
        private readonly GameStateMachine _gameStateMachine;
        private readonly SceneLoader _sceneLoader;

        public BootstrapState(GameStateMachine gameStateMachine, SceneLoader sceneLoader)
        {
            _gameStateMachine = gameStateMachine;
            _sceneLoader = sceneLoader;
        }

        public void Enter()
        {
            InitializeDoTween();
            _gameStateMachine.Enter<LoadLevelState, string>($"{Constants.Scenes.Level1}");
        }

        public void Exit()
        {
        }

        private void InitializeDoTween()
        {
            DOTween.SetTweensCapacity(1250, 50);
        }
    }
}