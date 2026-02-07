using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Crossatro.Grid
{
    /// <summary>
    /// Represent a single word in the crossword grid
    /// 
    /// A grid word tracks:
    /// - The solution
    /// - The position and orientation in the grid
    /// - the letter validation state
    /// </summary>
    [Serializable]
    public class GridWord
    {
        // ============================================================
        // Configuration
        // ============================================================

        /// <summary>
        /// The correct answer for this word
        /// </summary>
        public string SolutionWord { get; set; }

        /// <summary>
        /// The clue shown to the player
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Grid position where the word starts:
        /// For horizontal words, this is the leftmost letter position.
        /// For vertical worlds, this is the topmost letter positon.
        /// </summary>
        public Vector2 StartPosition { get; set; }

        /// <summary>
        /// True = Horizontal
        /// False = Vertical
        /// </summary>
        public bool IsRow { get; set; }

        /// <summary>
        /// Difficulty level of a word.
        /// Used for score and damage calculation.
        /// </summary>
        public int Difficulty { get; set; }

        // ============================================================
        // State
        // ============================================================

        /// <summary>
        /// True if the player has correctly completed the word.
        /// Once validate, the word cannot be modified
        /// </summary>
        public bool IsValidated { get; set; }

        /// <summary>
        /// Fired when this word is validated;
        /// </summary>
        public event Action<GridWord> OnValidate;

        // ============================================================
        // Data
        // ============================================================

        // Maps grid position -> solution letter
        // Example: (0,0) -> 'C'
        private Dictionary<Vector2, char> _solutionPositions;

        // Maps grid position -> current letter (what player typed)
        // Example: (0,0) -> 'V'
        private Dictionary<Vector2, char> _currentPositions;

        // Positions that have been individually validated
        private HashSet<Vector2> _validatePositions;

        // ============================================================
        // Initialization
        // ============================================================

        /// <summary>
        /// Initialize world after setting SolutionWord, StartPosition and IsRow.
        /// </summary>
        public void Initialize()
        {
            if (string.IsNullOrEmpty(SolutionWord))
            {
                Debug.LogError("[GridWord] Cannot initialize: SolutionWord is null or empty!");
                return;
            }

            _solutionPositions = new Dictionary<Vector2, char>();
            _currentPositions = new Dictionary<Vector2, char>();
            _validatePositions = new HashSet<Vector2>();

            // Build position mapping for each letter
            for (int i = 0; i < SolutionWord.Length; i++)
            {
                // Calculate position based on orientation
                Vector2 position = CalculateLetterPosition(i);

                _solutionPositions[position] = SolutionWord[i];
                _currentPositions[position] = '\0';
            }
        }

        /// <summary>
        /// Calculate the grid position for a letter at index i
        /// 
        /// Horizontal:
        ///     Start at (x, y), each letter moves +1 on X axis
        ///     Position = (StartX + i, StartY)
        ///     
        /// Vertical:
        ///     Start at (x, y), each letter moves -1 on Y axis
        ///     Position = (StartX, StartY - i)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private Vector2 CalculateLetterPosition(int index)
        {
            if (IsRow)
            {
                // Horizontal: Move right along X axis
                return new Vector2(StartPosition.x + index, StartPosition.y);
            }
            else
            {
                // Vertical: Move down along Y axis
                return new Vector2(StartPosition.x, StartPosition.y - index);
            }
        }

        // ============================================================
        // Getter - Data
        // ============================================================

        /// <summary>
        /// Get all positions and their solution letters.
        /// Return a copy to prevent external modification.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Vector2, char> GetAllLetterSoltutionPosition()
        {
            return new Dictionary<Vector2, char>(_solutionPositions);
        }

        /// <summary>
        /// Get the solution letter at a specific position.
        /// Return '\0' if position is not part of this word.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public char GetSolutionLetterAtLocation(Vector2 position)
        {
            return _solutionPositions.GetValueOrDefault(position, '\0');
        }

        /// <summary>
        /// Check if a position is part of this word.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool ContainPosition(Vector2 position)
        {
            return _currentPositions.ContainsKey(position);
        }

        /// <summary>
        /// Get the end position of this word.
        /// </summary>
        public Vector2 EndPosition => CalculateLetterPosition(SolutionWord.Length - 1);

        /// <summary>
        /// Get the length of this word.
        /// </summary>
        public int length => SolutionWord.Length;

        // ============================================================
        // Getter - State
        // ============================================================

        /// <summary>
        /// Get all position and their current letters (what player typed).
        /// Return a copy to prevent external modification.
        /// </summary>
        /// <returns></returns>
        public Dictionary<Vector2, char> GetAllLetterCurrentWordPosition()
        {
            return new Dictionary<Vector2, char>(_currentPositions);
        }

        /// <summary>
        /// Get current word as a string (what player typed).
        /// Empty positions become '\0' characters.
        /// 
        /// Example: If player typed "CH" for "CHAT", returns "CH\0\0"
        /// </summary>
        /// <returns></returns>
        public string GetCurrentWordToString()
        {
            if (_currentPositions == null || _currentPositions.Count == 0)
                return string.Empty;

            // Sort position by their order in the word
            var sortedPositions = _currentPositions.Keys
                .OrderBy(pos => IsRow ? pos.x : -pos.y)
                .ToList();

            // Build string from sorted positions
            var chars = sortedPositions.Select(pos => _currentPositions[pos]).ToArray();
            return new string(chars);
        }

        /// <summary>
        /// Check if a specific position has been validated
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool IsPositionValidated(Vector2 position)
        {
            return _validatePositions.Contains(position);
        }

        // ============================================================
        // Setter - State
        // ============================================================

        public void SetLetterAtLocation(Vector2 position, char letter)
        {
            // Can't modify if world is fully validated
            if (IsValidated) return;

            // Can't modify if position is individually validated
            if (_validatePositions.Contains(position)) return;

            /// Only modify if position is part of this word
            if (_currentPositions.ContainsKey(position))
            {
                _currentPositions[position] = char.ToUpper(letter);
            }
        }

        /// <summary>
        /// Clear all non validated letters
        /// </summary>
        public void ClearNonValidatedLetters()
        {
            if (IsValidated) return;

            foreach (var position in _currentPositions.Keys.ToList())
            {
                if (!_validatePositions.Contains(position))
                {
                    _currentPositions[position] = '\0';
                }
            }
        }

        // ============================================================
        // Validation
        // ============================================================

        /// <summary>
        /// Validate a single position
        /// Usefull for cards like "Reveal a letter on the grid"
        /// 
        /// This will:
        /// - Set the current letter to the solution letter
        /// - Mark the position as validated and then can't be changed by the player
        /// </summary>
        /// <param name="position"></param>
        public void ValidatePosition(Vector2 position)
        {
            if (!_solutionPositions.ContainsKey(position)) return;
            if (_validatePositions.Contains(position)) return;

            _validatePositions.Add(position);
            _currentPositions[position] = _solutionPositions[position];

            // Check if all position are now validated
            if (_validatePositions.Count == _solutionPositions.Count)
            {
                Validate();
            }
        }

        /// <summary>
        /// Validate the entire word.
        /// Called when player correctly completed the word.
        /// </summary>
        public void Validate()
        {
            if (IsValidated) return;

            IsValidated = true;

            // Mark all position as validated => Can't be modified
            foreach (var position in _solutionPositions.Keys)
            {
                _validatePositions.Add(position);
                _currentPositions[position] = _solutionPositions[position];
            }

            Debug.Log($"[GridWord] Validated: {SolutionWord}");
        }

        // ============================================================
        // Utility
        // ============================================================

        /// <summary>
        /// Get all position occupied by this word.
        /// </summary>
        /// <returns></returns>
        public List<Vector2> GetAllPosition()
        {
            return _solutionPositions?.Keys.ToList() ?? new List<Vector2>();
        }
    }
}
