using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralInterior.Types
{
    // <summary>
    /// Holds 2 Vector2Int points which represents a grid's span.
    /// Constructor will always ensure both x and y of Min to be smaller than Max
    /// </summary>
    [System.Serializable]
    public struct GridSpan
    {
        public Vector2Int Min;
        public Vector2Int Max;

        public GridSpan(Vector2Int init)
        {
            Min = Max = init;
        }
        
        public GridSpan(Vector2Int inMin, Vector2Int inMax)
        {
            // ensure x and y in Min is always smaller than Max
            Min = inMin;
            Max = inMax;

            if (Min.x > Max.x) 
                (Max.x, Min.x) = (Min.x, Max.x);

            if (Min.y > Max.y)
                (Max.y, Min.y) = (Min.y, Max.y);
        }

        public GridSpan(ICollection<Vector2Int> points)
        {
            if (points.Count == 0)
            {
                Min = Max = Vector2Int.zero;
                return;
            }
            
            Min = Max = points.First();

            foreach (Vector2Int point in points)
            {
                Min.x = Mathf.Min(point.x, Min.x);
                Min.y = Mathf.Min(point.y, Min.y);
                
                Max.x = Mathf.Max(point.x, Max.x);
                Max.y = Mathf.Max(point.y, Max.y);

            }
        }

        /// <summary>
        /// Get the XY dimension of this Vector2Int_Range
        /// </summary>
        /// <returns></returns>
        public Vector2Int GetDimension()
        {
            Vector2Int diff = Max - Min;
            return new Vector2Int(Mathf.Abs(diff.x) + 1, Mathf.Abs(diff.y) + 1);
        }

        public int GetArea()
        {
            Vector2Int dim = GetDimension();
            return dim.x * dim.y;
        }

        public bool Contains(Vector2Int point)
        {
            return point.x >= Mathf.Min(Min.x, Max.x) &&
                   point.x <= Mathf.Max(Min.x, Max.x) &&
                   point.y >= Mathf.Min(Min.y, Max.y) &&
                   point.y <= Mathf.Max(Min.y, Max.y);
        }
    }
}