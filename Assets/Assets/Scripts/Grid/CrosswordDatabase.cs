using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
        [System.NonSerialized] private bool _isLoaded = false;

        // Indexes for fast lookup
        private Dictionary<int, List<WordEntry>> _byLength = new Dictionary<int, List<WordEntry>>();
        private Dictionary<string, List<WordEntry>> _byTheme = new Dictionary<string, List<WordEntry>>();

        // ============================================================
        // Data
        // ============================================================

        [HideInInspector] [SerializeField] private List<WordEntry> _bakedWords = new List<WordEntry>();

        // ============================================================
        // State
        // ============================================================

        [HideInInspector] [SerializeField] private string _bakedLanguage = "fr";
        [HideInInspector] [SerializeField] private bool _isBaked = false;

        // ============================================================
        // Properties
        // ============================================================

        public string Language => _language;
        public int TotalWordCount => _allWords.Count;
        public bool IsLoaded => _isLoaded;

        // ============================================================
        // Lifecycle
        // ============================================================

        private void OnEnable()
        {
            _isLoaded = false;
        }

        // ============================================================
        // Loading
        // ============================================================

        /// <summary>
        /// Load and index words from JSON source.
        /// </summary>
        public void Load()
        {
            if (!_isBaked)
            {
                Debug.LogError("[CrosswordDatabase] Database not baked. Click on the 'Bake JSON to Database' on the inspector.");
                return;
            }

            _allWords = new List<WordEntry>(_bakedWords);
            _language = _bakedLanguage;
            _byLength = new Dictionary<int, List<WordEntry>>();
            _byTheme = new Dictionary<string, List<WordEntry>>();

            foreach (var entry in _bakedWords)
            {
                int len = entry.word.Length;

                if (!_byLength.TryGetValue(len, out var lengthList))
                {
                    lengthList = new List<WordEntry>();
                    _byLength[len] = lengthList;
                }
                lengthList.Add(entry);

                foreach (string theme in entry.themes)
                {
                    string t = theme.Trim().ToLowerInvariant();
                    if (string.IsNullOrEmpty(t)) continue;

                    if (!_byTheme.TryGetValue(theme, out var themeList))
                    {
                        themeList = new List<WordEntry>();
                        _byTheme[theme] = themeList;
                    }
                    themeList.Add(entry);
                }
            }

            _isLoaded = true;

            if (_logStats)
            {
                string lengthInfo = string.Join(", ",
                    _byLength.Keys.OrderBy(k => k)
                    .Select(k => k + ":" + _byLength[k].Count));

                string themeInfo = string.Join(", ",
                    _byTheme.Keys.OrderBy(k => k));

                Debug.Log($"[CrosswordDatabase] Loaded {_allWords.Count} words " +
                          $"({_language}). Lengths: {lengthInfo}. " +
                          $"Themes: {themeInfo}.");
            }
        }

        /// <summary>
        /// Auto load if not loaded yet
        /// </summary>
        private void EnsureLoaded()
        {
            if (!_isLoaded) Load();
        }

        // ============================================================
        // Baking
        // ============================================================

#if UNITY_EDITOR
        public void BakeDatabase()
        {
            if (_jsonSource == null)
            {
                Debug.LogError("[CrosswordDatabase] Database not assigned!");
                return;
            }

            WordDatabaseRoot root = JsonUtility.FromJson<WordDatabaseRoot>(_jsonSource.text);

            if (root == null || root.words == null) return;

            _bakedWords.Clear();
            _bakedLanguage = root.language ?? "unknown";

            foreach (var entry in root.words)
            {
                if (string.IsNullOrEmpty(entry.word)) continue; 

                entry.word = entry.word.Trim().ToUpperInvariant();
                entry.difficulty = Mathf.Clamp(entry.difficulty, 1, 9);
                entry.clues = entry.clues ?? new List<string>();
                entry.themes = entry.themes ?? new List<string>();

                for (int i = 0; i < entry.themes.Count; i++)
                    entry.themes[i] = entry.themes[i].Trim().ToLowerInvariant();

                _bakedWords.Add(entry);
            }

            _isBaked = true;

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            Debug.Log($"[CrosswordDatabase] {_bakedWords.Count} words pre calculated and saved.");
        }
#endif

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