using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemPrice;
    public TextMeshProUGUI itemDescription;
    public Button buyButton;


    public void Init(Item item)
    {
        iconImage.sprite = item.itemObject.itemSprite;
        itemName.text = item.itemObject.itemName;
        itemPrice.text = item.itemObject.itemPrice.ToString();
        itemDescription.text = item.itemObject.itemDescription;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => OnBuyButtonClick(item));

    }

    public void OnBuyButtonClick(Item item)
    {
        ShopManager.Instance.BuyItem(item);
    }
}
