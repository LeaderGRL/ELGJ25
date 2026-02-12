using UnityEngine;

namespace Crossatro.Turn
{
    /// <summary>
    /// Configuration for the turn system.
    /// </summary>

    [CreateAssetMenu(fileName = "TurnConfig", menuName = "Crossatro/Turn Config")]
    public class TurnConfig: ScriptableObject
    {
        // ============================================================
        // Player Phase
        // ============================================================

        [Header("Player Phase")]
        [Tooltip("Duration of the player turn in seconds")]
        [SerializeField] private float _playerPhaseDuration = 90f;

        [Tooltip("Publish TimerTickEvent every x seconds")]
        [SerializeField] private float _timerTickInterval = 1f;

        // ============================================================
        // Enemy Phase
        // ============================================================

        [Header("Enemy Phase")]
        [Tooltip("Delay between each enemy action in seconds")]
        [SerializeField] private float _enemyActionDelay = 0.5f;

        [Tooltip("Max duration for the enemy phase")]
        [SerializeField] private float _enemyPhaseTimeout = 15f;

        // ============================================================
        // General
        // ============================================================

        [Header("General")]
        [Tooltip("Delay before the first turn starts")]
        [SerializeField] private float _gameStartDelay = 2f;

        [Tooltip("Brief pause between phases")]
        [SerializeField] private float _phaseTransitionDelay = 0.5f;

        // ============================================================
        // Accessors
        // ============================================================

        public float PlayerPhaseDuration => _playerPhaseDuration;
        public float TimerTickInterval => _timerTickInterval;
        public float EnemyActionDelay => _enemyActionDelay;
        public float PhaseTransitionDelay => _phaseTransitionDelay;
        public float GameStartDelay => _gameStartDelay;
        public float EnemyPhaseTimeout => _enemyPhaseTimeout;
    }
}
