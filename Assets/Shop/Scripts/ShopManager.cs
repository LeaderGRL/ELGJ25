using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ShopManager : MonoBehaviour
{
    

    [SerializeField] private ItemObject[] itemsObject;
    [SerializeField] private GameObject itemPrefab;

    [Header("UI")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform shopItemContainer;

    [Header("References")]
    public Player player;
    public Timer timer;
    public CoinController coinController;
    [SerializeField] private PlayerCameraController m_playerCameraControllerr;

    [Header("Event")]
    public UnityEvent onItemBought;
    public UnityEvent onShopOpened;

    public InputActionAsset inputActionAsset;
    public InputActionReference openShopAction;

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
        openShopAction.action.performed += _ => OpenShop();
        //GenerateItem();
    }


    //void GenerateItem()
    //{
    //    foreach (var item in items)
    //    {
    //        GameObject newItemObject = Instantiate(itemPrefab, shopItemContainer);
    //        Item newItemComponent = newItemObject.GetComponent<Item>();

        //        if (newItemComponent == null)
        //            return;

        //        newItemComponent.itemObject = item;
        //        ShopItemView itemView = newItemComponent.GetComponent<ShopItemView>();

        //        if (itemView == null)
        //            return;

        //        itemView.Init(newItemComponent);
        //    }
        //}

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

    public void OpenShop()
    {
        shopPanel.GetComponent<Canvas>().enabled = true;
        //shopPanel.SetActive(true);
        m_playerCameraControllerr.enabled = false;
        onShopOpened.Invoke();
    }

    public void CloseShop()
    {
        shopPanel.GetComponent<Canvas>().enabled = false;
        m_playerCameraControllerr.enabled = true;
        //shopPanel.SetActive(false);
    }

    public Item GetRandomItem()
    {
        Item item = new Item();
        item.itemObject = itemsObject[Random.Range(0, itemsObject.Length)];
        return item;
    }
}
