using UnityEngine;

namespace Crossatro.Board
{
    [CreateAssetMenu(fileName = "TileAnimationConfig", menuName = "Crossatro/Tile Animation Config")]
    public class TileConfig: ScriptableObject
    {
        [Header("Size")]
        [Tooltip("Size of a cell")]
        [SerializeField] private float _gridCellSize;

        [Header("Spawn Animation")]
        [Tooltip("Duration of the scale animation when tile appears")]
        [SerializeField] private float _spawnDuration = 0.5f;

        [Tooltip("Delay increment between each tile spawn")]
        [SerializeField] private float _spawnStaggerDelay = 0.05f;

        [Header("Jump Animation")]
        [Tooltip("Height of the bounce when a letter is typed")]
        [SerializeField] private float _jumpPower = 1f;

        [Tooltip("Total duration of the jump animation")]
        [SerializeField] private float _jumpDuration = 0.2f;

        [Header("Audio")]
        [Tooltip("Sound effect play on letter input")]
        [SerializeField] private AudioClip _popSfx;

        [Tooltip("Pitch randomization range for the pop sound")]
        [SerializeField] private float _minPitch = 0.8f;
        [SerializeField] private float _maxPitch = 1.2f;

        // ============================================================
        // Public Accessors
        // ============================================================

        public float GridCellSize => _gridCellSize;
        public float SpawnDuration => _spawnDuration;
        public float SpawnStaggerDelay => _spawnStaggerDelay;
        public float JumpPower => _jumpPower;
        public float JumpDuration => _jumpDuration;
        public AudioClip PopSfx => _popSfx;
        public float MinPitch => _minPitch;
        public float MaxPitch => _maxPitch;
    }
}
