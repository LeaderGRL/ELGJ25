using UnityEngine;

namespace Crossatro.Enemy
{
    
    public struct EnemySpawnEvent
    {
        public Enemy Enemy;
        public Vector2 GridPosition;
        public int TurnNumber;
    }

    public struct EnemyDamagedEvent
    {
        public Enemy Enemy;
        public int Damage;
        public int RemainingHealth;
    }

    public struct EnemyDeathEvent
    {
        public Enemy Enemy;
        public int GoldReward;
        public Vector2 GridPosition;
    }

    public struct EnemyMovedEvent
    {
        public Enemy Enemy;
        public Vector2 FromPosition;
        public Vector2 ToPosition;
        public int RemainingMovement;
    }

    public struct EnemyAttackedEvent
    {
        public Enemy Enemy;
        public int Damage;
    }

    public struct EnemyCoinsChangedEvent
    {
        public int NewAmount;
        public int Delta;
    }
}
