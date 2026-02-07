using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics.Geometry;
using UnityEngine;

namespace Crossatro.Grid
{
    /// <summary>
    /// Manage all words in a crossword grid.
    /// 
    /// - Store and manage all GridWord instances
    /// - Find words at specific grid positions
    /// - Track completion (All words validated)
    /// - provide grid level operation like reveal letters or get clues
    /// </summary>
    public class CrossWordsGameGrid
    {
        // ============================================================
        // Properties
        // ============================================================

        /// <summary>
        /// All words in this grid.
        /// </summary>
        public List<GridWord> Words { get; set; }

        // ============================================================
        // Event
        // ============================================================

        /// <summary>
        /// Fired when words are validate
        /// Parameter is the last word validated
        /// </summary>
        public event Action<GridWord> OnValidateAllWords;

        /// <summary>
        /// Fired when a new word is added to the grid
        /// </summary>
        public event Action<GridWord> OnAddWord;

        // ============================================================
        // Cache
        // ============================================================

        // Cache for positions -> letter mapping
        private Dictionary<Vector2, char> _gridValuesCache;
        private bool _cacheValid = false;

        // ============================================================
        // Constructors
        // ============================================================

        /// <summary>
        /// Create a CrossWordGameGrid with the given words
        /// </summary>
        /// <param name="words"></param>
        public CrossWordsGameGrid(List<GridWord> words)
        {
            Words = new List<GridWord>();

            foreach (GridWord w in words)
            {
                AddWord(w);
            }
        }

        // ============================================================
        // Words Management
        // ============================================================

        /// <summary>
        /// Add a word to the grid
        /// </summary>
        /// <param name="word"></param>
        public void AddWord(GridWord word)
        {
            if (word == null)
            {
                Debug.LogError("[CrossWordGameGrid] Cannot add null word!0");
                return;
            }

            Words.Add(word);

            // Subscribe to validation evetn
            word.OnValidate += OnWordValidateCallback;

            // Invalidate cache
            _cacheValid = false;

            // Notify listener
            OnAddWord?.Invoke(word);
        }

        /// <summary>
        /// Remove a word from the grid
        /// </summary>
        /// <param name="word"></param>
        public void RemoveWord(GridWord word)
        {
            if (word != null) return;

            word.OnValidate -= OnWordValidateCallback;
            Words.Remove(word);
            _cacheValid = false;
        }

        /// <summary>
        /// Called when any word is validated
        /// </summary>
        /// <param name="word"></param>
        private void OnWordValidateCallback(GridWord word)
        {
            if (CheckAllWordValidated())
            {
                Debug.Log("[CrossWordGameGrid] All words valdiated!");
                OnValidateAllWords?.Invoke(word);
            }
        }

        private bool CheckAllWordValidated()
        {
            foreach (GridWord word in Words)
            {
                if (word.IsValidated)
                    return false;
            }
            return true;
        }

        // ============================================================
        // Grid Values
        // ============================================================

        /// <summary>
        /// Get all grid positions and their solution letters
        /// Uses caching for performance optimization.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Vector2, char> GetWordsToGridValues()
        {
            if (!_cacheValid)
            {
                RebuildCache();
            }
            return new Dictionary<Vector2, char>(_gridValuesCache);
        }

        private void RebuildCache()
        {
            _gridValuesCache = GetAnyWordListToGridValues(Words);
            _cacheValid = true;
        }

        private Dictionary<Vector2, char> GetAnyWordListToGridValues(List<GridWord> words)
        {
            Dictionary<Vector2, char> result = new();

            foreach (GridWord gridWord in words)
            {
                foreach (var letterLocation in gridWord.GetAllLetterSoltutionPosition())
                {
                    result[letterLocation.Key] = letterLocation.Value;
                }
            }

            return result; 
        }

        // ============================================================
        // Word lookup by position
        // ============================================================

        /// <summary>
        /// Get the first word at a specific position
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public GridWord GetWordAtLocation(Vector2 location)
        {
            var gridValues = GetWordsToGridValues();

            if (!gridValues.ContainsKey(location)) return null;

            // Check if this position is part of a horizontal word
            bool isPartOfRow = gridValues.ContainsKey(new Vector2(location.x +1, location.y)) ||
                gridValues.ContainsKey(new Vector2(location.x - 1, location.y));

            // Find the start position
            Vector2 startPosition;
            bool isRow;

            if (isPartOfRow)
            {
                // Find the leftmost position
                int offset = 0;
                while (gridValues.ContainsKey(new Vector2(location.x + offset - 1, location.y)))
                {
                    offset--;
                }
                startPosition = new Vector2(location.x + offset, location.y);
                isRow = true;
            }
            else
            {
                // Find the topmost position
                int offset = 0;
                while (gridValues.ContainsKey(new Vector2(location.x, location.y - offset + 1)))
                {
                    offset--;
                }
                startPosition = new Vector2(location.x, location.y - offset);
                isRow = false;
            }

            return GetWordWithStartPosition(startPosition, isRow);
        }
        
