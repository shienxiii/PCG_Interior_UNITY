using System.Collections.Generic;
using UnityEngine;
using ProceduralInterior.Types;

namespace ProceduralInterior.FunctionLib
{
    public static class GridParser
    {
        /// <summary>
        /// Parse the provided grid to determine if the samples to parse
        /// matches any of the provided SampleState on the whitelist as well as the room index
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="parsedSamples">index to the samples on the grid to check</param>
        /// <param name="roomIndex">room index to match</param>
        /// <param name="whitelist">SampleState(s) to match</param>
        /// <returns></returns>
        public static bool ParseSample_Whitelist(Sample[,] grid, List<Vector2Int> parsedSamples, int roomIndex,
            params SampleState[] whitelist)
        {
            if (grid == null || parsedSamples == null)
                return false;

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            foreach (Vector2Int point in parsedSamples)
            {
                // Outside the grid.
                if (point.x < 0 || point.x >= width ||
                    point.y < 0 || point.y >= height)
                    return false;

                Sample sample = grid[point.x, point.y];

                // test that sample is valid and part of the provided roomIndex
                if (sample == null || sample.RoomIndex != roomIndex)
                    return false;

                bool whitelistMatch = false;

                for (int i = 0; i < whitelist.Length && !whitelistMatch; i++)
                    if (sample.State == whitelist[i])
                        whitelistMatch = true;

                // any fail and we return false
                if (!whitelistMatch)
                    return false;
            }

            return true;
        }

