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

        private void OnEnable()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.CoinsChanged += OnCoinsChanged;
                OnCoinsChanged(CurrencyManager.Instance.Coins);
            }
        }

        private void OnDisable()
        {
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.CoinsChanged -= OnCoinsChanged;
            }
        }

        private void OnCoinsChanged(int amount)
        {
            if (coinText != null)
            {
                coinText.text = amount.ToString();
            }
        }
    }
}
