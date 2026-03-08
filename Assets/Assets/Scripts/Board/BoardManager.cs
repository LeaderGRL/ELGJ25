using Crossatro.Enemy;
using Crossatro.Grid;
using Unity.Collections;
using UnityEngine;

namespace Crossatro.Board
{
    /// <summary>
    /// How the crossword grid is created.
    /// </summary>
    public enum GenerationMode
    {
        /// <summary>Auto generate a new random mask each time.</summary>
        Random,
        /// <summary>Use a custom GridMask asset.</summary>
        Custom,
    }

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

        // ============================================================
        // Configuration
        // ============================================================

        [Header("Grid Generation")]
        [Tooltip("Scriptable object with word database and generation settings")]
        [SerializeField] private CrosswordDatabase _wordDatabase;

        [Tooltip("Random = auto generate mask each time. Custom = use the mask below.")]
        [SerializeField] private GenerationMode _generationMode = GenerationMode.Random;

        [Tooltip("Custom grid mask")]
        [SerializeField] private GridMask _mask;

        [Header("Grid Size")]
        [SerializeField] private int _gridWidth = 9;
        [SerializeField] private int _gridHeight = 9;

        [Tooltip("Optional theme filter (empty = any theme)")]
        [SerializeField] private string _theme = "";

        [Header("Difficulty")]
        [Tooltip("Difficulty range of the words")]
        [SerializeField] private int _minDifficulty = 1;
        [SerializeField] private int _maxDifficulty = 9;

        [Tooltip("Seed for grid generation. -1 for random.")]
        [SerializeField] private int _seed = -1;
        [SerializeField][Range(0, 1)] private float _blackRatio;

        // ============================================================
        // UI
        // ============================================================

        [Header("UI")]
        [Tooltip("InputField for typing words")]
        [SerializeField] private TMPro.TMP_InputField _inputField;

        // ============================================================
        // Debug
        // ============================================================

        [Header("Debug")]
        [Tooltip("Log detailled information during setup")]
        [SerializeField] private bool _verboseLogging = true;

        // ============================================================
        // State
        // ============================================================

        private CrossWordsGameGrid _grid;

        private CrosswordGridBuilder.BuildResult _buildResult;

        // ============================================================
        // Properties
        // ============================================================

        /// <summary>
        /// Heart position in word coordinates.
        /// </summary>
        public Vector2 HeartPosition => _buildResult?.HeartPosition ?? Vector2.zero;

        /// <summary>
        /// Generated or predifined mask.
        /// </summary>
        public GridMask Mask => _buildResult?.Mask;

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

            _enemyManager.Initialize(_board.GetAllTilePosition(), _buildResult.HeartPosition);

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
            builder.SetDebugMode(_verboseLogging);

            string theme = string.IsNullOrWhiteSpace(_theme) ? null : _theme;

            if (_generationMode == GenerationMode.Custom && _mask != null)
            {
                // Use predefined mask
                Log($"Using custom mask: {_mask.Width}x{_mask.Height}");
                _buildResult = builder.Build(_mask, _wordDatabase, _minDifficulty, _maxDifficulty, theme);
            }
            else
            {
                if (_generationMode == GenerationMode.Custom && _mask == null)
                    Debug.LogWarning("[BoardManager] Custom mode selected but no mask assigned! " +
                                     "Falling back to random generation.");

                // Generate random mask
                Log($"Generating random {_gridWidth}x{_gridHeight} grid " + $"(black ratio: {_blackRatio})");
                _buildResult = builder.BuildRandom(_wordDatabase, _gridWidth, _gridHeight, _minDifficulty, _maxDifficulty, theme, _blackRatio);
            }

            _grid = _buildResult.Grid;
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

            PlaceHeartTile();

            Log($"Place {_board.TileCount} tiles on the board.");
        }

        /// <summary>
        /// Place a dedicated heart tile at the computed position.
        /// </summary>
        private void PlaceHeartTile()
        {
            if (_buildResult == null) return;

            Vector2 heartPos = _buildResult.HeartPosition;

            // Skip if already occupied (shouldn't happen with proper mask)
            if (_board.HasTileAt(heartPos))
            {
                Debug.LogWarning($"[BoardManager] Heart position {heartPos} is already occupied!");
                return;
            }

            HeartTile heartObj = _tileFactory.CreateHeartTile(_board.transform);
            if (heartObj == null) return;
            heartObj.name = "HeartTile";
           
            _board.PlaceTile(heartPos, heartObj);
            _enemyManager.AddObstaclePosition(heartPos); // Enemies can't walk on heartTile

            Log($"Heart tile placed at {heartPos}");
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

            if (_wordDatabase == null)
            {
                Debug.LogError("[BoardManager] CrosswordDatabase is not assigned!");
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
