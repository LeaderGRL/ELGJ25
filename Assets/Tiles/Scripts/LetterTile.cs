using System;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "LetterTile", menuName = "Tiles/LetterTile")]
public class LetterTileObject : ScriptableObject
{
    public char letter;
    public Sprite sprite;
}

public class LetterTile : Tile
{
    [SerializeField] LetterTileObject letterTileObject;
    
    [field: SerializeField] 
    public TextMeshProUGUI DisplayText { get; private set; }
}
