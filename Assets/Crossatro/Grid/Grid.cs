using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid
{
    
    public Dictionary<Vector2Int, char> CharacterPlacements { get; private set; }
    public List<GridWord> Words { get; private set; }

    public Grid(Dictionary<Vector2Int, char> charPlacements,  List<GridWord> m_words)
    {
        CharacterPlacements = charPlacements;
        m_words = m_words;
    }

    public GridWord? GetWordAtLocation(Vector2Int location)
    {
        bool isRow = CharacterPlacements.Keys.Contains(new Vector2Int(location.x + 1, location.y)) || 
                     CharacterPlacements.Keys.Contains(new Vector2Int(location.x - 1, location.y));


        int i = 0;
        while (CharacterPlacements.Keys.Contains(isRow ? new Vector2Int(location.x - i - 1, location.y) : new Vector2Int(location.x , location.y - i - 1)))
        {
            i--;
        }
            
        Vector2Int startPosition = isRow ? new Vector2Int(location.x - i , location.y) : new Vector2Int(location.x , location.y - i );

        return GetWordWithStartPosition(startPosition, isRow);
    }

    public GridWord? GetWordWithStartPosition(Vector2Int startPosition, bool isRow)
    {
        foreach (var word in Words)
        {
            if (word.StartPosition == startPosition && word.IsRow == isRow)
            {
                return word;
            }
        }

        return null;
    }

    public Vector2Int GetGridSize()
    {
        var MinMaxpos = GetMinAndMaxPositionCharacterPlacement();
        Vector2Int size = new Vector2Int(MinMaxpos.Value.x - MinMaxpos.Key.x, MinMaxpos.Value.y - MinMaxpos.Key.y);
        return size;
    }
    
    public KeyValuePair<Vector2Int, Vector2Int> GetMinAndMaxPositionCharacterPlacement()
    {
        Vector2Int minValue = Vector2Int.zero;
        Vector2Int maxValue = Vector2Int.zero;
        foreach (var  key in CharacterPlacements.Keys)
        {
            if (key.x < minValue.x)
            {
                minValue.x = key.x;
            }
            if (key.y < minValue.y)
            {
                minValue.y = key.y;
            }
            if (key.x > maxValue.x)
            {
                maxValue.x = key.x;
            }
            if (key.y > maxValue.y)
            {
                maxValue.y = key.y;
            }
        }
        return new KeyValuePair<Vector2Int, Vector2Int>(minValue, maxValue);
    }
    
    public struct GridWord
    {
        public Vector2Int StartPosition;
        public string Word;
        public bool IsRow;

        public Dictionary<Vector2Int, char> GetAllLetterPositions()
        {
            Dictionary<Vector2Int, char> result = new();
            for (int i = 0; i < Word.Length; i++)
            {
                result[IsRow ? new Vector2Int(StartPosition.x + i, StartPosition.y) : new Vector2Int(StartPosition.x , StartPosition.y - i)] = Word[i];
            }

            return result;
        }
    }
    
}
