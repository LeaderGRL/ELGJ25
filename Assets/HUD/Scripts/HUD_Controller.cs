using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class HUD_Controller : MonoBehaviour
{
    [SerializeField] private HUD_View hudView;

    [SerializeField] private PlayerCameraController m_playerCameraController;

    [Header("References")]
    public Player player;
    public Timer timer;
    public CoinController coinController;

    [Header("Event")]
    private UnityEvent onItemBought;
    private UnityEvent onShopOpened;
    private UnityEvent onShopClosed;

    public void OpenShop()
    {
        m_playerCameraController.enabled = false;
        hudView.OpenShopView();
    }

    public void CloseShop()
    {
        m_playerCameraController.enabled = true;
        hudView.CloseShopView();
    }

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
