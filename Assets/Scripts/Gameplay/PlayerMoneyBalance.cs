using System;
using UnityEngine;

namespace Gameplay
{
    public class PlayerMoneyBalance : MonoBehaviour
    {
        [field: SerializeField] public uint Amount { get; private set; }

        private void Awake()
        {
            Amount = (uint) PlayerPrefs.GetInt(nameof(PlayerMoneyBalance), 0);
        }

        public event Action<uint> AmountChanged;

        public void AddMoney(int amount)
        {
            if (amount < 1) throw new ArgumentException();

            Amount += Convert.ToUInt32(amount);
            AmountChanged?.Invoke(Amount);
            PlayerPrefs.SetInt(nameof(PlayerMoneyBalance), (int) Amount);
        }

        public bool TrySpend(uint amount)
        {
            if (amount > Amount) return false;

            Amount -= amount;
            AmountChanged?.Invoke(Amount);
            return true;
        }
    }
}