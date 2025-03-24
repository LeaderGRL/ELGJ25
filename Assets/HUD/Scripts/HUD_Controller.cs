using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUD_Controller : MonoBehaviour
{
    [SerializeField] private HUD_View hudView;
    [SerializeField] private PlayerCameraController m_playerCameraController;

    [Header("References")]
    public Player player;
    public Timer timer;
    public CoinController coinController;
    public ItemDatabase itemDatabase;
    public Shop shop;
    [SerializeField] private GameObject shopPanel;

    [Header("Event")]
    private UnityEvent onItemBought;
    private UnityEvent onShopOpened;
    private UnityEvent onShopClosed;

    private List<GameObject> cards = new List<GameObject>();
    private Dictionary<GameObject, Item> cardItemDictionnary = new Dictionary<GameObject, Item>();

    public void Start()
    {
        player.OnCoinChange += OnCoinChange;
        InitShop();
    }

    public void OpenShop()
    {
        m_playerCameraController.enabled = false;
        hudView.OpenShopView();
        SetCardInteraction();
    }

    public void CloseShop()
    {
        m_playerCameraController.enabled = true;
        hudView.CloseShopView();
    }

    public void InitShop()
    {
        for (int i = 0; i < 3; i++)
        {
            var cardItemObject = itemDatabase.GetRandomItem();
            Item cardItem = new Item();
            cardItem.itemObject = cardItemObject;
            var card = Instantiate(cardItemObject.itemPrefab, shopPanel.transform);
            cardItemDictionnary.Add(card, cardItem);

            card.GetComponent<Button>().onClick.AddListener(() => OnBuyButtonClick(cardItem));
        }
    }

    public void SetCardInteraction()
    {
        foreach (KeyValuePair<GameObject, Item> cardItem in cardItemDictionnary)
        {
            if (player.GetCoins() >= cardItem.Value.itemObject.itemPrice && !cardItem.Value.isBought)
            {
                cardItem.Key.GetComponent<Button>().interactable = true;
            }
            else
            {
                cardItem.Key.GetComponent<Button>().interactable = false;
            }
        }
    }

    public void OnBuyButtonClick(Item item)
    {
        if (player.GetCoins() < item.itemObject.itemPrice)
        {
            Debug.Log("Not enough coins to buy : " + item.name);
            return;
        }

        shop.BuyItem(item);

        // Get Key from Value
        GameObject card = cardItemDictionnary.FirstOrDefault(x => x.Value == item).Key;
        card.GetComponent<Button>().interactable = false;

        item.isBought = true;

        SetCardInteraction();
    }

    public void OnCoinChange(int coin)
    {
        //Debug.Log("button interactable");
        //buyButton.interactable = player.GetCoins() >= item.itemObject.itemPrice;
    }
}
