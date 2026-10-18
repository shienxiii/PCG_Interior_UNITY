using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralInterior;
using ProceduralInterior.FunctionLib;
using ProceduralInterior.Types;
using UnityEngine;

// Static class used to handle filling the room 
public static class RoomFurnisher
{
    public static void FurnishRoom(Interior interior, int roomIndex, Furniture furniture)
    {
        // for now only add 1 furniture for testing purpose
        if (!interior || interior.Rooms == null || roomIndex < 0 || roomIndex >= interior.Rooms.Count) return;
        
        // we want to use the assigned furnishing random on the interior
        System.Random random = interior.FurnishingRandom;
        
        // get a copy of the room spaces, we'll be editing this as we populate the room
        List<GridSpan> spaces = interior.Rooms[roomIndex].Spaces.ToList();
        
        // we want to get the furniture's required span
        Vector2Int furnitureSpan = furniture.GetRequiredSpan();
        
        // filter out spaces that are unable to fit the furniture's span
        List<int> usableSpaces = GetUsableSpaceForSpan(spaces, furnitureSpan);
        
        // return if there's no space we can use
        if (usableSpaces == null || usableSpaces.Count == 0) return;

        // pick a random space
        int i = SamplerHelperFunctions.GetRandomElement(usableSpaces, random);

        // determine the rotations we can use for the furniture
        List<float> plausibleRotation = new();
        
        if (CanFitInSpace(spaces[i].GetDimension(), furnitureSpan))
        {
            plausibleRotation.Add(0.0f);
            plausibleRotation.Add(180.0f);
        }

        if (CanFitInSpace(spaces[i].GetDimension(), new Vector2Int(furnitureSpan.y, furnitureSpan.x)))
        {
            plausibleRotation.Add(90.0f);
            plausibleRotation.Add(270.0f);
        }
        
        // we know at this stage plausibleRotation will never be null or empty
        float yRotationEuler = SamplerHelperFunctions.GetRandomElement(plausibleRotation, random);

        // Get the grid span of consisting of both the grid space to occupy and reserved
        
        

    }

    /// <summary>
    /// Get the index of the spaces that can fit the given span
    /// </summary>
    /// <param name="inSpaces">list of spaces</param>
    /// <param name="span">the span we're looking to git into the spaces</param>
    /// <returns>index of the spaces that can fit the given span</returns>
    private static List<int> GetUsableSpaceForSpan(List<GridSpan> inSpaces, Vector2Int span)
    {
        List<int> usableSpace = new();

        for (int i = 0; i < inSpaces.Count; i++)
        {
            Vector2Int spaceDim = inSpaces[i].GetDimension();

            // we want to get any space that can fit the given span including if it's rotated 90-deg or 270-deg
            if(CanFitInSpace(spaceDim, span) ||
               CanFitInSpace(spaceDim, new Vector2Int(span.y, span.x)))
                usableSpace.Add(i);
        }

        return usableSpace;
    }

    private static bool CanFitInSpace(Vector2Int spaceDimension, Vector2Int requiredSpan)
    {
        return requiredSpan.x <= spaceDimension.x && requiredSpan.y <= spaceDimension.y;
    }

    /// <summary>
    /// Calculate and return the span of the samples provided
    /// </summary>
    /// <param name="samples"></param>
    /// <param name="span"></param>
    /// <returns></returns>
    public static Vector2Int CalculateSpan(List<Vector2Int> samples, out GridSpan span)
    {
        if (samples == null || samples.Count == 0)
        {
            span = new GridSpan(Vector2Int.zero);
            return Vector2Int.zero;
        }

        span = new GridSpan(samples[0]);

        for (int i = 1; i < samples.Count; i++)
        {
            Vector2Int point = samples[i];
                
            span.A.x = Math.Min(span.A.x, point.x);
            span.A.y = Math.Min(span.A.y, point.y);
            span.B.x = Math.Max(span.B.x, point.x);
            span.B.y = Math.Max(span.B.y, point.y);
        }

        return span.GetDimension();
    }

    public static List<T> MergeLists<T>(params List<T>[] inLists)
    {
        int size = 0;
        foreach (List<T> list in inLists)
            if(list != null) size += list.Count;

        List<T> mergedList = new List<T>(size);

        foreach (List<T> list in inLists)
            if(list != null) mergedList.AddRange(list);

        return mergedList;
    }
}
