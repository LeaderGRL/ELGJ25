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

    //public struct PlayerDamagedEvent
    //{
    //    public int Damage;
    //    public int RemainingHealth;
    //}

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

    // ============================================================
    // Game State Event
    // ============================================================

    public struct GameStartedEvent
    {
        public int StartingHealth;
        public int StartingCoins;
    }

    public struct GameOverEvent
    {
        public bool Victory;
        public int FinalScore;
        public int TurnsCompleted;
    }

    public struct BoardCompletedEvent
    {
        public int BoardIndex;
        public int BonusReward;
    }

    public struct NewBoardGeneratedEvent
    {
        public int BoardIndex;
        public int WordCount;
    }

    // ============================================================
    // Turn Event
    // ============================================================

    public struct TurnStartedEvent
    {
        public int TurnNumber;
        public float TimeRemaining;
    }

    public struct TurnEndedEvent
    {
        public int TurnNumber;
        public int WordsCompleted;
        public int DamageDealt;
    }

    public struct TimerTickEvent
    {
        public float TimeRemaining;
        public float MaxTime;
    }

    public struct TimerExpiredEvent { }

    // ============================================================
    // Shop Event
    // ============================================================

    public struct ShopOpenedEvent { }
    public struct ShopClosedEvent { }

    public struct ItemPurchasedEvent
    {
        public string ItemName;
        public int Cost;
        public int RemainingCoins;
    }
}