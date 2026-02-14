using UnityEngine;

namespace Crossatro.Heart
{
    /// <summary>
    /// Configuration for the heart.
    /// The heart is an element the player have to protect to survive.
    /// </summary>
    [CreateAssetMenu(fileName = "HeartConfig", menuName = "Crossatro/Heart Config")]
    public class HeartConfig: ScriptableObject
    {
        // ============================================================
        // Player Health
        // ============================================================

        [SerializeField] private int _baseHp;

        // ============================================================
        // Visual
        // ============================================================

        /// <summary>
        /// Percentage of hp where we sending critical heal alert.
        /// </summary>
        [SerializeField] private int _criticalTreshold;

        // ============================================================
        // API
        // ============================================================

        public int BaseHp => _baseHp;
        public int CriticalTreshold => _criticalTreshold;
    }
}
