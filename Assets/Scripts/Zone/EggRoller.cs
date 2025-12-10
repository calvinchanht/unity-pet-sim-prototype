using System;
using System.Collections;
using UnityEngine;
using PetSimLite.Data;
using PetSimLite.Economy;
using PetSimLite.Player;

namespace PetSimLite.Zone
{
    public struct EggRollResult
    {
        public EggData Egg;
        public PetData Pet;
    }

    /// <summary>
    /// Trigger around an egg that auto-rolls while the player stays inside and has coins.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class EggRoller : MonoBehaviour
    {
        public static event Action<EggRollResult> EggRolled;

        [SerializeField] private EggData eggData;

        private bool _playerInside;
        private Coroutine _rollRoutine;
        private Collider _trigger;

        private void Awake()
        {
            _trigger = GetComponent<Collider>();
            if (_trigger != null)
            {
                _trigger.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<PlayerController>() == null)
            {
                return;
            }

            _playerInside = true;
            StartRolling();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<PlayerController>() == null)
            {
                return;
            }

            _playerInside = false;
            StopRolling();
        }

        private void OnDisable()
        {
            _playerInside = false;
            StopRolling();
        }

        private void StartRolling()
        {
            if (_rollRoutine == null)
            {
                _rollRoutine = StartCoroutine(RollLoop());
            }
        }

        private void StopRolling()
        {
            if (_rollRoutine != null)
            {
                StopCoroutine(_rollRoutine);
                _rollRoutine = null;
            }
        }

        private IEnumerator RollLoop()
        {
            while (_playerInside)
            {
                if (eggData == null)
                {
                    yield break;
                }

                float interval = Mathf.Max(0.05f, eggData.RollIntervalSeconds);

                var currency = CurrencyManager.Instance;
                if (currency != null && currency.SpendCoins(eggData.EggCost))
                {
                    var pet = RollPet();
                    if (pet != null)
                    {
                        EggRolled?.Invoke(new EggRollResult
                        {
                            Egg = eggData,
                            Pet = pet
                        });
                    }
                }

                yield return new WaitForSeconds(interval);
            }

            _rollRoutine = null;
        }

        private PetData RollPet()
        {
            var table = eggData.DropTable;
            if (table == null || table.Length == 0)
            {
                return null;
            }

            int totalWeight = 0;
            for (int i = 0; i < table.Length; i++)
            {
                totalWeight += Mathf.Max(0, table[i].weight);
            }

            if (totalWeight <= 0)
            {
                return null;
            }

            int roll = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;
            for (int i = 0; i < table.Length; i++)
            {
                cumulative += Mathf.Max(0, table[i].weight);
                if (roll < cumulative)
                {
                    return table[i].pet;
                }
            }

            return table[table.Length - 1].pet;
        }

        public void SetEggData(EggData data)
        {
            eggData = data;
        }
    }
}
