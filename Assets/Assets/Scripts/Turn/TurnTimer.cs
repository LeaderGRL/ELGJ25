using UnityEngine;
using UnityEngine.Rendering;

namespace Crossatro.Turn
{
    /// <summary>
    /// Countdown timer for timed phases.
    /// PPublishes TimerTickEvent at regular intervals and TimerExpiredEvent when time runs out.
    /// </summary>
    public class TurnTimer
    {
        // ============================================================
        // State
        // ============================================================

        private float _totalDuration;
        private float _timeRemaining;
        private float _tickInterval;
        private float _timeSinceLastTick;
        private TurnPhase _phase;

        private bool _isRunning;
        private bool _isInfinite;

        // ============================================================
        // Properties
        // ============================================================

        public float TimeRemaining => _timeRemaining;
        public float TotalDuration => _totalDuration;
        public bool IsRunning => _isRunning;
        public bool IsInfinite => _isInfinite;
        public bool IsExpired => !_isInfinite && _timeRemaining <= 0f;

        /// <summary>
        /// Normalized elapsed time
        /// </summary>
        public float NormalizedElapsed => _totalDuration > 0f ? 1f - (_timeRemaining / _totalDuration) : 1f;

        // ============================================================
        // LifeCycle
        // ============================================================

        /// <summary>
        /// Start a new countdown.
        /// </summary>
        /// <param name="duration">Total time in seconds</param>
        /// <param name="tickInterval">How often to publish TimerTickEvent</param>
        /// <param name="phase">Which phase this timer belongs to</param>
        public void Start(float duration, float tickInterval, TurnPhase phase)
        {
            _totalDuration = duration;
            _timeRemaining = duration;
            _tickInterval = tickInterval;
            _timeSinceLastTick = 0f;
            _phase = phase;
            _isRunning = true;
            _isInfinite = false;

            PublishTick();
        }

        /// <summary>
        /// Stop the timer without publishing expired event.
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Call every frame/
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            if (!_isRunning) return;
            if (_isInfinite) return;

            _timeRemaining -= deltaTime;
            _timeSinceLastTick += deltaTime;


            // Publish tick at regular interval
            if (_timeSinceLastTick >= _tickInterval)
            {
                _timeSinceLastTick -= _tickInterval;
                PublishTick();
            }

            // Check expiration
            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                _isRunning = false;
                PublishTick();
                PublishExpired();
            }
        }

        // ============================================================
        // Modifiers
        // ============================================================

        /// <summary>
        /// Add extra time to the current countdown.
        /// </summary>
        /// <param name="time"></param>
        public void AddTime(float time)
        {
            if (!_isRunning) return;

            _timeRemaining += time;
            _totalDuration += time;

            Debug.Log($"[TurnTimer] Added {time}s. " + $"Remaining: {_timeRemaining:F1}s");
        }

        /// <summary>
        /// Make the timer infinite.
        /// </summary>
        public void SetInfinite()
        {
            _isInfinite = true;

            Debug.Log("[TurnTimer] Timer set to infinite");
        }

        /// <summary>
        /// Pause the timer.
        /// </summary>
        public void Pause()
        {
            _isRunning = false;
        }

        /// <summary>
        /// Resume a paused timer.
        /// </summary>
        public void Resume()
        {
            if (_isInfinite || IsExpired) return;
            _isRunning = true;
        }

        // ============================================================
        // Event publishing
        // ============================================================

        private void PublishTick()
        {
            EventBus.Instance.Publish(new TimerTickEvent
            {
                TimeRemaining = _timeRemaining,
                TotalDuration = _totalDuration,
                NormalizedElapsed = NormalizedElapsed,
            });
        }

        private void PublishExpired()
        {
            EventBus.Instance.Publish(new TimerExpiredEvent
            {
                Phase = _phase,
            });
        }
    }
}
