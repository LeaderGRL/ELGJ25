using UnityEngine;

namespace Crossatro.Combat
{
    /// <summary>
    /// Calculate damage deal to enemy with effects and letter weight.
    /// </summary>
    public class DamageCalculator
    {
        /// <summary>
        /// Calculate damage/score of a word based on
        /// - Letter weight
        /// - Word length => +1 multiplicator for each letter
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public int CalculateDamageFromLetterWeight(string word)
        {
            int score = 0;
            for (int i = 0; i <= word.Length; i++)
            {
                score += LetterWeights.Instance.GetWeight(word[i]) * (i + 1);
            }
            return score;
        }


    }
}
