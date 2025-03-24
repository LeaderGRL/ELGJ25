using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Shop : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public Timer timer;
    public CoinController coinController;
    [SerializeField] private PlayerCameraController m_playerCameraController;

    [Header("Event")]
    public UnityEvent onItemBought;

    public void BuyItem(Item item)
    {
        if (player.GetCoins() >= item.itemObject.itemPrice)
        {
            coinController.RemoveCoins(item.itemObject.itemPrice);

            foreach (var effect in item.itemObject.itemEffects)
            {
                Debug.Log("Applying effect: " + effect.name);
                effect.ApplyEffect();
            }

            onItemBought.Invoke();
            Debug.Log("Item bought: " + item.itemObject.itemName);
        }
        else
        {
            Debug.Log("Not enough coins to buy: " + item.itemObject.itemName);
        }
    }
}
