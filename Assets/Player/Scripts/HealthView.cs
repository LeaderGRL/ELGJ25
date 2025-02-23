using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthView : MonoBehaviour
{
    public Image heartIcon;
    public TextMeshProUGUI healthText;

    public void UpdateHealth(int health)
    {
        healthText.text = health.ToString();
    }

    public void UpdateHeartColor(Color color)
    {
        healthText.color = color;
    }
}
