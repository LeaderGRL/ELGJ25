using UnityEngine;

namespace Crossatro.Events
{
    // ============================================================
    // Words Event
    // ============================================================

    public struct WorldCompletedEvent
    {
        public string Word;
        public int Damage;
        public int Coins;
    }

    // ============================================================
    // Combat Event
    // ============================================================
    
    public struct PlayerDamageEvent
    {
        public int Damage;
        public int RemainingHealth;
    }

    // ============================================================
    // Player Event
    // ============================================================

    public struct HealthChangedEvent
    {
        public int NewAmount;
        public int MaxAmount;
        public int Delta;
    }

    public struct CoinsChangedEvent
    {
        public int NewAmount;
        public int Delta;
    }

    public struct  ScoreChangedEvent
    {
        public int NewScore;
        public int Delta;
    }


}