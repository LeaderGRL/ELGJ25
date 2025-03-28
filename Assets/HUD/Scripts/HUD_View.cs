using NUnit.Framework;
using Rive.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_View : MonoBehaviour
{
    [SerializeField] private RiveWidget shopIcon;
    [SerializeField] private RiveWidget heartIcon;
    [SerializeField] private RiveWidget trophyIcon;
    [SerializeField] private RiveWidget closeIcon;
    [SerializeField] private Image background;
    [SerializeField] private GameObject shopPanel;

    private List<GameObject> cards = new List<GameObject>();


    public void Start()
    {

    }

    public void OpenShopView()
    {
        background.enabled = true;
        closeIcon.enabled = true;
        shopIcon.enabled = false;
        heartIcon.enabled = false;
        trophyIcon.enabled = false;
        shopPanel.SetActive(true);
    }

    public void CloseShopView()
    {
        background.enabled = false;
        closeIcon.enabled = false;
        shopIcon.enabled = true;
        heartIcon.enabled = true;
        trophyIcon.enabled = true;
        shopPanel.SetActive(false);
    }

    public void AddItemToShop(GameObject cardPrefab)
    {
        var card = Instantiate(cardPrefab, shopPanel.transform);
        cards.Add(card);
    }

}