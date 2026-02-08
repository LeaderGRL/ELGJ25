using DG.Tweening;
using UnityEngine;

namespace Crossatro.Board
{
    /// <summary>
    /// Base class for tiles placed on the board.
    /// This class is responsible for:
    /// - Track grid position
    /// - Manage visual state
    /// - Provide spawn animation
    /// </summary>
    public class Tile: MonoBehaviour
    {
        // ============================================================
        // Constant
        // ============================================================

        private const float SPAWN_DURATION = 0.5f;
        private const float GRID_CELL_SIZE = 1.2f;

        // ============================================================
        // State
        // ============================================================

        /// <summary>
        /// Grid position of this tile
        /// </summary>
        public Vector2 GridPosition { get; private set; }

        /// <summary>
        /// Current state of this tile
        /// </summary>
        public TileState CurrentState { get; private set; } = TileState.Default;

        // ============================================================
        // Position
        // ============================================================

        /// <summary>
        /// Set this tile grid position and update its world position
        /// </summary>
        /// <param name="gridPosition"></param>
        public void SetGridPosition(Vector2 gridPosition)
        {
            GridPosition = gridPosition;
            transform.localPosition = GridToWorldPosition(GridPosition);
        }

        /// <summary>
        /// Convert grid coordinates to local world position.
        /// Tiles are spaced by GRID_CELL_SIZE on X/Z
        /// </summary>
        /// <param name="gridPost"></param>
        /// <returns></returns>
        public static Vector3 GridToWorldPosition(Vector2 gridPos)
        {
            return new Vector3(gridPos.x * GRID_CELL_SIZE, 0f, gridPos.y * GRID_CELL_SIZE);
        }

        // ============================================================
        // Visual State
        // ============================================================

        /// <summary>
        /// Change the tile's visual state by updating its layer
        /// We use the rendering pipeline to apply different materials according to layers
        /// </summary>
        /// <param name="state"></param>
        public virtual void SetState(TileState state)
        {
            CurrentState = state;
            gameObject.layer = TileLayers.StateToLayer(state);
        }

        /// <summary>
        /// Check if this tile can be interacted.
        /// </summary>
        public bool IsInteractable => CurrentState != TileState.Validated;

        // ============================================================
        // Animation
        // ============================================================

        /// <summary>
        /// Play spawn animation => Scale from 0 to full size + Bounce.
        /// </summary>
        /// <param name="delay"></param>
        public void PlaySpawnAnimation(float delay = 0f)
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, SPAWN_DURATION)
                .SetEase(Ease.OutBack)
                .SetDelay(delay);
        }
    }
}
