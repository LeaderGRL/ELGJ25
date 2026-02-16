using Crossatro.Events;
using DG.Tweening.Core;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Crossatro.Enemy
{
    /// <summary>
    /// Enemy attacking the heart on the grid.
    /// </summary>
    public class Enemy: MonoBehaviour
    {
        // ============================================================
        // Configuration
        // ============================================================

        [Header("Config")]
        [SerializeField] private EnemyData _data;

        [Header("Debug")]
        [SerializeField] private bool _verbosLogging = true;

        // ============================================================
        // Stats
        // ============================================================

        [Header("Stats")]
        [SerializeField] private int _maxHp;
        [SerializeField] private int _currentHp;
        [SerializeField] private int _currentPA;
        [SerializeField] private int _currentPM;
        [SerializeField] private int _currentAttackDamage;

        // ============================================================
        // State
        // ============================================================

        [Header("State")]
        private bool _isDead = false;

        // ============================================================
        // Position
        // ============================================================

        [Header("Position")]
        [SerializeField] private Vector2Int _currentPosition;

        // ============================================================
        // Properties
        // ============================================================

        public EnemyData Data => _data;
        public string Name => _data != null ? _data.Name : "Unknown";
        public int CurrentHp => _currentHp;
        public int MaxHp => _maxHp;
        public Vector2Int GridPosition => _currentPosition;
        public int CurrentAttackDamage => _currentAttackDamage;
        public int CurrentAttackRange => _data != null ? _data.BaseAttackDistance : 1;
        public int CurrentPM => _currentPM;
        public int CurrentPA => _currentPA;
        public float HpPercent => MaxHp > 0 ? (float)_currentHp / MaxHp : 0;

        // ============================================================
        // Initialization
        // ============================================================

        /// <summary>
        /// Initialize this enemy with data and spawn position.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="gridPosition"></param>
        public void Initialize(EnemyData data, Vector2Int gridPosition)
        {
            _data = data;
            _currentHp = data.BaseHp;
            _maxHp = _currentHp;
            _currentPosition = gridPosition;
            _isDead = false;

            ResetTurnResources();

            Log($"Spawned at {gridPosition} ({data.BaseHp} HP, {data.BaseMovementPoint} PM, " + $"{data.BaseActionPoint} PA, range  {data.BaseAttackDistance})");
        }

        // ============================================================
        // Turn lifecycle
        // ============================================================

        /// <summary>
        /// Reset movement and action points at the start of each turn.
        /// </summary>
        public void ResetTurnResources()
        {
            if (_data == null) return;

            _currentPA = _data.BaseActionPoint;
            _currentPM = Mathf.Max(0, _data.BaseMovementPoint);
        }

        // ============================================================
        // Movement
        // ============================================================

        /// <summary>
        /// Move along a path, consuming PM for each step.
        /// </summary>
        /// <param name="path">Full path from current position to target</param>
        private List<Vector2Int> Move(List<Vector2Int> path)
        {
            var moved = new List<Vector2Int>();

            for (int i = 0; i < path.Count && _currentPM > 0; i++)
            {
                Vector2Int nextPos = path[i];
                Vector2Int previousPos = _currentPosition;

                _currentPosition = nextPos;
                _currentPM--;

                moved.Add(nextPos);

                EventBus.Instance.Publish(new EnemyMovedEvent
                {
                    Enemy = this,
                    FromPosition = previousPos,
                    ToPosition = nextPos,
                    RemainingMovement = _currentPM,
                });
            }

            if (moved.Count > 0)
            {
                Log($"Moved {moved.Count} steps to {_currentPosition}. " + $"{_currentPM} PM remaining");
            }

            return moved;
        }

        // ============================================================
        // Attack
        // ============================================================

        /// <summary>
        /// Check if enemy can attack the heart from its current position.
        /// </summary>
        /// <param name="heartPosition">Position of the heart to attack</param>
        private bool CanAttack(Vector2Int heartPosition)
        {
            if (_currentPM <= 0) return false;

            return EnemyPathFinding.IsInRange(_currentPosition, heartPosition, CurrentAttackRange);
        }

        /// <summary>
        /// Execute an attack against the heart.
        /// </summary>
        private int Attack(Vector2Int heartPosition)
        {
            if (_data == null) return 0;

            if (!CanAttack(heartPosition)) return 0;

            _currentPA--;

            EventBus.Instance.Publish(new PlayerDamageEvent
            {
                Damage = _currentAttackDamage,
            });

            Log($"Attacked Heart for {_currentAttackDamage} damage. " + $"{_currentPA} PA remaining");

            return _currentAttackDamage;
        }

        // ============================================================
        // Damage and death
        // ============================================================

        /// <summary>
        /// Apply damage to this enemy.
        /// </summary>
        /// <param name="amount"></param>
        private int TakeDamage(int amount)
        {
            if (_isDead || amount <= 0) return 0;

            _currentHp = Mathf.Max(0, _currentHp - amount);

            EventBus.Instance.Publish(new EnemyDamagedEvent
            {
                Enemy = this,
                Damage = _currentHp,
                RemainingHealth = _currentHp,
            });

            Log($"Took {amount} damage. HP: {_currentHp / _maxHp}");

            if (_currentHp <= 0)
                Die();

            return amount;
        }

        /// <summary>
        /// Handle enemy death
        /// </summary>
        private void Die()
        {
            _isDead = true;

            EventBus.Instance.Publish(new EnemyDeathEvent
            {
                Enemy = this,
                GoldReward = _data.GoldReward,
                GridPosition = _currentPosition,
            });

            Log("Died!");
        }

        // ============================================================
        // Debug
        // ============================================================

        private void Log(string message)
        {
            if (_verbosLogging)
                Debug.Log($"[Enemy:{Name}] {message}");
        }
    }
}
