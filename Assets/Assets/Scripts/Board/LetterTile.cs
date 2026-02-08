using TMPro;
using UnityEngine;

namespace Crossatro.Board
{
    /// <summary>
    /// A tile that display a single letter on the board.
    /// This tile is reponsible for:
    /// - Display a letter character
    /// - Show/Hide a clue popup/ui
    /// - Play visual feedback
    /// </summary>
    public class LetterTile: Tile
    {
        // ============================================================
        // Configuration
        // ============================================================

        [Header("Display")]
        [SerializeField] private TextMeshProUGUI _displayText;

        // ============================================================
        // API
        // ============================================================

        /// <summary>
        /// The TextMeshPro component displaying the letter
        /// </summary>
        public TextMeshProUGUI DisplayText => _displayText;

        /// <summary>
        /// Set the displayed letter on this tile.
        /// </summary>
        /// <param name="letter">Character to display</param>
        public void SetLetter(char letter)
        {
            if (_displayText == null) return;
            _displayText.text = letter == '\0' ? "" : letter.ToString();
        }

        /// <summary>
        /// Get the currently displayed letter
        /// </summary>
        public char GetLetter()
        {
            if (_displayText == null || string.IsNullOrEmpty(_displayText.text))
                return '\0';
            return _displayText.text[0];
        }

        /// <summary>
        /// Cleat the displayed letter.
        /// </summary>
        public void ClearLetter()
        {
            SetLetter('\0');
        }
    }
}
