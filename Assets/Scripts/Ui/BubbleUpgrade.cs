using Gameplay;
using Infrastructure;
using UnityEngine;

namespace Ui
{
    public class BubbleUpgrade : UpgradeButton
    {
        [SerializeField] private Bubble _BrickContainer;
        [SerializeField] private Animator _pipeAnimator;

        protected override void Upgrade()
        {
            _BrickContainer.UpgradeMaxBricksCount();
            _pipeAnimator.Play(Constants.AnimatorPipeController.States.Upgrade);
        }

        protected override void SetUpgradeName()
        {
            UpgradeName = nameof(BubbleUpgrade);
        }
    }
}