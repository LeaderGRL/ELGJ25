using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public static class CharacterPlacementGenerator
{
    private static Random rng = new Random();
    public static CrossWordsGameGrid GenerateCharPlacements(WordDatabaseJSON possibleWords,  int wordNumber, string anagram)
    {
        List<WordData> wordsShuffled = possibleWords.words.OrderBy(_ => rng.Next()).ToList();
        Dictionary<Vector2, char> result = new();
        List <GridWord> addedWords = new();
        List<int> addedWordsIndexs = new List<int>();
        
        bool isRow = true;
        
        GridWord wordToAdd = new GridWord();
        wordToAdd.SolutionWord = wordsShuffled[0].word;
        wordToAdd.IsRow = isRow;
        wordToAdd.StartPosition = Vector2.zero;
        wordToAdd.Difficulty = wordsShuffled[0].difficulty;
        wordToAdd.Description = wordsShuffled[0].description1;
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
                    Vector2 startPos = possibleStartPositions[index];
                    var wordToAddLoop = new GridWord();
                    
                    wordToAddLoop.SolutionWord = wordsShuffled[i].word;
                    wordToAddLoop.IsRow = isRow;
                    wordToAddLoop.StartPosition = startPos;
                    wordToAddLoop.Difficulty = wordsShuffled[i].difficulty;
                    wordToAddLoop.Description = wordsShuffled[i].description1;
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

        CrossWordsGameGrid crossWordsGameGrid = new CrossWordsGameGrid(addedWords);

        return crossWordsGameGrid;
    }

    public static void GenrateCharPlacementsForExistingGrid(
        WordDatabaseJSON possibleWords,  int wordNumber, string anagram, CrossWordsGameGrid existingCrossWordsGameGrid, GridWord gridWord, Action onEndCallback = null)
    {
        List<WordData> possibleWordsListFiltered = new List<WordData>();
        foreach (var word in possibleWords.words)
        {
            if (!existingCrossWordsGameGrid.Words.Any((gridWord)  => (gridWord.SolutionWord == word.word)))
            {
                possibleWordsListFiltered.Add(word);
            }
        }
        possibleWordsListFiltered = possibleWordsListFiltered.OrderBy(_ => rng.Next()).ToList();

        List<int> wordAddedIndexs = new List<int>();
        int iterations = 0;
        int remainingWordsToAdd = wordNumber;
        GridWord lastWordAdded = gridWord;
        
        while (remainingWordsToAdd > 0 && iterations < possibleWordsListFiltered.Count)
        {
            if (wordAddedIndexs.Contains(iterations))
            {
                iterations++;
                continue;
            }

            var gridValue = existingCrossWordsGameGrid.GetWordsToGridValues();
            bool isRow = !lastWordAdded.IsRow;
            string newWordString = possibleWordsListFiltered[iterations].word;
            var correspondingIndexs =
                GetCorespondingIndexs(newWordString, lastWordAdded.SolutionWord);
            if (correspondingIndexs.Count == 0)
            {
                iterations++;
                continue;
            }
            
            var possibleStartPositions =
                GetPossibleStartPositions(gridValue, correspondingIndexs, newWordString, lastWordAdded.StartPosition,
                    isRow);

            if (possibleStartPositions.Count == 0)
            {
                iterations++;
                continue;
            }

            int index = UnityEngine.Random.Range(0, possibleStartPositions.Count);
            Vector2 startPos = possibleStartPositions[index];
            var wordToAddLoop = new GridWord();
            
            wordToAddLoop.SolutionWord = possibleWordsListFiltered[iterations].word;
            wordToAddLoop.IsRow = isRow;
            wordToAddLoop.StartPosition = startPos;
            wordToAddLoop.Difficulty = possibleWordsListFiltered[iterations].difficulty;
            wordToAddLoop.Description = possibleWordsListFiltered[iterations].description1;
            wordToAddLoop.Initialize();
            if (lastWordAdded.IsValidated)
            {
                foreach (var letterSolutionPosition in lastWordAdded.GetAllLetterSolutionPositions())
                {
                    if (wordToAddLoop.GetAllLetterSolutionPositions().ContainsKey(letterSolutionPosition.Key))
                    {
                        wordToAddLoop.SetLetterAtLocation(letterSolutionPosition.Key, letterSolutionPosition.Value);
                    }
                }
            }
                    
            existingCrossWordsGameGrid.AddWord(wordToAddLoop);
            wordAddedIndexs.Add(iterations);
            lastWordAdded = wordToAddLoop;
            iterations = 0;
            remainingWordsToAdd--;
        }
        onEndCallback?.Invoke();
    }
    
    private static List<Vector2> GetPossibleStartPositions(
        Dictionary<Vector2, char> currentGrid, 
        List<KeyValuePair<int, int>> correspondingIndexs, 
        string newWord, 
        Vector2 lastWordStartPosition,
        bool isRow)
    {
        List<Vector2> result = new();
        bool isLastWordRow = !isRow;
        foreach (var keyValuePair in correspondingIndexs)
        {
            
            Vector2 intersectionPos = isLastWordRow ?
                new Vector2(lastWordStartPosition.x + keyValuePair.Value, lastWordStartPosition.y) :
                new Vector2(lastWordStartPosition.x, lastWordStartPosition.y  - keyValuePair.Value) ;
            Vector2 newStartPos = isRow ?
                new Vector2(intersectionPos.x - keyValuePair.Key, intersectionPos.y) :
                new Vector2(intersectionPos.x, intersectionPos.y  + keyValuePair.Key) ;
            if (CanBeStartPos(currentGrid, newWord, newStartPos, isRow, intersectionPos))
            {
                result.Add(newStartPos);
            }
        }
        
        return result;
    }

    private static bool CanBeStartPos(Dictionary<Vector2, char> currentGrid, string newWord, Vector2 startPos, bool isRow, Vector2 intersectionPos)
    {
        for (int i = 0; i < newWord.Length; i++)
        {
            char letter = newWord[i];
            Vector2 letterPosition =
                isRow ? new Vector2(startPos.x + i, startPos.y) : new Vector2(startPos.x, startPos.y - i);
            if (currentGrid.ContainsKey(letterPosition) && currentGrid[letterPosition] != letter)
            {
                return false;
            }

            Vector2 top = new Vector2(letterPosition.x, letterPosition.y + 1);
            Vector2 bot = new Vector2(letterPosition.x, letterPosition.y - 1);
            Vector2 left = new Vector2(letterPosition.x - 1, letterPosition.y );
            Vector2 right = new Vector2(letterPosition.x + 1, letterPosition.y );
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
    
    private static void AddWord(Dictionary<Vector2, char> dictionary, string word, Vector2 startPosition, bool isRow)
    {
        for (int i = 0; i < word.Length; i++)
        {
            Vector2 position =  isRow ? 
                new         (startPosition.x + i , startPosition.y ) : 
                new Vector2(startPosition.x  , startPosition.y - i ) ;
            
            dictionary[position] = word[i];
        }
    }
    
    
}
