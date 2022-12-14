using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gameplay.LevelStateMachine
{
    public class GameOverState : IState
    {
        private readonly Bubble _bubble;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly FinishLevelButton _finishLevelButton;
        private readonly GameOverScreen _gameOverScreen;
        private readonly LevelStateMachine _levelStateMachine;

        private readonly List<string> _scenePool = new()
        {
            Constants.Scenes.Level2,
            Constants.Scenes.Level4,
            Constants.Scenes.Level5,
            Constants.Scenes.Level6,
            Constants.Scenes.Level7
        };

        public GameOverState(LevelStateMachine levelStateMachine, ICoroutineRunner coroutineRunner, Bubble bubble,
            GameOverScreen gameOverScreen,
            FinishLevelButton finishLevelButton)
        {
            _levelStateMachine = levelStateMachine;
            _coroutineRunner = coroutineRunner;
            _bubble = bubble;
            _gameOverScreen = gameOverScreen;
            _finishLevelButton = finishLevelButton;
        }


        public void Exit()
        {
            _bubble.TotalSuckedBricks = 0;
            _gameOverScreen.gameObject.SetActive(false);
        }

        public void Enter()
        {
            var suckedBricksPercent = CalculatePercentage();
            _gameOverScreen.Activate(suckedBricksPercent, _bubble.TotalSuckedBricks);
            _finishLevelButton.Clicked += FinishLevelButtonOnClicked;
        }

        private void FinishLevelButtonOnClicked()
        {
            _coroutineRunner.StartCoroutine(LoadNextRandomLevel());
            _finishLevelButton.Clicked -= FinishLevelButtonOnClicked;
        }

        private IEnumerator LoadNextRandomLevel()
        {
            var nextLevel = GetRandomScene();
            SceneManager.UnloadSceneAsync(Level.CurrentScene);
            Resources.UnloadUnusedAssets();
            SceneManager.LoadScene(nextLevel, LoadSceneMode.Additive);
            Level.CurrentScene = nextLevel;
            _levelStateMachine.Enter<EntryState>();
            yield return null;
        }

        private string GetRandomScene()
        {
            var scenes = _scenePool.ToList();
            scenes.Remove(Level.CurrentScene);
            return scenes[Random.Range(0, _scenePool.Count - 1)];
        }

        private int CalculatePercentage()
        {
            var bricksCount = Object.FindObjectOfType<BricksRoot>().BricksCount;
            var bricksRate = (float) _bubble.TotalSuckedBricks / bricksCount;
            return Mathf.FloorToInt(bricksRate * 100);
        }
    }
}