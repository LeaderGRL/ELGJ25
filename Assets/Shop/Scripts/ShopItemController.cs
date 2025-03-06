using UnityEngine;
using UnityEngine.UI;

public class ShopItemController : MonoBehaviour
{
    public Player player;
    public ShopItemView shopItemView;
    public Button buyButton;
    public CrossWordGridGenerator crossWordGridGenerator;

    private Item item;

    private void Start()
    {
        crossWordGridGenerator.GetCrossWordsGameGrid().OnValidateAllWorlds += PickRandomItem;
        PickRandomItem();
    }

    public void OnEnable()
    {
        player.OnCoinChange += OnCoinChange;
        //ShopManager.Instance.onShopOpened.AddListener(PickRandomItem);
    }

    private void OnDisable()
    {
        //crossWordGridGenerator.GetCrossWordsGameGrid().OnValidateAllWorlds -= PickRandomItem;
        //ShopManager.Instance.onShopOpened.RemoveListener(PickRandomItem);
    }

    private void PickRandomItem(GridWord grid)
    {
        Debug.Log("PickRandomItem !!");
        Item item = ShopManager.Instance.GetRandomItem();
        this.item = item;
        Init(item);
    }

    private void PickRandomItem()
    {
        Item item = ShopManager.Instance.GetRandomItem();
        this.item = item;
        Init(item);
    }

    public void Init(Item item)
    {
        buyButton.interactable = player.GetCoins() >= item.itemObject.itemPrice;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => OnBuyButtonClick(item));

        shopItemView.Init(item);
    }

    public void OnBuyButtonClick(Item item)
    {
        if (player.GetCoins() < item.itemObject.itemPrice)
        {
            Debug.Log("Not enough coins to buy : " + item.name);
            return;
        }
        ShopManager.Instance.BuyItem(item);
        buyButton.interactable = false;
        ShopManager.Instance.CloseShop();
    }

    public void OnCoinChange(int coin)
    {
        Debug.Log("button interactable");
        buyButton.interactable = player.GetCoins() >= item.itemObject.itemPrice;
    }
}
