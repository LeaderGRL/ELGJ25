using UnityEngine;
using UnityEngine.Rendering;

namespace Crossatro.Heart
{
    /// <summary>
    /// The heart is the element on the grid the player have to protect to stay alive.
    /// This heart is responsible for
    /// - Manage damage
    /// - Manage heal
    /// </summary>
    public class Heart: MonoBehaviour
    {
        // ============================================================
        // Configuration
        // ============================================================

        [Header("Heart Config")]
        [SerializeField] private HeartConfig _heartConfig;

        // ============================================================
        // Stats
        // ============================================================

        [Header("Heal")]
        [SerializeField] private int _currentHealth;
        [SerializeField] private int _maxHealth;

        // ============================================================
        // Properties
        // ============================================================

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public bool IsAlive => _currentHealth > 0;

        // ============================================================
        // Lifecycle
        // ============================================================

        public void Initialize()
        {
            if (_heartConfig == null)
            {
                Debug.LogError("[Heart] Heart config is not assigned!");
                return;
            }

            _currentHealth = _heartConfig.BaseHp;
            _maxHealth = _currentHealth;
        }


        // ============================================================
        // Event Subscription
        // ============================================================

        private void SubscribeToEvents()
        {
            //EventBus.Instance.Subscribe<HealthChangedEvent>(OnHealthChanged);
            //EventBus.Instance.Subscribe<MaxHealthChangedEvent>(OnMaxHealthChanged);
        }

        private void UnsubscribeFromEvents()
        {
            //EventBus.Instance.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
            //EventBus.Instance.Unsubscribe<MaxHealthChangedEvent>(OnMaxHealthChanged);
        }

        // ============================================================
        // Health management
        // ============================================================

        /// <summary>
        /// Apply damage to the heart.
        /// </summary>
        /// <param name="amount"></param>
        public int TakeDamage(int amount)
        {
            if (!IsAlive) return 0;        

            int previousHealth = _currentHealth;
            _currentHealth = Mathf.Max(0, _currentHealth - amount);

            if (_currentHealth <= 0)
                HandleDeath(amount);

            PublishHealChanged(previousHealth);

            return _currentHealth;
        }

        /// <summary>
        /// Handle heart reaching 0 hp.
        /// </summary>
        /// <param name="damage"></param>
        public void HandleDeath(int damage)
        {
            PublishDeath(damage);
        }

        /// <summary>
        /// Heal the heart.
        /// </summary>
        /// <param name="amount"></param>
        private void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;

            int previousHealth = _currentHealth;
            _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);

            PublishHealChanged(previousHealth);
        }

        /// <summary>
        /// Update the max hp point the heart can have.
        /// </summary>
        /// <param name="amount"></param>
        private void UpdateMaxHealth(int amount)
        {
            if (!IsAlive) return;

            int previousMaxHealth = _maxHealth;
            _maxHealth = Mathf.Max(1, _maxHealth + amount);

            PublishMaxHealChanged(previousMaxHealth);
        }


        // ============================================================
        // Event publishing
        // ============================================================

        private void PublishHealChanged(int previousHealth)
        {
            EventBus.Instance.Publish(new HealthChangedEvent
            {
                PreviousHealth = previousHealth,
                CurrentHealth = _currentHealth,
                Delta = Mathf.Abs(_currentHealth - previousHealth)
            });
        }

        private void PublishMaxHealChanged(int previousMaxHealth)
        {
            EventBus.Instance.Publish(new MaxHealthChangedEvent
            {
                PreviousMaxHealth = previousMaxHealth,
                CurrentMaxHealth = _currentHealth,
                Delta = Mathf.Abs(previousMaxHealth - _currentHealth),
            });
        }

        private void PublishDeath(int damage)
        {
            EventBus.Instance.Publish(new HeartDestroyedEvent
            {
                Damage = damage,
                TurnNumber = 0
            });
        }
    }
}
