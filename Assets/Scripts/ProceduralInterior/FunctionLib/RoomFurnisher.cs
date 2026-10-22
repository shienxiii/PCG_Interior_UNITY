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
    public static void FurnishRoom(Interior interior, int roomIndex, RoomTheme theme)
    {
        if (!interior || interior.Rooms == null ||
            roomIndex < 0 || roomIndex >= interior.Rooms.Count
            || !theme)
            return;
        
        System.Random random = interior.FurnishingRandom; // The random stream for furnishing
        Room room = interior.Rooms[roomIndex]; // room we're populating
        List<GridSpan> spaces = room.Spaces; // spaces in the room

        // go through the main furniture to spawn
        foreach (Furniture mPrefab in theme.Main)
        {
            if (!mPrefab) continue;

            // Get the blocking dimension of the furniture
            Vector2Int blockDim = mPrefab.GetBlockingDimension();
            
            // filter out spaces that are unable to fit the furniture's span
            List<int> validSpaces = GetUsableSpaceForDimension(spaces, blockDim);
            
            if (validSpaces.Count == 0) continue; // PLACEHOLDER: for now, continue to next furniture if no valid spaces 
            
            // pick a random space
            int i = SamplerHelperFunctions.GetRandomElement(validSpaces, random);

            Furniture furnitureInst = TrySpawnInSpace(interior, roomIndex, mPrefab, spaces[i], random);
            
            if(furnitureInst)
                room.AddFurniture(furnitureInst);
        }
    }

    private static Furniture TrySpawnInSpace(Interior interior, int roomIndex,
                                        Furniture prefab, GridSpan space,
                                        System.Random random)
    {
        // null check not needed here as the caller should have checked beforehand

        Vector2Int spaceDim = space.GetDimension();
        // check plausible rotation we can use for the furniture
        List<float> plausibleRotation = GetPlausibleRotations(spaceDim, prefab.GetBlockingDimension());
        
        // at this stage plausibleRotation will never be null or empty, and all values are valid to use without check
        float yRotationEuler = SamplerHelperFunctions.GetRandomElement(plausibleRotation, random);
        
        // Calculate the required span and samples placement to spawn the furniture with the selected rotation 
        GridSpan furnitureSpan = prefab.GetGridSpaceWithRotation(yRotationEuler, out List<Vector2Int> occupies, out  List<Vector2Int> walkables, 
                                                            out List<Vector2Int> reserves, out List<Vector2Int> blockingReserves);
        Vector2Int furnitureDim = furnitureSpan.GetDimension();
        Vector2Int furnitureOffset = furnitureSpan.Min * -1; // where 0,0 is in furnitureSpan
        
        // calculate how much space we can move the furniture around this space
        Vector2Int wiggleSpace = spaceDim - furnitureDim + Vector2Int.one;
        
        // decide the offset to spawn the furniture, add (1, 1) during random.Next as maxValue is exclusive
        Vector2Int spawnOffset = new Vector2Int(random.Next(wiggleSpace.x), random.Next(wiggleSpace.y)) + space.Min;
        
        // calculate where the (0,0) of the furniture will be located
        Vector2Int spawnPoint = spawnOffset + furnitureOffset;
        
        /*
        // Offset the points retrieved to the spawn points
        void OffsetPoints(List<Vector2Int> lists, Vector2Int offset)
        {
            for (int i = 0; i < lists.Count; i++)
                lists[i] += offset;
        }
        
        OffsetPoints(occupies, spawnPoint);
        OffsetPoints(walkables, spawnPoint);
        OffsetPoints(reserves, spawnPoint);
        OffsetPoints(blockingReserves, spawnPoint);
        
        // check that occupies do not overlap any samples not labelled FREE
        GridParser.ParseSample_Whitelist(interior.Grid, occupies, roomIndex, SampleState.FREE);
        GridParser.ParseSample_Whitelist(interior.Grid, blockingReserves, roomIndex, SampleState.FREE);*/
        
        // Convert the sample states and do final check to make sure we're not blocking any path, if yes reverse changes
        
        // Calculate the transform
        Vector3 position = interior.GridPointToWorldPoint(spawnPoint);
        Quaternion rotation = Quaternion.Euler(new Vector3(0.0f, yRotationEuler, 0.0f));

        return UnityEngine.Object.Instantiate(prefab, position, rotation);
    }

    private static List<float> GetPlausibleRotations(Vector2Int spaceDim, Vector2Int furnitureDim)
    {
        List<float> plausibleRotation = new();
        
        if (CanFitInSpace(spaceDim, furnitureDim))
        {
            plausibleRotation.Add(0.0f);
            plausibleRotation.Add(180.0f);
        }

        if (CanFitInSpace(spaceDim, new Vector2Int(furnitureDim.y, furnitureDim.x)))
        {
            plausibleRotation.Add(90.0f);
            plausibleRotation.Add(270.0f);
        }

        return plausibleRotation;
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
