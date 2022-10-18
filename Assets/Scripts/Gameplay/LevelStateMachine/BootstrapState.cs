using Infrastructure;
using UnityEngine;
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
        {
            Debug.Log("bootstrapstate exit");
        }

        public void Enter()
        {
            Debug.Log("bootstrapstate enter");
            Level.CurrentScene = Constants.Scenes.Level6;
            SceneManager.LoadScene(Level.CurrentScene, LoadSceneMode.Additive);
            _stateMachine.Enter<EntryState>();
        }
    }
}