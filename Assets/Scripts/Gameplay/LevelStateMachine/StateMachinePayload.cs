using System;
using Ui;
using UnityEngine;

namespace Gameplay.LevelStateMachine
{
    [Serializable]
    public class StateMachinePayload
    {
        [field: SerializeField] public Animator TapToStartAnimator { get; private set; }
        [field: SerializeField] public FlaskRoot FlaskRoot { get; private set; }
        [field: SerializeField] public GameStartButton GameStartButton { get; private set; }
        [field: SerializeField] public Bubble Bubble { get; private set; }
        [field: SerializeField] public BricksSeller BricksSeller { get; private set; }
        [field: SerializeField] public GameObject UpgradeButtons { get; private set; }
        [field: SerializeField] public BalancePresenter BalancePresenter { get; private set; }
        [field: SerializeField] public PipeController PipeController { get; private set; }
        [field: SerializeField] public BrickSucker BrickSucker { get; private set; }
        [field: SerializeField] public GameOverScreen GameOverScreen { get; private set; }
        [field: SerializeField] public FinishLevelButton FinishLevelButton { get; private set; }
        [field: SerializeField] public SuctionAxisController AxisController { get; private set; }
    }
}