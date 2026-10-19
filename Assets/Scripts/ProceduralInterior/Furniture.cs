using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralInterior.Types;
using ProceduralInterior.FunctionLib;
using UnityEngine;

namespace ProceduralInterior
{
    /// <summary>
    /// These are content that fills a room and may or may not consume sample spaces
    /// </summary>
    public class Furniture : MonoBehaviour
    {
        [SerializeField]
        private GridSpan _span;
        
        [SerializeField, Tooltip("Sample points this furniture need to occupy, relative to this object's transform")]
        private List<Vector2Int> _occupy;
        
        [SerializeField, Tooltip("Sample points around this furniture to mark as walkable, preventing the points from being occupied while ensuring it's walkable.")]
        private List<Vector2Int> _walkables;
        
        [SerializeField, Tooltip("Sample points around on furniture that needs to be reserved to ensure player can reach this object")]
        private List<Vector2Int> _reserves;

        /// <summary>
        /// Get the required dimension to spawn this furniture.
        /// This consists of grid to occupy and to reserve.
        /// Does not consider any rotation
        /// </summary>
        /// <returns>The span dimension consisting of both occupied and reserved samples</returns>
        public Vector2Int GetDefaultDimension()
        {
            //NOTE: _span is calculated with a combination of _occupy and _reserves
            // we need to take up at least 1 dimension, so we'll get the dimension from _span as is
            return _span.GetDimension();
        }

        /// <summary>
        /// Calculate the grid point of this Furniture with rotation applied
        /// </summary>
        /// <param name="inPrefab">the furniture we're spawning</param>
        /// <param name="yRotationEuler">rotation to be applied on to the furniture</param>
        /// <param name="outOccupies">list of points to mark as occupied</param>
        /// <param name="outReserves">list of points to mark as reserved</param>
        /// <param name="outWalkables">list of points to mark as walkable</param>
        /// <returns>GridSpan that this furniture required to spawn</returns>
        public GridSpan GetGridSpaceWithRotation(float yRotationEuler,
                                                    out List<Vector2Int> outOccupies,
                                                    out List<Vector2Int> outReserves,
                                                    out List<Vector2Int> outWalkables)
        {
            if (Mathf.Approximately(yRotationEuler % 360.0f, 0.0f))
            {
                outOccupies = _occupy;
                outReserves = _reserves;
                outWalkables = _walkables;
                return _span;
            }
            
            float radians = yRotationEuler  * Mathf.Deg2Rad;
            
            outOccupies = RotateOffsets(_occupy, radians);
            outReserves = RotateOffsets(_reserves, radians);
            outWalkables = RotateOffsets(_walkables, radians);

            List<Vector2Int> requiredSamples = SamplerHelperFunctions.MergeLists(outOccupies, outReserves);
            GridSpan requiredSpan = CalculateSpan(requiredSamples);

            return requiredSpan;
        }

        private static List<Vector2Int> RotateOffsets(List<Vector2Int> offsets, float radians)
        {
            List<Vector2Int> spawnOffset = new();
            
            // Calculate standard sin and cos
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            foreach (Vector2Int offset in offsets)
            {
                int newX = Mathf.RoundToInt(offset.x * cos + offset.y * sin);
                int newY = Mathf.RoundToInt(-offset.x * sin + offset.y * cos);
                
                spawnOffset.Add(new Vector2Int(newX, newY));
            }

            return spawnOffset;
        }

        /// <summary>
        /// Calculate and return the span of the samples provided
        /// </summary>
        /// <param name="points">points we're calculating the span for</param>
        /// <param name="span">The GridSpan dimension covering all the given points</param>
        /// <returns>The GridSpan dimension covering all the given points. If empty will return a 1x1 GridSpan of (0,0)</returns>
        public static GridSpan CalculateSpan(List<Vector2Int> points)
        {
            if (points == null || points.Count == 0)
                return new GridSpan(Vector2Int.zero);

            GridSpan span = new GridSpan(points[0]);

            for (int i = 1; i < points.Count; i++)
            {
                Vector2Int point = points[i];
                
                span.A.x = Math.Min(span.A.x, point.x);
                span.A.y = Math.Min(span.A.y, point.y);
            
                span.B.x = Math.Max(span.B.x, point.x);
                span.B.y = Math.Max(span.B.y, point.y);
            }

            return span;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            List<Vector2Int> samples = SamplerHelperFunctions.MergeLists(_occupy, _reserves);
            _span = CalculateSpan(samples);
        }

        private void OnDrawGizmos()
        {
            ProceduralInteriorSettings settings = InteriorManager.Settings;
            if (!settings) return;

            Vector3 sampleSize = new Vector3(settings.SampleDimension.x - 0.1f, 0.0f, settings.SampleDimension.x - 0.1f);
            float yRotationEuler = transform.rotation.eulerAngles.y;
            
            GridSpan span = GetGridSpaceWithRotation(yRotationEuler, out List<Vector2Int> occupies, out  List<Vector2Int> reserves, out List<Vector2Int> walkables);
            
            DrawDebugSamples(occupies, Color.red, sampleSize);
            DrawDebugSamples(reserves, Color.green, sampleSize);
            DrawDebugSamples(walkables, Color.cyan, sampleSize);

            DrawDebugSample(span.A, Color.white, sampleSize);
            DrawDebugSample(span.B, Color.black, sampleSize);
        }

        private void DrawDebugSamples(List<Vector2Int> samples, Color debugColor, Vector3 sampleSize)
        {
            Gizmos.color = debugColor;

            if (samples != null)
            {
                foreach (Vector2Int point in samples)
                {
                    Vector3 offset = new Vector3(point.x * sampleSize.x, 0.0f, point.y * sampleSize.x);
                    Vector3 pos = transform.position + offset;

                    Gizmos.DrawCube(pos, sampleSize);
                }
            }
        }

        private void DrawDebugSample(Vector2Int sample, Color debugColor, Vector3 sampleSize)
        {
            Gizmos.color = debugColor;
            
            Vector3 offset = new Vector3(sample.x * sampleSize.x, 0.0f, sample.y * sampleSize.x);
            Vector3 pos = transform.position + offset;
            Gizmos.DrawSphere(pos, sampleSize.x / 3);
        }
    }
#endif
}
