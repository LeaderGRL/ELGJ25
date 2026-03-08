using UnityEngine;

namespace Crossatro.Board
{
    /// <summary>
    /// Publish when a letter is typed on a selected word on the board.
    /// </summary>
    public struct LetterTypedEvent
    {
        public char Letter;
        public Vector2 TilePosition;
    }
}
