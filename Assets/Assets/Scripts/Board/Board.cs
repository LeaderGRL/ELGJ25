using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Crossatro.Board
{
    /// <summary>
    /// Manage the board and tile placement
    /// </summary>
    public class Board: MonoBehaviour
    {
        // ============================================================
        // Configuration
        // ============================================================

        [Header("Animation & Audio")]
        [Tooltip("Shared animation/audio config for tiles")]
        [SerializeField] private TileConfig _tileConfig;

        [Header("Clue Popup")]
        [Tooltip("Popup gameobject repositioned above the selected tile")]
        [SerializeField] private GameObject _cluePopup;

        [Tooltip("Text component inside de popup displaying the clue")]
        [SerializeField] private TextMeshPro _cluePopupText;

        [Tooltip("Height offset above the tile")]
        [SerializeField] private float _popupHeightOffset = 2f;

        // ============================================================
        // Tile storage
        // ============================================================

        // Position -> Tile
        Dictionary<Vector2, Tile> _positionTile = new();

        // Tile -> Position
        Dictionary<Tile, Vector2> _tilePosition = new();

        // ============================================================
        // Animation state
        // ============================================================

        private float _currentSpawnDelay;

        // ============================================================
        // Properties
        // ============================================================

        /// <summary>
        /// Number of tiles currently on the board
        /// </summary>
        public int TileCount => _positionTile.Count;

        /// <summary>
        /// All occupied grid positions.
        /// </summary>
        public IEnumerable<Vector2> OccupiedPositions => _positionTile.Keys;

        private void Awake()
        {
            HideCluePopup();
        }

        // ============================================================
        // Tile Placement
        // ============================================================

        /// <summary>
        /// Place a tile at the given grid position.
        /// If a tile already exist at that position, destroy it and replaced it.
        /// Play a staggered spawn animation.
        /// </summary>
        /// <param name="gridPosition">Grid coordinate for the tile</param>
        /// <param name="tile">Tile instance to place</param>
        public void PlaceTile(Vector2 gridPosition, Tile tile)
        {
            if (tile == null)
            {
                Debug.LogError("[Board] Cannot place a null tile!");
                return;
            }

            // Remove existing tile at this position if any
            RemoveTileAt(gridPosition);

            // Register in both dictionaries for O(1) lookups
            _positionTile[gridPosition] = tile;
            _tilePosition[tile] = gridPosition;

            // Setup tile position and parent
            tile.transform.SetParent(transform);
            tile.SetGridPosition(gridPosition);
            tile.SetState(TileState.Default);

            // Play staggered spawn animation
            PlaySpawnAnimation(tile);
        }

        /// <summary>
        /// Remove the tile at the given position
        /// </summary>
        /// <param name="gridPosition">Position to clear</param>
        public void RemoveTileAt(Vector2 gridPosition)
        {
            if (!_positionTile.TryGetValue(gridPosition, out Tile existingTile))
                return;

            _positionTile.Remove(gridPosition);
            _tilePosition.Remove(existingTile);

            if (existingTile != null && existingTile.gameObject != null)
            {
                existingTile.gameObject.SetActive(false);
                Destroy(existingTile.gameObject);
            }
        }

        /// <summary>
        /// Remove all tiles from the board
        /// </summary>
        public void ClearAllTiles()
        {
            // Destroy all tiles GameObject
            foreach (var tile in _tilePosition.Keys)
            {
                if (tile != null && tile.gameObject != null)
                {
                    Destroy(tile.gameObject);
                }
            }
            _positionTile.Clear();
            _tilePosition.Clear();
            _currentSpawnDelay = 0;

            HideCluePopup();
        }

        // ============================================================
        // Tile lookups
        // ============================================================

        /// <summary>
        /// Get the tile at a grid position.
        /// </summary>
        /// <param name="gridPosition">Tile position</param>
        /// <returns>The tile</returns>
        public Tile GetTileAt(Vector2 gridPosition)
        {
            _positionTile.TryGetValue(gridPosition, out Tile tile);
            return tile;
        }

        /// <summary>
        /// Get the LetterTile at a grid position.
        /// </summary>
        /// <param name="gridPosition">Tile position</param>
        /// <returns>The LetterTile</returns>
        public LetterTile GetLetterTileAt(Vector2 gridPosition)
        {
            return GetTileAt(gridPosition) as LetterTile;
        }

        /// <summary>
        /// Get the grid position of a tile instance
        /// </summary>
        /// <param name="tile">Tile instance</param>
        /// <returns>Grid position or null if tile is not on the board</returns>
        public Vector2? GetPositionOf(Tile tile)
        {
            if (tile != null && _tilePosition.TryGetValue(tile, out Vector2 position))
                return position;

            return null;
        }

        /// <summary>
        /// Check if a position has a tile on it
        /// </summary>
        /// <param name="gridPosition">Position on the grid</param>
        /// <returns></returns>
        public bool HasTileAt(Vector2 gridPosition)
        {
            return _positionTile.ContainsKey(gridPosition);
        }

        public HashSet<Vector2> GetAllTilePosition()
        {
            return _positionTile.Keys.ToHashSet();
        }

        // ============================================================
        // Tile state
        // ============================================================

        /// <summary>
        /// Set the visual state of a tile at the given position.
        /// </summary>
        /// <param name="gridPosition">Position on the grid</param>
        /// <param name="state">State of the tile to set</param>
        public void SetTileState(Vector2 gridPosition, TileState state)
        {
            if (_positionTile.TryGetValue(gridPosition, out Tile tile))
            {
                tile.SetState(state);
            }
        }

        /// <summary>
        /// Set the visual state of multiple tiles at once.
        /// Used for selecting/deselecting all letters of a word.
        /// </summary>
        /// <param name="positions">Position of multiple tiles</param>
        /// <param name="state">State of tiles to set</param>
        public void SetTileStates(IEnumerable<Vector2> positions, TileState state)
        {
            foreach (Vector2 position in positions)
            {
                SetTileState(position, state);
            }
        }

        // ============================================================
        // Clue popup
        // ============================================================

        /// <summary>
        /// Show the clue popup above a specific tile
        /// </summary>
        /// <param name="gridPosition">Position of the tile to show the clue above</param>
        /// <param name="clueText">Clue text to display</param>
        public void ShowCluePopup(Vector2 gridPosition, string clueText)
        {
            if (_cluePopup == null) return;

            // Calculate world position above the tile
            Vector3 tileWorldPos = Tile.GridToWorldPosition(gridPosition);
            _cluePopup.transform.localPosition = new Vector3(tileWorldPos.x, tileWorldPos.y + _popupHeightOffset, tileWorldPos.z);

            // Set text
            if (_cluePopupText != null)
            {
                _cluePopupText.text = clueText ?? "";
            }

            _cluePopup.SetActive(true);
        }

        /// <summary>
        /// Hide the clue popup.
        /// </summary>
        public void HideCluePopup()
        {
            if (_cluePopup != null )
            {
                _cluePopup.SetActive(false);
            }
        }

        // ============================================================
        // Tile animation
        // ============================================================

        /// <summary>
        /// Play the spawn animation on a tile
        /// Uses staggerd delay for sequential tile appearance.
        /// </summary>
        /// <param name="tile"></param>
        private void PlaySpawnAnimation(Tile tile)
        {
            if (_tileConfig == null)
            {
                // just ensure to see tiles.
                tile.transform.localScale = Vector3.one;
                return;
            }

            tile.transform.localScale = Vector3.zero;
            tile.transform.DOScale(Vector3.one, _tileConfig.SpawnDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(_currentSpawnDelay);

            _currentSpawnDelay += _tileConfig.SpawnStaggerDelay;
        }

        public void PlayTileJumpAnimation(Vector2 gridPosition)
        {
            if (_tileConfig == null) return;

            Tile tile = GetTileAt(gridPosition);
            if (tile == null) return;

            DG.Tweening.Sequence jumpSequence = DOTween.Sequence();

            jumpSequence.Append(
                tile.transform.DOBlendableLocalMoveBy(
                    new Vector3(0f, _tileConfig.JumpPower, 0f),
                    _tileConfig.JumpPower / 2f
                ).SetEase(Ease.OutQuad)
            );

            jumpSequence.Append(
                tile.transform.DOBlendableLocalMoveBy(
                    new Vector3(0f, -_tileConfig.JumpPower, 0f),
                    _tileConfig.JumpDuration / 2f
                ).SetEase(Ease.OutQuad)
            );

            PlayTilePopSound();
        }

        /// <summary>
        /// Play the pop sound effect via soundmanager
        /// </summary>
        private void PlayTilePopSound()
        {

        }

        /// <summary>
        /// Reset the spawn delay counter.
        /// Call this before placing a new batch of tiles to restart the staggered animation effects.
        /// </summary>
        public void ResetSpawnDelay()
        {
            _currentSpawnDelay = 0f;
        }

        // ============================================================
        // Raycast Helper
        // ============================================================

        /// <summary>
        /// Raycast from a screen position to find whiche tile is under the cursor.
        /// Only hits tile on interactive layers.
        /// </summary>
        /// <param name="screenPosition">Mouse position in screen space</param>
        /// <param name="gridPosition">output grid position of the hit tile</param>
        /// <param name="tile">output hit tile</param>
        /// <returns></returns>
        public bool RaycastToTile(Vector3 screenPosition, out Vector2 gridPosition, out Tile tile)
        {
            gridPosition = default;
            tile = null;

            Camera cam = Camera.main;
            if ( cam == null ) return false;

            Ray ray = cam.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, TileLayers.InteractionMask))
            {
                Tile hitTile = hit.transform.GetComponent<Tile>();

                if ( hitTile != null && _tilePosition.TryGetValue(hitTile, out Vector2 pos))
                {
                    gridPosition = pos;
                    tile = hitTile;
                    return true;
                }
            }
            return false;
        }

        // ============================================================
        // Letter Display (convenience for BoardController)
        // ============================================================

        /// <summary>
        /// Update the displayed letter on a tile and play the jump animation.
        /// </summary>
        /// <param name="gridPosition">Tile position.</param>
        /// <param name="letter">Character to display.</param>
        public void UpdateTileLetter(Vector2 gridPosition, char letter)
        {
            LetterTile letterTile = GetLetterTileAt(gridPosition);
            if (letterTile == null) return;

            letterTile.SetLetter(letter);
            PlayTileJumpAnimation(gridPosition);
        }
    }
}
