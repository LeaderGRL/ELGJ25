using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Shop/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField]
    private List<ItemObject> items = new List<ItemObject>();

    public List<ItemObject> Items => items;

    public void AddItem(ItemObject item)
    {
        if (!items.Contains(item))
        {
            items.Add(item);
        }
    }

    public void RemoveItem(ItemObject item)
    {
        items.Remove(item);
    }

    public ItemObject GetItemByName(string name)
    {
        return items.FirstOrDefault(item => item.itemName == name);
    }

    public ItemObject GetItem(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            return items[index];
        }
        return null;
    }

    public ItemObject GetRandomItem()
    {
        if (items.Count == 0)
        {
            Debug.LogWarning("Item database is empty. Cannot retrieve a random item.");
            return null;
        }

        int randomIndex = Random.Range(0, items.Count);
        return items[randomIndex];
    }

    public List<ItemObject> GetRandomItems(int quantity)
    {
        List<ItemObject> randomItems = new List<ItemObject>();

        if (items.Count == 0)
        {
            Debug.LogWarning("Item database is empty. Cannot retrieve random items.");
            return randomItems;
        }

        quantity = Mathf.Min(quantity, items.Count);

        // Create a temporary copy of the items list to avoid duplicates
        List<ItemObject> tempItems = new List<ItemObject>(items);

        for (int i = 0; i < quantity; i++)
        {
            int randomIndex = Random.Range(0, tempItems.Count);
            randomItems.Add(tempItems[randomIndex]);
            tempItems.RemoveAt(randomIndex);
        }

        return randomItems;
    }
}