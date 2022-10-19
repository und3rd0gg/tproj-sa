using Infrastructure;
using Ui;
using UnityEngine;

namespace Gameplay.LevelStateMachine
{
    public class BuyState : IState
    {
        private readonly BalancePresenter _balancePresenter;
        private readonly GameStartButton _gameStartButton;
        private readonly LevelStateMachine _stateMachine;
        private readonly GameObject _upgradeButtons;


        public BuyState(LevelStateMachine stateMachine, GameObject upgradeButtons, BalancePresenter balancePresenter,
            GameStartButton gameStartButton)
        {
            _stateMachine = stateMachine;
            _upgradeButtons = upgradeButtons;
            _balancePresenter = balancePresenter;
            _gameStartButton = gameStartButton;
        }


        public void Enter()
        {
            _upgradeButtons.SetActive(true);
            _gameStartButton.gameObject.SetActive(true);
            _gameStartButton.Clicked += EnterPlayState;
        }

        public void Exit()
        {
            _gameStartButton.gameObject.SetActive(false);
            _upgradeButtons.SetActive(false);
            _balancePresenter.gameObject.SetActive(false);
        }


        private void EnterPlayState()
        {
            _stateMachine.Enter<PlayState>();
        }
    }
}