using Crossatro.Enemy;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Crossatro.Grid
{
    /// <summary>
    /// Build a crossword grid by selecting and placing words from the database.
    /// </summary>
    public class CrosswordGridBuilder
    {
        // ============================================================
        // Constants
        // ============================================================

        /// <summary>
        /// Maximum iterations before giving up on placing more words.
        /// Prevents infinite loops when no valid placement exists.
        /// </summary>
        private const int MAX_ITERATIONS = 10000;

        /// <summary>
        /// Number of full passes through the candidate list before giving up.
        /// </summary>
        private const int MAX_PASSES = 3;

        // ============================================================
        // State
        // ============================================================

        private readonly System.Random _rng;

        /// <summary>
        /// Grid of already placed letters.
        /// </summary>
        private Dictionary<Vector2, char> _placedLetters;

        /// <summary>
        /// Words successfully placed to the grid.
        /// </summary>
        private List<GridWord> _placedWords;

        /// <summary>
        /// Heart position => dedicated tile.
        /// </summary>
        private Vector2 _heartPosition;
        private bool _heartPositionComputed;

        // ============================================================
        // Constructor
        // ============================================================

        /// <summary>
        /// Create a new grid builder.
        /// </summary>
        /// <param name="seed">Optional seed for specific grid acess</param>
        public CrosswordGridBuilder(int seed = -1)
        {
            _rng = seed >= 0 ? new System.Random(seed) : new System.Random();
        }

        // ============================================================
        // API
        // ============================================================

        /// <summary>
        /// Build a crossword grid by placing words from the database.
        /// </summary>
        /// <param name="database">Words database</param>
        /// <param name="targetWordCount">Number of word to place</param>
        /// <returns></returns>
        public CrossWordsGameGrid Build(WordDatabaseJSON database, int targetWordCount)
        {
            if (database == null || database.words == null || database.words.Count == 0)
            {
                Debug.LogError("[CrosswordGridBuilder] Database is null or empty!");
                return new CrossWordsGameGrid(new List<GridWord>());
            }

            // Reset state
            _placedLetters = new Dictionary<Vector2, char>();
            _placedWords = new List<GridWord>();
            _heartPositionComputed = false;

            List<WordData> allWords = database.words.ToList();

            // Shuffle the word list
            List<WordData> shuffleWords = database.words
                .OrderBy(_ => _rng.Next())
                .ToList();

            // Place the first word at the origin
            PlaceFirstWord(shuffleWords[0]);

            // Multiple passes to maximize word placement
            HashSet<string> usedWords = new HashSet<string> { shuffleWords[0].word };

            // Try to place remaining words
            PlaceRemainingWords(allWords, usedWords, targetWordCount);

            // Compute heart position after grid is built.
            ComputeHeartPosition();

            Debug.Log($"[CrosswordGridBuilder] built grid with " + $"{_placedWords.Count}/{targetWordCount} words");

            return new CrossWordsGameGrid(_placedWords);
        }

        /// <summary>
        /// Get the computed heart position.
        /// </summary>
        /// <returns></returns>
        public Vector2 GetHeartPosition()
        {
            if (!_heartPositionComputed)
            {
                Debug.LogWarning("[CrosswordGridBuilder] Heart position not yet computed. Call Build() first.");
                return Vector2.zero;
            }

            return _heartPosition;
        }

        // ============================================================
        // Word Placement
        // ============================================================


        /// <summary>
        /// Place the first word at the origin.
        /// </summary>
        /// <param name="wordData"></param>
        private void PlaceFirstWord(WordData wordData)
        {
            GridWord word = CreateGridWord(wordData, Vector2.zero, isRow: true);
            CommitWord(word);
        }

        /// <summary>
        /// Try to place words until we reach the target count.
        /// </summary>
        /// <param name="candidates"></param>
        /// <param name="targetCount"></param>
        private void PlaceRemainingWords(List<WordData> allCandidates, HashSet<string> usedWord, int targetCount)
        {
            int iterations = 0;

            for (int i = 0; i < MAX_PASSES; i++)
            {
                if (_placedWords.Count >= targetCount) break;

                // Shuffle unplaced candidates each pass for variety
                List<WordData> candidates = allCandidates
                    .Where(w => !usedWord.Contains(w.word))
                    .OrderBy(_ => _rng.Next())
                    .ToList();

                foreach (WordData candidate in candidates)
                {
                    if (_placedWords.Count >= targetCount) break;
                    if (iterations++ >= MAX_ITERATIONS) break;

                    // Try to place this word against all existing words
                    GridWord bestPlacement = FindBestPlacement(candidate);

                    if (bestPlacement != null)
                    {
                        CommitWord(bestPlacement);
                        usedWord.Add(candidate.word);
                    }
                }

                if (iterations >= MAX_ITERATIONS)
                {
                    Debug.LogWarning($"[CrosswordGridBuilder] Hit max iterations on pass {i + 1}." + $"Placed {_placedWords.Count}/{targetCount} words.");
                    break;
                }
            }
        }

        /// <summary>
        /// Try placing a word against every already placed word.
        /// Scores each valid placement by how many intersections it creates.
        /// </summary>
        /// <param name="candidate"></param>
        /// <returns>Best placement or null if not found</returns>
        private GridWord FindBestPlacement(WordData candidate)
        {
            var scoredPlacement = new List<(GridWord word, int score)>();

            // Try against every existing word on the grid
            foreach (GridWord existingWord in _placedWords)
            {
                // New word must be perpendicular to existing word
                bool newIsRow = !existingWord.IsRow;

                var intersections = FindLetterIntersections(candidate.word, existingWord.SolutionWord);

                if (intersections.Count ==  0) continue;

                var validPosition = FindValidStartPosition(candidate.word, existingWord, intersections, newIsRow);

                foreach (Vector2 startPos in validPosition)
                {
                    GridWord placement = CreateGridWord(candidate, startPos, newIsRow);
                    int score = ScorePlacement(placement);
                    scoredPlacement.Add((placement, score));
                }
            }

            if (scoredPlacement.Count == 0) return null;

            // sort by score descending
            scoredPlacement.Sort((a, b) => b.score.CompareTo(a.score));

            // Pick from the top candidates
            int topCount = Mathf.Min(3, scoredPlacement.Count);
            int pick = _rng.Next(topCount);
            return scoredPlacement[pick].word;
        }
        
        /// <summary>
        /// Score a potential placement by counting how many existing letters it intersects with.
        /// </summary>
        /// <param name="word"></param>
        /// <returns>number if word intersection</returns>
        private int ScorePlacement(GridWord word)
        {
            int intersections = 0;
            var positions = word.GetAllLetterSolutionPositions();

            foreach (var kvp in positions)
            {
                if (_placedLetters.ContainsKey(kvp.Key))
                    intersections++;
            }

            return intersections;
        }

        /// <summary>
        /// Advance to next candidate index, wrapping around.
        /// Index 0 is always the first word already placed.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        private int NextCandidateIndex(int current, int total)
        {
            int next = current + 1;
            if (next >= total) next = 1;
            return next;
        }

        private bool TryPlaceCandidateAgainstGrid(WordData wordData, out GridWord result)
        {
            result = null;

            // Shuffle anchor order for variety
            List<int> anchorOrder = Enumerable.Range(0, _placedWords.Count)
                .OrderBy(_ => _rng.Next())
                .ToList();

            foreach (int anchorIndex in anchorOrder)
            {
                GridWord anchor = _placedWords[anchorIndex];

                bool newIsRow = !anchor.IsRow;

                // Find shared letters
                var intersections = FindLetterIntersections(wordData.word, anchor.SolutionWord);
                if (intersections.Count == 0) continue;

                // Find valid start positions from theses intersections
                var ValidPositions = FindValidStartPosition(wordData.word, anchor, intersections, newIsRow);

                if (ValidPositions.Count == 0) continue;

                // Pick a random valid position
                int index = _rng.Next(ValidPositions.Count);
                Vector2 startPos = ValidPositions[index];

                result = CreateGridWord(wordData, startPos, newIsRow);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to place a word crossing the last word.
        /// Find shared letters between the two words, then checks if any resulting position is valid.
        /// </summary>
        /// <param name="wordData"></param>
        /// <param name="lastWord"></param>
        /// <param name="isRow"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool TryPlaceWord(WordData wordData, GridWord lastWord, bool isRow, out GridWord result)
        {
            result = null;

            // Find all letter intersections between the candidate and the last word
            var intersections = FindLetterIntersections(wordData.word, lastWord.SolutionWord);
            if (intersections.Count == 0) return false;
            
            // Try each intersection to find a valid start position
            var validPositions = FindValidStartPosition(wordData.word, lastWord, intersections, isRow);

            if (validPositions.Count == 0) return false;

            // Pick a random valid position
            int index = _rng.Next(validPositions.Count);
            Vector2 startPos = validPositions[index];

            result = CreateGridWord(wordData, startPos, isRow);
            return true;
        }

        // ============================================================
        // Heart position
        // ============================================================

        /// <summary>
        /// Find a position for the heart that is adjacent to a letter 
        /// and as close as possible from the grid center.
        /// </summary>
        private void ComputeHeartPosition()
        {
            if (_placedLetters.Count == 0)
            {
                _heartPosition = Vector2.zero;
                _heartPositionComputed = true;
                return;
            }

            // Calculate center of mass
            float sumX = 0f, sumY = 0f;
            foreach (var pos in _placedLetters.Keys)
            {
                sumX += pos.x;
                sumY += pos.y;
            }

            float centerX = sumX / _placedLetters.Count;
            float centerY = sumY / _placedLetters.Count;

            // All occupied positions
            HashSet<Vector2> occupied = new HashSet<Vector2>(_placedLetters.Keys);

            // Find empty positions adjacent to a least one tile
            Vector2[] directions = {Vector2.up, Vector2.down, Vector2.left, Vector2.right};
            HashSet<Vector2> candidates = new HashSet<Vector2>();

            foreach (var pos in _placedLetters.Keys)
            {
                foreach (var dir in directions)
                {
                    Vector2 neighbor = pos + dir;
                    if (!occupied.Contains(neighbor))
                        candidates.Add(neighbor);
                }
            }

            if (candidates.Count == 0)
            {
                // Fallback: use any adjacent position to first letter
                Vector2 firstLetter = _placedLetters.Keys.First();
                _heartPosition = firstLetter + Vector2.up;
                _heartPositionComputed = true;
                return;
            }

            // Score candidates
            Vector2 bestPos = Vector2.zero;
            float bestScore = float.MaxValue;

            foreach (var candidate in candidates)
            {
                float dx = candidate.x - centerX;
                float dy = candidate.y - centerY;
                float distToCenter = dx * dx + dy * dy;

                // Count adjacent tiles
                int adjacentTiles = 0;
                foreach (var dir in directions)
                {
                    if (occupied.Contains(candidate + dir))
                        adjacentTiles++;
                }

                // Score: low distance + bonus for connectivity
                // Substract adjacentTiles to favor well connected spots
                float score = distToCenter - (adjacentTiles * 5f);

                if (score < bestScore)
                {
                    bestScore = score;
                    bestPos = candidate;
                }
            }

            _heartPosition = bestPos;
            _heartPositionComputed = true;

            Debug.Log($"[CrosswordGridBuilder] Heart position: {_heartPosition}" + $"(center of mass: ({centerX:F1}, {centerY:F1}))");
        }

        // ============================================================
        // Intersection detection
        // ============================================================

        /// <summary>
        /// Find all index pairs where two words share the same letter.
        /// </summary>
        /// <param name="newWord"></param>
        /// <param name="lastWord"></param>
        /// <returns>Pairs if (newWordIndex, lastWordIndex)</returns>
        private List<(int newIndex, int lastIndex)> FindLetterIntersections(string newWord, string lastWord)
        {
            var intersections = new List<(int, int)>();

            for (int i = 0; i < newWord.Length; i++)
            {
                for (int j = 0; j < lastWord.Length; j++)
                {
                    if (newWord[i] == lastWord[j])
                    {
                        intersections.Add((i, j));
                    }
                }
            }

            return intersections;
        }


        // ============================================================
        // Position validation
        // ============================================================


        /// <summary>
        /// For each interesection, calculate where the new word would start and check is that placement is valid.
        /// </summary>
        /// <param name="newWord"></param>
        /// <param name="lastWord"></param>
        /// <param name="intersections"></param>
        /// <param name="isRow"></param>
        /// <returns></returns>
        private List<Vector2> FindValidStartPosition(string newWord, GridWord lastWord, List<(int newIndex, int lastIndex)> intersections, bool isRow)
        {
            var validPosition = new List<Vector2>();
            bool isLastWordRow = !isRow;

            foreach (var (newIndex, lastIndex) in intersections)
            {
                // Calculate where the intersection letter sits on the grid
                Vector2 intersecionPos = CalculateLetterPosition(lastWord.StartPosition, lastIndex, isLastWordRow);

                // Calculate where the new word would start
                Vector2 startPos = CalculateStartPosition(intersecionPos, newIndex, isRow);

                // Validate the entire word at this position
                if (IsPlacementValid(newWord, startPos, isRow, intersecionPos))
                {
                    validPosition.Add(startPos);
                }
            }

            return validPosition;
        }

        /// <summary>
        /// Check if placing a word at the given position is valid.
        /// Rules:
        /// - No letter conflitcts
        /// - No parallel adjacency
        /// - No extension conflicts
        /// </summary>
        /// <param name="word"></param>
        /// <param name="startPos"></param>
        /// <param name="isRow"></param>
        /// <param name="intersectionPos"></param>
        /// <returns></returns>
        private bool IsPlacementValid(string word, Vector2 startPos, bool isRow, Vector2 intersectionPos)
        {
            for (int i = 0; i < word.Length; i++)
            {
                char letter = word[i];
                Vector2 letterPos = CalculateLetterPosition(startPos, i, isRow);

                // No letter conflitcts
                if (_placedLetters.TryGetValue(letterPos, out char existing))
                {
                    if (existing != letter) return false;
                }

                // Adjacency checks
                if (!IsAdjacencyValid(letterPos, intersectionPos, isRow, i, word.Length))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check adjacency rules for a single letter position.
        /// - Letters perpendicual to the word direction must not have neighbors
        /// - First letter must not have a neighbors before it.
        /// - Last letter must not habe a neighbor after it.
        /// </summary>
        /// <param name="letterPos"></param>
        /// <param name="intersectionPos"></param>
        /// <param name="isRow"></param>
        /// <param name="letterIndex"></param>
        /// <param name="wordLenght"></param>
        /// <returns></returns>
        private bool IsAdjacencyValid(Vector2 letterPos, Vector2 intersectionPos, bool isRow, int letterIndex, int wordLength)
        {
            if (isRow)
            {
                // Check above and below
                Vector2 above = new Vector2(letterPos.x, letterPos.y + 1);
                Vector2 below = new Vector2(letterPos.x, letterPos.y - 1);

                if (letterPos.x != intersectionPos.x)
                    if (_placedLetters.ContainsKey(above) || _placedLetters.ContainsKey(below))
                        return false;

                // Check before first letter
                if (letterIndex == 0)
                {
                    Vector2 left = new Vector2(letterPos.x - 1, letterPos.y);
                    if (_placedLetters.ContainsKey(left)) return false;
                }

                // Check after last letter
                if (letterIndex == wordLength - 1)
                {
                    Vector2 right = new Vector2(letterPos.x + 1, letterPos.y);
                    if (_placedLetters.ContainsKey(right)) return false;
                }
            }
            else
            {
                // Check left and right
                Vector2 left = new Vector2(letterPos.x -1, letterPos.y);
                Vector2 right = new Vector2(letterPos.x + 1, letterPos.y);

                if (letterPos.y != intersectionPos.y)
                {
                    if (_placedLetters.ContainsKey(left) || _placedLetters.ContainsKey(right))
                        return false;
                }

                // Check before first letter
                if (letterIndex == 0)
                {
                    Vector2 above = new Vector2(letterPos.x, letterPos.y + 1);
                    if (_placedLetters.ContainsKey(above)) return false;
                }

                // Check after last letter
                if (letterIndex == wordLength - 1)
                {
                    Vector2 below = new Vector2(letterPos.x, letterPos.y - 1);
                    if (_placedLetters.ContainsKey(below)) return false;
                }
            }

            return true;
        }

        // ============================================================
        // Position calculation
        // ============================================================

        /// <summary>
        /// Calculate the grid position of a letter at the given index.
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="index"></param>
        /// <param name="isRow"></param>
        /// <returns></returns>
        private Vector2 CalculateLetterPosition(Vector2 startPos, int index, bool isRow)
        {
            return isRow ? new Vector2(startPos.x + index, startPos.y) : new Vector2(startPos.x, startPos.y - index);
        }

        /// <summary>
        /// Calculate the start position of a word give where one of its letters must be placed.
        /// </summary>
        /// <param name="intersectionPos"></param>
        /// <param name="letterIndex"></param>
        /// <param name="isRow"></param>
        /// <returns></returns>
        private Vector2 CalculateStartPosition(Vector2 intersectionPos, int letterIndex, bool isRow)
        {
            return isRow ? new Vector2(intersectionPos.x - letterIndex, intersectionPos.y) : new Vector2(intersectionPos.x, intersectionPos.y + letterIndex);
        }

        // ============================================================
        // Word creation & commit
        // ============================================================

        /// <summary>
        /// Create a GridWord instance from word data and placement info.
        /// </summary>
        /// <param name="wordData"></param>
        /// <param name="startPos"></param>
        /// <param name="isRow"></param>
        /// <returns></returns>
        private GridWord CreateGridWord(WordData wordData, Vector2 startPos, bool isRow)
        {
            GridWord word = new GridWord();
            word.SolutionWord = wordData.word;
            word.StartPosition = startPos;
            word.IsRow = isRow;
            word.Difficulty = wordData.difficulty;
            word.Clues = wordData.description1;
            word.Initialize();
            return word;
        }

        /// <summary>
        /// Add a word to the grid => Register its letters and store it.
        /// </summary>
        /// <param name="word">Word to add to the grid</param>
        private void CommitWord(GridWord word)
        {
            var letters = word.GetAllLetterSolutionPositions();
            foreach (var kvp in letters)
            {
                _placedLetters[kvp.Key] = kvp.Value;
            }

            _placedWords.Add(word);
        }
    }
}
