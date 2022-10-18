using System;
using System.Collections.Generic;
using Infrastructure;

namespace Gameplay.LevelStateMachine
{
    public class LevelStateMachine : StateMachine
    {
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly StateMachinePayload _stateMachinePayload;

        public LevelStateMachine(ICoroutineRunner coroutineRunner, StateMachinePayload stateMachinePayload)
        {
            _coroutineRunner = coroutineRunner;
            _stateMachinePayload = stateMachinePayload;
            InitializeStateMap();
        }


        private void InitializeStateMap()
        {
            States = new Dictionary<Type, IExitableState>
            {
                [typeof(BootstrapState)] = new BootstrapState(this),
                [typeof(EntryState)] = new EntryState(this, _coroutineRunner, _stateMachinePayload.TapToStartAnimator,
                    _stateMachinePayload.GameStartButton),
                [typeof(PlayState)] = new PlayState(this, _coroutineRunner, _stateMachinePayload.FlaskRoot,
                    _stateMachinePayload.Bubble, _stateMachinePayload.PipeController, _stateMachinePayload.BrickSucker,
                    _stateMachinePayload.BricksSeller, _stateMachinePayload.AxisController),
                [typeof(SellState)] = new SellState(this, _stateMachinePayload.BricksSeller,
                    _stateMachinePayload.BalancePresenter),
                [typeof(BuyState)] = new BuyState(this, _stateMachinePayload.UpgradeButtons,
                    _stateMachinePayload.BalancePresenter, _stateMachinePayload.GameStartButton),
                [typeof(GameOverState)] =
                    new GameOverState(this, _coroutineRunner, _stateMachinePayload.Bubble,
                        _stateMachinePayload.GameOverScreen,
                        _stateMachinePayload.FinishLevelButton)
                //[typeof(LoadLevelState)] = new LoadLevelState(this, new SceneLoader(_coroutineRunner))
            };
        }
    }
}