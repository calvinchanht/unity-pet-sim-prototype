using System;
using UnityEngine;

namespace PetSimLite.Economy
{
    /// <summary>
    /// Tracks coins, provides add/spend methods, and notifies listeners when the amount changes.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }

        [SerializeField] private int startingCoins = 0;

        private int _coins;
        public event Action<int> CoinsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _coins = startingCoins;
            CoinsChanged?.Invoke(_coins);
        }

        public int Coins => _coins;

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;

            _coins += amount;
            CoinsChanged?.Invoke(_coins);
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0) return true;
            if (_coins < amount) return false;

            _coins -= amount;
            CoinsChanged?.Invoke(_coins);
            return true;
        }
    }
}
