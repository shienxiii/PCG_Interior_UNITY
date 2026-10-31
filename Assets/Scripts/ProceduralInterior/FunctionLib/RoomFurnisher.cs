using System.Collections.Generic;
using System.Linq;
using ProceduralInterior.Types;
using UnityEngine;

// Static class used to handle filling the room 
namespace ProceduralInterior.FunctionLib
{
    public static class RoomFurnisher
    {
        public static void FurnishRoom(Interior interior, int inRoomIndex, RoomTheme inTheme)
        {
            if (!interior || interior.Rooms == null ||
                inRoomIndex < 0 || inRoomIndex >= interior.Rooms.Count
                || !inTheme)
                return;
        
            System.Random random = interior.FurnishingRandom; // The random stream for furnishing
            Room room = interior.Rooms[inRoomIndex]; // room we're populating
            List<GridSpan> spaces = room.Spaces; // spaces in the room

            // go through the main furniture to spawn
            foreach (Furniture mPrefab in inTheme.Main)
            {
                if (!mPrefab) continue;

                Furniture furnitureInst = TrySpawn(interior, inRoomIndex, mPrefab, spaces);
            
                if(furnitureInst)
                    room.AddFurniture(furnitureInst);
            }
        }

        private static Furniture TrySpawn(Interior interior, int inRoomIndex, Furniture inPrefab, List<GridSpan> inAvailableSpaces)
        {
            Vector2Int furnitureDim = inPrefab.GetBlockingDimension();
            
            // filter out spaces that are unable to fit the furniture
            Dictionary<int, List<int>> validSpaces = FilterSpaceToFitDimension(inAvailableSpaces, furnitureDim);
            List<int> keys = validSpaces.Keys.ToList();
        
            if (validSpaces.Count == 0) return null;

            // Try to spawn until successful or run out of space to test
            while (keys.Count > 0)
            {
                int key = SamplerHelperFunctions.GetRandomElement(keys, interior.FurnishingRandom, true);

                Furniture instance = TrySpawnInSpace(interior, inRoomIndex, inPrefab, inAvailableSpaces[key], validSpaces[key], interior.FurnishingRandom);
                if (instance != null)
                {
                    // recalculate the space and append to availableSpaces
                    List<GridSpan> remainingSpace = SpaceGenerator.GetSpacesInSpan(interior.Grid,
                        inAvailableSpaces[key],
                        inRoomIndex,
                        SampleState.FREE,
                        SampleState.RESERVED,
                        SampleState.WALKABLE);

                    // remove the space used from availableSpaces
                    inAvailableSpaces.RemoveAt(key);
                
                    // add remainingSpaces to availableSpaces
                    inAvailableSpaces.AddRange(remainingSpace);
                
                    return instance;
                }
            }

            return null;
        }

        private static Furniture TrySpawnInSpace(Interior interior, int inRoomIndex,
            Furniture inPrefab, GridSpan inSpace,
            List<int> inPlausibleRotations,
            System.Random inRandom)
        {
            // null check not needed here as the caller should have checked beforehand
            Sample[,] grid = interior.Grid;

            int yRotation = SamplerHelperFunctions.GetRandomElement(inPlausibleRotations, inRandom, true);
            
            Vector2Int wiggleSpace = GetPostRotationWiggleSpaceAndSpan(inPrefab, inSpace, yRotation,
                                                                        out GridSpan furnitureSpan);

            // decide the offset to spawn the furniture, add (1, 1) during random.Next as maxValue is exclusive
            Vector2Int spawnOffset = inSpace.Min + new Vector2Int(inRandom.Next(wiggleSpace.x + 1), inRandom.Next(wiggleSpace.y + 1));
        
            // calculate where the (0,0) of the furniture will be located
            Vector2Int spawnPoint = spawnOffset + (furnitureSpan.Min * -1);
        
            GetSamplePointsToConsume(inPrefab, yRotation, spawnPoint,
                                    out List<Vector2Int> occupies,
                                    out List<Vector2Int> walkables,
                                    out List<Vector2Int> reserves,
                                    out List<Vector2Int> blockingReserves);
        
            // check if furniture space will overlap any important samples it shouldn't overlap
            bool occupyCheck = GridParser.ParseWhitelistedSamples(grid, occupies, inRoomIndex, SampleState.FREE);
            bool reserveCheck = GridParser.ParseWhitelistedSamples(grid, reserves, inRoomIndex, SampleState.FREE, SampleState.RESERVED, SampleState.WALKABLE);
            bool bReserveCheck = GridParser.ParseWhitelistedSamples(grid, blockingReserves, inRoomIndex, SampleState.FREE);

            // don't spawn if overlap check fails
            if (!occupyCheck || !reserveCheck || !bReserveCheck) return null;
        
            // Convert the sample states and do final check to make sure we're not blocking any path, if yes reverse changes
        
        
            // Calculate the transform
            Vector3 position = interior.GridPointToWorldPoint(spawnPoint);
            Quaternion rotation = Quaternion.Euler(new Vector3(0.0f, yRotation, 0.0f));
        
            Furniture instance = UnityEngine.Object.Instantiate(inPrefab, position, rotation);


            // if we failed to spawn and we still have some rotation to test, try to spawn with another rotation
            if (!instance && inPlausibleRotations.Count > 0)
                instance = TrySpawnInSpace(interior, inRoomIndex, inPrefab, inSpace, inPlausibleRotations, inRandom);

            return instance;
        }

