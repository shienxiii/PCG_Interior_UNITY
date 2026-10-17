using System.Collections.Generic;
using ProceduralInterior;
using ProceduralInterior.Types;
using UnityEngine;

// Static class used to handle filling the room 
public static class RoomFurnisher
{
    public static void FurnishRoom(Interior interior, int roomIndex, Furniture furniture)
    {
        // for now only add 1 furniture for testing purpose
        if (!interior || interior.Rooms == null || roomIndex < 0 || roomIndex >= interior.Rooms.Count) return;
        
        // get the room
        Room room = interior.Rooms[roomIndex];
        List<GridSpan> spaces = room.Spaces;

    }
    
    public static void TrySpawnFurniture(Furniture prefab, Sample[,] grid, int roomIndex, float yRotationEuler)
    {
        
    }
    
}
