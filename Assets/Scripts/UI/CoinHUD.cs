using TMPro;
using UnityEngine;
using PetSimLite.Economy;

namespace PetSimLite.UI
{
    /// <summary>
    /// Displays current coins; subscribe to CurrencyManager events.
    /// </summary>
    public class CoinHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI coinText;

        private CurrencyManager _currency;

        private void OnEnable()
        {
            // If manager already exists, subscribe immediately; otherwise wait for initialization.
            if (CurrencyManager.Instance != null)
            {
                Attach(CurrencyManager.Instance);
            }

            CurrencyManager.InstanceInitialized += OnCurrencyInitialized;
        }

        private void OnDisable()
        {
            CurrencyManager.InstanceInitialized -= OnCurrencyInitialized;
            Detach();
        }

        private void OnCoinsChanged(int amount)
        {
            if (coinText != null)
            {
                coinText.text = amount.ToString();
            }
        }

        private void OnCurrencyInitialized(CurrencyManager manager)
        {
            Attach(manager);
        }

        private void Attach(CurrencyManager manager)
        {
            if (manager == null || _currency == manager) return;

            Detach();
            _currency = manager;
            _currency.CoinsChanged += OnCoinsChanged;
            OnCoinsChanged(_currency.Coins);
        }

        private void Detach()
        {
            if (_currency != null)
            {
                _currency.CoinsChanged -= OnCoinsChanged;
            }
            _currency = null;
        }
    }
}
