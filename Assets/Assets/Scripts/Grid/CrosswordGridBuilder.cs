using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Crossatro.Grid
{
    /// <summary>
    /// Build a crossword grid by selecting and placing words from the database.
    /// </summary>
    public class CrosswordGridBuilder
    {
        // ============================================================
        // Constants
        // ============================================================

        /// <summary>
        /// Maximum iterations before giving up on placing more words.
        /// Prevents infinite loops when no valid placement exists.
        /// </summary>
        private const int MAX_ITERATIONS = 10000;

        // ============================================================
        // State
        // ============================================================

        private readonly System.Random _rng;

        /// <summary>
        /// Grid of already placed letters.
        /// </summary>
        private Dictionary<Vector2, char> _placedLetters;

        /// <summary>
        /// Words successfully placed to the grid.
        /// </summary>
        private List<GridWord> _placedWords;

        // ============================================================
        // Constructor
        // ============================================================

        /// <summary>
        /// Create a new grid builder.
        /// </summary>
        /// <param name="seed">Optional seed for specific grid acess</param>
        public CrosswordGridBuilder(int seed = -1)
        {
            _rng = seed >= 0 ? new System.Random(seed) : new System.Random();
        }

        // ============================================================
        // API
        // ============================================================

        /// <summary>
        /// Build a crossword grid by placing words from the database.
        /// </summary>
        /// <param name="database">Words database</param>
        /// <param name="targetWordCount">Number of word to place</param>
        /// <returns></returns>
        public CrossWordsGameGrid Build(WordDatabaseJSON database, int targetWordCount)
        {
            if (database == null || database.words == null || database.words.Count == 0)
            {
                Debug.LogError("[CrosswordGridBuilder] Database is null or empty!");
                return new CrossWordsGameGrid(new List<GridWord>());
            }

            // Reset state
            _placedLetters = new Dictionary<Vector2, char>();
            _placedWords = new List<GridWord>();

            // Shuffle the word list
            List<WordData> shuffleWords = database.words
                .OrderBy(_ => _rng.Next())
                .ToList();

            // Place the first word at the origin
            PlaceFirstWord(shuffleWords[0]);

            // Try to place remaining words
            PlaceRemainingWords(shuffleWords, targetWordCount);

            Debug.Log($"[CrosswordGridBuilder] built grid with " + $"{_placedWords.Count}/{targetWordCount} words");

            return new CrossWordsGameGrid(_placedWords);
        }

        // ============================================================
        // Word Placement
        // ============================================================


        /// <summary>
        /// Place the first word at the origin.
        /// </summary>
        /// <param name="wordData"></param>
        private void PlaceFirstWord(WordData wordData)
        {
            GridWord word = CreateGridWord(wordData, Vector2.zero, isRow: true);
            CommitWord(word);
        }

        /// <summary>
        /// Try to place words until we reach the target count.
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="targetCount"></param>
        private void PlaceRemainingWords(List<WordData> candidates, int targetCount)
        {
            // Track which candidate indices we've already placed
            HashSet<int> usedIndices = new HashSet<int> { 0 };

            int passesWithoutPlacement = 0;
            int iterations = 0;
            int candidateIndex = 1;

            while (_placedWords.Count < targetCount && iterations < MAX_ITERATIONS)
            {
                iterations++;

                if (usedIndices.Contains(candidateIndex))
                {
                    candidateIndex = NextCandidateIndex(candidateIndex, candidates.Count);
                    continue;
                }

                if (TryPlaceCandidateAgainstGrid(candidates[candidateIndex], out GridWord newWord))
                {
                    CommitWord(newWord);
                    usedIndices.Add(candidateIndex);
                    passesWithoutPlacement = 0;
                }

                candidateIndex = NextCandidateIndex( candidateIndex, candidates.Count); 

                if (candidateIndex == 1)
                {
                    passesWithoutPlacement++;

                    // Stop after 2 full passes without placing any word
                    if (passesWithoutPlacement >= 2) break; 
                }
            }

            if (iterations >=  MAX_ITERATIONS)
            {
                Debug.LogWarning($"[CrosswordGridBuilder] Hit max iterations ({MAX_ITERATIONS}). " + $"Placed {_placedWords.Count}/{targetCount} words.");
            }
        }

        /// <summary>
        /// Advance to next candidate index, wrapping around.
        /// Index 0 is always the first word already placed.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        private int NextCandidateIndex(int current, int total)
        {
            int next = current + 1;
            if (next >= total) next = 1;
            return next;
        }

        private bool TryPlaceCandidateAgainstGrid(WordData wordData, out GridWord result)
        {
            result = null;

            // Shuffle anchor order for variety
            List<int> anchorOrder = Enumerable.Range(0, _placedWords.Count)
                .OrderBy(_ => _rng.Next())
                .ToList();

            foreach (int anchorIndex in anchorOrder)
            {
                GridWord anchor = _placedWords[anchorIndex];

                bool newIsRow = !anchor.IsRow;

                // Find shared letters
                var intersections = FindLetterIntersections(wordData.word, anchor.SolutionWord);
                if (intersections.Count == 0) continue;

                // Find valid start positions from theses intersections
                var ValidPositions = FindValidStartPosition(wordData.word, anchor, intersections, newIsRow);

                if (ValidPositions.Count == 0) continue;

                // Pick a random valid position
                int index = _rng.Next(ValidPositions.Count);
                Vector2 startPos = ValidPositions[index];

                result = CreateGridWord(wordData, startPos, newIsRow);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to place a word crossing the last word.
        /// Find shared letters between the two words, then checks if any resulting position is valid.
        /// </summary>
        /// <param name="wordData"></param>
        /// <param name="lastWord"></param>
        /// <param name="isRow"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool TryPlaceWord(WordData wordData, GridWord lastWord, bool isRow, out GridWord result)
        {
            result = null;

            // Find all letter intersections between the candidate and the last word
            var intersections = FindLetterIntersections(wordData.word, lastWord.SolutionWord);
            if (intersections.Count == 0) return false;
            
            // Try each intersection to find a valid start position
            var validPositions = FindValidStartPosition(wordData.word, lastWord, intersections, isRow);

            if (validPositions.Count == 0) return false;

            // Pick a random valid position
            int index = _rng.Next(validPositions.Count);
            Vector2 startPos = validPositions[index];

            result = CreateGridWord(wordData, startPos, isRow);
            return true;
        }

        // ============================================================
        // Intersection detection
        // ============================================================

        /// <summary>
        /// Find all index pairs where two words share the same letter.
        /// </summary>
        /// <param name="newWord"></param>
        /// <param name="lastWord"></param>
        /// <returns>Pairs if (newWordIndex, lastWordIndex)</returns>
        private List<(int newIndex, int lastIndex)> FindLetterIntersections(string newWord, string lastWord)
        {
            var intersections = new List<(int, int)>();

            for (int i = 0; i < newWord.Length; i++)
            {
                for (int j = 0; j < lastWord.Length; j++)
                {
                    if (newWord[i] == lastWord[j])
                    {
                        intersections.Add((i, j));
                    }
                }
            }

            return intersections;
        }


        // ============================================================
        // Position validation
        // ============================================================


        /// <summary>
        /// For each interesection, calculate where the new word would start and check is that placement is valid.
        /// </summary>
        /// <param name="newWord"></param>
        /// <param name="lastWord"></param>
        /// <param name="intersections"></param>
        /// <param name="isRow"></param>
        /// <returns></returns>
        private List<Vector2> FindValidStartPosition(string newWord, GridWord lastWord, List<(int newIndex, int lastIndex)> intersections, bool isRow)
        {
            var validPosition = new List<Vector2>();
            bool isLastWordRow = !isRow;

            foreach (var (newIndex, lastIndex) in intersections)
            {
                // Calculate where the intersection letter sits on the grid
                Vector2 intersecionPos = CalculateLetterPosition(lastWord.StartPosition, lastIndex, isLastWordRow);

                // Calculate where the new word would start
                Vector2 startPos = CalculateStartPosition(intersecionPos, newIndex, isRow);

                // Validate the entire word at this position
                if (IsPlacementValid(newWord, startPos, isRow, intersecionPos))
                {
                    validPosition.Add(startPos);
                }
            }

            return validPosition;
        }

        /// <summary>
        /// Check if placing a word at the given position is valid.
        /// Rules:
        /// - No letter conflitcts
        /// - No parallel adjacency
        /// - No extension conflicts
        /// </summary>
        /// <param name="word"></param>
        /// <param name="startPos"></param>
        /// <param name="isRow"></param>
        /// <param name="intersectionPos"></param>
        /// <returns></returns>
        private bool IsPlacementValid(string word, Vector2 startPos, bool isRow, Vector2 intersectionPos)
        {
            for (int i = 0; i < word.Length; i++)
            {
                char letter = word[i];
                Vector2 letterPos = CalculateLetterPosition(startPos, i, isRow);

                // No letter conflitcts
                if (_placedLetters.TryGetValue(letterPos, out char existing))
                {
                    if (existing != letter) return false;
                }

                // Adjacency checks
                if (!IsAdjacencyValid(letterPos, intersectionPos, isRow, i, word.Length))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check adjacency rules for a single letter position.
        /// - Letters perpendicual to the word direction must not have neighbors
        /// - First letter must not have a neighbors before it.
        /// - Last letter must not habe a neighbor after it.
        /// </summary>
        /// <param name="letterPos"></param>
        /// <param name="intersectionPos"></param>
        /// <param name="isRow"></param>
        /// <param name="letterIndex"></param>
        /// <param name="wordLenght"></param>
        /// <returns></returns>
        private bool IsAdjacencyValid(Vector2 letterPos, Vector2 intersectionPos, bool isRow, int letterIndex, int wordLength)
        {
            if (isRow)
            {
                // Check above and below
                Vector2 above = new Vector2(letterPos.x, letterPos.y + 1);
                Vector2 below = new Vector2(letterPos.x, letterPos.y - 1);

                if (letterPos.x != intersectionPos.x)
                    if (_placedLetters.ContainsKey(above) || _placedLetters.ContainsKey(below))
                        return false;

                // Check before first letter
                if (letterIndex == 0)
                {
                    Vector2 left = new Vector2(letterPos.x - 1, letterPos.y);
                    if (_placedLetters.ContainsKey(left)) return false;
                }

                // Check after last letter
                if (letterIndex == wordLength - 1)
                {
                    Vector2 right = new Vector2(letterPos.x + 1, letterPos.y);
                    if (_placedLetters.ContainsKey(right)) return false;
                }
            }
            else
            {
                // Check left and right
                Vector2 left = new Vector2(letterPos.x -1, letterPos.y);
                Vector2 right = new Vector2(letterPos.x + 1, letterPos.y);

                if (letterPos.y != intersectionPos.y)
                {
                    if (_placedLetters.ContainsKey(left) || _placedLetters.ContainsKey(right))
                        return false;
                }

                // Check before first letter
                if (letterIndex == 0)
                {
                    Vector2 above = new Vector2(letterPos.x, letterPos.y + 1);
                    if (_placedLetters.ContainsKey(above)) return false;
                }

                // Check after last letter
                if (letterIndex == wordLength - 1)
                {
                    Vector2 below = new Vector2(letterPos.x, letterPos.y - 1);
                    if (_placedLetters.ContainsKey(below)) return false;
                }
            }

            return true;
        }

        // ============================================================
        // Position calculation
        // ============================================================

        /// <summary>
        /// Calculate the grid position of a letter at the given index.
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="index"></param>
        /// <param name="isRow"></param>
        /// <returns></returns>
        private Vector2 CalculateLetterPosition(Vector2 startPos, int index, bool isRow)
        {
            return isRow ? new Vector2(startPos.x + index, startPos.y) : new Vector2(startPos.x, startPos.y - index);
        }

        /// <summary>
        /// Calculate the start position of a word give where one of its letters must be placed.
        /// </summary>
        /// <param name="intersectionPos"></param>
        /// <param name="letterIndex"></param>
        /// <param name="isRow"></param>
        /// <returns></returns>
        private Vector2 CalculateStartPosition(Vector2 intersectionPos, int letterIndex, bool isRow)
        {
            return isRow ? new Vector2(intersectionPos.x - letterIndex, intersectionPos.y) : new Vector2(intersectionPos.x, intersectionPos.y + letterIndex);
        }

        // ============================================================
        // Word creation & commit
        // ============================================================

        /// <summary>
        /// Create a GridWord instance from word data and placement info.
        /// </summary>
        /// <param name="wordData"></param>
        /// <param name="startPos"></param>
        /// <param name="isRow"></param>
        /// <returns></returns>
        private GridWord CreateGridWord(WordData wordData, Vector2 startPos, bool isRow)
        {
            GridWord word = new GridWord();
            word.SolutionWord = wordData.word;
            word.StartPosition = startPos;
            word.IsRow = isRow;
            word.Difficulty = wordData.difficulty;
            word.Description = wordData.description1;
            word.Initialize();
            return word;
        }

        /// <summary>
        /// Add a word to the grid => Register its letters and store it.
        /// </summary>
        /// <param name="word">Word to add to the grid</param>
        private void CommitWord(GridWord word)
        {
            var letters = word.GetAllLetterSolutionPositions();
            foreach (var kvp in letters)
            {
                _placedLetters[kvp.Key] = kvp.Value;
            }

            _placedWords.Add(word);
        }
    }
}
