using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class CharacterPlacementGenerator
{
    public static Dictionary<Vector2Int, char> GenerateCharPlacements(List<string> possibleWords,  int wordNumber, string anagram)
    {
        Dictionary<Vector2Int, char> result = new();
        List <KeyValuePair <Vector2Int, string>> addedWords = new();
        bool isRow = true;
        AddWord(result, possibleWords[0], Vector2Int.zero, isRow);
        addedWords.Add(new KeyValuePair<Vector2Int, string>(Vector2Int.zero,possibleWords[0]));
        isRow = !isRow;
        
        int i = 1;
        while (addedWords.Count < wordNumber && i < possibleWords.Count)
        {
            string newWord = possibleWords[i];
            var lastWord = addedWords[addedWords.Count - 1];
            
            var corespondingIndexs = GetCorespondingIndexs(newWord, lastWord.Value);
            if (corespondingIndexs.Count > 0)
            {
                var possibleStartPositions = GetPossibleStartPositions(result, corespondingIndexs, newWord,
                    lastWord.Key, isRow);
                if (possibleStartPositions.Count > 0)
                {
                    int index = Random.Range(0, possibleStartPositions.Count);
                    Vector2Int startPos = possibleStartPositions[index];
                    AddWord(result, newWord, startPos, isRow);
                    isRow = !isRow;
                    addedWords.Add(new KeyValuePair<Vector2Int, string>(startPos, newWord));
                
                }
            }
            i++;
        }
        

        return result;
    }

    private static List<Vector2Int> GetPossibleStartPositions(
        Dictionary<Vector2Int, char> currentGrid, 
        List<KeyValuePair<int, int>> correspondingIndexs, 
        string newWord, 
        Vector2Int lastWordStartPosition,
        bool isRow)
    {
        List<Vector2Int> result = new();
        bool isLastWordRow = !isRow;
        foreach (var keyValuePair in correspondingIndexs)
        {
            
            Vector2Int intersectionPos = isLastWordRow ?
                new Vector2Int(lastWordStartPosition.x + keyValuePair.Value, lastWordStartPosition.y) :
                new Vector2Int(lastWordStartPosition.x, lastWordStartPosition.y  - keyValuePair.Value) ;
            Vector2Int newStartPos = isRow ?
                new Vector2Int(intersectionPos.x - keyValuePair.Key, intersectionPos.y) :
                new Vector2Int(intersectionPos.x, intersectionPos.y  + keyValuePair.Key) ;
            if (CanBeStartPos(currentGrid, newWord, newStartPos, isRow, intersectionPos))
            {
                result.Add(newStartPos);
            }
        }
        
        return result;
    }

    private static bool CanBeStartPos(Dictionary<Vector2Int, char> currentGrid, string newWord, Vector2Int startPos, bool isRow, Vector2Int intersectionPos)
    {
        for (int i = 0; i < newWord.Length; i++)
        {
            char letter = newWord[i];
            Vector2Int letterPosition =
                isRow ? new Vector2Int(startPos.x + i, startPos.y) : new Vector2Int(startPos.x, startPos.y - i);
            if (currentGrid.ContainsKey(letterPosition) && currentGrid[letterPosition] != letter)
            {
                return false;
            }

            if (isRow)
            {
                Vector2Int top = new Vector2Int(letterPosition.x, letterPosition.y + 1);
                Vector2Int bot = new Vector2Int(letterPosition.x, letterPosition.y - 1);
                if ((currentGrid.ContainsKey(top) || currentGrid.ContainsKey(bot) )&& intersectionPos.x != letterPosition.x)
                {
                    return false;
                }
            }
            else
            {
                Vector2Int left = new Vector2Int(letterPosition.x - 1, letterPosition.y );
                Vector2Int right = new Vector2Int(letterPosition.x + 1, letterPosition.y );
                if ((currentGrid.ContainsKey(left) || currentGrid.ContainsKey(right)) && intersectionPos.y != letterPosition.y)
                {
                    return false;
                }
            }
            
        }

        return true;
    }

    private static List<KeyValuePair<int, int>> GetCorespondingIndexs( string newWord, string lastWord)
    {
        List<KeyValuePair<int, int>> correspondingIndex = new();
        for (int i = 0; i < newWord.Length; i++)
        {
            for (int j = 0; j < lastWord.Length; j++)
            {
                if (newWord[i] == lastWord[j])
                {
                    correspondingIndex.Add(new KeyValuePair<int, int>(i,j));
                }
            }
        }
        
        
        return correspondingIndex;
    }
    
    
    
    private static void AddWord(Dictionary<Vector2Int, char> dictionary, string word, Vector2Int startPosition, bool isRow)
    {
        for (int i = 0; i < word.Length; i++)
        {
            Vector2Int position =  isRow ? 
                new Vector2Int(startPosition.x + i , startPosition.y ) : 
                new Vector2Int(startPosition.x  , startPosition.y - i ) ;
            
            dictionary[position] = word[i];
        }
    }
    
    
}
