using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class Bubble : MonoBehaviour
    {
        [SerializeField] private Vector3 _brickStartPosition;
        [SerializeField] private Vector3[] _brickRotationStartPositions;
        [SerializeField] private float _rotationPreparationTime;
        [SerializeField] private float _brickAdditionDelay = 1;
        [SerializeField] private Transform _bricksPivot;

        private Queue<Brick> _bricks;
        private int _currentStartPositionIndex;

        [field: SerializeField] public uint MaxBricks { get; private set; }
        
        public int CurrentBricksCount => _bricks.Count;
        public int TotalSuckedBricks { get; set; } = 0;

        private void Awake()
        {
            MaxBricks = (uint) PlayerPrefs.GetInt(nameof(MaxBricks), 50);
            MaxBricksUpgraded?.Invoke(MaxBricks);
            _bricks = new Queue<Brick>();
        }

        private void OnDrawGizmos()
        {
            const float sphereRadius = 0.03f;
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_brickStartPosition, sphereRadius);
            Gizmos.color = Color.green;

            foreach (var position in _brickRotationStartPositions) Gizmos.DrawSphere(position, sphereRadius);
        }

        public event Action<uint> BricksCountUpdated;
        public event Action<uint> MaxBricksUpgraded;
        public event Action BubbleFilled;

        public void AddBrick(Brick brick)
        {
            TotalSuckedBricks++;
            _bricks.Enqueue(brick);
            BricksCountUpdated?.Invoke(Convert.ToUInt32(_bricks.Count));
            brick.gameObject.SetActive(false);

            if (_bricks.Count == MaxBricks)
            {
                BubbleFilled?.Invoke();
                return;
            }

            StartCoroutine(BrickAdditionDelayRoutine(brick));
        }

        public void UpgradeMaxBricksCount()
        {
            const uint additionalBricks = 50;
            MaxBricks += additionalBricks;
            MaxBricksUpgraded?.Invoke(MaxBricks);
            PlayerPrefs.SetInt(nameof(MaxBricks), (int) MaxBricks);
        }

        public Brick GetBrick()
        {
            var brick = _bricks.Dequeue();
            BricksCountUpdated?.Invoke(Convert.ToUInt32(_bricks.Count));
            return brick;
        }

        public int Clear()
        {
            var bricksCount = _bricks.Count;

            foreach (var brick in _bricks)
            {
                Destroy(brick.gameObject);
            }
            
            _bricks.Clear();
            BricksCountUpdated?.Invoke(Convert.ToUInt32(_bricks.Count));
            return bricksCount;
        }

        public int GetBricksCount()
        {
            return _bricks.Count;
        }

        private IEnumerator BrickAdditionDelayRoutine(Brick brick)
        {
            yield return new WaitForSeconds(_brickAdditionDelay);
            brick.gameObject.SetActive(true);
            brick.transform.localScale = brick.ScaleModifier;
            brick.transform.position = _brickStartPosition;
            brick.transform
                .DOMove(_brickRotationStartPositions[_currentStartPositionIndex], _rotationPreparationTime)
                .SetEase(Ease.Linear)
                .onComplete = () => brick.transform.SetParent(_bricksPivot, true);

            _currentStartPositionIndex++;

            if (_currentStartPositionIndex > _brickRotationStartPositions.Length - 1) _currentStartPositionIndex = 0;
        }
    }
}