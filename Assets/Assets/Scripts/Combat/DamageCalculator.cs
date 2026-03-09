using UnityEngine;

namespace Crossatro.Combat
{
    /// <summary>
    /// Calculates damage/score for completed words.
    ///
    /// Formula:
    ///   score = (letterWeights + bonusFlat) × difficultyMult × lengthBonus × (comboMult + bonusMult)
    /// </summary>
    public static class DamageCalculator
    {
        /// <summary>
        /// Calculate the full scoring result for a completed word.
        /// </summary>
        /// <param name="word">The completed word</param>
        /// <param name="difficulty">Word difficulty level</param>
        /// <param name="comboIndex">Words already completed this turn</param>
        /// <param name="config">Scoring configuration asset</param>
        /// <param name="bonusFlat">Flat score bonus from modifiers</param>
        /// <param name="bonusMult">Multiplicative bonus from modifiers</param>
        /// <returns>Detailed scoring breakdown</returns>
        public static ScoringResult Calculate(
            string word,
            int difficulty,
            int comboIndex,
            ScoreConfig config,
            int bonusFlat = 0,
            float bonusMult = 0f)
        {
            if (string.IsNullOrEmpty(word))
                return default;

            int letterScore = LetterWeights.Instance.CalculateBaseScore(word);
            float diffMult = config.GetDifficultyMultiplier(difficulty);
            float lenBonus = config.GetLengthBonus(word.Length);
            float comboMult = 1f + comboIndex * config.ComboMultStep;

            float raw = (letterScore + bonusFlat) * diffMult * lenBonus * (comboMult + bonusMult);
            int finalScore = Mathf.RoundToInt(raw);

            return new ScoringResult
            {
                LetterScore = letterScore,
                DifficultyMult = diffMult,
                LengthBonus = lenBonus,
                ComboMult = comboMult,
                BonusFlat = bonusFlat,
                BonusMult = bonusMult,
                FinalScore = finalScore,
            };
        }
    }
}