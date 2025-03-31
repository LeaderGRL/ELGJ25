using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class CrossWordsGameGrid
{
    
    public List<GridWord> Words { get; private set; }
    public event Action<GridWord> OnValidateAllWorlds;
    public event Action<GridWord> OnAddWord; 

    public CrossWordsGameGrid( List<GridWord> words)
    {
        Words = new List<GridWord>();
        foreach (var word in words)
        {
            AddWord(word);
        }
    }

    public Dictionary<Vector2, char> GetWordsToGridValues()
    {
        return GetAnyWordListToGridValues(Words);
    }

    private Dictionary<Vector2, char> GetAnyWordListToGridValues(List<GridWord> words)
    {
        Dictionary<Vector2, char> result = new();
        foreach (var gridWord in words)
        {
            foreach (var letterLocation in gridWord.GetAllLetterSolutionPositions())
            {
                result[letterLocation.Key] = letterLocation.Value;
            }
        }

        return result;
    }

    public GridWord? GetWordAtLocation(Vector2 location)
    {
        var characterPlacements = GetWordsToGridValues();
        if (!characterPlacements.ContainsKey(location))
        {
            return null;
        }
        bool isRow = characterPlacements.Keys.Contains(new Vector2(location.x + 1, location.y)) || 
                     characterPlacements.Keys.Contains(new Vector2(location.x - 1, location.y));
        bool isColumn = characterPlacements.Keys.Contains(new Vector2(location.x , location.y + 1)) || 
                        characterPlacements.Keys.Contains(new Vector2(location.x , location.y - 1));

        int i = 0;
        while (characterPlacements.Keys.Contains(isRow ? new Vector2(location.x + i - 1, location.y) : 
                   new Vector2(location.x , location.y - i + 1)))
        {
            i--;
        }

        Vector2 startPosition =
            isRow ? new Vector2(location.x + i, location.y) : new Vector2(location.x, location.y - i);

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
            Debug.Log("All words validated");
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
    public List<GridWord> GetAllWordAtLocation(Vector2 location)
    {
        var characterPlacements = GetWordsToGridValues();

        if (!characterPlacements.ContainsKey(location))
        {
            return null;
        }

        List<GridWord> result = new List<GridWord>();
        bool isRow = characterPlacements.Keys.Contains(new Vector2(location.x + 1, location.y)) || 
                     characterPlacements.Keys.Contains(new Vector2(location.x - 1, location.y));
        bool isColumn = characterPlacements.Keys.Contains(new Vector2(location.x , location.y + 1)) || 
                        characterPlacements.Keys.Contains(new Vector2(location.x , location.y - 1));
        int i = 0;
        int j = 0;
        
        if (isRow)
        {
            while (characterPlacements.Keys.Contains(new Vector2(location.x + i - 1, location.y)))
            {
                i--;
            }

            Vector2 startPosition = new Vector2(location.x + i, location.y);
            result.Add(GetWordWithStartPosition(startPosition, true));
        }

        if (isColumn)
        {
            while (characterPlacements.Keys.Contains(new Vector2(location.x , location.y - j + 1)))
            {
                j--;
            }
            Vector2 startPosition = new Vector2(location.x , location.y - j );
            result.Add(GetWordWithStartPosition(startPosition, false));

        }

        return result;
    }
    
    public GridWord? GetWordWithStartPosition(Vector2 startPosition, bool isRow)
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

    public Vector2 GetGridSize()
    {
        var MinMaxpos = GetMinAndMaxPositionCharacterPlacement();
        Vector2 size = new Vector2(MinMaxpos.Value.x - MinMaxpos.Key.x, MinMaxpos.Value.y - MinMaxpos.Key.y);
        return size;
    }
    
    public KeyValuePair<Vector2, Vector2> GetMinAndMaxPositionCharacterPlacement()
    {
        return GetMinAndMaxPositionCharacterPlacementOfWordList(Words);
    }

    private KeyValuePair<Vector2, Vector2> GetMinAndMaxPositionCharacterPlacementOfWordList(List<GridWord> gridWords)
    {
        Vector2 minValue = Vector2.zero;
        Vector2 maxValue = Vector2.zero;
        foreach (var  key in GetAnyWordListToGridValues(gridWords).Keys)
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
        return new KeyValuePair<Vector2, Vector2>(minValue, maxValue);
    }

    public Vector2 GetMiddleWordList(List<GridWord> gridWords)
    {
        var minPosMaxPos = GetMinAndMaxPositionCharacterPlacementOfWordList(gridWords);
        var wordListSize = new Vector2(minPosMaxPos.Value.x - minPosMaxPos.Key.x,
            minPosMaxPos.Value.y - minPosMaxPos.Key.y);
        Vector2 result = new Vector2(minPosMaxPos.Key.x + (wordListSize.x / 2), minPosMaxPos.Key.y + (wordListSize.y / 2));
        return result;
    }
    
    public Vector2 GetMiddleGrid()
    {
        var minPos = GetMinAndMaxPositionCharacterPlacement().Key;
        var gridSize = GetGridSize();

        Vector2 result = new Vector2(minPos.x + (gridSize.x / 2), minPos.y + (gridSize.y / 2));
        
        return result;
    }

    public List<GridWord> GetAllNonValidatedWords()
    {
        List<GridWord> result = new List<GridWord>();
        foreach (var word in Words)
        {
            if (word.IsValidated)
            {
                continue;
            }
            result.Add(word);
        }
        
        return result;
    }

    public String GetClue(Vector2 location)
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

    public List<Vector2> RevealLetterInAllWords(char letter)
    {
        HashSet<Vector2> revealedPositions = new HashSet<Vector2>();

        foreach (GridWord word in Words)
        {
            var solutionLetters = word.GetAllLetterSolutionPositions();
            foreach (var kvp in solutionLetters)
            {
                if (kvp.Value == char.ToUpper(letter))
                {
                    var wordsAtPosition = GetAllWordAtLocation(kvp.Key);
                    foreach (var w in wordsAtPosition)
                    {
                        w.SetLetterAtLocation(kvp.Key, kvp.Value);

                        if (!w.IsValidated)
                        {
                            w.ValidatePosition(kvp.Key);
                        }
                    }
                    revealedPositions.Add(kvp.Key);
                }
            }
        }
        return revealedPositions.ToList();
    }

    public char GetCurrentLetterAtPosition(Vector2 position)
    {
        var wordsHere = GetAllWordAtLocation(position);
        return wordsHere?[0]?.GetCurrentLetterAtLocation(position) ?? '\0';
    }

}
