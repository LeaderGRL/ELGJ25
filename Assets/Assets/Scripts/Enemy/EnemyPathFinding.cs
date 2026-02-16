using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Crossatro.Enemy
{
    /// <summary>
    /// BFS pathfinding on the grid.
    /// Finds the shortest path from an enemy position to the heart, avoiding obstacles.
    /// </summary>
    public static class EnemyPathFinding
    {
        /// <summary>
        /// 4 directional movement.
        /// </summary>
        private static readonly Vector2Int[] Directions = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
        };

        // ============================================================
        // PathFinding
        // ============================================================

        /// <summary>
        /// Find the shortest path from start to target using BFS.
        /// </summary>
        /// <param name="start">Starting grid position</param>
        /// <param name="target">Target grid position</param>
        /// <param name="isWalkable">Function that return true if a position can be walked on</param>
        /// <param name="maxSearchDepth">Safety limit to prevent infinite search</param>
        /// <returns>List of positions from start to target</returns>
        public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int target, System.Func<Vector2Int, bool> isWalkable, int maxSearchDepth = 200)
        {
            if (start == target) return new List<Vector2Int>();

            // BFS setup
            var queue = new Queue<Vector2Int>();
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            var visited = new HashSet<Vector2Int>();

            queue.Enqueue(start);
            visited.Add(start);

            int nodeExplored = 0;

            while (queue.Count > 0 && nodeExplored < maxSearchDepth)
            {
                Vector2Int current = queue.Dequeue();
                nodeExplored++;

                // Check all 4 neighbors
                for (int i = 0; i < Directions.Length; i++)
                {
                    Vector2Int neighbor = current + Directions[i];

                    bool canWalk = neighbor == target || isWalkable(neighbor);
                    if (!canWalk) continue;

                    visited.Add(neighbor);
                    cameFrom[neighbor] = current;

                    // Found the target => Reconstruct path
                    if (neighbor == target)
                    {
                        return ReconstructPath(cameFrom, start, target);
                    }

                    queue.Enqueue(neighbor);
                }
            } 
            
            return new List<Vector2Int>();
        }

        // ============================================================
        // Grid Topology
        // ============================================================

        /// <summary>
        /// Calculate the center of all tile positions then return the closest tile to that center.
        /// </summary>
        /// <param name="tilePositions"></param>
        /// <returns></returns>
        public static Vector2Int FindCenterTile(HashSet<Vector2Int> tilePositions)
        {
            if (tilePositions.Count == 0)
            {
                Debug.LogError("[EnemyPathFinding] Cannot find center of empty grid");
                return Vector2Int.zero;
            }

            float sumX = 0;
            float sumY = 0;

            foreach (var pos in tilePositions)
            {
                sumX += pos.x;
                sumY += pos.y;
            }

            float centerX = sumX / tilePositions.Count;
            float centerY = sumY / tilePositions.Count;

            // Find the closest tile to the center
            Vector2Int closest = Vector2Int.zero;
            float closestDist = float.MaxValue;

            foreach (var pos in tilePositions)
            {
                float dx = pos.x - centerX;
                float dy = pos.y - centerY;
                float dist = dx * dx + dy * dy;

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = pos;
                }
            }

            return closest;
        }

        /// <summary>
        /// Find all border tiles.
        /// </summary>
        /// <param name="tilePositions"></param>
        /// <returns></returns>
        public static List<Vector2Int> FindBorderTiles(HashSet<Vector2Int> tilePositions)
        {
            var borders = new List<Vector2Int>();

            foreach (var pos in tilePositions)
            {
                int neighborCount = 0;
                for (int i = 0; i < Directions.Length; i++)
                {
                    if (tilePositions.Contains(pos + Directions[i]))
                        neighborCount++;
                }

                if (neighborCount < 4)
                    borders.Add(pos);
            }

            return borders;
        }

        public static List<Vector2Int> FindEndpointTiles(HashSet<Vector2Int> tilePositions)
        {
            var endpoints = new List<Vector2Int>();

            foreach (var pos in tilePositions)
            {
                int neighborCount = 0;

                for (int i = 0; i < Directions.Length;i++)
                {
                    if (tilePositions.Contains(pos + Directions[i]))
                        neighborCount++;
                }

                if (neighborCount == 1)
                    endpoints.Add(pos);
            }

            return endpoints;
        }

        /// <summary>
        /// Select the best spawn positions, prioritizing distance from heart.
        /// </summary>
        /// <param name="tilePositions"></param>
        /// <param name="heartPosition"></param>
        /// <param name="count"></param>
        /// <param name="minPathDistance"></param>
        /// <returns></returns>
        public static List<Vector2Int> SelectSpawnPosition(HashSet<Vector2Int> tilePositions, Vector2Int heartPosition, int count = 10, int minPathDistance = 4)
        {
            // BFS distances from heart to all reachable tiles
            var distances = BFSDistancesFromHeart(heartPosition, tilePositions);

            var endpoints = FindEndpointTiles(tilePositions);
            var border = FindBorderTiles(tilePositions);
            var endpointSet = new HashSet<Vector2Int>(endpoints);

            // Build candidate list
            var candidates = new List<(Vector2Int pos, int dist, bool isEndPoint)>();

            foreach (var pos in endpoints)
            {
                if (distances.TryGetValue(pos, out int dist) && dist >= minPathDistance)
                    candidates.Add((pos, dist, true));
            }

            foreach (var pos in border)
            {
                if (endpointSet.Contains(pos)) continue;
                if (distances.TryGetValue(pos, out int dist) && dist >= minPathDistance)
                    candidates.Add((pos, dist, false));
            }

            // Sort: endpoints fist, then by path distance descending
            candidates.Sort((a, b) =>
            {
                if (a.isEndPoint != b.isEndPoint)
                    return a.isEndPoint ? -1 : 1;

                return b.dist.CompareTo(a.dist);
            });

            return candidates.Take(count).Select(c => c.pos).ToList();
        }

        // ============================================================
        // Distance utility
        // ============================================================

        /// <summary>
        /// Get the Manhattan distance between two position.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int ManhattanDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// Check if a position is within range of a target.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="target"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool IsInRange(Vector2Int position, Vector2Int target, int range)
        {
            return ManhattanDistance(position, target) <= range;
        }

        /// <summary>
        /// Compute BFS distance from heart to every reachable tile.
        /// </summary>
        /// <param name="heartPosition"></param>
        /// <param name="tilePositions"></param>
        /// <returns></returns>
        public static Dictionary<Vector2Int, int> BFSDistancesFromHeart(Vector2Int heartPosition, HashSet<Vector2Int> tilePositions)
        {
            var distances = new Dictionary<Vector2Int, int>();
            var queue = new Queue<Vector2Int>();

            if (!tilePositions.Contains(heartPosition)) return distances;

            queue.Enqueue(heartPosition);
            distances[heartPosition] = 0;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                int currentDist = distances[current];

                for (int i = 0; i < Directions.Length; i++)
                {
                    Vector2Int neighbor = current + Directions[i];

                    if (!tilePositions.Contains(neighbor)) continue;
                    if (distances.ContainsKey(neighbor)) continue;

                    distances[neighbor] = currentDist + 1;
                    queue.Enqueue(neighbor);
                }
            }

            return distances;
        }

        // ============================================================
        // Internal
        // ============================================================

        /// <summary>
        /// Reconstruct the path from BFS results.
        /// </summary>
        /// <param name="cameFrom"></param>
        /// <param name="start"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int target)
        {
            var path = new List<Vector2Int>();
            Vector2Int current = target;

            while (current != start)
            {
                path.Add(current);
                current = cameFrom[current];
            }

            // Reverse so path goes from start to target
            path.Reverse();
            return path;
        }


    }
}
