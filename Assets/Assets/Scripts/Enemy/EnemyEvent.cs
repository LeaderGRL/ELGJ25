using UnityEngine;

namespace Crossatro.Enemy
{
    
    public struct EnemySpawnEvent
    {
        public Enemy Enemy;
        public Vector2Int GridPosition;
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
        public Vector2Int GridPosition;
    }

    public struct EnemyMovedEvent
    {
        public Enemy Enemy;
        public Vector2Int FromPosition;
        public Vector2Int ToPosition;
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
