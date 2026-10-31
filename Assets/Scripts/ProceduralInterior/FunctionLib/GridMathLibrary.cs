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
    /// <param name="points"></param>
    /// <returns></returns>
    public static List<Vector2Int> RotatePoints(int rotation, ICollection<Vector2Int> points)
    {
        List<Vector2Int> outPoints = new();
        
        foreach (Vector2Int point in points)
            outPoints.Add(rotation switch
            {
                90  => new Vector2Int(point.y, -point.x),
                180 => new Vector2Int(-point.x, -point.y),
                270 => new Vector2Int(-point.y, point.x),
                _ => point
            });

        return outPoints;
    }
    
    public static List<Vector2Int> RotatePoints(int rotation, params Vector2Int[] points)
    {
        List<Vector2Int> outPoints = new();
        
        foreach (Vector2Int point in points)
            outPoints.Add(rotation switch
            {
                90  => new Vector2Int(point.y, -point.x),
                180 => new Vector2Int(-point.x, -point.y),
                270 => new Vector2Int(-point.y, point.x),
                _ => point
            });

        return outPoints;
    }

    /// <summary>
    /// Rotates a collection of Vector2Int points and offset it's position.
    /// NOTE: ROTATION is applied BEFORE POSITION OFFSET
    /// </summary>
    /// <param name="rotation"></param>
    /// <param name="offset"></param>
    /// <param name="points"></param>
    /// <returns></returns>
    public static List<Vector2Int> RotatePointsAndOffset(int rotation, Vector2Int offset,
                                                            ICollection<Vector2Int> points)
    {
        List<Vector2Int> outPoints = new();

        foreach (Vector2Int point in points)
            outPoints.Add(rotation switch
            {
                90  => new Vector2Int(point.y, -point.x) + offset,
                180 => new Vector2Int(-point.x, -point.y) + offset,
                270 => new Vector2Int(-point.y, point.x) + offset,
                _ => point + offset
            });

        return outPoints;
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
        List<Vector2Int> points = RotatePoints(rotation, inSpan.Min, inSpan.Max);

        return new GridSpan(points);
    }
}
