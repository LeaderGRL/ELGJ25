using UnityEngine;

[CreateAssetMenu(fileName = "Gameconfig", menuName ="Crossatro/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Player Settings")]
    public int StartingHealth = 100;
    public int StartingCoin = 5;

    [Header("Turn Settings")]
    public float TurnDuration = 120f;

    [Header("Shop Settings")]
    public int ShopItemCount = 3;
    public int BaseRerollCost = 2;
    public int RerollCostMultiplier = 2;

    [Header("Board Settings")]
    public int DefaultWordsNumberOnBoard = 15;

}
