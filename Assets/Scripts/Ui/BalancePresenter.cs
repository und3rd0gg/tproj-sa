using Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Ui
{
    public class BalancePresenter : MonoBehaviour
    {
        [SerializeField] private PlayerMoneyBalance _balance;
        [SerializeField] private Text _text;

        private void Reset()
        {
            _balance = FindObjectOfType<PlayerMoneyBalance>();
            _text = GetComponentInChildren<Text>();
        }

        private void OnEnable()
        {
            _balance.AmountChanged += OnAmountChanged;
        }

        private void OnDisable()
        {
            _balance.AmountChanged -= OnAmountChanged;
        }

        private void OnAmountChanged(uint amount)
        {
            _text.text = amount.ToString();
        }
    }
}