        /// <summary>
        /// Get all words at a specific position.
        /// Used for intersection where we have both horizontal and vertical words.
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public List<GridWord> GetAllWordAtLocation(Vector2 location)
        {
            var gridValues = GetWordsToGridValues();

            if (!gridValues.ContainsKey(location)) return null;

            List<GridWord> result = new List<GridWord>();

            bool isPartOfRow = gridValues.ContainsKey(new Vector2(location.x + 1, location.y)) ||
                gridValues.ContainsKey(new Vector2(location.x - 1, location.y));

            if (isPartOfRow)
            {
                // Find the leftmost position
                int offset = 0;
                while (gridValues.ContainsKey(new Vector2(location.x + offset - 1, location.y)))
                {
                    offset--;
                }
                Vector2 startPosition = new Vector2(location.x + offset, location.y);
                var word = GetWordWithStartPosition(startPosition, true);
                if (word != null)
                {
                    result.Add(word);
                }
            }

            bool isPartOfColumn = gridValues.ContainsKey(new Vector2(location.x, location.y + 1)) ||
                gridValues.ContainsKey(new Vector2(location.x, location.y - 1));

            if (isPartOfColumn)
            {
                // Find the topmost position
                int offset = 0;
                while (gridValues.ContainsKey(new Vector2(location.x, location.y - offset + 1)))
                {
                    offset--;
                }
                Vector2 startPosition = new Vector2(location.x, location.y - offset);
                var word = GetWordWithStartPosition(startPosition, true);
                if (word != null)
                {
                    result.Add(word);
                }
            }

            return result.Count > 0 ? result : null;
        }

        public GridWord GetWordWithStartPosition(Vector2 startPosition, bool isRow)
        {
            foreach (var word in Words)
            {
                if (word.StartPosition == startPosition && word.IsRow == isRow)
                    return word;
            }
            return null;
        }

        // ============================================================
        // Grid Dimensions
        // ============================================================

        public Vector2 GetGridSize()
        {
            var minMax = GetMinAndMaxPositionCharacterPlacement();
            return new Vector2(
                minMax.Value.x - minMax.Key.x,
                minMax.Value.y - minMax.Key.y
            );
        }

        /// <summary>
        /// Get min and max positions.
        /// Key = Min
        /// Value = Max
        /// </summary>
        /// <returns></returns>
        public KeyValuePair<Vector2, Vector2> GetMinAndMaxPositionCharacterPlacement()
        {
            return GetMinAndMaxPositionCharacterPlacementOfWordList(Words);
        }

        private KeyValuePair<Vector2, Vector2> GetMinAndMaxPositionCharacterPlacementOfWordList(List<GridWord> gridWords)
        {
            if (gridWords == null || gridWords.Count == 0)
                return new KeyValuePair<Vector2, Vector2>(Vector2.zero, Vector2.zero);

            Vector2 minValue = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxValue = new Vector2(float.MinValue, float.MinValue);

            foreach (var key in GetAnyWordListToGridValues(gridWords).Keys)
            {
                if (key.x < minValue.x) minValue.x = key.x;
                if (key.y < minValue.y) minValue.y = key.y;
                if (key.x > maxValue.x) maxValue.x = key.x;
                if (key.y > maxValue.y) maxValue.y = key.y;
            }

            // If no position found, return zero
            if (minValue.x == float.MaxValue)
                return new KeyValuePair<Vector2, Vector2>(Vector2.zero, Vector2.zero);

            return new KeyValuePair<Vector2, Vector2>(minValue, maxValue);
        }

        // ============================================================
        // Clues
        // ============================================================

        //public string GetClue(Vector2 location)
        //{

        //}

        // ============================================================
        // Letter Reveal
        // ============================================================

        public List<Vector2> RevealLetterInAllWords(char letter)
        {
            HashSet<Vector2> revealedPositions = new HashSet<Vector2>();

            foreach (var word in Words)
            {
                var solutionLetters = word.GetAllLetterSoltutionPosition();

                foreach (var kvp in solutionLetters)
                {
                    if (kvp.Value == char.ToUpper(letter))
                    {
                        // Reveal in all words at this position (for intersections)
                        var wordsAtPosition = GetAllWordAtLocation(kvp.Key);

                        if (wordsAtPosition != null)
                        {
                            foreach (var w in wordsAtPosition)
                            {
                                w.SetLetterAtLocation(kvp.Key, kvp.Value);

                                if (!w.IsValidated)
                                {
                                    w.ValidatePosition(kvp.Key);
                                }
                            }
                        }
                        revealedPositions.Add(kvp.Key);
                    }
                }
            }
            return revealedPositions.ToList();
        }

        // ============================================================
        // Statistics
        // ============================================================

        /// <summary>
        /// Total number of words.
        /// </summary>
        public int WordCount => Words.Count;

        /// <summary>
        /// Number of validated words.
        /// </summary>
        public int ValidatedWordCount => Words.Count(w => w.IsValidated);
    }
}