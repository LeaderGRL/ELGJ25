using UnityEngine;

namespace Crossatro.Turn
{
    /// <summary>
    /// Published when a new turn begins.
    /// Increment turn number.
    /// </summary>
    public struct TurnStartedEvent
    {
        public int TurnNumber;
    }

    /// <summary>
    /// Published when the game transistions between phases.
    /// </summary>
    public struct PhaseChangedEvent
    {
        public TurnPhase PreviousPhase;
        public TurnPhase NewPhase;
        public int TurnNumber;
    }

    /// <summary>
    /// Published every second during times phases.
    /// </summary>
    public struct TimerTickEvent
    {
        /// <summary>
        /// Remaining time in seconds.
        /// </summary>
        public float TimeRemaining;

        /// <summary>
        /// Total duration for this phases in seconds.
        /// </summary>
        public float TotalDuration;

        /// <summary>
        /// Normalized progress (0 = full, 1 = expired)
        /// </summary>
        public float NormalizedElapsed;
    }

    /// <summary>
    /// Published when the phase timer runs out to force phase transision.
    /// </summary>
    public struct TimerExpieredEvent
    {
        public TurnPhase Phase;
    }

    /// <summary>
    /// Published when the player manually ends their turn.
    /// </summary>
    public struct PlayerPassedEvent
    {
        public int TurnNumber;
        public float TimeRemaining;
    }

    /// <summary>
    /// Published when all enemies have completed their actions.
    /// </summary>
    public struct EnemyPhaseCompletedEvent
    {
        public int TurnNumber;
    }
}
