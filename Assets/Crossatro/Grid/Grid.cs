using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Grid
{
    
    public Dictionary<Vector2Int, char> CharacterPlacements { get; private set; }
    public List<GridWord> Words { get; private set; }

    public Grid(Dictionary<Vector2Int, char> charPlacements,  List<GridWord> words)
    {
        CharacterPlacements = charPlacements;
        Words = words;
    }

    public GridWord? GetWordAtLocation(Vector2Int location)
    {
        if (!CharacterPlacements.ContainsKey(location))
        {
            return null;
        }
        bool isRow = CharacterPlacements.Keys.Contains(new Vector2Int(location.x + 1, location.y)) || 
                     CharacterPlacements.Keys.Contains(new Vector2Int(location.x - 1, location.y));
        bool isColumn = CharacterPlacements.Keys.Contains(new Vector2Int(location.x , location.y + 1)) || 
                        CharacterPlacements.Keys.Contains(new Vector2Int(location.x , location.y - 1));

        int i = 0;
        while (CharacterPlacements.Keys.Contains(isRow ? new Vector2Int(location.x + i - 1, location.y) : new Vector2Int(location.x , location.y - i + 1)))
        {
            i--;
        }
            
        Vector2Int startPosition = isRow ? new Vector2Int(location.x + i , location.y) : new Vector2Int(location.x , location.y - i );

        return GetWordWithStartPosition(startPosition, isRow);
    }

    /// <summary>
    /// The result of this function can have max  2 elements (for one intersection)
    /// and return null if the grid don't contain letter at this location
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    public List<GridWord> GetAllWordAtLocation(Vector2Int location)
    {
        if (!CharacterPlacements.ContainsKey(location))
        {
            return null;
        }

        List<GridWord> result = new List<GridWord>();
        bool isRow = CharacterPlacements.Keys.Contains(new Vector2Int(location.x + 1, location.y)) || 
                     CharacterPlacements.Keys.Contains(new Vector2Int(location.x - 1, location.y));
        bool isColumn = CharacterPlacements.Keys.Contains(new Vector2Int(location.x , location.y + 1)) || 
                        CharacterPlacements.Keys.Contains(new Vector2Int(location.x , location.y - 1));
        int i = 0;
        int j = 0;
        
        if (isRow)
        {
            while (CharacterPlacements.Keys.Contains(new Vector2Int(location.x + i - 1, location.y)))
            {
                i--;
            }

            Vector2Int startPosition = new Vector2Int(location.x + i, location.y);
            result.Add(GetWordWithStartPosition(startPosition, true));
        }

        if (isColumn)
        {
            while (CharacterPlacements.Keys.Contains(new Vector2Int(location.x , location.y - j + 1)))
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
    

    public class GridWord
    {
        public Vector2Int StartPosition;
        public string SolutionWord;
        private string m_currentWord = "";
        public bool IsRow;

        public Dictionary<Vector2Int, char> GetAllLetterSolutionPositions()
        {
            Dictionary<Vector2Int, char> result = new();
            for (int i = 0; i < SolutionWord.Length; i++)
            {
                result[IsRow ? new Vector2Int(StartPosition.x + i, StartPosition.y) : new Vector2Int(StartPosition.x , StartPosition.y - i)] = SolutionWord[i];
            }

            return result;
        }
        public Dictionary<Vector2Int, char> GetAllLetterCurrentWordPositions()
        {
            Dictionary<Vector2Int, char> result = new();
            for (int i = 0; i < SolutionWord.Length; i++)
            {
                if (i >= m_currentWord.Length)
                {
                    result[IsRow ? new Vector2Int(StartPosition.x + i, StartPosition.y) : new Vector2Int(StartPosition.x , StartPosition.y - i)] = '\0';

                    continue;
                }
                result[IsRow ? new Vector2Int(StartPosition.x + i, StartPosition.y) : new Vector2Int(StartPosition.x , StartPosition.y - i)] = m_currentWord[i];
            }

            return result;
        }

        public char GetCurrentLetterAtLocation(Vector2Int location)
        {
            char result = '\0';
            if ((IsRow && (location.y != StartPosition.y || location.x - StartPosition.x > m_currentWord.Length - 1)) || 
                (!IsRow && (location.x != StartPosition.x || StartPosition.y - location.y >  m_currentWord.Length - 1)))
            {
                return result;
            }

            if (IsRow)
            {
                result = m_currentWord[location.x - StartPosition.x];
            }
            else
            {
                result = m_currentWord[StartPosition.y - location.y];
            }
            return result;
        }

        public void  SetLetterAtLocation(Vector2Int location, char letter)
        {
            string newText = "";
            foreach (var letterLocation in GetAllLetterCurrentWordPositions())
            {
                if (location == letterLocation.Key)
                {
                    newText += letter;
                }
                else
                {
                    newText += letterLocation.Value;
                }
            }

            m_currentWord = newText;
        }
        
        public bool TrySetCurrentWord(string text)
        {
            if (text.Length > SolutionWord.Length)
            {
                return false;
            }
            m_currentWord = text;
            return true;
        }

        public string GetCurrentWord()
        {
            return m_currentWord;
        }
    }
    
}
