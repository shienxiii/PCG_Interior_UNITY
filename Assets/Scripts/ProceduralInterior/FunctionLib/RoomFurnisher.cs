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
        
        // we want to use the assigned furnishing random of the interior
        System.Random random = interior.FurnishingRandom;
        
        // get a copy of the room spaces, we'll be editing this as we populate the room
        List<GridSpan> spaces = interior.Rooms[roomIndex].Spaces.ToList();
        
        // we want to get the furniture's dimension
        Vector2Int furnitureDimension = furniture.GetDefaultDimension();
        
        // filter out spaces that are unable to fit the furniture's span
        List<int> usableSpaces = GetUsableSpaceForDimension(spaces, furnitureDimension);
        
        // return if there's no space we can use
        if (usableSpaces == null || usableSpaces.Count == 0) return;

        // pick a random space
        int i = SamplerHelperFunctions.GetRandomElement(usableSpaces, random);
        Vector2Int spaceDim = spaces[i].GetDimension();
        
        // determine the rotations we can use for the furniture
        List<float> plausibleRotation = new();
        
        if (CanFitInSpace(spaceDim, furnitureDimension))
        {
            plausibleRotation.Add(0.0f);
            plausibleRotation.Add(180.0f);
        }

        if (CanFitInSpace(spaceDim, new Vector2Int(furnitureDimension.y, furnitureDimension.x)))
        {
            plausibleRotation.Add(90.0f);
            plausibleRotation.Add(270.0f);
        }
        
        // we know at this stage plausibleRotation will never be null or empty, and all values are valid to use without check
        float yRotationEuler = SamplerHelperFunctions.GetRandomElement(plausibleRotation, random);

        // Calculate the required info to place this furniture
        GridSpan span = furniture.GetGridSpaceWithRotation(yRotationEuler, out List<Vector2Int> occupies, out  List<Vector2Int> reserves, out List<Vector2Int> walkables);
        Vector2Int furnitureDim = span.GetDimension();

        // calculate how much space we can move the furniture around this space
        Vector2Int moveSpace = spaceDim - furnitureDim;


    }

    /// <summary>
    /// Get the index of the spaces that can fit the given span
    /// </summary>
    /// <param name="inSpaces">list of spaces</param>
    /// <param name="dimension">the span we're looking to git into the spaces</param>
    /// <returns>index of the spaces that can fit the given span</returns>
    private static List<int> GetUsableSpaceForDimension(List<GridSpan> inSpaces, Vector2Int dimension)
    {
        List<int> usableSpace = new();

        for (int i = 0; i < inSpaces.Count; i++)
        {
            Vector2Int spaceDim = inSpaces[i].GetDimension();

            // we want to get any space that can fit the given span including if it's rotated 90-deg or 270-deg
            if(CanFitInSpace(spaceDim, dimension) ||
               CanFitInSpace(spaceDim, new Vector2Int(dimension.y, dimension.x)))
                usableSpace.Add(i);
        }

        return usableSpace;
    }

    private static bool CanFitInSpace(Vector2Int spaceDimension, Vector2Int minDimension)
    {
        return minDimension.x <= spaceDimension.x && minDimension.y <= spaceDimension.y;
    }

    
}
