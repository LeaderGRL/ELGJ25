using UnityEngine;

public class HealthController : MonoBehaviour
{
    public Player player;
    public HealthView healthView;

    private void OnEnable()
    {
        player.OnTakeDamage += OnTakeDamage;
    }

    private void OnDisable()
    {
        player.OnTakeDamage -= OnTakeDamage;
    }

    private void OnTakeDamage(int health)
    {
        healthView.UpdateHealth(health);
    }

    public void AddHealth(int amount)
    {
        player.AddHealth(amount);
    }

    public void RemoveHealth(int amount)
    {
        player.TakeDamage(amount);
    }
}
