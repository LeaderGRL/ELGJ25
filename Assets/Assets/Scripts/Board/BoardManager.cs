using Crossatro.Enemy;
using Crossatro.Grid;
using Unity.Collections;
using UnityEngine;

namespace Crossatro.Board
{
    /// <summary>
    /// Generate a crossword grid, create tiles and wires all components.
    /// </summary>
    public class BoardManager: MonoBehaviour
    {
        // ============================================================
        // References
        // ============================================================

        [Header("Board Components")]
        [SerializeField] private Board _board;
        [SerializeField] private BoardInputHandler _inputHandler;
        [SerializeField] private BoardController _boardController;
        [SerializeField] private TileFactory _tileFactory;
        [SerializeField] private EnemyManager _enemyManager;

        [Header("Grid Generation")]
        [Tooltip("Scriptable object with word database and generation settings")]
        [SerializeField] private GridGenerationData _gridGenerationData;

        [Header("UI")]
        [Tooltip("InputField for typing words")]
        [SerializeField] private TMPro.TMP_InputField _inputField;

        [Header("Debug")]
        [Tooltip("Log detailled information during setup")]
        [SerializeField] private bool _verboseLogging = true;

        [Tooltip("Seed for grid generation. -1 for random.")]
        [SerializeField] private int _seed = -1;

        // ============================================================
        // State
        // ============================================================

        private CrossWordsGameGrid _grid;

        // ============================================================
        // Lifecycle
        // ============================================================

        private void Start()
        {
            // Ensure core services exist
            _ = EventBus.Instance;

            Log("Starting Board setup...");

            if (!ValidateReferences()) return;

            GenerateGrid();

            PopulateBoardWithTiles();

            _inputHandler.SetBoard(_board);

            _boardController.Initialize(_grid);

            _enemyManager.Initialize(_board.GetAllTilePosition());

            Log($"Board ready! {_grid.Words.Count} words generated.");
            LogWordList();
        }

        // ============================================================
        // Setup
        // ============================================================

        /// <summary>
        /// Generate crossword grid data using CrosswordGridBuilder.
        /// </summary>
        private void GenerateGrid()
        {
            Log("Generating crossword grid..");

            var builder = new CrosswordGridBuilder(_seed);

            _grid = builder.Build(_gridGenerationData.Database, _gridGenerationData.NumWordsToGenerate);
        }

        
        /// <summary>
        /// Create a tile for each letter position in the grid and place it on the board.
        /// </summary>
        private void PopulateBoardWithTiles()
        {
            Log("Placing tiles on board..");

            _board.ResetSpawnDelay();

            foreach (GridWord word in _grid.Words)
            {
                var letterPosition = word.GetAllLetterSolutionPositions();

                foreach (var kvp in letterPosition)
                {
                    Vector2 position = kvp.Key;

                    // Skip if a tile already exist at this position
                    if (_board.HasTileAt(position)) continue;

                    // Create tile via factory
                    LetterTile tile = _tileFactory.CreateTile(_board.transform);
                    if (tile == null) continue;

                    // Place on board
                    _board.PlaceTile(position, tile);
                }
            }

            Log($"Place {_board.TileCount} tiles on the board.");
        }

        // ============================================================
        // Validation
        // ============================================================

        /// <summary>
        /// Check all required references are assigned.
        /// </summary>
        /// <returns></returns>
        private bool ValidateReferences()
        {
            bool valid = true;

            if (_board == null)
            {
                Debug.LogError("[BoardTest] Board is not assigned!");
                valid = false;
            }

            if (_inputHandler == null)
            {
                Debug.LogError("[BoardTest] BoardInputHandler is not assigned!");
                valid = false;
            }

            if (_boardController == null)
            {
                Debug.LogError("[BoardTest] BoardController is not assigned!");
                valid = false;
            }

            if (_tileFactory == null)
            {
                Debug.LogError("[BoardTest] TileFactory is not assigned!");
                valid = false;
            }

            if (_gridGenerationData == null)
            {
                Debug.LogError("[BoardTest] GridGenerationData is not assigned!");
                valid = false;
            }

            if (_gridGenerationData != null && _gridGenerationData.Database == null)
            {
                Debug.LogError("[BoardTest] GridGenerationData has no database loaded! " +
                               "Click 'Load Database' on the ScriptableObject.");
                valid = false;
            }

            if (_inputField == null)
            {
                Debug.LogWarning("[BoardTest] InputField not assigned. " +
                                 "Word typing will not work.");
            }

            return valid;
        }

        // ============================================================
        // Debug
        // ============================================================

        private void Log(string message)
        {
            if (_verboseLogging)
            {
                Debug.Log($"[BoardManager] {message}");
            }
        }

        /// <summary>
        /// Print all generated words and their positions for debugging.
        /// </summary>
        private void LogWordList()
        {
            if (!_verboseLogging) return;

            Debug.Log("=== Generated Words ===");
            foreach (GridWord word in _grid.Words)
            {
                string direction = word.IsRow ? "horizontal" : "vertical";
                Debug.Log($"  {direction} \"{word.SolutionWord}\" " +
                          $"at ({word.StartPosition.x}, {word.StartPosition.y}) " +
                          $"[Diff: {word.Difficulty}] " +
                          $"Clue: {word.Description}");
            }
            Debug.Log("=======================");
        }

    }
}
