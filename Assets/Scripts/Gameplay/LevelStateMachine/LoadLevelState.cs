using System.Collections.Generic;
using System.Linq;
using Gameplay.LevelStateMachine;
using UnityEngine;

namespace Infrastructure
{
    public class LoadLevelState : IPayloadedState<string>
    {
        private readonly Gameplay.SceneLoader _sceneLoader;

        private readonly List<string> _scenePool = new()
        {
            Constants.Scenes.Level2,
            Constants.Scenes.Level4,
            Constants.Scenes.Level5,
            Constants.Scenes.Level6,
            Constants.Scenes.Level7
        };

        private readonly LevelStateMachine _stateMachine;

        private string _currentLevel;

        public LoadLevelState(LevelStateMachine stateMachine, Gameplay.SceneLoader sceneLoader)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
        }

        public void Enter(string payload = null)
        {
            Debug.Log("loadlevelState enter");

            if (_currentLevel == null)
            {
                _sceneLoader.Load(payload, null, () => _stateMachine.Enter<EntryState>());
                _currentLevel = payload;
                return;
            }

            var sceneToLoad = GetRandomScene();
            Debug.Log(sceneToLoad);
            _sceneLoader.Load(sceneToLoad, _currentLevel, () => _stateMachine.Enter<EntryState>());
            _currentLevel = sceneToLoad;
        }

        public void Exit()
        {
            Debug.Log("loadlevelstate exit");
        }

        private string GetRandomScene()
        {
            var scenes = _scenePool.ToList();
            scenes.Remove(_currentLevel);
            return scenes[Random.Range(0, _scenePool.Count - 1)];
        }
    }
}