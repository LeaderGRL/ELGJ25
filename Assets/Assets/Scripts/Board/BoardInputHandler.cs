using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Crossatro.Board
{
    /// <summary>
    /// Input handler for the board.
    /// Detect hover and clicks, manage visuals and fire events.
    /// </summary>
    public class BoardInputHandler: MonoBehaviour
    {
        // ============================================================
        // Configuration
        // ============================================================

        [Header("References")]
        [SerializeField] private Board _board;

        [Header("Input")]
        [SerializeField] private InputActionReference _selectAction;

        // ============================================================
        // Events
        // ============================================================

        /// <summary>
        /// Fired when the player click on a tile.
        /// </summary>
        public event Action<Vector2> OnTileClicked;

        /// <summary>
        /// Fire when the player newly hovering a tile.
        /// </summary>
        public event Action<Vector2, Tile> OnTileHoverEnter;

        /// <summary>
        /// Fire when the player move his cursor to an empty space.
        /// </summary>
        public event Action OnTileHoverExit;

        // ============================================================
        // State
        // ============================================================

        /// <summary>
        /// Currently hovered tile position.
        /// </summary>
        private Vector2? _currentHoverPosition;

        /// <summary>
        /// Track if input is enabled.
        /// </summary>
        private bool _inputEnabled = true;

        private void OnEnable()
        {
            if (_selectAction != null)
            {
                _selectAction.action.Enable();
                _selectAction.action.performed += HandleSelectPerformed;
            }
        }

        private void OnDisable()
        {
            if (_selectAction != null)
            {
                _selectAction.action.performed -= HandleSelectPerformed;
            }

            ClearHover();
        }

        private void Update()
        {
            if (!_inputEnabled || _board == null) return;

            UpdateHover();
        }

        // ============================================================
        // Hover Detection
        // ============================================================

        /// <summary>
        /// Check what tile is under the cursor and update hover state.
        /// </summary>
        private void UpdateHover()
        {
            if (_board.RaycastToTile(Input.mousePosition, out Vector2 hitPosition, out Tile hitTile))
            {
                // Only process if tile is interactable
                if (!hitTile.IsInteractable)
                {
                    ClearHover();
                    return;
                }

                // Did we move to a new tile ?
                if (_currentHoverPosition == null || _currentHoverPosition.Value != hitPosition)
                {
                    // Clear previous hover
                    ClearHoverVisual();

                    // Set new hover
                    _currentHoverPosition = hitPosition;
                    _board.SetTileState(hitPosition, TileState.Hovered);

                    OnTileHoverEnter?.Invoke(hitPosition, hitTile);
                }
            }
            else
            {
                // Cursor is not hover any tile
                if (_currentHoverPosition != null)
                {
                    ClearHover();
                }
            }
        }

        // ============================================================
        // Click Detection
        // ============================================================

        /// <summary>
        /// Called when the select input action is performed.
        /// </summary>
        /// <param name="callbackContext"></param>
        private void HandleSelectPerformed(InputAction.CallbackContext callbackContext)
        {
            if (!_inputEnabled || !callbackContext.performed) return;

            // Only fire click if we're currently hovering a tile
            if (_currentHoverPosition.HasValue)
            {
                OnTileClicked?.Invoke(_currentHoverPosition.Value);
            }
        }

        // ============================================================
        // Hover Visual Management
        // ============================================================

        /// <summary>
        /// Remove hover visual from the current tile and clear hover state.
        /// </summary>
        private void ClearHover()
        {
            ClearHoverVisual();
            _currentHoverPosition = null;
            OnTileHoverExit?.Invoke();
        }

        /// <summary>
        /// Reset the visual state of the currently hovered tile.
        /// </summary>
        private void ClearHoverVisual()
        {
            if (_currentHoverPosition.HasValue)
            {
                _board.SetTileState(_currentHoverPosition.Value, TileState.Default);
            }
        }

        // ============================================================
        // API
        // ============================================================

        /// <summary>
        /// Initialize with a board reference.
        /// </summary>
        /// <param name="board"></param>
        public void SetBoard(Board board)
        {
            _board = board;
        }

        /// <summary>
        /// Enable or disable input processing.
        /// </summary>
        /// <param name="enabled"></param>
        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;

            if (!enabled)
            {
                ClearHover();
            }
        }

        /// <summary>
        /// The grid position currently being hovered.
        /// </summary>
        public Vector2? CurrentHoverPosition => _currentHoverPosition;
    }
}
