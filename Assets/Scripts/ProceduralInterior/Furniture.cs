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
        
        [SerializeField, Tooltip("Sample points this object need to occupy, relative to this object's transform")]
        private List<Vector2Int> _occupy;
        [SerializeField, Tooltip("Sample points around this object to mark as walkable, preventing the points from being occupied while ensuring it's walkable.")]
        private List<Vector2Int> _walkables;
        [SerializeField, Tooltip("Sample points around this object that needs to be reserved to ensure player can reach this object")]
        private List<Vector2Int> _reserve;

        /// <summary>
        /// Calculate the grid point of this Furniture with rotation and position offset applied
        /// </summary>
        /// <param name="inPrefab">the furniture we're spawning</param>
        /// <param name="yRotationEuler">rotation to be applied on to the furniture</param>
        /// <param name="inOffset">this is the Vector2Int offset to apply to all points after rotation</param>
        /// <param name="outOccupies">list of points to mark as occupied</param>
        /// <param name="outReserves">list of points to mark as reserved</param>
        /// <param name="outWalkables">list of points to mark as walkable</param>
        public static void CalculateGridSpaceRequired(Furniture inPrefab, float yRotationEuler, Vector2Int inOffset,
                                                        out List<Vector2Int> outOccupies, out List<Vector2Int> outReserves, out List<Vector2Int> outWalkables)
        {
            float radians = yRotationEuler  * Mathf.Deg2Rad;
            
            outOccupies = LocalPointToSpawnPoint(inPrefab._occupy, inOffset, radians);
            outReserves = LocalPointToSpawnPoint(inPrefab._reserve, inOffset, radians);
            outWalkables = LocalPointToSpawnPoint(inPrefab._walkables, inOffset, radians);
        }

        private static List<Vector2Int> LocalPointToSpawnPoint(List<Vector2Int> localOffset, Vector2Int inSpawnPoint, float radians)
        {
            List<Vector2Int> spawnOffset = new();

            foreach (Vector2Int offset in localOffset)
                spawnOffset.Add(RotateOffset(offset, radians) + inSpawnPoint);

            return spawnOffset;
        }

        /// <summary>
        /// Calculates the local offset of a sample point when rotated from its of origin
        /// </summary>
        /// <param name="offset">the sample point</param>
        /// <param name="radians">rotation at origin in radians. calculate with [euler_angle * Mathf.Deg2Rad]</param>
        /// <returns></returns>
        private static Vector2Int RotateOffset(Vector2Int offset, float radians)
        {
            // Calculate standard sin and cos
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
        
            // Apply rotation matrix and round to nearest integer
            int newX = Mathf.RoundToInt(offset.x * cos + offset.y * sin);
            int newY = Mathf.RoundToInt(-offset.x * sin + offset.y * cos);
        
            return new Vector2Int(newX, newY);
        }

        private void OnValidate()
        {
            List<Vector2Int> samples = RoomFurnisher.MergeLists(_occupy, _reserve);
            RoomFurnisher.CalculateSpan(samples, out _span);
        }

        private void OnDrawGizmos()
        {
            ProceduralInteriorSettings settings = InteriorManager.Settings;
            if (!settings) return;

            Vector3 sampleSize = new Vector3(settings.SampleDimension.x - 0.1f, 0.0f, settings.SampleDimension.x - 0.1f);
            float yRotationEuler = transform.rotation.eulerAngles.y;
            
            CalculateGridSpaceRequired(this, yRotationEuler, Vector2Int.zero, out List<Vector2Int> occupies, out  List<Vector2Int> reserves, out List<Vector2Int> walkables);
            
            DrawDebugSamples(occupies, Color.red, sampleSize);
            DrawDebugSamples(reserves, Color.green, sampleSize);
            DrawDebugSamples(walkables, Color.cyan, sampleSize);
            
            List<Vector2Int> spanSamples = RoomFurnisher.MergeLists(occupies, reserves);
            RoomFurnisher.CalculateSpan(spanSamples, out GridSpan span);

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
}
