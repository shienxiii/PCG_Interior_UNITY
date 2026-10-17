using System;
using System.Collections.Generic;
using ProceduralInterior.Types;
using UnityEngine;

namespace ProceduralInterior.FunctionLib
{
    /// <summary>
    /// Contains functions to help with sample related calculations
    /// </summary>
    public static class SampleHelperFunctions
    {
        public static Vector2Int CalculateSpan(List<Vector2Int> samples, out GridSpan span)
        {
            if (samples == null || samples.Count == 0)
            {
                span = new GridSpan(Vector2Int.zero);
                return Vector2Int.zero;
            }

            span = new GridSpan(samples[0]);

            for (int i = 1; i < samples.Count; i++)
            {
                Vector2Int point = samples[i];
                
                span.A.x = Math.Min(span.A.x, point.x);
                span.A.y = Math.Min(span.A.y, point.y);
                span.B.x = Math.Max(span.B.x, point.x);
                span.B.y = Math.Max(span.B.y, point.y);
            }

            return span.GetDimension();
        }
    
        public static List<T> MergeLists<T>(params List<T>[] inLists)
        {
            int size = 0;
            foreach (List<T> list in inLists)
                if(list != null) size += list.Count;

            List<T> mergedList = new List<T>(size);

            foreach (List<T> list in inLists)
                if(list != null) mergedList.AddRange(list);

            return mergedList;
        }
    }
}
