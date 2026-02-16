using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace Crossatro.Enemy
{
    /// <summary>
    /// Define esnmy spawn waves for a grid.
    /// </summary>
    [CreateAssetMenu(fileName = "WaweConfig", menuName = "Wave Config")]
    public class WaweConfig: ScriptableObject
    {
        // ============================================================
        // Wave definitions
        // ============================================================

        [Header("Waves")]
        [Tooltip("List of waves. Each wave triggers at its specified turn")]
        [SerializeField] private List<Wave> _waves = new List<Wave>();

        [Header("Endless Mode")]
        [Tooltip("If true, repeat the last wave every N turn after all waves done")]
        [SerializeField] private bool _repeatLastWave = true;

        [Tooltip("How often to repeat the last wave")]
        [SerializeField] private int _repeatInterval = 5;

        // ============================================================
        // Public accessors
        // ============================================================

        public IReadOnlyList<Wave> Waves => _waves;
        public bool RepeatLastWave => _repeatLastWave;
        public int RepeatInterval => _repeatInterval;

        /// <summary>
        /// Get all spawn entries for a turn.
        /// </summary>
        /// <param name="turnNumber"></param>
        /// <returns></returns>
        public List<SpawnEntry> GetSpawnForTurn(int turnNumber)
        {
            var spawns = new List<SpawnEntry>();

            foreach (var wave in _waves)
            {
                if (wave.TurnNumber == turnNumber)
                    spawns.AddRange(wave.Spawns);
            }

            // Check endless repeat
            if (spawns.Count == 0 && _repeatLastWave && _waves.Count > 0)
            {
                Wave lastWave = _waves[_waves.Count - 1];
                if (turnNumber > lastWave.TurnNumber)
                {
                    int turnsSinceLast = turnNumber - lastWave.TurnNumber;
                    if (turnsSinceLast % _repeatInterval == 0)
                        spawns.AddRange(lastWave.Spawns);
                }
            }

            return spawns;
        }
    }

    /// <summary>
    /// A signle wave of enemies that spawns at a specific turn.
    /// </summary>
    [Serializable]
    public class Wave
    {
        [Tooltip("Turn number when this wave spawns")]
        public int TurnNumber;

        [Tooltip("Enemies to spawn in this wave")]
        public List<SpawnEntry> Spawns = new List<SpawnEntry>();
    }
    
    /// <summary>
    /// A single enemy spawn within a wave
    /// </summary>
    [Serializable]
    public class SpawnEntry
    {
        [Tooltip("Enemy type to spawn")]
        public EnemyData EnemyData;

        [Tooltip("Number of this enemy type to spawn")]
        public int Count = 1;
    }
}
