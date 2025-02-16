using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ShopManager : MonoBehaviour
{
    

    [SerializeField] private ItemObject[] items;
    [SerializeField] private Item itemPrefab;

    [SerializeField] private Transform shopItemContainer;

    [Header("References")]
    public Player player;
    public Timer timer;

    [Header("Event")]
    public UnityEvent onItemBought;


    public static ShopManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GenerateItem();
    }

    void GenerateItem()
    {
        foreach (var item in items)
        {
            Item newItem = Instantiate(itemPrefab, shopItemContainer);
            newItem.itemObject = item;
            ShopItemView itemView = newItem.GetComponent<ShopItemView>();

            if (itemView == null)
                return;

            itemView.Init(newItem);
        }
    }

    public void BuyItem(Item item)
    {
        if (player.GetCoins() >= item.itemObject.itemPrice)
        {
            player.RemoveCoins(item.itemObject.itemPrice);
            onItemBought.Invoke();
            Debug.Log("Item bought: " + item.itemObject.itemName);
        }
        else
        {
            Debug.Log("Not enough coins to buy: " + item.itemObject.itemName);
        }
    }
}
