using System;
using System.Collections.Generic;
using UnityEngine;

namespace Crossatro.Combat
{
    /// <summary>
    /// LetterWeights defines the point value for each letter.
    /// Used to calculate damage/score when a word is completed.
    /// </summary>
    [CreateAssetMenu(fileName = "LetterWeights", menuName = "Crossatro/Letter Weights")]
    public class LetterWeights: ScriptableObject
    {
        private static LetterWeights _instance;

        /// <summary>
        /// Get the LetterWeights instance.
        /// </summary>
        public static LetterWeights Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<LetterWeights>("LetterWeights");

                    if (_instance == null)
                    {
                        Debug.LogWarning("[LetterWeights] No instance found! Using default values.");
                        _instance = CreateDefaultInstance();
                    }
                }
                return _instance;
            }

            set => _instance = value;
        }

        // ============================================================
        // Configuration
        // ============================================================

        [Header("Letter Values")]
        [Tooltip("Point value for eache letter A-Z")]
        [SerializeField]
        private LetterValue[] _letterValue = new LetterValue[26];

        // ============================================================
        // Data
        // ============================================================

        private Dictionary<char, int> _weightLookup;

        // ============================================================
        // Initialization
        // ============================================================

        private void OnEnable()
        {
            if (_instance == null)
                _instance = this;

            BuildLookupDictionnary();
        }

        private void OnValidate()
        {
            // Ensure we have exactly 26 letters
            if (_letterValue == null || _letterValue.Length != 26)
                InitializeDefaultValue();

            BuildLookupDictionnary();
        }

        /// <summary>
        /// Initialize with default Scrabble values.
        /// </summary>
        private void InitializeDefaultValue()
        {
            _letterValue = new LetterValue[26];

            // Default French Scrabble point
            var defaults = new Dictionary<char, int>
            {
                // 25 pts — common letters
                { 'E', 25 }, { 'A', 25 }, { 'I', 25 }, { 'N', 25 }, { 'O', 25 },
                { 'R', 25 }, { 'S', 25 }, { 'T', 25 }, { 'U', 25 }, { 'L', 25 },

                // 50 pts — medium letters
                { 'D', 50 }, { 'G', 50 }, { 'M', 50 },

                // 75 pts — good letters
                { 'B', 75 }, { 'C', 75 }, { 'P', 75 },

                // 100 pts — high-value letters
                { 'F', 100 }, { 'H', 100 }, { 'V', 100 },

                // 200 pts — rare letters
                { 'J', 200 }, { 'Q', 200 },

                // 250 pts — epic letters
                { 'K', 250 }, { 'W', 250 }, { 'X', 250 }, { 'Y', 250 }, { 'Z', 250 },
            };

            for (int i = 0; i < 26; i++)
            {
                char letter = (char)('A' + i);
                _letterValue[i] = new LetterValue
                {
                    Letter = letter,
                    Weight = defaults.GetValueOrDefault(letter, 1)
                };
            }
        }

        private void BuildLookupDictionnary()
        {
            _weightLookup = new Dictionary<char, int>();

            foreach (var lv in _letterValue)
            {
                if (lv.Letter != '\0')
                    _weightLookup[char.ToUpper(lv.Letter)] = lv.Weight;
            }
        }

        /// <summary>
        /// Create a default instance at runtime
        /// </summary>
        /// <returns></returns>
        private static LetterWeights CreateDefaultInstance()
        {
            var instance = CreateInstance<LetterWeights>();
            instance.InitializeDefaultValue();
            instance.BuildLookupDictionnary();
            return instance;
        }

        // ============================================================
        // Public API
        // ============================================================

        /// <summary>
        /// Get the point value for a letter
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public int GetWeight(char letter)
        {
            if (_weightLookup == null)
                BuildLookupDictionnary();

            letter = char.ToUpper(letter);
            return _weightLookup.GetValueOrDefault(letter, 25);
        }

        /// <summary>
        /// Calculate the sum of letter weight for a word
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public int CalculateBaseScore(string word)
        {
            if (string.IsNullOrEmpty(word)) return 0;

            int total = 0;
            foreach (char c in word.ToUpper())
            {
                total += GetWeight(c);
            }

            return total;
        }

    }

    // ============================================================
    // Serializable letter weight pair
    // ============================================================

    /// <summary>
    /// Pair letter with its point value.
    /// </summary>
    [Serializable]
    public struct LetterValue
    {
        public char Letter;
        public int Weight;
    }
}
