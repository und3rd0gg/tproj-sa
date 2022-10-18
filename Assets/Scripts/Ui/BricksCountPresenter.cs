using Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    public class BricksCountPresenter : MonoBehaviour
    {
        [SerializeField] private uint _bricksCount;
        [SerializeField] private Text text;
        [SerializeField] private Bubble _bubble;

        private uint _maxBricks;

        private void OnEnable()
        {
            _maxBricks = _bubble.MaxBricks;
            OnMaxBricksUpgraded(_bubble.MaxBricks);
            _bubble.BricksCountUpdated += UpdateBricksCount;
            _bubble.MaxBricksUpgraded += OnMaxBricksUpgraded;
        }

        private void OnDisable()
        {
            _bubble.BricksCountUpdated -= UpdateBricksCount;
            _bubble.MaxBricksUpgraded -= OnMaxBricksUpgraded;
        }

        private void UpdateBricksCount(uint newCount)
        {
            _bricksCount = newCount;
            UpdateInfo(newCount, _maxBricks);
        }

        private void OnMaxBricksUpgraded(uint newValue)
        {
            _maxBricks = newValue;
            UpdateInfo(_bricksCount, _maxBricks);
        }

        private void UpdateInfo(uint bricksCount, uint maxCount)
        {
            text.text = $"{bricksCount}/{maxCount}";
        }
    }
}