        /// <summary>
        /// Calculate available wiggle space for the given Furniture in the given space
        /// after a rotation is applied as well as determine the center point offset of
        /// the furniture's Min and Max from the 0,0
        /// </summary>
        /// <param name="inPrefab">the furniture to test</param>
        /// <param name="inSpace">the space to test</param>
        /// <param name="inRotation">the rotation to be applied</param>
        /// <param name="rotatedSpan">Min offset</param>
        /// <returns>the available wiggle space after the rotation</returns>
        private static Vector2Int GetPostRotationWiggleSpaceAndSpan(Furniture inPrefab, GridSpan inSpace, int inRotation, out GridSpan rotatedSpan)
        {
            // Calculate the rotated span and determine the centerOffset
            rotatedSpan = GridMathLibrary.RotateSpan(inRotation, inPrefab.BlockSpan);

            Vector2Int furnitureDim = rotatedSpan.GetDimension();
            
            // calculate how much space we can move the furniture around this space
            return inSpace.GetDimension() - furnitureDim;
        }

        private static void GetSamplePointsToConsume(Furniture inPrefab, int inRotation, Vector2Int inSpawnPoint,
            out List<Vector2Int> outOccupies,
            out List<Vector2Int> outWalkables,
            out List<Vector2Int> outReserves,
            out List<Vector2Int> outBlockingReserves)
        {
            outOccupies         = GridMathLibrary.RotatePointsAndOffset(inRotation, inSpawnPoint, inPrefab.Occupy);
            outWalkables        = GridMathLibrary.RotatePointsAndOffset(inRotation, inSpawnPoint, inPrefab.Walkables);
            outReserves         = GridMathLibrary.RotatePointsAndOffset(inRotation, inSpawnPoint, inPrefab.Reserves);
            outBlockingReserves = GridMathLibrary.RotatePointsAndOffset(inRotation, inSpawnPoint, inPrefab.BlockingReserves);
        }

        /// <summary>
        /// Get the index of the spaces that can fit the given dimension
        /// </summary>
        /// <param name="inSpaces">list of spaces</param>
        /// <param name="inDimension">the dimension we're looking to git into the spaces</param>
        /// <returns>Dictionary of Key: index of spaces that can fit the dimension Value: list of plausible rotation to fit the space</returns>
        private static Dictionary<int, List<int>> FilterSpaceToFitDimension(List<GridSpan> inSpaces, Vector2Int inDimension)
        {
            Dictionary<int, List<int>> usableSpaces = new();

            for (int i = 0; i < inSpaces.Count; i++)
            {
                Vector2Int spaceDim = inSpaces[i].GetDimension();

                List<int> rotations = GetPlausibleRotations(spaceDim, inDimension);

                if(rotations is { Count: > 0 })
                    usableSpaces.Add(i, rotations);
            }

            return usableSpaces;
        }
    
        private static List<int> GetPlausibleRotations(Vector2Int inSpaceDim, Vector2Int inFurnitureDim)
        {
            List<int> plausibleRotation = new();
        
            if (CanFitInSpace(inSpaceDim, inFurnitureDim))
            {
                plausibleRotation.Add(0);
                plausibleRotation.Add(180);
            }

            if (CanFitInSpace(inSpaceDim, new Vector2Int(inFurnitureDim.y, inFurnitureDim.x)))
            {
                plausibleRotation.Add(90);
                plausibleRotation.Add(270);
            }

            return plausibleRotation;
        }

        private static bool CanFitInSpace(Vector2Int inSpaceDim, Vector2Int inMinDim)
        {
            return inMinDim.x <= inSpaceDim.x && inMinDim.y <= inSpaceDim.y;
        }

    
    }
}
