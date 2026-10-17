using System.Collections.Generic;
using UnityEngine;
using ProceduralInterior;
using ProceduralInterior.Types;

public static class GridParser
{
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

    private static bool IsValidPoint(Sample[,] grid, Vector2Int point)
    {
        return point.x >= 0 &&
               point.x < grid.GetLength(0) &&
               point.y >= 0 &&
               point.y < grid.GetLength(1) &&
               grid[point.x, point.y] != null;
    }
}
