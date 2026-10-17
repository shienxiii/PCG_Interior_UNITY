using System;
using System.Collections.Generic;
using ProceduralInterior.Types;
using UnityEngine;

namespace ProceduralInterior
{
    /// <summary>
    /// These are content that fills a room and may or may not consume sample spaces
    /// </summary>
    public class Furniture : MonoBehaviour
    {
        // This is the size of the rough span required both occupied and reserve to fulfill
        [SerializeField] 
        private GridSpan _span;
        
        [SerializeField, Tooltip("Sample points this object need to occupy, relative to this object's transform")]
        private List<Vector2Int> _occupy;
        [SerializeField, Tooltip("Sample points around this object to mark as walkable, preventing the points from being occupied while ensuring it's walkable.")]
        private List<Vector2Int> _walkables;
        [SerializeField, Tooltip("Sample points around this object that needs to be reserved to ensure player can reach this object")]
        private List<Vector2Int> _reserve;

        /// <summary>
        /// Calculate the grid space a furniture will occupy and reserve
        /// </summary>
        /// <param name="inPrefab">the furniture we're spawning</param>
        /// <param name="yRotationEuler">rotation to be applied on to the furniture</param>
        /// <param name="inSpawnPoint">the point to spawn in the target grid space. this is where 0,0 of the furniture will be located</param>
        /// <param name="outOccupies">list of points to mark as occupied</param>
        /// <param name="outReserves">list of points to mark as reserved</param>
        /// <param name="outWalkables">list of points to mark as walkable</param>
        public static void CalculateGridSpaceRequired(Furniture inPrefab, float yRotationEuler, Vector2Int inSpawnPoint,
                                                        out List<Vector2Int> outOccupies, out List<Vector2Int> outReserves, out List<Vector2Int> outWalkables)
        {
            float radians = yRotationEuler  * Mathf.Deg2Rad;
            
            outOccupies = LocalPointToSpawnPoint(inPrefab._occupy, inSpawnPoint, radians);
            outReserves = LocalPointToSpawnPoint(inPrefab._reserve, inSpawnPoint, radians);
            outWalkables = LocalPointToSpawnPoint(inPrefab._walkables, inSpawnPoint, radians);
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

            List<Vector2Int> occupies;
            List<Vector2Int> reserves;
            List<Vector2Int> walkables;
            
            CalculateGridSpaceRequired(this, transform.rotation.eulerAngles.y, Vector2Int.zero, out occupies, out reserves, out walkables);
            
            // draw occupied
            Gizmos.color = Color.red;

            if (occupies != null)
            {
                foreach (Vector2Int point in occupies)
                {
                    Vector3 positionOffset = new Vector3(point.x * sampleSize.x, 0.0f, point.y * sampleSize.x);
                    Vector3 pos = transform.position + positionOffset;

                    Gizmos.DrawCube(pos, sampleSize);
                }
            }

            Gizmos.color = Color.green;
            
            if (reserves != null)
            {
                foreach (Vector2Int point in reserves)
                {
                    Vector3 offset = new Vector3(point.x * sampleSize.x, 0.0f, point.y * sampleSize.x);
                    Vector3 pos = transform.position + offset;

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
            
            Gizmos.color = Color.yellow;

            {
                Vector3 offset = new Vector3(_span.A.x * sampleSize.x, 0.0f, _span.A.y * sampleSize.x);
                Vector3 pos = transform.position + offset;
                Gizmos.DrawSphere(pos, sampleSize.x / 3);
            }
            
            {
                Vector3 offset = new Vector3(_span.B.x * sampleSize.x, 0.0f, _span.B.y * sampleSize.x);
                Vector3 pos = transform.position + offset;
                Gizmos.DrawSphere(pos, sampleSize.x / 3);
            }
        }
    }
}
