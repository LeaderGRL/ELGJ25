using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Crossatro.Grid
{
    /// <summary>
    /// Fills a crossword grid using backtracking.
    /// 
    /// - Pre-compute crossongs between slots
    /// - For each slot, list all words fitting
    /// - Backtrack with MRV
    /// </summary>
    public class CrosswordSolver
    {
        // ============================================================
        // Configuration
        // ============================================================

        /// <summary>
        /// Maximum backtrack steps before giving up.
        /// </summary>
        private const int MAX_BACKTRACKS = 50000;

        // ============================================================
        // State
        // ============================================================

        private readonly System.Random _rng;

        private Dictionary<int, Slot> _slotById;

        // ============================================================
        // Constructor
        // ============================================================{

        public CrosswordSolver(int seed = -1)
        {
            _rng = seed >= 0 ? new System.Random(seed) : new System.Random();
        }

        // ============================================================
        // API
        // ============================================================

        /// <summary>
        /// Attempt to fill slots with words from the database.
        /// </summary>
        /// <param name="slots">Slots to fill</param>
        /// <param name="database">Word database</param>
        /// <param name="minDifficulty">Minimum word difficulty</param>
        /// <param name="maxDiffitulty">Maximum word difficulty</param>
        /// <param name="theme">Optional theme filter</param>
        /// <returns>True if all slots were filled successfully</returns>
        public bool Solve(List<Slot> slots, CrosswordDatabase database, int minDifficulty, int maxDiffitulty, string theme = null)
        {
            if (slots ==  null || slots.Count == 0)
            {
                Debug.LogWarning("[CrosswordSolver] No slots to fill");
                return true;
            }

            _slotById = new Dictionary<int, Slot>();
            foreach (var slot in slots)
                _slotById[slot.Id] = slot;

            // Pre compute crossings between slots
            var crossings = FindCrossings(slots);

            // Build initial domain for each slots
            var domains = BuildDomains(slots, database, minDifficulty, maxDiffitulty, theme);

            foreach (var slot in slots)
            {
                if (!domains.ContainsKey(slot.Id) || domains[slot.Id].Count == 0)
                {
                    Debug.LogWarning($"[CrosswordSolver] Slot {slot} has no valid words! " +
                        $"(length={slot.Length}, difficulty= {minDifficulty}-{maxDiffitulty}");

                    return false;
                }
            }

            Debug.Log($"[CrosswordSolver] Solving {slots.Count} slots. " +
                $"Domain sizes {string.Join(", ", slots.Select(s => $"{s.Length}L:{domains[s.Id].Count}"))}");

            // Backtracking solve
            int backtracks = 0;
            bool success = Backtrack(slots, domains, crossings, new HashSet<string>(), ref backtracks);

            if (success)
                Debug.Log($"[CrosswordSolver] Solved! {backtracks} backtracks.");
            else
                Debug.LogWarning($"[CrosswordSolver] Failed after {backtracks} backtracks.");

            return success;
        }

        // ============================================================
        // Crossing detection
        // ============================================================

        /// <summary>
        /// A crossing between two slots.
        /// </summary>
        public struct Crossing
        {
            /// <summary>
            /// Index within the first slot word
            /// </summary>
            public int IndexA;

            /// <summary>
            /// Index within the second slot word
            /// </summary>
            public int IndexB;

            /// <summary>
            /// The other slot id
            /// </summary>
            public int OtherSlotId;
        }

        /// <summary>
        /// Find all cell level between slots.
        /// </summary>
        /// <param name="slots"></param>
        /// <returns></returns>
        private Dictionary<int, List<Crossing>> FindCrossings(List<Slot> slots)
        {
            var crossings = new Dictionary<int, List<Crossing>>();
            foreach (var slot in slots)
                crossings[slot.Id] = new List<Crossing>();

            // Build position
            var cellMap = new Dictionary<(int x, int y), List<(int slotId, int index)>>();

            foreach (var slot in slots)
            {
                for (int i  = 0; i < slot.Length;  i++)
                {
                    var pos = slot.GetCellPosition(i);

                    if (!cellMap.ContainsKey(pos))
                        cellMap[pos] = new List<(int slotId, int index)>();

                    foreach (var existing in cellMap[pos])
                    {
                        crossings[slot.Id].Add(new Crossing
                        {
                            IndexA = i,
                            IndexB = existing.index,
                            OtherSlotId = existing.slotId
                        });

                        crossings[existing.slotId].Add(new Crossing
                        {
                            IndexA = existing.index,
                            IndexB = i,
                            OtherSlotId = slot.Id
                        });
                    }

                    cellMap[pos].Add((slot.Id, i));
                }
            }

            int totalCrossings = crossings.Values.Sum(c => c.Count) / 2;
            Debug.Log("[CrosswordSolver] Found " + totalCrossings + " crossings.");

            return crossings;
        }

        // ============================================================
        // Domain building
        // ============================================================

        /// <summary>
        /// Build the initial domain (valid word list) for each slot.
        /// Filter by length, difficulty and optionnaly theme.
        /// </summary>
        /// <param name="slots"></param>
        /// <param name="database"></param>
        /// <param name="minDifficulty"></param>
        /// <param name="maxDifficulty"></param>
        /// <param name="theme"></param>
        /// <returns></returns>
        private Dictionary<int, List<string>> BuildDomains(List<Slot> slots, CrosswordDatabase database, int minDifficulty, int maxDifficulty, string theme)
        {
            var domains = new Dictionary<int, List<string>>();

            foreach (var slot in slots)
            {
                List<WordEntry> candidates;

                if (!string.IsNullOrEmpty(theme))
                {
                    // Theme filtered
                    candidates = database.GetWordsByThemeAndLength(theme, slot.Length);

                    // If not enough themed words, supplement with general pool
                    if (candidates.Count < 5)
                    {
                        var general = database.GetWordsByLengthAndDifficulty(slot.Length, minDifficulty, maxDifficulty);
                        candidates = candidates.Union(general).ToList();
                    }
                }
                else
                {
                    candidates = database.GetWordsByLengthAndDifficulty(slot.Length, minDifficulty, maxDifficulty);

                    // If difficulty range is too strict, widen
                    if (candidates.Count < 3)
                        candidates = database.GetWordsByLength(slot.Length);
                }

                // Extract word strings
                var wordStrings = candidates
                    .Select(w => w.word.ToUpperInvariant())
                    .Distinct()
                    .OrderBy(_ => _rng.Next())
                    .ToList();

                domains[slot.Id] = wordStrings;
            }

            return domains;
        }

        // ============================================================
        // Backtracking solver
        // ============================================================

        /// <summary>
        /// Recursive backtracking with forward checking.
        /// </summary>
        /// <param name="slots">All slots</param>
        /// <param name="domains">Current domains</param>
        /// <param name="crossings">Pre compute crossings</param>
        /// <param name="usedWords">Word already placed</param>
        /// <param name="backtracks">Counter for debug</param>
        /// <returns>True if a solution was found</returns>
        private bool Backtrack(List<Slot> slots, Dictionary<int, List<string>> domains, Dictionary<int, List<Crossing>> crossings, HashSet<string> usedWords, ref int backtracks)
        {
            if (backtracks >= MAX_BACKTRACKS)
                return false;

            // Find the next unfilled slot using MRV
            Slot target = SelectUnfilledSlot(slots, domains);

            // All slots filled
            if (target == null)
                return true;

            // Try each word in the domain
            List<string> candidates = new List<string>(domains[target.Id]);

            foreach (string word in candidates)
            {
                // Skip if word already used elsewhere
                if (usedWords.Contains(word)) continue;

                if (word.Length != target.Length) continue;

                // Check if word is compatible with already placed crossing words
                if (!IsCompatible(target, word, slots, crossings)) continue;

                // Place the word
                target.PlacedString = word;
                usedWords.Add(word);    

                // Forward check
                var pruned = ForwardCheck(target, word, slots, domains, crossings);

                // Check if any crossing domain is now empty
                bool deadEnd = false;
                foreach (var crossing in crossings[target.Id])
                {
                    if (!_slotById.TryGetValue(crossing.OtherSlotId, out Slot other)) continue;
                    if (other.PlacedString == null && domains[other.Id].Count == 0)
                    {
                        deadEnd = true;
                        break;
                    }
                }

                if (!deadEnd)
                {
                    // Recurse
                    if (Backtrack(slots, domains, crossings, usedWords, ref backtracks))
                        return true;
                }

                // Backtrack => undo placement and restore domains
                backtracks++;
                target.PlacedString = null;
                usedWords.Remove(word);
                RestorePruned(domains, pruned);
            }

            return false;
        }

        /// <summary>
        /// Select the unfilled slot with the smallest domain.
        /// This drastically reduces the search space.
        /// </summary>
        private Slot SelectUnfilledSlot(List<Slot> slots, Dictionary<int, List<string>> domains)
        {
            Slot best = null;
            int bestCount = int.MaxValue;

            foreach (var slot in slots)
            {
                if (slot.PlacedString != null) continue; // Already filled

                int count = domains[slot.Id].Count;
                if (count < bestCount)
                {
                    bestCount = count;
                    best = slot;
                }
            }

            return best;
        }

        /// <summary>
        /// Check if a word is compatible with already placed words at crossings.
        /// </summary>
        private bool IsCompatible(
            Slot target, string word,
            List<Slot> slots,
            Dictionary<int, List<Crossing>> crossings)
        {
            foreach (var crossing in crossings[target.Id])
            {
                if (!_slotById.TryGetValue(crossing.OtherSlotId, out Slot other)) continue;
                if (other.PlacedString == null) continue; // Not filled yet

                if (crossing.IndexA < 0 || crossing.IndexA >= word.Length) return false;
                if (crossing.IndexB < 0 || crossing.IndexB >= other.PlacedString.Length) return false;

                if (word[crossing.IndexA] != other.PlacedString[crossing.IndexB])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Forward checking => after placing a word, prune incompatible
        /// words from crossing slots domains.
        /// </summary>
        private Dictionary<int, List<string>> ForwardCheck(
            Slot placed, string word,
            List<Slot> slots,
            Dictionary<int, List<string>> domains,
            Dictionary<int, List<Crossing>> crossings)
        {
            var pruned = new Dictionary<int, List<string>>();

            foreach (var crossing in crossings[placed.Id])
            {
                if (!_slotById.TryGetValue(crossing.OtherSlotId, out Slot other)) continue;
                if (other.PlacedString != null) continue;

                // Bounds check on placed word index
                if (crossing.IndexA < 0 || crossing.IndexA >= word.Length)
                    continue;

                char requiredLetter = word[crossing.IndexA];
                int otherIndex = crossing.IndexB;

                if (!domains.TryGetValue(other.Id, out var otherDomain)) continue;

                var removed = new List<string>();
                var remaining = new List<string>();

                foreach (string candidate in otherDomain)
                {
                    if (otherIndex < 0 || otherIndex >= candidate.Length)
                    {
                        removed.Add(candidate);
                        continue;
                    }

                    if (candidate[otherIndex] == requiredLetter)
                        remaining.Add(candidate);
                    else
                        removed.Add(candidate);
                }

                if (removed.Count > 0)
                {
                    domains[other.Id] = remaining;

                    if (pruned.ContainsKey(other.Id))
                        pruned[other.Id].AddRange(removed);
                    else
                        pruned[other.Id] = removed;
                }
            }

            return pruned;
        }

        /// <summary>
        /// Restore pruned words to domains.
        /// </summary>
        private void RestorePruned(
            Dictionary<int, List<string>> domains,
            Dictionary<int, List<string>> pruned)
        {
            foreach (var kvp in pruned)
            {
                domains[kvp.Key].AddRange(kvp.Value);
            }
        }
    }
}