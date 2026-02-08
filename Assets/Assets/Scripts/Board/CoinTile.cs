using UnityEngine;

namespace Crossatro.Board
{
    /// <summary>
    /// A letter tile that grants bonus coins when its word is completed
    /// </summary>
    public class CoinTile : LetterTile
    {
        [Header("Coin Reward")]
        [SerializeField] private int _coinValue = 1;

        /// <summary>
        /// Number of coins awarded when this tile is validated.
        /// </summary>
        public int CoinValue => _coinValue;
    }
}
