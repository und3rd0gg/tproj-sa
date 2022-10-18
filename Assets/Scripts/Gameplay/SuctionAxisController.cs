using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Gameplay
{
    public class SuctionAxisController : MonoBehaviour, IDeactivatable
    {
        [SerializeField] [Range(0.0001f, 2f)] private float _speed;
        [SerializeField] private SuctionArea _suctionArea;
        [SerializeField] private float _yOffset;
        [SerializeField] private Vector3 _boxExtents;
        [SerializeField] private Vector3 _center;

        private Vector3 _lastPosition;
        private Vector3 _startPosition;
        private bool _canMove = false;

        private void Awake()
        {
            _startPosition = transform.position;
            _center = _suctionArea.transform.position;
        }

        private void OnEnable()
        {
            StartCoroutine(CheckWayRoutine());
        }

        private void Update()
        {
            _center.y = _suctionArea.transform.position.y;
            _center.y -= _boxExtents.y / 2;

            if (_canMove)
            {
                MoveDown();
            }
        }

        public void Activate()
        {
            enabled = true;
        }

        public void Deactivate()
        {
            enabled = false;
        }

        public void MoveToStart(float returnTime)
        {
            _lastPosition = transform.position;
            enabled = false;
            transform.DOMove(_startPosition, returnTime);
        }

        public void MoveToLastPosition(float returnTime)
        {
            enabled = true;
            transform.DOMove(_lastPosition, returnTime);
        }

        public void ResetLastPosition()
        {
            _lastPosition = _startPosition;
        }

        private void MoveDown()
        {
            var offset = transform.position;
            offset.y -= Time.deltaTime * _speed;
            transform.position = offset;
        }

        private IEnumerator CheckWayRoutine()
        {
            while (true)
            {
                var bricks = Physics.OverlapBox(_center, _boxExtents, Quaternion.identity);
                var bricksCount = 0;
                foreach (var brick in bricks)
                {
                    if (brick.gameObject.layer == 11)
                    {
                        bricksCount++;
                    }
                }
                _canMove = bricksCount < 3;
                yield return new WaitForSeconds(0.5f);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(_center, _boxExtents);
        }
    }
}