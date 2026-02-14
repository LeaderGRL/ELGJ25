using UnityEngine;

namespace Crossatro.Heart
{
    /// <summary>
    /// Publish when the heart is taking damage or being healed.
    /// </summary>
    public struct HealthChangedEvent
    {
        public int PreviousHealth;
        public int CurrentHealth;
        public int Delta;
    }

    /// <summary>
    /// Publish when the max health changed.
    /// </summary>
    public struct MaxHealthChangedEvent
    {
        public int PreviousMaxHealth;
        public int CurrentMaxHealth;
        public int Delta;
    }

    /// <summary>
    /// Publish when the player died.
    /// </summary>
    public struct HeartDestroyedEvent
    {
        public int Damage;
        public int TurnNumber;
    }
}
