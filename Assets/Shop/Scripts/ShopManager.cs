using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ShopManager : MonoBehaviour
{
    

    [SerializeField] private ItemObject[] items;
    [SerializeField] private GameObject itemPrefab;

    [Header("UI")]
    [SerializeField] private GameObject shopPanel;
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
            GameObject newItemObject = Instantiate(itemPrefab, shopItemContainer);
            Item newItemComponent = newItemObject.GetComponent<Item>();

            if (newItemComponent == null)
                return;

            newItemComponent.itemObject = item;
            ShopItemView itemView = newItemComponent.GetComponent<ShopItemView>();

            //if (itemView == null)
            //    return;

            //itemView.Init(newItemComponent);
        }
    }

    public void BuyItem(Item item)
    {
        if (player.GetCoins() >= item.itemObject.itemPrice)
        {
            player.RemoveScore(item.itemObject.itemPrice);
            
            foreach (var effect in item.itemObject.itemEffects)
            {
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

    public void OpenShop()
    {
        shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }
}
