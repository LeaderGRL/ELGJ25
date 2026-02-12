using UnityEngine;

namespace Crossatro.Board
{
    /// <summary>
    /// States for board tiles
    /// Each state corresponds to a layer to apply different shaders per state
    /// </summary>
    public enum TileState
    {
        /// <summary>
        /// Default interactive state -> Can be hovered and selected
        /// </summary>
        Default = 0,

        /// <summary>
        /// Mouse is hovering over this tile
        /// </summary>
        Hovered = 1,

        /// <summary>
        /// Tile is part of the selected word
        /// </summary>
        Selected = 2,

        /// <summary>
        /// Tile is validated and lock
        /// </summary>
        Validated = 3,

        /// <summary>
        /// Mouse is hovering over selected tile
        /// </summary>
        HoverSelected = 4,
    }

    public static class TileLayers
    {
        // Layer name constants
        public const string LETTER = "Letter";
        public const string HOVER = "Hover";
        public const string SELECT = "Select";
        public const string VALIDATE = "Validate";
        public const string HOVERSELECTED = "HoverSelected";

        // Cached layer indices
        private static int? _letterLayer;
        private static int? _hoverLayer;
        private static int? _selectedLayer;
        private static int? _validateLayer;
        private static int? _hoverSelelectedLayer;
        private static int? _interactionMask;

        public static int LetterLayer => _letterLayer ??= LayerMask.NameToLayer(LETTER);
        public static int HoverLayer => _hoverLayer ??= LayerMask.NameToLayer(HOVER);
        public static int SelectLayer => _selectedLayer ??= LayerMask.NameToLayer(SELECT);
        public static int ValidateLayer => _validateLayer ??= LayerMask.NameToLayer(VALIDATE);
        public static int HoverSelectedLayer => _hoverSelelectedLayer ??= LayerMask.NameToLayer(HOVERSELECTED);

        /// <summary>
        /// Combined mask for interactive tiles
        /// Used for raycasting to detect clickable tiles
        /// </summary>
        public static int InteractionMask => _interactionMask ??= LayerMask.GetMask(LETTER, HOVER, SELECT, HOVERSELECTED);

        /// <summary>
        /// Convert a TileState to its corresponding layer index.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static int StateToLayer(TileState state)
        {
            return state switch
            {
                TileState.Default => LetterLayer,
                TileState.Hovered => HoverLayer,
                TileState.Selected => SelectLayer,
                TileState.Validated => ValidateLayer,
                TileState.HoverSelected => HoverSelectedLayer,
                _ => LetterLayer,
            };
        }

        /// <summary>
        /// Convert layer index to a TileState.
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static TileState LayerToState(int layer)
        {
            if (layer == HoverLayer) return TileState.Hovered;
            if (layer == SelectLayer) return TileState.Selected;
            if (layer == ValidateLayer) return TileState.Validated;
            if (layer == HoverSelectedLayer) return TileState.HoverSelected;
            return TileState.Default;
        }
    }
}
