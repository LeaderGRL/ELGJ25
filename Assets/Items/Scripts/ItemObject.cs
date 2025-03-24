using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Shop/Item")]
public class ItemObject : ScriptableObject
{
    public string itemName;

    [TextArea(15, 20)]
    public string itemDescription;

    public Sprite itemSprite;
    public int itemPrice;

    public List<ItemEffectObject> itemEffects;

    public GameObject itemPrefab;
}


