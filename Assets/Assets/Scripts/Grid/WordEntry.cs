using System;
using System.Collections.Generic;

namespace Crossatro.Grid
{
    /// <summary>
    /// A single word entry in the crossword database.
    /// Serializable from JSON.
    /// </summary>
    [Serializable]
    public class WordEntry
    {
        /// <summary>
        /// The word itself, uppercase, no accents for grid matching.
        /// </summary>
        public string word;

        /// <summary>
        /// Difficulty rating.
        /// Used to control grid difficulty.
        /// </summary>
        public int difficulty;

        /// <summary>
        /// List of clues/definitions for this word.
        /// </summary>
        public List<string> clues;

        /// <summary>
        /// Thematic tags for themed grids.
        /// </summary>
        public List<string> themes;
    }

    /// <summary>
    /// Root object for JSON deserialization of the word database.
    /// </summary>
    [Serializable]
    public class WordDatabaseRoot
    {
        /// <summary>
        /// Language code ("fr", "en").
        /// </summary>
        public string language;

        /// <summary>
        /// All word entries in this database.
        /// </summary>
        public List<WordEntry> words;
    }
}