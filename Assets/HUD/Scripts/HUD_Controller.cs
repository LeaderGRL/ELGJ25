using NUnit.Framework;
using Rive.Components;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUD_Controller : MonoBehaviour
{
    [SerializeField] private HUD_View m_hudView;
    [SerializeField] private PlayerCameraController m_playerCameraController;

    [Header("References")]
    [SerializeField] private GameObject m_shopPanel;
    public Player player;
    public Timer timer;
    public CoinController coinController;
    public ItemDatabase itemDatabase;
    public Shop shop;

    [Header("Event")]
    private UnityEvent onItemBought;
    private UnityEvent onShopOpened;
    private UnityEvent onShopClosed;

    private List<GameObject> m_cards = new List<GameObject>();
    private Dictionary<GameObject, Item> m_cardItemDictionnary = new Dictionary<GameObject, Item>();

    public void Start()
    {
        player.OnCoinChange += OnCoinChange;
        InitShop();
    }

    public void OpenShop()
    {
        m_playerCameraController.enabled = false;
        m_hudView.OpenShopView();
        SetCardInteraction();

        foreach (KeyValuePair<GameObject, Item> cardItem in m_cardItemDictionnary)
        {
            cardItem.Key.GetComponentInChildren<RiveWidget>().Artboard.SetTextRun("CostText", cardItem.Value.itemObject.itemPrice.ToString());
        }
    }

    public void CloseShop()
    {
        m_playerCameraController.enabled = true;
        m_hudView.CloseShopView();
    }

    public void InitShop()
    {
        for (int i = 0; i < 3; i++)
        {
            var cardItemObject = itemDatabase.GetRandomItem();
            Item cardItem = new Item();
            cardItem.itemObject = cardItemObject;
            var card = Instantiate(cardItemObject.itemPrefab, m_shopPanel.transform);
            m_cardItemDictionnary.Add(card, cardItem);

            card.GetComponent<Button>().onClick.AddListener(() => OnBuyButtonClick(cardItem));
        }
    }

    public void SetCardInteraction()
    {
        foreach (KeyValuePair<GameObject, Item> cardItem in m_cardItemDictionnary)
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
        GameObject card = m_cardItemDictionnary.FirstOrDefault(x => x.Value == item).Key;
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
