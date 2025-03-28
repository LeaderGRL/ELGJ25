using Rive;
using Rive.Components;
using TMPro;
using UnityEngine;

public class CoinView : MonoBehaviour
{
    [SerializeField] private RiveWidget RiveWidget;

    public void UpdateCoin(int coin)
    {
        RiveWidget.Artboard.SetTextRun("GemsText", coin.ToString());
    }
}
