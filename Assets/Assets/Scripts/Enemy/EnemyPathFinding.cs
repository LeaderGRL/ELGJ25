using NUnit.Framework;
using System.Collections.Generic;
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
