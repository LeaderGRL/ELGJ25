using UnityEngine;

namespace Crossatro.Board
{
    /// <summary>
    /// Create tile instance for the board.
    /// </summary>
    public class TileFactory: MonoBehaviour
    {
        // ============================================================
        // Configuration
        // ============================================================

        [Header("Prefabs")]
        [SerializeField] private LetterTile _letterTilePrefab;
        [SerializeField] private CoinTile _coinTilePrefab;
        [SerializeField] private HeartTile _heartTilePrefab;

        [Header("Tile Settings")]
        [Tooltip("Percentage chance that a tile will be a CoinTile")]
        [SerializeField] private int _coinTileChance;

        // ============================================================
        // API
        // ============================================================

        /// <summary>
        /// Instantiate tiles => Not placed !
        /// </summary>
        /// <param name="parent">Parent hierarchy</param>
        /// <returns>Created tile instance</returns>
        public LetterTile CreateTile(Transform parent)
        {
            // TODO: Update later if there is more special tile and create a function for them all
            bool isCoinTile = _coinTilePrefab != null && Random.Range(1, 101) <= _coinTileChance;

            LetterTile tilePrefab = isCoinTile ? _coinTilePrefab : _letterTilePrefab;

            if (tilePrefab == null)
            {
                Debug.LogError("[TileFactory] Letter tile prefab is not assigned!");
                return null;
            }

            LetterTile tile = Instantiate(tilePrefab, parent);
            tile.ClearLetter();
            return tile;
        }

        /// <summary>
        /// Instantiate LetterTile => Not placed !
        /// </summary>
        /// <param name="parent">Parent hierarchy</param>
        /// <returns>Created LetterTile instance</returns>
        public LetterTile CreateLetterTile(Transform parent)
        {
            if (_letterTilePrefab == null)
            {
                Debug.LogError("[TileFactory] Letter tile prefab is not assigned!");
                return null;
            }

            LetterTile letterTile = Instantiate(_letterTilePrefab, parent);
            letterTile.ClearLetter();
            return letterTile;
        }

        public HeartTile CreateHeartTile(Transform parent)
        {
            if (_heartTilePrefab == null)
            {
                Debug.LogError("[TileFactory] Heart tile prefab is not assigned!");
                return null;
            }

            HeartTile heartTile = Instantiate(_heartTilePrefab, parent);
            return heartTile;
        }



    }
}