using System.Collections;
using Infrastructure;
using UnityEngine;

namespace Gameplay.LevelStateMachine
{
    public class PlayState : IState
    {
        private readonly SuctionAxisController _axisController;
        private readonly BricksSeller _bricksSeller;
        private readonly BrickSucker _brickSucker;
        private readonly Bubble _bubble;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly FlaskRoot _flaskRoot;
        private readonly LevelStateMachine _levelStateMachine;
        private readonly PipeController _pipeController;

        private bool _firstEnter = true;


        public PlayState(LevelStateMachine levelStateMachine, ICoroutineRunner coroutineRunner, FlaskRoot flaskRoot,
            Bubble bubble, PipeController pipeController, BrickSucker brickSucker, BricksSeller bricksSeller,
            SuctionAxisController axisController)
        {
            _levelStateMachine = levelStateMachine;
            _coroutineRunner = coroutineRunner;
            _flaskRoot = flaskRoot;
            _bubble = bubble;
            _pipeController = pipeController;
            _brickSucker = brickSucker;
            _bricksSeller = bricksSeller;
            _axisController = axisController;
        }


        public void Enter()
        {
            Debug.Log("playstate enter");

            if (!_firstEnter) _coroutineRunner.StartCoroutine(_flaskRoot.MovePipeToLastPosition());

            _firstEnter = false;
            _pipeController.enabled = true;
            _flaskRoot.Activate();
            _bubble.BubbleFilled += OnBubbleFilled;
            _brickSucker.GroundTouched += OnGroundTouched;
        }

        public void Exit()
        {
            Debug.Log("playstate exit");
            _bubble.BubbleFilled -= OnBubbleFilled;
            _brickSucker.GroundTouched -= OnGroundTouched;
        }

        private void OnGroundTouched()
        {
            _coroutineRunner.StartCoroutine(OnGroundTouchedRoutine());
        }

        private IEnumerator OnGroundTouchedRoutine()
        {
            Debug.Log("ground touched");
            _flaskRoot.Deactivate();
            _pipeController.enabled = false;
            yield return _coroutineRunner.StartCoroutine(_flaskRoot.ReturnPipe());
            _firstEnter = true;
            _axisController.ResetLastPosition();
            yield return _coroutineRunner.StartCoroutine(_bricksSeller.SellBricks());
            BricksSellerOnSaleCompleted();
        }

        private void BricksSellerOnSaleCompleted()
        {
            _bricksSeller.SaleCompleted -= BricksSellerOnSaleCompleted;
            _levelStateMachine.Enter<GameOverState>();
        }

        private void OnBubbleFilled()
        {
            _coroutineRunner.StartCoroutine(OnBubbleFilledRoutine());
        }

        private IEnumerator OnBubbleFilledRoutine()
        {
            _flaskRoot.Deactivate();
            _pipeController.enabled = false;
            yield return _coroutineRunner.StartCoroutine(_flaskRoot.ReturnPipe());
            _levelStateMachine.Enter<SellState>();
        }
    }
}