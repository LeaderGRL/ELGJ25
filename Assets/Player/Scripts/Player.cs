using System;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int m_health = 100;
    private int m_score = 0;
    private int m_coins = 15;

    public event Action<int> OnTakeDamage;
    public event Action<int> OnScoreChange; 
    public event Action<int> OnCoinChange;

    public void TakeDamage(int damage)
    {
        m_health -= damage;
        OnTakeDamage?.Invoke(m_health);
        if (m_health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died");
    }

    public void AddScore(int amount)
    {
        m_score += amount;
        OnScoreChange?.Invoke(m_score);
    }

    public void RemoveScore(int amount)
    {
        m_score -= amount;
        OnScoreChange?.Invoke(m_score);
    }

    public void AddHealth(int amount)
    {
        m_health += amount;
        OnTakeDamage?.Invoke(m_health);

    }

    public void RemoveHealth(int amount)
    {
        m_health -= amount;
        OnTakeDamage?.Invoke(m_health);
    }

    public void AddCoins(int amount)
    {
        m_coins += amount;
        OnCoinChange?.Invoke(m_coins);
    }

    public void RemoveCoins(int amount)
    {
        m_coins -= amount;
        OnCoinChange?.Invoke(m_coins);
    }

    public int GetCoins()
    {
        return m_coins;
    }

    public int GetScore()
    {
        return m_score;
    }

    public int GetHealth()
    {
        return m_health;
    }
}
