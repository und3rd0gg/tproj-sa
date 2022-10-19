using Infrastructure;
using Ui;
using UnityEngine;

namespace Gameplay.LevelStateMachine
{
    public class SellState : IState
    {
        private readonly BalancePresenter _balancePresenter;
        private readonly BricksSeller _bricksSeller;
        private readonly LevelStateMachine _levelStateMachine;


        public SellState(LevelStateMachine levelStateMachine, BricksSeller bricksSeller,
            BalancePresenter balancePresenter)
        {
            _levelStateMachine = levelStateMachine;
            _bricksSeller = bricksSeller;
            _balancePresenter = balancePresenter;
        }


        public void Enter()
        {
            _balancePresenter.gameObject.SetActive(true);
            _bricksSeller.SellBricks(null);
            _bricksSeller.SaleCompleted += BricksSellerOnSaleCompleted;
        }

        public void Exit()
        {
            _bricksSeller.SaleCompleted -= BricksSellerOnSaleCompleted;
        }


        private void BricksSellerOnSaleCompleted()
        {
            _levelStateMachine.Enter<BuyState>();
        }
    }
}