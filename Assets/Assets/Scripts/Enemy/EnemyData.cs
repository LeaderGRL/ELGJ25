using UnityEngine;

namespace Crossatro.Enemy
{

    [CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy Data")]
    public class EnemyData: ScriptableObject
    {
        // ============================================================
        // Identity
        // ============================================================

        [Header("Identity")]
        [Tooltip("Display name of this enemy")]
        [SerializeField] private string _name;

        [Tooltip("Short description of the enemy")]
        [TextArea(2, 4)]
        [SerializeField] private string _description = "";

        [Tooltip("Prefab to instantiate for this enemy")]
        [SerializeField] private GameObject _prefab;

        // ============================================================
        // Stats
        // ============================================================

        [Header("Stats")]
        [SerializeField] private int _baseHp;
        [SerializeField] private int _baseActionPoint;
        [SerializeField] private int _baseMovementPoint;
        [SerializeField] private int _baseAttackDistance;
        [SerializeField] private int _baseAttackDamage;

        // ============================================================
        // Economy
        // ============================================================

        [Header("Economy")]
        [Tooltip("Gold reward on death")]
        [SerializeField] private int _goldReward;

        // ============================================================
        // API
        // ============================================================

        public string Name => _name;
        public string Description => _description;
        public GameObject Prefab => _prefab;
        public int BaseHp => _baseHp;
        public int BaseActionPoint => _baseActionPoint;
        public int BaseMovementPoint => _baseMovementPoint;
        public int BaseAttackDistance => _baseAttackDistance;
        public int BaseAttackDamage => _baseAttackDamage;
        public int GoldReward => _goldReward;

    }
}
