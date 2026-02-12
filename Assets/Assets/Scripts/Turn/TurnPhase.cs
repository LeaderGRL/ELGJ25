using UnityEngine;

namespace Crossatro.Turn
{
    /// <summary>
    /// Phases of the game loop.
    /// </summary>
    public enum TurnPhase
    {
        None,
        PlayerPhase,
        EnemyPhase,
        GameOver,
    }
}
