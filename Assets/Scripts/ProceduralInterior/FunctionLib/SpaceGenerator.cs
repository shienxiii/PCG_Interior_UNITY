using System.Collections.Generic;
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

        public static List<GridSpan> GetSpacesInSpan(Sample[,] grid, GridSpan span, int roomIndex, params SampleState[] states)
        {
            if (grid == null) return null;

            bool[,] subGrid = GridParser.ExtractSpanWithConditions(grid,
                                                                    span,
                                                                    out int remaining,
                                                                    roomIndex,
                                                                    states);

            List<GridSpan> spaces = new();
            
            while (remaining > 0)
            {
                int area = GridParser.GetLargestRectangleInMatrix(subGrid, out GridSpan rect);
                
                // set blocks covered by rect to false 
                ArrayOperatorFunctions.MutateBlockOn2DArray(subGrid, false, rect.Min.x, rect.Max.x, rect.Min.y, rect.Max.y);

                // something went horribly wrong if area return 0
                if (area == 0) break;
                
                // restore the offset to interior relative
                rect.Min += span.Min;
                rect.Max += span.Min;

                spaces.Add(rect);
            }

            return spaces;
        }
    }
}