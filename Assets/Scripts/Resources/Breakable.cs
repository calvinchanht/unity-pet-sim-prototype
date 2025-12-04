using UnityEngine;
using PetSimLite.Economy;

namespace PetSimLite.Resources
{
    /// <summary>
    /// Simple breakable resource (coin/chest). Apply damage via code; on death it rewards coins.
    /// </summary>
    public class Breakable : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private int coinReward = 10;

        private int _currentHealth;
        private bool _isDead;

        private void Awake()
        {
            _currentHealth = maxHealth;
        }

        public void ApplyDamage(int damage)
        {
            if (_isDead || damage <= 0) return;

            _currentHealth -= damage;
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;

            CurrencyManager.Instance?.AddCoins(coinReward);
            Destroy(gameObject);
        }
    }
}
