using UnityEngine;
using PetSimLite.Data;
using PetSimLite.Economy;
using PetSimLite.Player;

namespace PetSimLite.Zone
{
    /// <summary>
    /// Handles gate unlock by spending coins. On success, disables collider/visuals to allow passage.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GateController : MonoBehaviour
    {
        [SerializeField] private GateData gateData;
        [SerializeField] private GameObject[] visualsToHide;

        private Collider _collider;
        private bool _opened;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            if (_collider != null)
            {
                _collider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_opened) return;
            if (other.GetComponent<PlayerController>() == null) return;

            TryOpenGate();
        }

        private void TryOpenGate()
        {
            int cost = gateData != null ? gateData.RequiredCoins : 0;
            if (cost <= 0)
            {
                OpenGate();
                return;
            }

            var currency = CurrencyManager.Instance;
            if (currency != null && currency.SpendCoins(cost))
            {
                OpenGate();
            }
            else
            {
                Debug.Log($"[Gate] Need {cost} coins to open.");
            }
        }

        private void OpenGate()
        {
            _opened = true;

            if (_collider != null)
            {
                _collider.enabled = false;
            }

            if (visualsToHide != null)
            {
                for (int i = 0; i < visualsToHide.Length; i++)
                {
                    if (visualsToHide[i] != null)
                    {
                        visualsToHide[i].SetActive(false);
                    }
                }
            }
        }

        public void SetGateData(GateData data)
        {
            gateData = data;
        }
    }
}
