using Crossatro.Enemy;
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
        // Result
        // ============================================================

        /// <summary>
        /// Result of a crossword build operation.
        /// </summary>
        public class BuildResult
        {
            /// <summary>The built crossword grid with all words.</summary>
            public CrossWordsGameGrid Grid;

            /// <summary>Heart position in Unity world coordinates.</summary>
            public Vector2 HeartPosition;

            /// <summary>The mask that was used.</summary>
            public GridMask Mask;

            /// <summary>All solved slots with their words.</summary>
            public List<Slot> Slots;

            /// <summary>True if the solver filled all slots.</summary>
            public bool IsFullySolved;

            /// <summary>Number of words placed.</summary>
            public int WordCount;
        }

        // ============================================================
        // State
        // ============================================================

        private readonly int _seed;
        private readonly System.Random _rng;

        /// <summary>
        /// Grid of already placed letters.
        /// </summary>
        private Dictionary<Vector2, char> _placedLetters;

        /// <summary>
        /// Words successfully placed to the grid.
        /// </summary>
        private List<GridWord> _placedWords;

        /// <summary>
        /// Heart position => dedicated tile.
        /// </summary>
        private Vector2 _heartPosition;
        private bool _heartPositionComputed;

        // ============================================================
        // Constructor
        // ============================================================

        /// <summary>
        /// Create a new grid builder.
        /// </summary>
        /// <param name="seed">Optional seed for specific grid acess</param>
        public CrosswordGridBuilder(int seed = -1)
        {
            _seed = seed;
            _rng = seed >= 0 ? new System.Random(seed) : new System.Random();
        }

        // ============================================================
        // API
        // ============================================================

        /// <summary>
        /// Build a crossword using a predefined mask.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="database"></param>
        /// <param name="minDifficulty"></param>
        /// <param name="maxDifficulty"></param>
        /// <param name="theme"></param>
        /// <returns></returns>
        public BuildResult Build(GridMask mask, CrosswordDatabase database, int minDifficulty = 1, int maxDifficulty = 9, string theme = null)
        {
            if (mask == null)
            {
                Debug.LogError("[CrosswordGridBuilder] Mask is null!");
                return CreateEmptyResult();
            }

            if (database == null)
            {
                Debug.LogError("[CrosswordGridBuilder] Database is null!");
                return CreateEmptyResult();
            }

            // Ensure databse is loaded
            if (!database.IsLoaded) database.Load();

            // Extract slot from mask
            List<Slot> slots = mask.FindSlots();
            Debug.Log($"[CrosswordGridBuilder] Mask {mask.Width}x{mask.Height}, " +
                $"{slots.Count} slots found.");

            if (slots.Count == 0)
            {
                Debug.LogWarning("[CrosswordGridBuilder] No slots found in mask!");
                return CreateEmptyResult(mask);
            }

            // Check feasability
            CheckFeasibility(slots, database, minDifficulty, maxDifficulty, theme);

            // Solve
            var solver = new CrosswordSolver(_seed);
            bool solved = solver.Solve(slots, database, minDifficulty, maxDifficulty, theme);

            // Build result
            return CreateResult(mask, slots, database, solved);
        }

        // <summary>
        /// Build a crossword using a randomly generated mask.
        /// </summary>
        /// <param name="database">Word database</param>
        /// <param name="width">Grid width</param>
        /// <param name="height">Grid height</param>
        /// <param name="minDifficulty">Minimum word difficulty</param>
        /// <param name="maxDifficulty">Maximum word difficulty</param>
        /// <param name="theme">Optional theme filter</param>
        /// <param name="blackRatio">Ratio of black cells</param>
        public BuildResult BuildRandom(
            CrosswordDatabase database,
            int width = 9,
            int height = 9,
            int minDifficulty = 1,
            int maxDifficulty = 9,
            string theme = null,
            float blackRatio = 0.25f)
        {
            // Generate a random mask
            GridMask mask = MaskGenerator.Generate(
                width, height,
                seed: _seed,
                blackRatio: blackRatio
            );

            return Build(mask, database, minDifficulty, maxDifficulty, theme);
        }

        // ============================================================
        // Result building
        // ============================================================

        /// <summary>
        /// Convert solved slots into a CrossWordsGameGrid + metadata.
        /// Maps each filled slot to a GridWord with position, direction, and word data.
        /// </summary>
        private BuildResult CreateResult(
            GridMask mask, List<Slot> slots,
            CrosswordDatabase database, bool fullySolved)
        {
            var gridWords = new List<GridWord>();
            int filledCount = 0;

            foreach (var slot in slots)
            {
                if (string.IsNullOrEmpty(slot.PlacedString)) continue;
                filledCount++;

                // Find the WordEntry for this word (to get clues, difficulty, etc.)
                WordEntry entry = FindWordEntry(slot.PlacedString, slot.Length, database);

                // Create GridWord
                GridWord gridWord = new GridWord();
                gridWord.SolutionWord = slot.PlacedString;
                gridWord.StartPosition = slot.GetCellWorldPosition(0);
                gridWord.IsRow = slot.IsHorizontal;
                gridWord.Difficulty = entry != null ? entry.difficulty : 5;

                // Pick a random clue if multiple are available
                if (entry != null && entry.clues != null && entry.clues.Count > 0)
                {
                    int clueIndex = _rng.Next(entry.clues.Count);
                    gridWord.Description = entry.clues[clueIndex];
                }
                else
                {
                    gridWord.Description = $"Mot de {slot.Length} lettres";
                }

                gridWord.Initialize();

                // Link back to slot
                slot.PlacedWord = entry;

                gridWords.Add(gridWord);
            }

            // Heart position
            //Vector2 heartPos = mask.HasHeart
            //    ? mask.GetHeartWorldPosition()
            //    : Vector2.zero;

            //Debug.Log($"[CrosswordGridBuilder] Built grid: {filledCount}/{slots.Count} words. " +
            //          $"Heart at {heartPos}. Fully solved: {fullySolved}");

            Vector2 heartPos = mask.GetHeartPosition();

            return new BuildResult
            {
                Grid = new CrossWordsGameGrid(gridWords),
                HeartPosition = heartPos,
                Mask = mask,
                Slots = slots,
                IsFullySolved = fullySolved,
                WordCount = filledCount
            };
        }

        /// <summary>
        /// Find the WordEntry matching a placed string.
        /// Used to recover clues, difficulty, and themes.
        /// </summary>
        private WordEntry FindWordEntry(string word, int length, CrosswordDatabase database)
        {
            var candidates = database.GetWordsByLength(length);
            return candidates.FirstOrDefault(
                w => w.word.Equals(word, System.StringComparison.OrdinalIgnoreCase));
        }

        private BuildResult CreateEmptyResult(GridMask mask = null)
        {
            return new BuildResult
            {
                Grid = new CrossWordsGameGrid(new List<GridWord>()),
                HeartPosition = Vector2.zero,
                Mask = mask,
                Slots = new List<Slot>(),
                IsFullySolved = false,
                WordCount = 0
            };
        }

        // ============================================================
        // Feasibility Check
        // ============================================================

        /// <summary>
        /// Check if the database has enough words to potentially fill the grid.
        /// </summary>
        private void CheckFeasibility(
            List<Slot> slots, CrosswordDatabase database,
            int minDiff, int maxDiff, string theme)
        {
            var lengthCounts = new Dictionary<int, int>();

            foreach (var slot in slots)
            {
                if (!lengthCounts.ContainsKey(slot.Length))
                    lengthCounts[slot.Length] = 0;
                lengthCounts[slot.Length]++;
            }

            foreach (var kvp in lengthCounts.OrderBy(k => k.Key))
            {
                int available;
                if (!string.IsNullOrEmpty(theme))
                {
                    available = database.GetWordsByThemeAndLength(theme, kvp.Key).Count;
                    // Include general pool as fallback
                    available += database.GetWordsByLengthAndDifficulty(
                        kvp.Key, minDiff, maxDiff).Count;
                }
                else
                {
                    available = database.GetWordsByLengthAndDifficulty(
                        kvp.Key, minDiff, maxDiff).Count;
                }

                if (available < kvp.Value)
                {
                    Debug.LogWarning($"[CrosswordGridBuilder] Need {kvp.Value} words of length {kvp.Key}, " +
                                     $"but only {available} available (difficulty {minDiff}-{maxDiff}" +
                                     $"{(theme != null ? $", theme '{theme}'" : "")}). " +
                                     "Solver may struggle or fail.");
                }
            }
        }
    }
}