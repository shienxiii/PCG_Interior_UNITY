using System.Collections.Generic;
using System.Linq;
using ProceduralInterior.Types;
using UnityEngine;

public static class GridMathLibrary
{
    /// <summary>
    /// rotates a list of offset from a given rotation
    /// IMPORTANT: This function assumes rotation is 0, 90, 180 and 270 degrees ONLY
    /// </summary>
    /// <param name="rotation"></param>
    /// <param name="offsets"></param>
    /// <returns></returns>
    public static List<Vector2Int> RotateOffsets(int rotation, List<Vector2Int> offsets)
    {
        List<Vector2Int> spawnOffset = new();
        
        foreach (Vector2Int offset in offsets)
            spawnOffset.Add(rotation switch
                            {
                                90  => new Vector2Int(offset.y, -offset.x),
                                180 => new Vector2Int(-offset.x, -offset.y),
                                270 => new Vector2Int(-offset.y, offset.x),
                                _ => offset
                            });

        return spawnOffset;
    }

    /// <summary>
    /// Rotate a GridSpan by the given rotation.
    /// IMPORTANT: This function assumes rotation is 0, 90, 180 and 270 degrees ONLY
    /// </summary>
    /// <param name="rotation"></param>
    /// <param name="inSpan"></param>
    /// <returns></returns>
    public static GridSpan RotateSpan(int rotation, GridSpan inSpan)
    {
        Vector2Int bl = inSpan.Min;
        Vector2Int tr = inSpan.Max;

        // using hash set as there is a possibility of some corners to be the same
        HashSet<Vector2Int> corners = new() {bl,
                                          tr,
                                          new Vector2Int(tr.x, bl.y) /*br*/,
                                          new Vector2Int(bl.x, tr.y) /*tl*/};

        List<Vector2Int> points = RotateOffsets(rotation, corners.ToList());

        return new GridSpan(points);
    }
}
