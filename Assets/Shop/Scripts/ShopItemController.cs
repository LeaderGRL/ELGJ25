using UnityEngine;
using UnityEngine.UI;

public class ShopItemController : MonoBehaviour
{
    public ShopItemView shopItemView;
    public Button buyButton;

    private void Start()
    {
        PickRandomItem();
    }

    public void OnEnable()
    {
        ShopManager.Instance.onShopOpened.AddListener(PickRandomItem);
    }

    private void OnDisable()
    {
        ShopManager.Instance.onShopOpened.RemoveListener(PickRandomItem);
    }

    private void PickRandomItem()
    {
        Item item = ShopManager.Instance.GetRandomItem();
        Init(item);
    }

    public void Init(Item item)
    {
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => OnBuyButtonClick(item));

        shopItemView.Init(item);
    }

    public void OnBuyButtonClick(Item item)
    {
        ShopManager.Instance.BuyItem(item);
        ShopManager.Instance.CloseShop();
    }
}
