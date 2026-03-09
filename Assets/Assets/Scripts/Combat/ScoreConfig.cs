using UnityEngine;

namespace Crossatro.Combat
{
    /// <summary>
    /// Scoring configuration for words damage calculation.
    /// </summary>
    [CreateAssetMenu(fileName = "ScoreConfig", menuName = "Crossatro/Score Config")]
    public class ScoreConfig: ScriptableObject
    {
        // ============================================================
        // Difficulty
        // ============================================================

        [Header("Difficulty Scaling")]
        [SerializeField] private float _difficultyStep = 0.5f;

        // ============================================================
        // Length bonus
        // ============================================================

        [Header("Length Bonus")]
        [SerializeField] private float _lengthBonusStep = 0.1f;

        // ============================================================
        // Combo
        // ============================================================

        [Header("Combo Scaling")]
        [SerializeField] private float _comboMultStep = 0.5f;

        // ============================================================
        // API
        // ============================================================

        public float DifficultyStep => _difficultyStep;
        public float LengthBonusStep => _lengthBonusStep;
        public float ComboMultStep => _comboMultStep;

        /// <summary>
        /// Compute the difficulty multiplier for a given level.
        /// </summary>
        public float GetDifficultyMultiplier(int difficulty)
        {
            return 1f + (Mathf.Clamp(difficulty, 1, 9) - 1) * _difficultyStep;
        }

        /// <summary>
        /// Compute the length bonus for a given word length.
        /// </summary>
        public float GetLengthBonus(int wordLength)
        {
            return 1f + wordLength * _lengthBonusStep;
        }
    }
}
