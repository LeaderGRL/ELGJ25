using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public static class CharacterPlacementGenerator
{
    private static Random rng = new Random();
    public static Grid GenerateCharPlacements(WordDatabaseJSON possibleWords,  int wordNumber, string anagram)
    {
        List<WordData> wordsShuffled = possibleWords.words.OrderBy(_ => rng.Next()).ToList();
        Dictionary<Vector2Int, char> result = new();
        List <Grid.GridWord> addedWords = new();
        List<int> addedWordsIndexs = new List<int>();
        
        bool isRow = true;
        
        Grid.GridWord wordToAdd = new Grid.GridWord();
        wordToAdd.SolutionWord = wordsShuffled[0].word;
        wordToAdd.IsRow = isRow;
        wordToAdd.StartPosition = Vector2Int.zero;
        wordToAdd.Difficulty = wordsShuffled[0].difficulty;
        wordToAdd.IsLocked = false;
        wordToAdd.Description = wordsShuffled[0].description;
        wordToAdd.Initialize();
        
        AddWord(result, wordToAdd.SolutionWord, wordToAdd.StartPosition, wordToAdd.IsRow);
        addedWords.Add(wordToAdd);
        addedWordsIndexs.Add(0);
        
        isRow = !isRow;
        bool canRestartLoop = false;
        
        
        int i = 1;
        while (addedWords.Count < wordNumber && (i < wordsShuffled.Count ||  canRestartLoop))
        {
            if (addedWordsIndexs.Contains(i))
            {
                i++;
                continue;
            }
            
            string newWord = wordsShuffled[i].word;
            var lastWord = addedWords[addedWords.Count - 1];
            
            var corespondingIndexs = GetCorespondingIndexs(newWord, lastWord.SolutionWord);
            if (corespondingIndexs.Count > 0)
            {
                var possibleStartPositions = GetPossibleStartPositions(result, corespondingIndexs, newWord,
                    lastWord.StartPosition, isRow);
                if (possibleStartPositions.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, possibleStartPositions.Count);
                    Vector2Int startPos = possibleStartPositions[index];
                    var wordToAddLoop = new Grid.GridWord();
                    
                    wordToAddLoop.SolutionWord = wordsShuffled[i].word;
                    wordToAddLoop.IsRow = isRow;
                    wordToAddLoop.StartPosition = startPos;
                    wordToAddLoop.Difficulty = wordsShuffled[i].difficulty;
                    wordToAddLoop.IsLocked = false;
                    wordToAddLoop.Description = wordsShuffled[i].description;
                    wordToAddLoop.Initialize();
                    
                    AddWord(result, wordToAddLoop.SolutionWord, wordToAddLoop.StartPosition, wordToAddLoop.IsRow);
                    addedWords.Add(wordToAddLoop);
                    addedWordsIndexs.Add(i);
                    isRow = !isRow;
                    canRestartLoop = true;
                }
            }

            if (i == wordsShuffled.Count - 1 )
            {
                i = 0;
                canRestartLoop = false;
                continue;
            }
            
            i++;
        }

        Grid grid = new Grid(result, addedWords);

        return grid;
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

            Vector2Int top = new Vector2Int(letterPosition.x, letterPosition.y + 1);
            Vector2Int bot = new Vector2Int(letterPosition.x, letterPosition.y - 1);
            Vector2Int left = new Vector2Int(letterPosition.x - 1, letterPosition.y );
            Vector2Int right = new Vector2Int(letterPosition.x + 1, letterPosition.y );
            if (isRow)
            {
                if ((currentGrid.ContainsKey(top) || currentGrid.ContainsKey(bot) )&& intersectionPos.x != letterPosition.x)
                {
                    return false;
                }

                if (i == 0 && currentGrid.ContainsKey(left))
                {
                    return false;
                }
                
                if (i ==newWord.Length-1 &&  currentGrid.ContainsKey(right))
                {
                    return false;
                }
            }
            else
            {
                if ((currentGrid.ContainsKey(left) || currentGrid.ContainsKey(right)) && intersectionPos.y != letterPosition.y)
                {
                    return false;
                }
                if (i == 0 && currentGrid.ContainsKey(top))
                {
                    return false;
                }
                if (i ==newWord.Length-1 &&  currentGrid.ContainsKey(bot))
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
