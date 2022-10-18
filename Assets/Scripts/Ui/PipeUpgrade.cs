using Gameplay;
using Infrastructure;
using UnityEngine;

namespace Ui
{
    public class PipeUpgrade : UpgradeButton
    {
        [SerializeField] private Animator _pipeAnimator;
        [SerializeField] private SuctionArea _suctionArea;

        protected override void Upgrade()
        {
            _pipeAnimator.Play(Constants.AnimatorPipeController.States.Upgrade);
            _suctionArea.Upgrade();
        }

        protected override void SetUpgradeName()
        {
            UpgradeName = nameof(PipeUpgrade);
        }
    }
}