using System;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int m_health = 100;
    private int m_score = 0;

    public event Action<int> OnTakeDamage;
    public event Action<int> OnChangeScore; 

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
        OnChangeScore?.Invoke(m_score);
    }

    public void RemoveScore(int amount)
    {
        m_score -= amount;
        OnChangeScore?.Invoke(m_score);
    }

    public int GetCoins()
    {
        return m_score;
    }
}
