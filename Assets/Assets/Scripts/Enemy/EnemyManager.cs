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
    /// 
    /// </summary>
    public class EnemyManager: MonoBehaviour
    {
        // ============================================================
        // Configuration
        // ============================================================

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

        private int _spawnPositionIndex = 0;

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

        public void Initialize(HashSet<Vector2> tilePositions)
        {
            _spawnPositions = new List<Vector2>();
            _tilePositions = new HashSet<Vector2>();
            _occupiedPositions = new HashSet<Vector2>();
            _obstaclePositions = new HashSet<Vector2>();

            _tilePositions = tilePositions;

            _heartPosition = EnemyPathFinding.FindCenterTile(tilePositions);


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
        private IEnumerator Spawn(EnemyData enemy)
        {
            var spawnPoints = EnemyPathFinding.SelectSpawnPosition(_tilePositions, _heartPosition);

            var enemyObject = Instantiate(enemy.Prefab, this.transform);
            var enemyComponent = enemyObject.AddComponent<Enemy>();
            enemyComponent.Initialize(enemy, spawnPoints[_spawnPositionIndex]);
            _enemies.Add(enemyComponent);
            _spawnPositions.Add(spawnPoints[_spawnPositionIndex]);
            _occupiedPositions.Add(spawnPoints[_spawnPositionIndex]);

            _spawnPositionIndex++;

            Debug.Log("[EnemyManager]: spawn enemy - " + enemy.name);

            yield return new WaitForSeconds(_waveConfig.SpawnDelay);
        }

        private void StartEnemyTurn(int turnNumber)
        {
            if (_waveConfig == null) return;
            
            var spawnEntries = _waveConfig.GetSpawnForTurn(turnNumber);

            if (spawnEntries.Count > 0)
            {
                Debug.Log("SpawnEntries COUNT : " +  spawnEntries.Count);
                foreach (var spawnEntry in spawnEntries)
                {
                    for (var i = 0; i < spawnEntry.Count; i++)
                    {
                        StartCoroutine(Spawn(spawnEntry.EnemyData));
                    }
                }
                return;
            }

            foreach (var enemy in _enemies)
            {
                enemy.ResetTurnResources();
                StartCoroutine(ProcessSingleEnemy(enemy));
            }


        }

        private IEnumerator ProcessSingleEnemy(Enemy enemy)
        {
            Debug.Log("[EnemyManager]: Processing single enemy");
            Debug.Log("[EnemyManager]: Enemy position: " + enemy.GridPosition + " and heart position " + _heartPosition);

            _enemyPath = EnemyPathFinding.FindPath(new Vector2(enemy.GridPosition.x, enemy.GridPosition.z), _heartPosition, _tilePositions.ToList(), _obstaclePositions.ToList());

            if (_enemyPath == null) yield break;

            if (_debugMode == true)
                _drawEnemyPathGizmo = true;

            enemy.Move(_enemyPath);
            enemy.Attack(_heartPosition);

            yield return new WaitForSeconds(1f);
        }

        private bool IsWalkable(Vector2 tilePosition)
        {
            if (_occupiedPositions.Contains(tilePosition) || _obstaclePositions.Contains(tilePosition))
                return false;

            return true;
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
        // Debug
        // ============================================================

        private void OnDrawGizmos()
        {
            if (_drawEnemyPathGizmo == false)
                return;

            Debug.Log("ON DRAW GIZZ");


            EnemyPathFinding.DrawPath(_enemyPath);
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(_heartPosition.x, 1, _heartPosition.y), new Vector3(0.2f, 0.2f, 0.2f));
        }




    }
}
