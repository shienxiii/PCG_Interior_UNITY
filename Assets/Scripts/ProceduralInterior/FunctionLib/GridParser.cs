using System.Collections.Generic;
using UnityEngine;
using ProceduralInterior;
using ProceduralInterior.Types;

public static class GridParser
{
    public static bool AreReservedSamplesReachable(Sample[,] samples, List<Vector2Int> reservedSamples)
    {
        if (samples == null || samples.Length == 0)
            return false;

        if (reservedSamples == null || reservedSamples.Count == 0)
            return true;

        Vector2Int start = reservedSamples[0];

        if (!IsValidPoint(samples, start))
            return false;

        Sample startSample = samples[start.x, start.y];

        if (startSample.State != SampleState.RESERVED)
            return false;
        
        int roomIndex = startSample.RoomIndex;
        
        int width = samples.GetLength(0);
        int height = samples.GetLength(1);

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

                if (!IsValidPoint(samples, next))
                    continue;

                if (visited.Contains(next))
                    continue;

                Sample sample = samples[next.x, next.y];

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

    private static bool IsValidPoint(
        Sample[,] samples,
        Vector2Int point)
    {
        return point.x >= 0 &&
               point.x < samples.GetLength(0) &&
               point.y >= 0 &&
               point.y < samples.GetLength(1) &&
               samples[point.x, point.y] != null;
    }
}