        public static bool CanOccupy(Sample[,] grid, List<Vector2Int> toOccupy, int roomIndex)
        {
            if (grid == null || toOccupy == null)
                return false;

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            foreach (Vector2Int point in toOccupy)
            {
                // Outside the grid.
                if (point.x < 0 || point.x >= width ||
                    point.y < 0 || point.y >= height)
                    return false;

                Sample sample = grid[point.x, point.y];

                // sample must not be null, be free and match the roomIndex
                if (sample == null ||
                    sample.State != SampleState.FREE ||
                    sample.RoomIndex != roomIndex)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determine if the provided points can be reserved
        /// </summary>
        /// <param name="grid">the grid to occupy</param>
        /// <param name="toReserve">points of sample to reserve</param>
        /// <param name="roomIndex">room index of the samples to reserve</param>
        /// <param name="requireAll">Whether to require all points to be reserved or only partial</param>
        /// <returns></returns>
        public static bool CanReserve(Sample[,] grid, List<Vector2Int> toReserve, int roomIndex)
        {
            if (grid == null || toReserve == null)
                return false;

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            foreach (Vector2Int point in toReserve)
            {
                // Outside the grid.
                if (point.x < 0 || point.x >= width ||
                    point.y < 0 || point.y >= height)
                    return false;

                Sample sample = grid[point.x, point.y];

                // sample must not be null or occupied, and must match the roomIndex
                if (sample == null ||
                    sample.State == SampleState.OCCUPIED ||
                    sample.RoomIndex != roomIndex)
                    return false;
            }

            return true;
        }

        public static bool AreReservedSamplesReachable(Sample[,] grid, List<Vector2Int> reservedSamples)
        {
            if (grid == null || grid.Length == 0)
                return false;

            if (reservedSamples == null || reservedSamples.Count == 0)
                return true;

            Vector2Int start = reservedSamples[0];

            if (!IsValidPoint(grid, start))
                return false;

            Sample startSample = grid[start.x, start.y];

            if (startSample.State != SampleState.RESERVED)
                return false;

            int roomIndex = startSample.RoomIndex;

            int width = grid.GetLength(0);
            int height = grid.GetLength(1);

            // Copy the list so the original is not modified.
            HashSet<Vector2Int> remaining = new HashSet<Vector2Int>(reservedSamples);

            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            Queue<Vector2Int> queue = new Queue<Vector2Int>();

            queue.Enqueue(start);
            visited.Add(start);
            remaining.Remove(start);

            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            while (queue.Count > 0 && remaining.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                foreach (Vector2Int direction in directions)
                {
                    Vector2Int next = current + direction;

                    if (!IsValidPoint(grid, next))
                        continue;

                    if (visited.Contains(next))
                        continue;

                    Sample sample = grid[next.x, next.y];

                    if (sample.State == SampleState.OCCUPIED ||
                        sample.RoomIndex != roomIndex)
                        continue;

                    visited.Add(next);
                    queue.Enqueue(next);

                    // Remove the reserved point once we reach it.
                    remaining.Remove(next);
                }
            }

            // Any remaining points were unreachable.
            return remaining.Count == 0;
        }

        /// <summary>
        /// Parse a chunk of sample to a bool matrix based off a
        /// given GridSpan matching the given roomIndex and SampleStates.
        /// </summary>
        /// <param name="grid">the grid to extract from</param>
        /// <param name="span">the span to extract</param>
        /// <param name="matchCount">number of true in the return matrix</param>
        /// <param name="roomIndex">room index to match</param>
        /// <param name="states">sample states to match</param>
        /// <returns>matrix of bool indicating the samples matching the given conditions</returns>
        public static bool[,] ExtractSpanWithConditions(Sample[,] grid, GridSpan span, out int matchCount,
            int roomIndex, params SampleState[] states)
        {
            matchCount = 0;

            if (grid == null ||
                !IsPointWithinGrid(grid, span.Min) ||
                !IsPointWithinGrid(grid, span.Max))
                return null;

            Vector2Int start = span.Min;
            Vector2Int dim = span.GetDimension();
            bool[,] matrix = new bool[dim.x, dim.y];

            for (int u = 0; u < dim.x; u++)
            {
                for (int v = 0; v < dim.y; v++)
                {
                    Sample sample = grid[start.x + u, start.y + v];

                    if (sample != null &&
                        sample.RoomIndex == roomIndex &&
                        SampleStateMatch(sample, states))
                    {
                        matrix[u, v] = true;
                        matchCount++;
                    }
                }
            }

            return matrix;
        }

        /// <summary>
        /// Calculate and return the area of the largest rectangle found in the bool matrix
        /// and pass the GridSpan which holds the indexes of making up the rectangle
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="outRect"></param>
        /// <returns>area of rectangle</returns>
        public static int GetLargestRectangleInMatrix(bool[,] matrix, out GridSpan outRect)
        {
            outRect = default;

            if (matrix == null || matrix.Length == 0) return 0;
            
            int maxArea = 0;
            int maxX = matrix.GetLength(0);
            int maxY = matrix.GetLength(1);

            // Evaluating grid using a combination of histogram and Monotonic Stack Algorithm
            int[] hist = new int[maxX];

            // Parsing from Y to X axis to make it easier to mentally visualize
            for (int y = 0; y < maxY; y++)
            {
                // update the histogram on every height(Y)
                for (int x = 0; x < maxX; x++)
                    hist[x] = matrix[x, y] ? hist[x] + 1 : 0;

                // evaluate the histogram using Monotonic Stack Algorithm
                Vector2Int rectDim = LargestRectangleOfHistogram(hist, out int startX);

                if (rectDim.x <= 0 || rectDim.y <= 0)
                    continue;

                // find the starting and end point on the rectangle
                Vector2Int rectStart = new Vector2Int(startX, y - rectDim.y + 1);
                Vector2Int rectEnd = new Vector2Int(startX + rectDim.x - 1, y);

                GridSpan rect = new GridSpan(rectStart, rectEnd);
                int area = rect.GetArea();

                if (area > maxArea)
                {
                    maxArea = area;
                    outRect = rect;
                }
            }

            return maxArea;
        }
        
        /// <summary>
        /// Takes in and finds the largest rectangle in a histogram as well as the starting index.
        /// Note this function uses the Monotonic Stack Algorithm.
        /// </summary>
        /// <param name="histogram">the histogram to be evaluated</param>
        /// <param name="startIndex">starting index of the largest rectangle</param>
        /// <returns>The dimension of the largest rectangle</returns>
        public static Vector2Int LargestRectangleOfHistogram(int[] histogram, out int startIndex)
        {
            startIndex = 0;
            if (histogram == null || histogram.Length == 0)
                return Vector2Int.zero;

            Stack<int> stack = new Stack<int>();

            int maxArea = 0;
            int maxWidth = 0;
            int maxHeight = 0;

            int count = histogram.Length;

            // Iterate through all bars plus an extra virtual boundary (height 0) 
            // to flush out remaining elements in the stack at the end.
            for (int i = 0; i <= count; i++)
            {
                // The height of the current bar (0 if we reached the virtual end)
                int currentHeight = (i == count) ? 0 : histogram[i];

                while (stack.Count > 0 && currentHeight <= histogram[stack.Peek()])
                {
                    int height = histogram[stack.Pop()];
                    int width = stack.Count == 0 ? i : i - stack.Peek() - 1;
                    int area = height * width;

                    if (area > maxArea)
                    {
                        maxArea = area;
                        maxWidth = width;
                        maxHeight = height;
                        startIndex = stack.Count == 0 ? 0 : stack.Peek() + 1;
                    }
                }

                if (i < count)
                    stack.Push(i);

            }

            Vector2Int dimension = new Vector2Int(maxWidth, maxHeight);

            return dimension;
        }

        private static bool SampleStateMatch(Sample sample, params SampleState[] states)
        {
            foreach (SampleState state in states)
                if (sample.State == state)
                    return true;

            return false;
        }

        // Check if point is within the given grid
        public static bool IsPointWithinGrid(Sample[,] grid, Vector2Int point)
        {
            return point.x >= 0 &&
                   point.x < grid.GetLength(0) &&
                   point.y >= 0 &&
                   point.y < grid.GetLength(1);
        }

        // Check if point is within the given grid and the value is not null
        public static bool IsValidPoint(Sample[,] grid, Vector2Int point)
        {
            return IsPointWithinGrid(grid, point) &&
                   grid[point.x, point.y] != null;
        }
    }
}