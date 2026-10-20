using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralInterior;
using ProceduralInterior.FunctionLib;
using ProceduralInterior.Types;
using Unity.Mathematics;
using UnityEngine;

// Static class used to handle filling the room 
public static class RoomFurnisher
{
    public static void FurnishRoom(Interior interior, int roomIndex, Furniture inFurniture)
    {
        // for now only add 1 furniture for testing purpose
        if (!interior || interior.Rooms == null || roomIndex < 0 || roomIndex >= interior.Rooms.Count) return;
        
        // we want to use the assigned furnishing random of the interior
        System.Random random = interior.FurnishingRandom;
        
        // get a copy of the room spaces, we'll be editing this as we populate the room
        Room room = interior.Rooms[roomIndex];
        List<GridSpan> spaces = room.Spaces.ToList();
        
        // we want to get the furniture's dimension
        Vector2Int furnitureDimension = inFurniture.GetDefaultDimension();
        
        // filter out spaces that are unable to fit the furniture's span
        List<int> usableSpaces = GetUsableSpaceForDimension(spaces, furnitureDimension);
        
        // return if there's no space we can use
        if (usableSpaces == null || usableSpaces.Count == 0) return;

        // pick a random space
        int i = SamplerHelperFunctions.GetRandomElement(usableSpaces, random);
        GridSpan space = spaces[i];
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
        GridSpan furnitureSpan = inFurniture.GetGridSpaceWithRotation(yRotationEuler, out List<Vector2Int> occupies, out  List<Vector2Int> reserves, out List<Vector2Int> walkables);
        Vector2Int furnitureDim = furnitureSpan.GetDimension();
        
        // calculate the offset of span to figure out where 0,0 is offset from, which would be the negative of span.Min
        Vector2Int spanOffset = furnitureSpan.Min * -1;

        // calculate how much space we can move the furniture around this space
        Vector2Int moveSpace = spaceDim - furnitureDim + Vector2Int.one;
        
        // decide the offset to spawn the furniture, add (1, 1) during random.Next as maxValue is exclusive
        Vector2Int offset = new Vector2Int(random.Next(moveSpace.x), random.Next(moveSpace.y));
        
        // calculate where the (0,0) of the furniture will be located
        Vector2Int spawnPoint = space.Min + spanOffset + offset;
        
        // Calculate the transform
        Vector3 position = interior.GridPointToWorldPoint(spawnPoint);
        Quaternion rotation = Quaternion.Euler(new Vector3(0.0f, yRotationEuler, 0.0f));

        Furniture instance = GameObject.Instantiate(inFurniture, position, rotation);
        room.AddFurniture(instance);
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
