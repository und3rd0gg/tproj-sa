using Infrastructure;
using UnityEngine;

namespace Gameplay.LevelStateMachine
{
    public class Level : MonoBehaviour, ICoroutineRunner
    {
        public static string CurrentScene;
        [SerializeField] private StateMachinePayload _stateMachinePayload;

        private LevelStateMachine _levelStateMachine;

        private void Awake()
        {
            _levelStateMachine = new LevelStateMachine(this, _stateMachinePayload);
            _levelStateMachine.Enter<BootstrapState>();
        }
    }
}