using Infrastructure;
using UnityEngine.SceneManagement;

namespace Gameplay.LevelStateMachine
{
    public class BootstrapState : IState
    {
        private readonly LevelStateMachine _stateMachine;

        public BootstrapState(LevelStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Exit()
        { }

        public void Enter()
        {
            Level.CurrentScene = Constants.Scenes.Level6;
            SceneManager.LoadScene(Level.CurrentScene, LoadSceneMode.Additive);
            _stateMachine.Enter<EntryState>();
        }
    }
}