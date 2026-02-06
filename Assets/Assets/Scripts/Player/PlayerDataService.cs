using Crossatro.Events;
using UnityEngine;

public class PlayerDataService
{
    [SerializeField] private int _maxHealth = 100;
    [SerializeField] private int _currentHealth;
    [SerializeField] private int _coins;
    [SerializeField] private int _score;

    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;
    public int Coins => _coins;
    public int Score => _score;
    public bool IsDead => _currentHealth <= 0;

    public void Initialize(int startingHealth, int startingCoins)
    {
        _maxHealth = startingHealth;
        _currentHealth = startingHealth;
        _coins = startingCoins;
        _score = 0;

        PublishHealthChanged(0);
        PublishCoinsChanged(0);
        PublishScoreChanged(0);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;

        int previousHealth = _currentHealth;
        _currentHealth = Mathf.Max(0, CurrentHealth - damage);

        PublishHealthChanged(-(previousHealth + _currentHealth));

        EventBus.Instance.Publish(new PlayerDamageEvent
        {
            Damage = damage,
            RemainingHealth = _currentHealth,
        });
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        int previousHealth = _currentHealth;
        _currentHealth = Mathf.Min(_maxHealth, CurrentHealth + amount);

        PublishHealthChanged(_currentHealth - previousHealth);
    }

    public void IncreaseMaxHealth(int amount)
    {
        if (amount <= 0) return;

        _maxHealth += amount;
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0 || _coins <= amount) return false;

        _coins -= amount;
        PublishCoinsChanged(-amount);
        return true;
    }

    public void AddCoins(int amount)
    {
        if (amount < 0) return;

        _coins += amount;
        PublishCoinsChanged(amount);
    }

    public void AddScore(int amount)
    {
        if (amount < 0) return;

        _score += amount;
        PublishScoreChanged(amount);
    }

    private void PublishHealthChanged(int delta)
    {
        EventBus.Instance?.Publish(new HealthChangedEvent
        {
            NewAmount = _currentHealth,
            MaxAmount = _maxHealth,
            Delta = delta,
        });
    }

    private void PublishCoinsChanged(int delta)
    {
        EventBus.Instance?.Publish(new CoinsChangedEvent
        {
            NewAmount = _coins,
            Delta = delta,
        });
    }

    private void PublishScoreChanged(int delta)
    {
        EventBus.Instance?.Publish(new ScoreChangedEvent
        {
            NewScore = _score,
            Delta = delta,
        });
    }
}
