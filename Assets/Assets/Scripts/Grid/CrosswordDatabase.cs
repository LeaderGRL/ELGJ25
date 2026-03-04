using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crossatro.Grid
{
    /// <summary>
    /// Loads words from JSON and provide fast lookup
    /// by length, difficulty ans theme.
    /// </summary>
    [CreateAssetMenu(fileName = "crosswordDatabase", menuName = "Crossatro/Crossword Database")]
    public class CrosswordDatabase : ScriptableObject
    {
        // ============================================================
        // Configuration
        // ============================================================

        [Header("Source")]
        [Tooltip("JSON file containing word entries")]
        [SerializeField] private TextAsset _jsonSource;

        [Header("Debug")]
        [SerializeField] private bool _logStats = true;

        // ============================================================
        // Runtime state
        // ============================================================

        private List<WordEntry> _allWords = new List<WordEntry>();
        private string _language = "en";
        [SerializeField] private bool _isLoaded = false;

        // Indexes for fast lookup
        private Dictionary<int, List<WordEntry>> _byLength = new Dictionary<int, List<WordEntry>>();
        private Dictionary<string, List<WordEntry>> _byTheme = new Dictionary<string, List<WordEntry>>();

        // ============================================================
        // Properties
        // ============================================================

        public string Language => _language;
        public int TotalWordCount => _allWords.Count;
        public bool IsLoaded => _isLoaded;

        // ============================================================
        // Loading
        // ============================================================

        /// <summary>
        /// Load and index words from JSON source.
        /// </summary>
        public void Load()
        {
            _allWords.Clear();
            _byLength.Clear();
            _byTheme.Clear();
            _isLoaded = false;

            if (_jsonSource == null)
            {
                Debug.LogError("[CrosswordDatabase] No JSON source assigned!");
                return;
            }

            // Parse JSON
            WordDatabaseRoot root = JsonUtility.FromJson<WordDatabaseRoot>(_jsonSource.text);

            if (root == null || root.words == null)
            {
                Debug.LogError("[CrosswordDatabase] Failed to parse JSON!");
                return;
            }

            _language = root.language ?? "unknown";

            // Normalize and index words
            foreach (var entry in root.words)
            {
                if (string.IsNullOrEmpty(entry.word)) continue;

                // Normalize => Uppercase + Trim
                entry.word = entry.word.Trim().ToUpperInvariant();

                // Clamp difficulty
                entry.difficulty = Mathf.Clamp(entry.difficulty, 1, 9);

                // Ensure list exist
                if (entry.clues == null) entry.clues = new List<string>();
                if (entry.themes == null) entry.themes = new List<string>();

                _allWords.Add(entry);

                // Index by length
                int len = entry.word.Length;
                if (!_byLength.ContainsKey(len))
                    _byLength[len] = new List<WordEntry>();
                _byLength[len].Add(entry);

                // Index by theme
                foreach (string theme in entry.themes)
                {
                    string t = theme.Trim().ToLowerInvariant();
                    if (!_byTheme.ContainsKey(t))
                        _byTheme[t] = new List<WordEntry>();
                    _byTheme[t].Add(entry);
                }
            }

            _isLoaded = true;

            if (_logStats)
            {
                Debug.Log($"[CrosswordDatabase] Loaded {_allWords.Count} words ({_language}). " +
                          $"Lengths: {string.Join(", ", _byLength.Keys.OrderBy(k => k).Select(k => $"{k}:{_byLength[k].Count}"))}. " +
                          $"Themes: {string.Join(", ", _byTheme.Keys.OrderBy(k => k))}.");
            }
        }

        /// <summary>
        /// Auto load if not loaded yet
        /// </summary>
        private void EnsureLoaded()
        {
            Debug.Log($"Loaded: {_isLoaded}");
            if (!_isLoaded) Load();
        }

        // ============================================================
        // Query API
        // ============================================================

        /// <summary>
        /// Get all words of specific length.
        /// </summary>
        /// <param name="length">Length of words</param>
        /// <returns></returns>
        public List<WordEntry> GetWordsByLength(int length)
        {
            EnsureLoaded();
            return _byLength.TryGetValue(length, out var list) ? list : new List<WordEntry>();
        }

        /// <summary>
        /// Get all words of specific length within a difficulty range.
        /// </summary>
        /// <param name="length">Length of words</param>
        /// <param name="minDifficulty">Minimum difficulty</param>
        /// <param name="maxDifficulty">Maximum difficulty</param>
        /// <returns></returns>
        public List<WordEntry> GetWordsByLengthAndDifficulty(int length, int minDifficulty, int maxDifficulty)
        {
            return GetWordsByLength(length)
                .Where(w => w.difficulty >= minDifficulty && w.difficulty <= maxDifficulty)
                .ToList();
        }

        /// <summary>
        /// Get all words matching a specific theme.
        /// </summary>
        /// <param name="theme"></param>
        /// <returns></returns>
        public List<WordEntry> GetWordsByTheme(string theme)
        {
            EnsureLoaded();
            string t = theme.Trim().ToLowerInvariant();
            return _byTheme.TryGetValue(t, out var list) ? list : new List<WordEntry>();
        }

        public List<WordEntry> GetWordsByThemeAndLength(string theme, int length)
        {
            return GetWordsByTheme(theme)
                .Where(w => w.word.Length == length)
                .ToList();
        }

        /// <summary>
        /// Get all available word lengths in the database.
        /// </summary>
        /// <returns></returns>
        public List<int> GetAvailableLengths()
        {
            EnsureLoaded();
            return _byLength.Keys.OrderBy(k =>  k).ToList();
        }

        /// <summary>
        /// Get all available themes in the database.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAvailableThemes()
        {
            EnsureLoaded();
            return _byTheme.Keys.OrderBy(k => k).ToList();
        }

        /// <summary>
        /// Check if the database has enough words of a given length to fill the grid.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="minCount"></param>
        /// <returns></returns>
        public bool HasWordsOfLength(int length, int minCount = 1)
        {
            EnsureLoaded();
            return _byLength.TryGetValue(length, out var list) && list.Count > minCount;
        }
    }
}