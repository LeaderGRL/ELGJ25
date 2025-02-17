using UnityEngine;

public class Player : MonoBehaviour
{
    private int health = 100;
    private int coins = 100;

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died");
    }

    public void AddCoins(int amount)
    {
        coins += amount;
    }

    public void RemoveCoins(int amount)
    {
        coins -= amount;
    }

    public int GetCoins()
    {
        return coins;
    }
}
