using Crossatro.Heart;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Crossatro.Turn
{
    /// <summary>
    /// Manage the game loop withs phases transitions.
    /// </summary>
    public class TurnManager: MonoBehaviour
    {
        // ============================================================
        // Configuration
        // ============================================================

        [Header("Configuration")]
        [SerializeField] private TurnConfig _turnConfig;

        [Header("Debug")]
        [SerializeField] private bool _verboseLogging = true;

        // ============================================================
        // State
        // ============================================================

        private TurnPhase _currentPhase = TurnPhase.None;
        private int _turnNumber;
        private TurnTimer _timer;
        private bool _isTransitioning;

        // ============================================================
        // Properties
        // ============================================================

        public TurnPhase CurrentPhase => _currentPhase;
        public int TurnNumber => _turnNumber;
        public TurnTimer Timer => _timer;

        // ============================================================
        // Lifecycle
        // ============================================================

        private void Awake()
        {
            _timer = new TurnTimer();
        }

        private void Start()
        {
            SubscribeToEvents();
            StartCoroutine(StartGameRoutine());
        }

        private void Update()
        {
            // Drive the timer every frame
            if (_currentPhase == TurnPhase.PlayerPhase)
                _timer.Update(Time.deltaTime);

            // Direct expiration check
            //if (_timer.IsExpired && !_isTransitioning)
            //{
            //    Log("Timer expired (detected in Update)");
            //    EndPlayerPhase();
            //}
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        // ============================================================
        // Events subscription
        // ============================================================

        private void SubscribeToEvents()
        {
            EventBus.Instance.Subscribe<TimerExpiredEvent>(OnTimerExpired);
            EventBus.Instance.Subscribe<PlayerPassedEvent>(OnPlayerPassed);
            EventBus.Instance.Subscribe<EnemyPhaseCompletedEvent>(OnEnemyPhaseCompleted);
            EventBus.Instance.Subscribe<HeartDestroyedEvent>(OnHeartDestroyed);
        }

        private void UnsubscribeFromEvents()
        {
            EventBus.Instance.Unsubscribe<TimerExpiredEvent>(OnTimerExpired);
            EventBus.Instance.Unsubscribe<PlayerPassedEvent>(OnPlayerPassed);
            EventBus.Instance.Unsubscribe<EnemyPhaseCompletedEvent>(OnEnemyPhaseCompleted);
            EventBus.Instance.Unsubscribe<HeartDestroyedEvent>(OnHeartDestroyed);
        }

        // ============================================================
        // Game Start
        // ============================================================
        
        private IEnumerator StartGameRoutine()
        {
            Log("Game Starting..");

            // Wait for intro, animation etc..
            yield return new WaitForSeconds(_turnConfig.GameStartDelay);

            _turnNumber = 1;
            StartPlayerPhase();
        }

        // ============================================================
        // Phase Transitions
        // ============================================================

        /// <summary>
        /// Begin the player phase => Start timer, enable input.
        /// </summary>
        private void StartPlayerPhase()
        {
            TurnPhase previous = _currentPhase;
            _currentPhase = TurnPhase.PlayerPhase;
            _isTransitioning = false;

            // Publish turn started event
            EventBus.Instance.Publish(new TurnStartedEvent
            {
                TurnNumber = _turnNumber,
            });

            // Start the countdown timer
            _timer.Start(_turnConfig.PlayerPhaseDuration, _turnConfig.TimerTickInterval, TurnPhase.PlayerPhase);

            PublishPhaseChanged(previous);

            Log($"Turn {_turnNumber} - Player phase started " + $"({_turnConfig.PlayerPhaseDuration}s");
        }

        /// <summary>
        /// Begin the enemy phase, disable player input and let enemies act.
        /// </summary>
        private void StartEnemyPhase()
        {
            TurnPhase previous = _currentPhase;
            _currentPhase = TurnPhase.EnemyPhase;
            _isTransitioning = false;

            PublishPhaseChanged(previous);

            Log($"Turn {_turnNumber} - Enemy Phase started");

            // Execute all enemy actions.
            StartCoroutine(EnemyPhaseTimeoutRoutine());
        }

        /// <summary>
        /// Next turn after enemies finish.
        /// </summary>
        private void StartNextTurn()
        {
            _turnNumber++;
            StartCoroutine(PhaseTransitionRoutine(TurnPhase.PlayerPhase));
        }

        /// <summary>
        /// End the game.
        /// </summary>
        public void TriggerGameOver()
        { 
            if (_currentPhase == TurnPhase.GameOver) return;

            TurnPhase previous = _currentPhase;
            _currentPhase = TurnPhase.GameOver;

            _timer.Stop();

            PublishPhaseChanged(previous);

            Log("Game Over");
        }

        // ============================================================
        // Transition routines
        // ============================================================

        /// <summary>
        /// Brief delay between phases for visual transitions.
        /// </summary>
        /// <param name="nextPhase"></param>
        /// <returns></returns>
        private IEnumerator PhaseTransitionRoutine(TurnPhase nextPhase)
        {
            _isTransitioning = true;

            if (_turnConfig.PhaseTransitionDelay > 0f)
            {
                yield return new WaitForSeconds(_turnConfig.PhaseTransitionDelay);
            }

            switch (nextPhase)
            {
                case TurnPhase.PlayerPhase:
                    StartPlayerPhase();
                    break;
                case TurnPhase.EnemyPhase:
                    StartEnemyPhase();
                    break;
            }
        }

        private IEnumerator EnemyPhaseTimeoutRoutine()
        {
            yield return new WaitForSeconds(_turnConfig.EnemyPhaseTimeout);

            if (_currentPhase == TurnPhase.EnemyPhase && !_isTransitioning)
            {
                Log("Enemy phase timed out - force next turn to start");
                StartNextTurn();
            }
        }

        // ============================================================
        // Event Handlers
        // ============================================================

        private void OnTimerExpired(TimerExpiredEvent evt)
        {
            //if (_currentPhase != TurnPhase.PlayerPhase) return;
            //if (_isTransitioning) return;

            Log("Player phase timer expired");
            EndPlayerPhase();
        }

        private void OnPlayerPassed(PlayerPassedEvent evt)
        {
            if (_currentPhase != TurnPhase.PlayerPhase) return;
            if (_isTransitioning) return;

            Log($"Player passed with {evt.TimeRemaining:F1}s remaining");
            EndPlayerPhase();
        }

        private void OnEnemyPhaseCompleted(EnemyPhaseCompletedEvent evt)
        {
            if (_currentPhase != TurnPhase.EnemyPhase) return;
            if (_isTransitioning) return;

            Log("Enemy phase completed");
            StartNextTurn();
        }

        private void OnHeartDestroyed(HeartDestroyedEvent evt)
        {
            Log("Game Over");
            evt.TurnNumber = _turnNumber;
            TriggerGameOver();
        }

        // ============================================================
        // Internal
        // ============================================================

        /// <summary>
        /// Logic for ending player phase.
        /// </summary>
        private void EndPlayerPhase()
        {
            _timer.Stop();
            StartCoroutine(PhaseTransitionRoutine(TurnPhase.EnemyPhase));
        }

        // ============================================================
        // API
        // ============================================================

        /// <summary>
        /// Ends the player phase earlier;
        /// </summary>
        public void PlayerPhase()
        {
            if (_currentPhase != TurnPhase.PlayerPhase) return;
            if (_isTransitioning) return;

            EventBus.Instance.Publish(new PlayerPassedEvent
            {
                TurnNumber = _turnNumber,
                TimeRemaining = _timer.TimeRemaining,
            });
        }

        /// <summary>
        /// Add bonus time to the current timer.
        /// </summary>
        /// <param name="seconds"></param>
        public void AddBonusTime(float seconds)
        {
            if (_currentPhase != TurnPhase.PlayerPhase) return;
            _timer.AddTime(seconds);
        }

        /// <summary>
        /// Make the current timer infinite.
        /// </summary>
        public void SetTimerInfinite()
        {
            if (_currentPhase != TurnPhase.PlayerPhase) return;
            _timer.SetInfinite();
        }

        /// <summary>
        /// Force the enemy phase to finish earlier.
        /// </summary>
        public void NotifyEnemyPhaseComplete()
        {
            EventBus.Instance.Publish(new EnemyPhaseCompletedEvent
            {
                TurnNumber = _turnNumber,
            });
        }

        // ============================================================
        // Event publishing
        // ============================================================

        private void PublishPhaseChanged(TurnPhase previous)
        {
            EventBus.Instance.Publish(new PhaseChangedEvent
            {
                PreviousPhase = previous,
                NewPhase = _currentPhase,
                TurnNumber = _turnNumber,
            });
        }

        // ============================================================
        // Debug
        // ============================================================

        private void Log(string message)
        {
            if (_verboseLogging)
                Debug.Log($"[TurnManager] {message}");
        }
    }
}
