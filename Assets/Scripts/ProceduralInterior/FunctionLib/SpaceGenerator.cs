using ProceduralInterior.Types;
using UnityEngine;

namespace ProceduralInterior.FunctionLib
{
    public static class SpaceGenerator
    {
        public static void CalculateRoomSpaces(Interior interior, int roomIndex)
        {
            if (!interior || roomIndex < 0 || roomIndex >= interior.Rooms.Count) return;

            Room room = interior.Rooms[roomIndex];
            bool[,] grid = GridParser.ExtractSpanWithConditions(interior.Grid, 
                                                                room.Span,
                                                                out int remaining,
                                                                roomIndex,
                                                                SampleState.FREE);

            while (remaining > 0)
            {
                int area = GridParser.GetLargestRectangleInMatrix(grid, out GridSpan rect);

                // something went horribly wrong if area return 0
                if (area == 0) break;
                    
                // set blocks covered by rect to false 
                ArrayOperatorFunctions.MutateBlockOn2DArray(grid, false, rect.Min.x, rect.Max.x, rect.Min.y, rect.Max.y);

                remaining -= area;
                rect.Min = room.RoomPointToInteriorPoint(rect.Min);
                rect.Max = room.RoomPointToInteriorPoint(rect.Max);
                room.Spaces.Add(rect);
            }

        }
    }
}