using System;
using System.Collections;
using DG.Tweening;
using Ui;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace Gameplay
{
    public class BricksSeller : MonoBehaviour
    {
        [SerializeField] private Bubble _bubble;
        [SerializeField] private PlayerMoneyBalance _playerBalance;
        [SerializeField] private int _sellingMultiplier;

        [SerializeField] private Transform _coin;
        [SerializeField] private BalancePresenter _balancePresenter;
        [SerializeField] private Transform _coinTarget;

        private ObjectPool<Transform> _objectPool;

        private void Awake()
        {
            _sellingMultiplier = PlayerPrefs.GetInt(nameof(_sellingMultiplier), 1);
            _objectPool = new ObjectPool<Transform>(CreateCoinsPool, OnTakeFromPool, OnReturnToPool, ActionOnDestroy,
                false, 300, 500);
        }

        public event Action SaleCompleted;

        private void ActionOnDestroy(Transform obj)
        {
            throw new NotImplementedException();
        }

        private void OnReturnToPool(Transform obj)
        {
            obj.gameObject.SetActive(false);
        }

        private void OnTakeFromPool(Transform obj)
        {
            obj.gameObject.SetActive(true);
        }

        private Transform CreateCoinsPool()
        {
            return Instantiate(_coin, transform.position, Quaternion.identity);
        }

        public void Upgrade()
        {
            _sellingMultiplier += 1;
            PlayerPrefs.SetInt(nameof(_sellingMultiplier), _sellingMultiplier);
        }

        public void SellBricks(object _)
        {
            StartCoroutine(SellBricks());
        }

        public IEnumerator SellBricks()
        {
            var bricksCount = _bubble.GetBricksCount();

            if (bricksCount > 300)
                bricksCount = 300;

            var coins = new Transform[bricksCount];

            yield return StartCoroutine(MoveCoinToBalancePresenterRoutine(coins));

            IEnumerator MoveCoinToBalancePresenterRoutine(Transform[] coins)
            {
                yield return StartCoroutine(MoveToRandomPosition());

                _bubble.Clear();

                var target = _coinTarget.position;

                const float animationDuration = 8f;

                for (var i = 0; i < coins.Length; i++)
                {
                    var coin = coins[i];
                    var speed = Vector3.Distance(coins[i].transform.position, target) / animationDuration;
                    coins[i].DOMove(target, speed).SetEase(Ease.InBack).OnComplete(() =>
                    {
                        _objectPool.Release(coin);
                        _playerBalance.AddMoney(_sellingMultiplier);
                    });
                }

                SaleCompleted?.Invoke();
            }

            IEnumerator MoveToRandomPosition()
            {
                const float animationDuration = 0.5f;

                for (var i = 0; i < bricksCount; i++)
                {
                    coins[i] = _objectPool.Get();
                    coins[i].position = transform.position;
                    coins[i].DOMove(coins[i].position + (Vector3) Random.insideUnitCircle, animationDuration);
                }

                yield return new WaitForSeconds(animationDuration);
            }
        }
    }
}