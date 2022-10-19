using System.Collections;
using Infrastructure;
using Ui;
using UnityEngine;

namespace Gameplay.LevelStateMachine
{
    public class EntryState : IState
    {
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly GameStartButton _gameStartButton;
        private readonly LevelStateMachine _levelStateMachine;
        private readonly float _shakeDelay = 3.5f;
        private readonly Animator _tapToStartAnimator;
        private Coroutine _shakeTextRoutine;


        public EntryState(LevelStateMachine levelStateMachine, ICoroutineRunner coroutineRunner,
            Animator tapToStartAnimator, GameStartButton gameStartButton)
        {
            _tapToStartAnimator = tapToStartAnimator;
            _levelStateMachine = levelStateMachine;
            _coroutineRunner = coroutineRunner;
            _gameStartButton = gameStartButton;
        }


        public void Enter()
        {
            _tapToStartAnimator.gameObject.SetActive(true);
            _gameStartButton.gameObject.SetActive(true);
            _shakeTextRoutine = _coroutineRunner.StartCoroutine(ShakeTextRoutine());
            _gameStartButton.Clicked += EnterPlayState;
        }

        public void Exit()
        {
            if (_shakeTextRoutine != null) _coroutineRunner.StopCoroutine(_shakeTextRoutine);

            _tapToStartAnimator.gameObject.SetActive(false);
            _gameStartButton.gameObject.SetActive(false);
            _gameStartButton.Clicked -= EnterPlayState;
        }


        private void EnterPlayState()
        {
            _levelStateMachine.Enter<PlayState>();
        }

        private IEnumerator ShakeTextRoutine()
        {
            while (true)
            {
                _tapToStartAnimator.Play(Constants.AnimatorTapToStartController.States.Shake);
                yield return new WaitForSeconds(_shakeDelay);
            }
        }
    }
}