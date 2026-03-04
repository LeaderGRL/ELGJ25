using System.Runtime.ExceptionServices;
using UnityEngine;

namespace Crossatro.Grid
{
    /// <summary>
    /// Generate random crossword grid masks.
    /// </summary>
    public static class MaskGenerator
    {
        // ============================================================
        // API
        // ============================================================

        /// <summary>
        /// Generate a random grid mask.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="seed"></param>
        /// <param name="blackRatio"></param>
        /// <param name="minSlotLength"></param>
        /// <returns></returns>
        public static GridMask Generate(int width = 10, int height = 10, int seed = -1, float blackRatio = 0.25f, int minSlotLength = 2)
        {
            var rng = seed >= 0 ? new System.Random(seed) : new System.Random();

            // Generate with retries
            for (int i = 0; i < 50; i++)
            {
                char[,] grid = GenerateRandomPattern(width, height, rng, blackRatio);

                // Place heart near center
                PlaceHeart(grid, width, height);

                // Validation
                if (ValidateMask(grid, width, height, minSlotLength))
                    return CreateMaskFromGrid(grid, width, height);
            }

            Debug.LogWarning("[MaskGenerator] Could not generate valid mask after 50 attempts." +
                "Using gallback open grid.");

            return GenerateFallback(width, height);
        }

        // ============================================================
        // Pattern generation
        // ============================================================
        
        /// <summary>
        /// Generate a random grid pattern.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="rng"></param>
        /// <param name="blackRatio"></param>
        /// <returns></returns>
        private static char[,] GenerateRandomPattern(int width, int height,  System.Random rng, float blackRatio)
        {
            char[,] grid = new char[width, height];

            // Start all white
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    grid[x, y] = '.';

            int totalCells = width * height;
            int targetBlack = Mathf.RoundToInt(totalCells * blackRatio);
            int currentBlack = 0;
            int maxAttemps = totalCells * 4;
            int attemps = 0;

            while (currentBlack < targetBlack && attemps < maxAttemps)
            {
                attemps++;

                int x = rng.Next(width);
                int y = rng.Next(height);

                // Skip center area
                int cx = width / 2;
                int cy = height / 2;
                if (Mathf.Abs(x - cx) <= 1 && Mathf.Abs(y - cy) <= 1)
                    continue;

                if (grid[x, y] != '.') continue;

                grid[x, y] = '#';
                currentBlack++;
            }

            // Remove isolated single white cells
            CleanupShortSlots(grid, width, height);

            return grid;
        }

        /// <summary>
        /// Remove white cells that would create slots shorter than 2.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private static void CleanupShortSlots(char[,] grid, int width, int height)
        {
            bool changed = true;
            int passes = 0;

            while (changed && passes < 10)
            {
                changed = false;
                passes++;

                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        if (grid[x, y] != '.') continue;

                        // Check horizontal slot length
                        int hLen = GetSlotLength(grid, x, y, width, height, true);
                        
                        // Check vertical slot length
                        int vLen = GetSlotLength(grid, x, y, width, height, false);

                        // If this cell is isolated, make it black
                        if (hLen == 1 &&  vLen == 1)
                        {
                            grid[x, y] = '#';
                            changed = true;
                        }
                    }
            }
        }

        /// <summary>
        /// Get the length of the white cell slot containing.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="horizontal"></param>
        /// <returns></returns>
        private static int GetSlotLength(char[,] grid, int x, int y, int width, int height, bool horizontal)
        {
            if (grid[x, y] != '.') return 0;

            int len = 1;

            // Count backward
            int bx = x, by = y;
            while (true)
            {
                if (horizontal) bx--; else by--;
                if (bx < 0 || by < 0 ||bx >= width || bx >= height) break;
                if (grid[bx, by] != '.') break;
                len++;
            }

            // Count forward
            int fx = x, fy = y;
            while (true)
            {
                if (horizontal) fx++; else fy++;
                if (fx < 0 || fy < 0 || fx >= width || fy >= height) break;
                if (grid[fx, fy] != '.') break;
                len++;
            }

            return len;
        }

        // ============================================================
        // Heart placement
        // ============================================================

