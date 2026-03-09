namespace Crossatro.Combat
{
    /// <summary>
    /// Detailed breakdown of a score calculation.
    /// Exposes each component for UI display and animation.
    /// </summary>
    public struct ScoringResult
    {
        // ============================================================
        // Base value
        // ============================================================

        /// <summary> Sum of all letter weights in the word </summary>
        public int LetterScore;

        // ============================================================
        // Multipliers
        // ============================================================

        /// <summary> Multiplier from word difficulty</summary>
        public float DifficultyMult;

        /// <summary> Bonus from word length </summary>
        public float LengthBonus;

        /// <summary> Combo multiplier from consecutive words this turn </summary>
        public float ComboMult;

        // ============================================================
        // Modifiers
        // ============================================================

        /// <summary> Flat bonus added to letter score</summary>
        public int BonusFlat;

        /// <summary> Additional multiplier from shop card effects</summary>
        public float BonusMult;

        // ============================================================
        // Final
        // ============================================================

        /// <summary> The final computed score/damage </summary>
        public int FinalScore;

        /// <summary>
        /// Effective total before rounding.
        /// (LetterScore + BonusFlat) × DifficultyMult × LengthBonus × (ComboMult + BonusMult)
        /// </summary>
        public float RawTotal =>
            (LetterScore + BonusFlat) * DifficultyMult * LengthBonus * (ComboMult * BonusMult);

        public override string ToString()
        {
            return $"{LetterScore + BonusFlat} pts + bonusFlat × {DifficultyMult:F1} diff × {LengthBonus:F1} len * {ComboMult + BonusMult} combot mult + bonus mult = {FinalScore}";
        }
    }
}