using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public class GameStateMachine : StateMachine
    {
        private readonly SceneLoader _sceneLoader;

        public GameStateMachine(SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
            InitializeStateMap();
        }

        private void InitializeStateMap()
        {
            States = new Dictionary<Type, IExitableState>
            {
                [typeof(BootstrapState)] = new BootstrapState(this, _sceneLoader),
                [typeof(LoadLevelState)] = new LoadLevelState(this, _sceneLoader)
            };
        }
    }
}