        /// <summary>
        /// Place the heart on an empty cell near the center.
        /// </summary>
        /// <param name=""></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void PlaceHeart(char[,] grid, int width, int height)
        {
            int cx = width / 2;
            int cy = height / 2;

            // Search outward from center for a suitable empty position.
            // Find a white cell near center and convert it to heart
            // Or find an adjacent black cell and convert it to heart.
            int bestX = -1, bestY = -1;
            float bestScore = float.MaxValue;

            for (int radius = 0; radius < Mathf.Max(width, height); radius++)
            {
                for (int dy = -radius;  dy <= radius; dy++)
                {
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius) continue;

                        int x = cx + dx;
                        int y = cy + dy;

                        if (x < 0 || x >= width || y >= height) continue;

                        // Must be a black cell
                        if (grid[x, y] != '#') continue;

                        // Must have at least one adjacent white cell
                        int adj = CountAdjacentWhite(grid, x, y, width, height);
                        if (adj == 0) continue;

                        float dist = dx * dx + dy * dy;
                        float score = dist - adj * 3f;

                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestX = x;
                            bestY = y;
                        }
                    }
                }

                if (bestX >= 0) break;
            }

            if (bestX >= 0)
                grid[bestX, bestY] = 'H';
            else
                grid[cx, cy] = 'H'; // Fallback
        }

        private static int CountAdjacentWhite(char[,] grid, int x, int y, int width, int height)
        {
            int count = 0;
            int[,] dirs = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };

            for (int i = 0; i < 4; i++)
            {
                int nx = x + dirs[i, 0];
                int ny = y + dirs[i, 1];
                if (nx >= 0 && nx < width && ny >= 0 && ny < height && grid[nx, ny] == '.')
                    count++;
            }

            return count;
        }

        // ============================================================
        // Validation
        // ============================================================

        /// <summary>
        /// Validate that the mask produces a usable crossword grid.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="minSlotLength"></param>
        /// <returns></returns>
        private static bool ValidateMask(char[,] grid, int width, int height, int minSlotLength)
        {
            // Find first white cell
            int startX = -1, startY = -1;
            int totalWalkable = 0;

            for (int y = 0; y < height && startX < 0; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[x, y] == '.' || grid[x, y] == 'H')
                    {
                        totalWalkable++;
                        if (startX < 0) { startX = x; startY = y; }
                    }
                }
            }

            // Count remaining walkable cells
            for (int y = (startX < 0 ? 0 : startY); y < height; y++)
                for (int x = 0; x < width; x++)
                    if ((y > startY || x > startX) && (grid[x, y] == '.' || grid[x, y] == 'H'))
                        totalWalkable++;

            // Recount properly
            totalWalkable = 0;
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (grid[x, y] == '.' || grid[x, y] == 'H')
                        totalWalkable++;

            if (totalWalkable < 4) return false;

            // BFS connectivity check
            var visited = new bool[width, height];
            var queue = new System.Collections.Generic.Queue<(int x, int y)>();
            queue.Enqueue((startX, startY));
            visited[startX, startY] = true;
            int reachable = 1;

            int[,] dirs = { { 0, 1 }, { 0, -1 }, { 1, 0 }, { -1, 0 } };

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int nx = cx + dirs[i, 0];
                    int ny = cy + dirs[i, 1];

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                    if (visited[nx, ny]) continue;
                    if (grid[nx, ny] != '.' && grid[nx, ny] != 'H') continue;

                    visited[nx, ny] = true;
                    reachable++;
                    queue.Enqueue((nx, ny));
                }
            }

            Debug.Log("[MaskGenerator] reachable : " + reachable +" -- Walkable: " + totalWalkable);


            // All walkable cells must be reachable
            if (reachable != totalWalkable) return false;

            //// Check minimum slot length (horizontal)
            //for (int y = 0; y < height; y++)
            //{
            //    int runLen = 0;
            //    for (int x = 0; x <= width; x++)
            //    {
            //        if (x < width && grid[x, y] == '.')
            //        {
            //            runLen++;
            //        }
            //        else
            //        {
            //            if (runLen == 1) return false;
            //            runLen = 0;
            //        }
            //    }
            //}

            //Debug.Log("[MaskGenerator] Horizontal length ok!");


            //// Check minimum slot length (vertical)
            //for (int x = 0; x < width; x++)
            //{
            //    int runLen = 0;
            //    for (int y = 0; y <= height; y++)
            //    {
            //        if (y < height && grid[x, y] == '.')
            //        {
            //            runLen++;
            //        }
            //        else
            //        {
            //            if (runLen == 1) return false;
            //            runLen = 0;
            //        }
            //    }
            //}

            //Debug.Log("[MaskGenerator] Vertical length ok!");


            return true;
        }

        // ============================================================
        // Fallback
        // ============================================================

        /// <summary>
        /// Generate a simple open grid as fallback.
        /// </summary>
        private static GridMask GenerateFallback(int width, int height)
        {
            char[,] grid = new char[width, height];

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    grid[x, y] = '.';

            // Black cells at corners
            grid[0, 0] = '#';
            grid[width - 1, 0] = '#';
            grid[0, height - 1] = '#';
            grid[width - 1, height - 1] = '#';

            // Heart at center
            grid[width / 2, height / 2] = 'H';

            return CreateMaskFromGrid(grid, width, height);
        }

        // ============================================================
        // Utility
        // ============================================================

        private static GridMask CreateMaskFromGrid(char[,] grid, int width, int height)
        {
            var mask = ScriptableObject.CreateInstance<GridMask>();

            string[] rows = new string[height];
            for (int y = 0; y < height; y++)
            {
                char[] chars = new char[width];
                for (int x = 0; x < width; x++)
                    chars[x] = grid[x, y];
                rows[y] = new string(chars);
            }

            mask.SetRows(rows);
            return mask;
        }
    }
}
