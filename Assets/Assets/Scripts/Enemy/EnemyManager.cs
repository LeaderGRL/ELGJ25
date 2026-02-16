using Crossatro.Turn;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crossatro.Enemy
{
    /// <summary>
    /// 
    /// </summary>
    public class EnemyManager: MonoBehaviour
    {
        // ============================================================
        // Configuration
        // ============================================================

        private WaveConfig _waveConfig;

        // ============================================================
        // References
        // ============================================================

        private List<Enemy> _enemies;

        // ============================================================
        // Lifecycle
        // ============================================================

        private void Awake()
        {
            _enemies = new List<Enemy>();
        }
        private void SubscribeToEvents()
        {
            EventBus.Instance.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Instance.Unsubscribe<PhaseChangedEvent>(OnPhaseChanged);
        }
        private IEnumerator Spawn(EnemyData enemy)
        {
            yield return _waveConfig.SpawnDelay;

            // Instantiate

            // Initialize

            // Add to enemies list
        }

        // ============================================================
        // Event handler
        // ============================================================

        private void OnPhaseChanged(PhaseChangedEvent evt)
        {
            if (evt.NewPhase != TurnPhase.EnemyPhase) return;

            StartEnemyTurn(evt.TurnNumber);


        }

        private void StartEnemyTurn(int turnNumber)
        {
            if (_waveConfig == null) return;

            var spawnEntries = _waveConfig.GetSpawnForTurn(turnNumber);
            
            if (spawnEntries != null || spawnEntries.Count > 0)
            {
                foreach (var spawnEntry in spawnEntries)
                {
                    for (var i = 0; i < spawnEntry.Count; i++)
                    {
                        Spawn(spawnEntry.EnemyData);
                    }
                }
            }

            foreach (var enemy in _enemies)
            {

            }


        }

        //private IEnumerator ProcessSingleEnemy(Enemy enemy)
        //{
        //    var path = EnemyPathFinding.FindPath(enemy.GridPosition, )
        //}



    }
}
