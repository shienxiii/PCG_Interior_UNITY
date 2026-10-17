using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralInterior.Types;
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
            if (_occupy != null && _occupy.Count > 0)
                _span.A = _span.B = _occupy[0];
            else if (_reserve != null && _reserve.Count > 0)
                _span.A = _span.B = _reserve[0];
            else
                return;

            if (_occupy != null)
            {
                foreach (Vector2Int point in _occupy)
                {
                    _span.A.x = Math.Min(_span.A.x, point.x);
                    _span.A.y = Math.Min(_span.A.y, point.y);
                    _span.B.x = Math.Max(_span.B.x, point.x);
                    _span.B.y = Math.Max(_span.B.y, point.y);
                }
            }

            if (_reserve != null)
            {
                foreach (Vector2Int point in _reserve)
                {
                    _span.A.x = Math.Min(_span.A.x, point.x);
                    _span.A.y = Math.Min(_span.A.y, point.y);
                    _span.B.x = Math.Max(_span.B.x, point.x);
                    _span.B.y = Math.Max(_span.B.y, point.y);
                }
            }
        }

        private void OnDrawGizmos()
        {
            ProceduralInteriorSettings settings = InteriorManager.Settings;
            if (!settings) return;

            Vector3 sampleSize = new Vector3(settings.SampleDimension.x - 0.1f, 0.0f, settings.SampleDimension.x - 0.1f);
            Vector3 position = transform.position;
            float yRotationEuler = transform.rotation.eulerAngles.y;

            
            
            CalculateGridSpaceRequired(this, yRotationEuler, Vector2Int.zero, out List<Vector2Int> occupies, out  List<Vector2Int> reserves, out List<Vector2Int> walkables);
            
            // draw occupied
            Gizmos.color = Color.red;

            if (occupies != null)
            {
                foreach (Vector2Int point in occupies)
                {
                    Vector3 offset = new Vector3(point.x * sampleSize.x, 0.0f, point.y * sampleSize.x);
                    Vector3 pos = position + offset;

                    Gizmos.DrawCube(pos, sampleSize);
                }
            }

            Gizmos.color = Color.green;
            
            if (reserves != null)
            {
                foreach (Vector2Int point in reserves)
                {
                    Vector3 offset = new Vector3(point.x * sampleSize.x, 0.0f, point.y * sampleSize.x);
                    Vector3 pos = position + offset;

                    Gizmos.DrawCube(pos, sampleSize);
                }
            }
            
            Gizmos.color = Color.cyan;
            
            if (walkables != null)
            {
                foreach (Vector2Int point in walkables)
                {
                    Vector3 offset = new Vector3(point.x * sampleSize.x, 0.0f, point.y * sampleSize.x);
                    Vector3 pos = transform.position + offset;

                    Gizmos.DrawCube(pos, sampleSize);
                }
            }
            
            List<Vector2Int> spanSamples = new();
            if (occupies != null)
                spanSamples.AddRange(occupies);
            if(reserves != null)
                spanSamples.AddRange(reserves);
            
            CalculateSpan(spanSamples, out GridSpan span);

                
            Gizmos.color = Color.yellow;
            {
                Vector3 offset = new Vector3(span.A.x * sampleSize.x, 0.0f, span.A.y * sampleSize.x);
                Vector3 pos = position + offset;
                Gizmos.DrawSphere(pos, sampleSize.x / 3);
            }
            
            Gizmos.color = Color.magenta;
            {
                Vector3 offset = new Vector3(span.B.x * sampleSize.x, 0.0f, span.B.y * sampleSize.x);
                Vector3 pos = position + offset;
                Gizmos.DrawSphere(pos, sampleSize.x / 3);
            }
        }
    }
}
