using Infrastructure;
using UnityEngine;

namespace Gameplay.LevelStateMachine
{
    public class Level : MonoBehaviour, ICoroutineRunner
    {
        [SerializeField] private StateMachinePayload _stateMachinePayload;

        private LevelStateMachine _levelStateMachine;
        
        public static string CurrentScene;

        private void Awake()
        {
            _levelStateMachine = new LevelStateMachine(this, _stateMachinePayload);
            _levelStateMachine.Enter<BootstrapState>();
        }
    }
}