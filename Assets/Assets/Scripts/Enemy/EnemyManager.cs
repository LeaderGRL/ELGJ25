using Crossatro.Turn;
using Mono.Cecil;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crossatro.Enemy
{
    /// <summary>
    /// Manage all enemies and their phases.
    /// 
    /// - Enemies spawn in border of the board.
    /// - Enemies walk on tiles.
    /// - Enemies use pathfinding to reach the heart.
    /// </summary>
    public class EnemyManager: MonoBehaviour
    {
        // ============================================================
        // Configuration
        // ============================================================

        [Header("Configuration")]
        [SerializeField] private WaveConfig _waveConfig;

        // ============================================================
        // References
        // ============================================================

        private List<Enemy> _enemies;

        // ============================================================
        // State
        // ============================================================

        private HashSet<Vector2> _tilePositions;
        private HashSet<Vector2> _occupiedPositions;
        private HashSet<Vector2> _obstaclePositions;
        private List<Vector2> _spawnPositions;
        private Vector2 _heartPosition;

        private List<Vector2> _enemyPath;

        // ============================================================
        // Debug
        // ============================================================

        [Header("Debug")]
        [SerializeField] private bool _debugMode;
        private bool _drawEnemyPathGizmo;

        // ============================================================
        // Lifecycle
        // ============================================================

        private void Awake()
        {
            _enemies = new List<Enemy>();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        public void Initialize(HashSet<Vector2> tilePositions, Vector2 heartPosition)
        {
            Debug.Log("[EnemyManager] heart position: " + heartPosition);
            _spawnPositions = new List<Vector2>();
            _tilePositions = new HashSet<Vector2>();
            _occupiedPositions = new HashSet<Vector2>();
            _obstaclePositions = new HashSet<Vector2>();

            _tilePositions = tilePositions;
            _heartPosition = heartPosition;

            _spawnPositions = EnemyPathFinding.SelectSpawnPosition(_tilePositions, _heartPosition);


        }

        // ============================================================
        // Event subscription
        // ============================================================

        private void SubscribeToEvents()
        {
            EventBus.Instance.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
            EventBus.Instance.Subscribe<TurnStartedEvent>(OnTurnStarted);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Instance.Unsubscribe<PhaseChangedEvent>(OnPhaseChanged);
            EventBus.Instance.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        }

        // ============================================================
        // Enemy phase execution
        // ============================================================

        /// <summary>
        /// process all enemies actions
        /// - Spawn first
        /// - Action
        /// </summary>
        /// <param name="turnNumber"></param>
        private void StartEnemyTurn(int turnNumber)
        {
            if (_waveConfig == null) return;

            UpdateOccupiedPositions();

            var spawnEntries = _waveConfig.GetSpawnForTurn(turnNumber);

            if (spawnEntries.Count > 0)
            {
                foreach (var spawnEntry in spawnEntries)
                {
                    for (var i = 0; i < spawnEntry.Count; i++)
                    {
                        StartCoroutine(Spawn(spawnEntry.EnemyData));
                    }
                }
                return;
            }

            var sortedEnemies = SortEnemyByClosestToHeart(_enemies);

            foreach (var enemy in sortedEnemies)
            {
                if (enemy.IsDead) continue;

                StartCoroutine(ProcessSingleEnemy(enemy));
            }

            EventBus.Instance.Publish(new Turn.EnemyPhaseCompletedEvent
            {
                TurnNumber = turnNumber,
            });
        }

        private IEnumerator ProcessSingleEnemy(Enemy enemy)
        {
            Debug.Log("[EnemyManager]: Processing single enemy");
            Debug.Log("[EnemyManager]: Enemy position: " + enemy.GridPosition + " and heart position " + _heartPosition);

            enemy.ResetTurnResources();

            if (_debugMode == true)
                _drawEnemyPathGizmo = true;

            enemy.Attack(_heartPosition);

            _enemyPath = EnemyPathFinding.FindPath(new Vector2(enemy.GridPosition.x, enemy.GridPosition.z), _heartPosition, _tilePositions.ToList(), _obstaclePositions.ToList());
            if (_enemyPath == null) yield break;

            if (_enemyPath.Count > 0 && _enemyPath[_enemyPath.Count - 1] == _heartPosition)
                _enemyPath.RemoveAt(_enemyPath.Count - 1);
            enemy.Move(_enemyPath);

            yield return new WaitForSeconds(1f);
        }

        // ============================================================
        // Spawn
        // ============================================================

        /// <summary>
        /// Spawn enemies in the spawn location
        /// </summary>
        /// <param name="enemy"></param>
        /// <returns></returns>
        private IEnumerator Spawn(EnemyData enemy)
        {

            var spawnPosition = FindBestSpawnPosition();
            if (spawnPosition == null)
                yield break;

            var enemyObject = Instantiate(enemy.Prefab, this.transform);
            var enemyComponent = enemyObject.AddComponent<Enemy>();
            enemyComponent.Initialize(enemy, spawnPosition.Value);
            _enemies.Add(enemyComponent);
            _spawnPositions.Add(spawnPosition.Value);
            _occupiedPositions.Add(spawnPosition.Value);

            Debug.Log("[EnemyManager]: spawn enemy - " + enemy.name);

            yield return new WaitForSeconds(_waveConfig.SpawnDelay);
        }

        /// <summary>
        /// Choose the best available spawn.
        /// If there is not available spawn, choose an unoccupied tile.
        /// </summary>
        /// <returns>Return spawn position or null if there is no available tile on the board</returns>
        private Vector2? FindBestSpawnPosition()
        {
            // Check if spawn position are available
            foreach (var pos in _spawnPositions)
            {
                if (!_occupiedPositions.Contains(pos) && !_obstaclePositions.Contains(pos))
                    return pos;
            }

            // Fallback => Unoccupied tile
            var borders = EnemyPathFinding.FindBorderTiles(_tilePositions);
            var shuffled = borders.OrderBy(_ => Random.value);
            foreach (var pos in shuffled)
            {
                if (!_occupiedPositions.Contains(pos) && !_obstaclePositions.Contains(pos) && pos != _heartPosition)
                    return pos;
            }

            return null;
        }

        // ============================================================
        // Sort
        // ============================================================

        /// <summary>
        /// Sort enemies to get the closest to the heart in first.
        /// </summary>
        /// <param name="enemies">List of enemies</param>
        /// <returns>Sorted list of enemies</returns>
        private List<Enemy> SortEnemyByClosestToHeart(List<Enemy> enemies)
        {
            return enemies.Where(e => !e.IsDead)
                .OrderBy(e => EnemyPathFinding.ManhattanDistance(e.GridPosition, _heartPosition))
                .ToList();
        }

        // ============================================================
        // Cleanup
        // ============================================================

        /// <summary>
        /// Update the occupied position list to avoid an enemy from walkin or spawning in another enemy.
        /// </summary>
        private void UpdateOccupiedPositions()
        {
            _occupiedPositions.Clear();
            foreach (var enemy in _enemies)
            {
                _occupiedPositions.Add(enemy.GridPosition);
            }
        }

        // ============================================================
        // Event handler
        // ============================================================

        private void OnPhaseChanged(PhaseChangedEvent evt)
        {
            if (evt.NewPhase != TurnPhase.EnemyPhase) return;

            Debug.Log($"[OnPhaseChanged]: phase change to: " + evt.NewPhase);
            StartEnemyTurn(evt.TurnNumber);
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {

        }

        // ============================================================
        // Obstacles
        // ============================================================

        public void AddObstaclePosition(Vector2 pos)
        {
            if (_obstaclePositions == null)
                _obstaclePositions = new HashSet<Vector2>();

            if (_obstaclePositions.Contains(pos)) return;

            _obstaclePositions.Add(pos);
        }

        // ============================================================
        // Debug
        // ============================================================

        private void OnDrawGizmos()
        {
            if (_drawEnemyPathGizmo == false)
                return;

            EnemyPathFinding.DrawPath(_enemyPath);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(_heartPosition.x, 1, _heartPosition.y), new Vector3(0.2f, 0.2f, 0.2f));
        }




    }
}
