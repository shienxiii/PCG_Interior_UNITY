using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProceduralInterior.FunctionLib
{
    public static class SamplerHelperFunctions
    {
        /// <summary>
        /// Returns true if the point is inside the polygon.
        /// Polygon points should be ordered around the perimeter
        /// (clockwise or counter-clockwise).
        /// </summary>
        public static bool IsPointWithinPolygon(List<Vector2> polygon, Vector2 point)
        {
            int count = 0;

            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 p0 = polygon[i];
                Vector2 p1 = polygon[(i + 1) % polygon.Count];

                if ((point.y < p0.y) != (point.y < p1.y) &&
                    (point.x < p0.x + ((point.y - p0.y) / (p1.y - p0.y)) * (p1.x - p0.x)))
                { count++; }
            }
            Vector2Int s = new(1, 1);
            return count % 2 == 1;
        }
        
        // Use a provided System.Random to get a random element on the provided list and optionally remove from list
        public static T GetRandomElement<T>(List<T> list, System.Random random, bool bRemoveFromList = false)
        {
            if (list == null || list.Count == 0) return default(T);
            
            T element = list[random.Next(list.Count)];

            if (bRemoveFromList)
                list.Remove(element);

            return element;
        }

        public static void GetCardinalSample(List<Vector2Int> validSamples, List<Vector2Int> cardinalSamples, Vector2Int sample)
        {
            void TestCardinalSample(Vector2Int cardinalSample)
            {
                // test if cardinal sample still exist in validSamples
                if (validSamples.Contains(cardinalSample) && !cardinalSamples.Contains(cardinalSample))
                    cardinalSamples.Add(cardinalSample);

            }

            TestCardinalSample(sample + Vector2Int.right);
            TestCardinalSample(sample + Vector2Int.left);
            TestCardinalSample(sample + Vector2Int.up);
            TestCardinalSample(sample + Vector2Int.down);
        }
        
        public static List<T> MergeCollectionsToList<T>(params ICollection<T>[] inLists)
        {
            int size = 0;
            foreach (ICollection<T> list in inLists)
                if(list != null) size += list.Count;

            List<T> mergedList = new List<T>(size);

            foreach (ICollection<T> list in inLists)
                if(list != null) mergedList.AddRange(list);

            return mergedList;
        }
    }
}