using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Crossatro.Grid
{
    /// <summary>
    /// Crossword solver using MAC (Maintaining Arc Consistency).
    /// </summary>
    public class CrosswordSolver
    {
        // ============================================================
        // Configuration
        // ============================================================

        private const int MAX_BACKTRACKS = 200_000;
        private const float MAX_TIME_SECONDS = 30f;

        // ============================================================
        // Crossing
        // ============================================================

        /// <summary>
        /// Represents an intersection between two slots at a single cell.
        /// MyIndex = letter index within the slot that OWNS this crossing entry.
        /// OtherIndex = letter index within OtherSlotId.
        /// </summary>
        public struct Crossing
        {
            public int MyIndex;
            public int OtherIndex;
            public int OtherSlotId;
        }

        // ============================================================
        // State
        // ============================================================

        private readonly System.Random _rng;

        // Slot lookup
        private Dictionary<int, Slot> _slotById;

        // Crossings per slot: slotId -> list of crossings
        private Dictionary<int, List<Crossing>> _crossings;

        // Domains: slotId -> set of candidate word strings (UPPER, no accents)
        private Dictionary<int, HashSet<string>> _domains;

        // Support counts: _support[slotId][position, letterIdx] = count of words
        // in domain that have letter (char)('A'+letterIdx) at that position.
        // Enables O(1) arc-consistency support checks.
        private Dictionary<int, int[,]> _support;

        // Trail: ordered log of (slotId, word) removals for undo
        private List<(int slotId, string word)> _trail;

        // Used words (no word may appear twice in the grid)
        private HashSet<string> _usedWords;

        // Timer
        private Stopwatch _timer;

        // ============================================================
        // Constructor
        // ============================================================

        public CrosswordSolver(int seed = -1)
        {
            _rng = seed >= 0 ? new System.Random(seed) : new System.Random();
        }

        // ============================================================
        // Public API
        // ============================================================

        /// <summary>
        /// Fill slots with words from the database using MAC search.
        /// Returns true if all slots were filled.
        /// </summary>
        public bool Solve(
            List<Slot> slots,
            CrosswordDatabase database,
            int minDifficulty = 1,
            int maxDifficulty = 9,
            string theme = null)
        {
            if (slots == null || slots.Count == 0)
            {
                Debug.LogWarning("[CrosswordSolver] No slots to fill.");
                return true;
            }

            _timer = Stopwatch.StartNew();

            // Build data structures
            _slotById = new Dictionary<int, Slot>();
            foreach (var s in slots) _slotById[s.Id] = s;

            FindCrossings(slots);
            BuildDomains(slots, database, minDifficulty, maxDifficulty, theme);

            // Check feasibility
            foreach (var slot in slots)
            {
                if (_domains[slot.Id].Count == 0)
                {
                    Debug.LogWarning($"[CrosswordSolver] Slot {slot} has empty domain " +
                                     $"(length={slot.Length}).");
                    return false;
                }
            }

            // Log domain sizes
            var domainInfo = string.Join(", ", slots.Select(s =>
                $"{s.Length}L:{_domains[s.Id].Count}"));
            Debug.Log($"[CrosswordSolver] Solving {slots.Count} slots. " +
                      $"Domains: {domainInfo}");

            _usedWords = new HashSet<string>();
            _trail = new List<(int slotId, string word)>();

            // Initial AC-3 pass (prune before search even starts)
            var initialArcs = GetAllArcs();
            if (!RunAC3(initialArcs))
            {
                Debug.LogWarning("[CrosswordSolver] Initial AC-3 found inconsistency.");
                return false;
            }

            // Log domains after AC-3
            int totalPruned = 0;
            foreach (var s in slots)
                totalPruned += _domains[s.Id].Count;
            Debug.Log($"[CrosswordSolver] After initial AC-3: " +
                      $"{totalPruned} total candidates remain.");

            // Search
            int backtracks = 0;

            bool success = Backtrack(slots, ref backtracks);

            _timer.Stop();

            if (success)
                Debug.Log($"[CrosswordSolver] Solved! {backtracks} backtracks, " +
                          $"{_timer.ElapsedMilliseconds}ms.");
            else
                Debug.LogWarning($"[CrosswordSolver] Failed after {backtracks} backtracks, " +
                                 $"{_timer.ElapsedMilliseconds}ms.");

            return success;
        }

        // ============================================================
        // Crossing Detection
        // ============================================================

        private void FindCrossings(List<Slot> slots)
        {
            _crossings = new Dictionary<int, List<Crossing>>();
            foreach (var s in slots)
                _crossings[s.Id] = new List<Crossing>();

            // Map each cell to the slots that use it
            var cellMap = new Dictionary<(int x, int y), List<(int slotId, int index)>>();

            foreach (var slot in slots)
            {
                for (int i = 0; i < slot.Length; i++)
                {
                    var pos = slot.GetCellPosition(i);
                    if (!cellMap.ContainsKey(pos))
                        cellMap[pos] = new List<(int, int)>();

                    // Create crossings with all existing entries at this cell
                    foreach (var existing in cellMap[pos])
                    {
                        _crossings[slot.Id].Add(new Crossing
                        {
                            MyIndex = i,
                            OtherIndex = existing.index,
                            OtherSlotId = existing.slotId
                        });
                        _crossings[existing.slotId].Add(new Crossing
                        {
                            MyIndex = existing.index,
                            OtherIndex = i,
                            OtherSlotId = slot.Id
                        });
                    }

                    cellMap[pos].Add((slot.Id, i));
                }
            }

            int total = _crossings.Values.Sum(c => c.Count) / 2;
            Debug.Log($"[CrosswordSolver] Found {total} crossings.");
        }

        // ============================================================
        // Domain Building
        // ============================================================

        private void BuildDomains(
            List<Slot> slots, CrosswordDatabase database,
            int minDiff, int maxDiff, string theme)
        {
            _domains = new Dictionary<int, HashSet<string>>();
            _support = new Dictionary<int, int[,]>();

            foreach (var slot in slots)
            {
                // Get candidates from database
                List<WordEntry> candidates;
                if (!string.IsNullOrEmpty(theme))
                {
                    candidates = database.GetWordsByThemeAndLength(theme, slot.Length);
                    if (candidates.Count < 5)
                    {
                        var general = database.GetWordsByLengthAndDifficulty(
                            slot.Length, minDiff, maxDiff);
                        candidates = candidates.Union(general).ToList();
                    }
                }
                else
                {
                    candidates = database.GetWordsByLengthAndDifficulty(
                        slot.Length, minDiff, maxDiff);
                    if (candidates.Count < 3)
                        candidates = database.GetWordsByLength(slot.Length);
                }

                // Convert to normalized uppercase strings, dedup
                var wordSet = new HashSet<string>();
                foreach (var c in candidates)
                {
                    string normalized = NormalizeWord(c.word.ToUpperInvariant());
                    if (normalized.Length == slot.Length)
                        wordSet.Add(normalized);
                }

                _domains[slot.Id] = wordSet;

                // Build support counts for this slot
                InitSupportCounts(slot.Id, slot.Length, wordSet);
            }
        }

        /// <summary>
        /// Initialize support count array for a slot from its initial domain.
        /// </summary>
        private void InitSupportCounts(int slotId, int length, HashSet<string> domain)
        {
            var counts = new int[length, 26];
            foreach (var word in domain)
            {
                for (int i = 0; i < word.Length; i++)
                {
                    int idx = word[i] - 'A';
                    if (idx >= 0 && idx < 26)
                        counts[i, idx]++;
                }
            }
            _support[slotId] = counts;
        }

        // ============================================================
        // AC-3 Engine
        // ============================================================

        /// <summary>
        /// Run AC-3 arc consistency algorithm.
        /// Each arc (A, B) means: "ensure every word in domain(A)
        /// has at least one supporting word in domain(B) at their crossing."
        /// Returns false if any domain becomes empty (inconsistency).
        /// </summary>
        private bool RunAC3(Queue<(int a, int b)> queue)
        {
            var inQueue = new HashSet<(int, int)>(queue);

            while (queue.Count > 0)
            {
                var (a, b) = queue.Dequeue();
                inQueue.Remove((a, b));

                // Skip if either slot is already assigned
                if (_slotById[a].PlacedString != null) continue;

                if (Revise(a, b))
                {
                    if (_domains[a].Count == 0) return false;

                    // Domain of A changed — re-check all neighbors of A (except B)
                    foreach (var cx in _crossings[a])
                    {
                        if (cx.OtherSlotId == b) continue;
                        if (_slotById[cx.OtherSlotId].PlacedString != null) continue;

                        var arc = (cx.OtherSlotId, a);
                        if (!inQueue.Contains(arc))
                        {
                            queue.Enqueue(arc);
                            inQueue.Add(arc);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Make slot A arc-consistent with slot B.
        /// Remove from A's domain any word whose crossing letter
        /// has no support in B's domain.
        /// Returns true if any word was removed.
        /// </summary>
        private bool Revise(int slotAId, int slotBId)
        {
            bool revised = false;

            // Find all crossings between A and B
            foreach (var cx in _crossings[slotAId])
            {
                if (cx.OtherSlotId != slotBId) continue;

                int posInA = cx.MyIndex;
                int posInB = cx.OtherIndex;

                var toRemove = new List<string>();
                foreach (var word in _domains[slotAId])
                {
                    char letter = word[posInA];
                    if (!HasSupport(slotBId, posInB, letter))
                        toRemove.Add(word);
                }

                foreach (var word in toRemove)
                {
                    RemoveFromDomain(slotAId, word);
                    revised = true;
                }
            }

            return revised;
        }

        /// <summary>
        /// O(1) check: does slot's current domain contain at least one word
        /// with the given letter at the given position?
        /// </summary>
        private bool HasSupport(int slotId, int position, char letter)
        {
            // If slot is assigned, check the placed word directly
            var slot = _slotById[slotId];
            if (slot.PlacedString != null)
            {
                return position >= 0 && position < slot.PlacedString.Length
                    && slot.PlacedString[position] == letter;
            }

            int idx = letter - 'A';
            if (idx < 0 || idx >= 26) return false;
            if (position < 0) return false;

            var counts = _support[slotId];
            if (position >= counts.GetLength(0)) return false;

            return counts[position, idx] > 0;
        }

        /// <summary>
        /// Get the initial queue of all arcs in the problem.
        /// </summary>
        private Queue<(int a, int b)> GetAllArcs()
        {
            var queue = new Queue<(int, int)>();
            foreach (var kvp in _crossings)
            {
                int slotId = kvp.Key;
                foreach (var cx in kvp.Value)
                    queue.Enqueue((slotId, cx.OtherSlotId));
            }
            return queue;
        }

        // ============================================================
        // Domain Management (trail-based)
        // ============================================================

        /// <summary>
        /// Remove a word from a slot's domain and update support counts.
        /// The removal is logged in the trail for undo.
        /// </summary>
        private void RemoveFromDomain(int slotId, string word)
        {
            if (!_domains[slotId].Remove(word)) return;

            var counts = _support[slotId];
            for (int i = 0; i < word.Length; i++)
            {
                int idx = word[i] - 'A';
                if (idx >= 0 && idx < 26)
                    counts[i, idx]--;
            }

            _trail.Add((slotId, word));
        }

        /// <summary>
        /// Undo a removal: add the word back to domain and support counts.
        /// </summary>
        private void UndoRemoval(int slotId, string word)
        {
            _domains[slotId].Add(word);

            var counts = _support[slotId];
            for (int i = 0; i < word.Length; i++)
            {
                int idx = word[i] - 'A';
                if (idx >= 0 && idx < 26)
                    counts[i, idx]++;
            }
        }

        /// <summary>Save current trail position for later restore.</summary>
        private int SaveState() => _trail.Count;

        /// <summary>Restore all removals back to the saved trail position.</summary>
        private void RestoreState(int savePoint)
        {
            for (int i = _trail.Count - 1; i >= savePoint; i--)
            {
                var (slotId, word) = _trail[i];
                UndoRemoval(slotId, word);
            }
            _trail.RemoveRange(savePoint, _trail.Count - savePoint);
        }

        // ============================================================
        // Backtracking Search (MAC)
        // ============================================================

        private bool Backtrack(List<Slot> slots, ref int backtracks)
        {
            if (backtracks >= MAX_BACKTRACKS) return false;
            if (_timer.Elapsed.TotalSeconds > MAX_TIME_SECONDS) return false;

            // Find most constrained unfilled slot (MRV heuristic)
            Slot target = SelectUnfilledSlot(slots);
            if (target == null) return true; // All slots filled!

            // Get candidates, shuffled for variety
            var candidates = _domains[target.Id]
                .Where(w => !_usedWords.Contains(w))
                .OrderBy(_ => _rng.Next())
                .ToList();

            foreach (var word in candidates)
            {
                if (backtracks >= MAX_BACKTRACKS) return false;
                if (_timer.Elapsed.TotalSeconds > MAX_TIME_SECONDS) return false;

                int savePoint = SaveState();

                // Assign
                target.PlacedString = word;
                _usedWords.Add(word);

                // Propagate constraints with AC-3
                bool consistent = AssignAndPropagate(target, word);

                if (consistent)
                {
                    if (Backtrack(slots, ref backtracks))
                        return true;
                }

                // Undo assignment
                backtracks++;
                target.PlacedString = null;
                _usedWords.Remove(word);
                RestoreState(savePoint);
            }

            return false;
        }

        /// <summary>
        /// After assigning a word to a slot, prune crossing domains
        /// and run AC-3 to propagate constraints.
        /// Returns false if any domain becomes empty.
        /// </summary>
        private bool AssignAndPropagate(Slot target, string word)
        {
            var ac3Queue = new Queue<(int, int)>();

            foreach (var cx in _crossings[target.Id])
            {
                int otherId = cx.OtherSlotId;
                var otherSlot = _slotById[otherId];
                if (otherSlot.PlacedString != null) continue;

                char requiredLetter = word[cx.MyIndex];
                int posInOther = cx.OtherIndex;

                // Remove words that don't match the required letter
                var toRemove = new List<string>();
                foreach (var candidate in _domains[otherId])
                {
                    if (candidate[posInOther] != requiredLetter)
                        toRemove.Add(candidate);
                }

                bool changed = false;
                foreach (var w in toRemove)
                {
                    RemoveFromDomain(otherId, w);
                    changed = true;
                }

                if (_domains[otherId].Count == 0) return false;

                // If domain changed, enqueue arcs for further propagation
                if (changed)
                {
                    foreach (var otherCx in _crossings[otherId])
                    {
                        if (otherCx.OtherSlotId == target.Id) continue;
                        if (_slotById[otherCx.OtherSlotId].PlacedString != null) continue;
                        ac3Queue.Enqueue((otherCx.OtherSlotId, otherId));
                    }
                }
            }

            // Cascade propagation
            return RunAC3(ac3Queue);
        }

        /// <summary>
        /// MRV (Minimum Remaining Values) heuristic:
        /// pick the unfilled slot with the smallest domain.
        /// Ties broken by most crossings (degree heuristic).
        /// </summary>
        private Slot SelectUnfilledSlot(List<Slot> slots)
        {
            Slot best = null;
            int bestCount = int.MaxValue;
            int bestDegree = -1;

            foreach (var slot in slots)
            {
                if (slot.PlacedString != null) continue;

                int count = _domains.TryGetValue(slot.Id, out var d) ? d.Count : 0;
                int degree = _crossings.TryGetValue(slot.Id, out var c) ? c.Count : 0;

                if (count < bestCount || (count == bestCount && degree > bestDegree))
                {
                    bestCount = count;
                    bestDegree = degree;
                    best = slot;
                }
            }

            return best;
        }

        // ============================================================
        // Text Normalization (French accent stripping)
        // ============================================================

        /// <summary>
        /// Normalize a word to A-Z only (strip French accents).
        /// Input should already be uppercase.
        /// </summary>
        private static string NormalizeWord(string word)
        {
            bool needsNormalization = false;
            foreach (char c in word)
            {
                if (c < 'A' || c > 'Z')
                {
                    needsNormalization = true;
                    break;
                }
            }

            if (!needsNormalization) return word;

            var sb = new System.Text.StringBuilder(word.Length);
            foreach (char c in word)
                sb.Append(StripAccent(c));
            return sb.ToString();
        }

        private static char StripAccent(char c)
        {
            if (c >= 'A' && c <= 'Z') return c;
            switch (c)
            {
                case 'À': case 'Á': case 'Â': case 'Ã': case 'Ä': case 'Å': return 'A';
                case 'È': case 'É': case 'Ê': case 'Ë': return 'E';
                case 'Ì': case 'Í': case 'Î': case 'Ï': return 'I';
                case 'Ò': case 'Ó': case 'Ô': case 'Õ': case 'Ö': return 'O';
                case 'Ù': case 'Ú': case 'Û': case 'Ü': return 'U';
                case 'Ç': return 'C';
                case 'Ñ': return 'N';
                case 'Ÿ': case 'Ý': return 'Y';
                case 'Œ': return 'O';
                case 'Æ': return 'A';
                default: return c;
            }
        }
    }
}