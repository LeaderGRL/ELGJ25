using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Grid
{
    
    public List<GridWord> Words { get; private set; }
    public event Action<GridWord> OnValidateAllWorlds;
    public event Action<GridWord> OnAddWord; 

    public Grid( List<GridWord> words)
    {
        Words = new List<GridWord>();
        foreach (var word in words)
        {
            AddWord(word);
        }
    }

    public Dictionary<Vector2Int, char> GetWordsToGridValues()
    {
        Dictionary<Vector2Int, char> result = new();
        foreach (var gridWord in Words)
        {
            foreach (var letterLocation in gridWord.GetAllLetterSolutionPositions())
            {
                result[letterLocation.Key] = letterLocation.Value;
            }
        }

        return result;
    }

    public GridWord? GetWordAtLocation(Vector2Int location)
    {
        var characterPlacements = GetWordsToGridValues();
        if (!characterPlacements.ContainsKey(location))
        {
            return null;
        }
        bool isRow = characterPlacements.Keys.Contains(new Vector2Int(location.x + 1, location.y)) || 
                     characterPlacements.Keys.Contains(new Vector2Int(location.x - 1, location.y));
        bool isColumn = characterPlacements.Keys.Contains(new Vector2Int(location.x , location.y + 1)) || 
                        characterPlacements.Keys.Contains(new Vector2Int(location.x , location.y - 1));

        int i = 0;
        while (characterPlacements.Keys.Contains(isRow ? new Vector2Int(location.x + i - 1, location.y) : 
                   new Vector2Int(location.x , location.y - i + 1)))
        {
            i--;
        }

        Vector2Int startPosition =
            isRow ? new Vector2Int(location.x + i, location.y) : new Vector2Int(location.x, location.y - i);

        return GetWordWithStartPosition(startPosition, isRow);
    }

    public void AddWord(GridWord word)
    {
        Words.Add(word);
        word.OnValidate += OnWordValidateCallback;
        OnAddWord?.Invoke(word);
    }

    private void OnWordValidateCallback(GridWord word)
    {
        bool isAllWordValidated = CheckAllWordValidated();
        if (isAllWordValidated)
        {
            OnValidateAllWorlds?.Invoke(word);
        }
    }

    private bool CheckAllWordValidated()
    {
        foreach (var word in Words)
        {
            if (!word.IsValidated)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// The result of this function can have max  2 elements (for one intersection)
    /// and return null if the grid don't contain letter at this location
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public List<GridWord> GetAllWordAtLocation(Vector2Int location)
    {
        var characterPlacements = GetWordsToGridValues();

        if (!characterPlacements.ContainsKey(location))
        {
            return null;
        }

        List<GridWord> result = new List<GridWord>();
        bool isRow = characterPlacements.Keys.Contains(new Vector2Int(location.x + 1, location.y)) || 
                     characterPlacements.Keys.Contains(new Vector2Int(location.x - 1, location.y));
        bool isColumn = characterPlacements.Keys.Contains(new Vector2Int(location.x , location.y + 1)) || 
                        characterPlacements.Keys.Contains(new Vector2Int(location.x , location.y - 1));
        int i = 0;
        int j = 0;
        
        if (isRow)
        {
            while (characterPlacements.Keys.Contains(new Vector2Int(location.x + i - 1, location.y)))
            {
                i--;
            }

            Vector2Int startPosition = new Vector2Int(location.x + i, location.y);
            result.Add(GetWordWithStartPosition(startPosition, true));
        }

        if (isColumn)
        {
            while (characterPlacements.Keys.Contains(new Vector2Int(location.x , location.y - j + 1)))
            {
                j--;
            }
            Vector2Int startPosition = new Vector2Int(location.x , location.y - j );
            result.Add(GetWordWithStartPosition(startPosition, false));

        }

        return result;
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
        foreach (var  key in GetWordsToGridValues().Keys)
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

    public Vector2Int GetMiddleGrid()
    {
        var minPos = GetMinAndMaxPositionCharacterPlacement().Key;
        var gridSize = GetGridSize();

        Vector2Int result = new Vector2Int(minPos.x + (gridSize.x / 2), minPos.y + (gridSize.y / 2));
        
        return result;
    }

    public String GetClue(Vector2Int location)
    {
        var words = GetAllWordAtLocation(location);
        if (words == null)
        {
            return "";
        }
        string result = "";
        foreach (var word in words)
        {
            result += word.Description + "\n";
        }
        return result;
    }


}
