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

            Furniture furnitureInst = TrySpawn(interior, roomIndex, mPrefab, spaces);
            
            if(furnitureInst)
                room.AddFurniture(furnitureInst);
        }
    }

    private static Furniture TrySpawn(Interior interior, int roomIndex, Furniture prefab, List<GridSpan> availableSpaces)
    {
        Vector2Int furnitureDim = prefab.GetBlockingDimension();
            
        // filter out spaces that are unable to fit the furniture
        Dictionary<int, List<float>> validSpaces = FilterSpaceToFitDimension(availableSpaces, furnitureDim);
        List<int> keys = validSpaces.Keys.ToList();
        
        if (validSpaces.Count == 0) return null;

        // Try to spawn until successful or run out of space to test
        while (keys.Count > 0)
        {
            int key = SamplerHelperFunctions.GetRandomElement(keys, interior.FurnishingRandom, true);

            Furniture instance = TrySpawnInSpace(interior, roomIndex, prefab, availableSpaces[key], validSpaces[key], interior.FurnishingRandom);
            if (instance != null)
            {
                // recalculate the space and append to availableSpaces


                // remove the space used from availableSpaces
                availableSpaces.RemoveAt(key);
                
                return instance;
            }
        }

        return null;
    }

    private static Furniture TrySpawnInSpace(Interior interior, int roomIndex,
                                        Furniture prefab, GridSpan space,
                                        List<float> plausibleRotations,
                                        System.Random random)
    {
        // null check not needed here as the caller should have checked beforehand

        float yRotationEuler = SamplerHelperFunctions.GetRandomElement(plausibleRotations, random, true);

        // Calculate the required span and samples placement to spawn the furniture with the selected rotation 
        GridSpan furnitureSpan = prefab.GetGridSpaceWithRotation(yRotationEuler,
                                                                    out List<Vector2Int> occupies,
                                                                    out  List<Vector2Int> walkables,
                                                                    out List<Vector2Int> reserves,
                                                                    out List<Vector2Int> blockingReserves);
        
        Vector2Int furnitureDim = furnitureSpan.GetDimension();
        Vector2Int centerOffset = furnitureSpan.Min * -1; // where 0,0 is in furnitureSpan if we're offsetting from Min
        
        // calculate how much space we can move the furniture around this space
        Vector2Int wiggleSpace = space.GetDimension() - furnitureDim + Vector2Int.one;
        
        // decide the offset to spawn the furniture, add (1, 1) during random.Next as maxValue is exclusive
        Vector2Int spawnOffset = space.Min + new Vector2Int(random.Next(wiggleSpace.x), random.Next(wiggleSpace.y));
        
        // calculate where the (0,0) of the furniture will be located
        Vector2Int spawnPoint = spawnOffset + centerOffset;
        
        // Convert the sample states and do final check to make sure we're not blocking any path, if yes reverse changes
        
        // Calculate the transform
        Vector3 position = interior.GridPointToWorldPoint(spawnPoint);
        Quaternion rotation = Quaternion.Euler(new Vector3(0.0f, yRotationEuler, 0.0f));
        
        Furniture instance = UnityEngine.Object.Instantiate(prefab, position, rotation);
        
        // if we failed to spawn and we still have some rotation to test, try to spawn with another rotation
        if (!instance && plausibleRotations.Count > 0)
            instance = TrySpawnInSpace(interior, roomIndex, prefab, space, plausibleRotations, random);

        return instance;
    }

    //
    private static void RecalculateSpace(Sample[,] samples, GridSpan space)
    {
        
    }
    /// <summary>
    /// Get the index of the spaces that can fit the given span as well as the usable rotation
    /// </summary>
    /// <param name="inSpaces">list of spaces</param>
    /// <param name="dimension">the dimension we're looking to git into the spaces</param>
    /// <returns>index of the spaces that can fit the given span</returns>
    private static Dictionary<int, List<float>> FilterSpaceToFitDimension(List<GridSpan> inSpaces, Vector2Int dimension)
    {
        Dictionary<int, List<float>> usableSpaces = new();

        for (int i = 0; i < inSpaces.Count; i++)
        {
            Vector2Int spaceDim = inSpaces[i].GetDimension();

            List<float> rotations = GetPlausibleRotations(spaceDim, dimension);

            if(rotations is { Count: > 0 })
                usableSpaces.Add(i, rotations);
        }

        return usableSpaces;
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

    private static bool CanFitInSpace(Vector2Int spaceDimension, Vector2Int minDimension)
    {
        return minDimension.x <= spaceDimension.x && minDimension.y <= spaceDimension.y;
    }

    
}
