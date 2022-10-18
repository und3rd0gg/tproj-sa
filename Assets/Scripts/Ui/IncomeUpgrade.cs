using Gameplay;
using Infrastructure;
using UnityEngine;

namespace Ui
{
    public class IncomeUpgrade : UpgradeButton
    {
        [SerializeField] private Animator _pipeAnimator;
        [SerializeField] private BricksSeller _bricksSeller;

        protected override void Upgrade()
        {
            _pipeAnimator.Play(Constants.AnimatorPipeController.States.Upgrade);
            _bricksSeller.Upgrade();
        }

        protected override void SetUpgradeName()
        {
            UpgradeName = nameof(IncomeUpgrade);
        }
    }
}