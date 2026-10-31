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
        private GridSpan _blockSpan;
        
        [SerializeField, Tooltip("Sample points this furniture need to occupy, relative to this object's transform")]
        private List<Vector2Int> _occupy;
        
        [SerializeField, Tooltip("Sample points around this furniture to mark as walkable, preventing the points from being occupied while ensuring it's walkable.")]
        private List<Vector2Int> _walkables;
        
        [SerializeField, Tooltip("Sample points that needs to be reserved to ensure player can reach this object")]
        private List<Vector2Int> _reserves;
        
        [SerializeField, Tooltip("Sample points that needs to be reserved to ensure player can reach this object, treated as non-path")]
        private List<Vector2Int> _blockingReserves;
        
        #if UNITY_EDITOR
        [SerializeField] private bool _drawDebug = true;
        [SerializeField] private bool _debugDrawSpan = false;
        [SerializeField] private bool _debugDrawSpan2 = false;
        #endif

        /// <summary>
        /// Get the required dimension to spawn this furniture.
        /// This consists of grid to occupy and to reserve.
        /// Does not consider any rotation
        /// </summary>
        /// <returns>The span dimension consisting of both occupied and reserved samples</returns>
        public Vector2Int GetBlockingDimension()
        {
            //NOTE: _span is calculated with a combination of _occupy and _reserves
            // we need to take up at least 1 dimension, so we'll get the dimension from _span as is
            return _blockSpan.GetDimension();
        }

        /// <summary>
        /// Calculate the grid point of this Furniture with rotation applied
        /// </summary>
        /// <param name="rotation">rotation to be applied on to the furniture</param>
        /// <param name="outOccupies">list of points to mark as OCCUPIED</param>
        /// <param name="outWalkables">list of points to mark as WALKABLE</param>
        /// <param name="outReserves">list of points to mark as RESERVED</param>
        /// <param name="outBlockingReserves">list of points to mark as RESERVED_BLOCK</param>
        /// <returns>Blocking span of this furniture</returns>
        public GridSpan GetGridSpaceWithRotation(int rotation,
                                                    out List<Vector2Int> outOccupies,
                                                    out List<Vector2Int> outWalkables,
                                                    out List<Vector2Int> outReserves,
                                                    out List<Vector2Int> outBlockingReserves)
        {
            outOccupies         = GridMathLibrary.RotateOffsets(rotation, _occupy);
            outWalkables        = GridMathLibrary.RotateOffsets(rotation, _walkables);
            outReserves         = GridMathLibrary.RotateOffsets(rotation, _reserves);
            outBlockingReserves = GridMathLibrary.RotateOffsets(rotation, _blockingReserves);

            List<Vector2Int> blockPoints = SamplerHelperFunctions.MergeLists(outOccupies, outReserves, outBlockingReserves);
            GridSpan blockSpan = new GridSpan(blockPoints);

            return blockSpan;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            List<Vector2Int> samples = SamplerHelperFunctions.MergeLists(_occupy, _reserves, _blockingReserves);
            _blockSpan = new GridSpan(samples);
        }

        private void OnDrawGizmos()
        {
            ProceduralInteriorSettings settings = InteriorManager.Settings;
            if (!settings) return;

            Vector3 sampleSize = new Vector3(settings.SampleDimension.x - 0.1f, 0.0f, settings.SampleDimension.x - 0.1f);
            int yRotationEuler = (int)(transform.rotation.eulerAngles.y + 0.1);
            
            GridSpan span = GetGridSpaceWithRotation(yRotationEuler, out List<Vector2Int> occupies, out  List<Vector2Int> walkables,
                                                        out List<Vector2Int> reserves, out List<Vector2Int> blockingReserves);
            
            DrawDebugSamples(occupies, Color.red, sampleSize);
            DrawDebugSamples(walkables, Color.cyan, sampleSize);
            DrawDebugSamples(reserves, Color.green, sampleSize);
            DrawDebugSamples(blockingReserves, Color.yellow, sampleSize);

            if (_debugDrawSpan)
            {
                DrawDebugSample(span.Min, Color.white, sampleSize);
                DrawDebugSample(span.Max, Color.black, sampleSize);
            }

            if (_debugDrawSpan2)
            {
                GridSpan sp = GridMathLibrary.RotateSpan((int)(yRotationEuler), _blockSpan);
                
                DrawDebugSample(sp.Min, Color.deepPink, sampleSize);
                DrawDebugSample(sp.Max, Color.purple, sampleSize);
            }
        }

        private void DrawDebugSamples(List<Vector2Int> samples, Color debugColor, Vector3 sampleSize)
        {
            if (!_drawDebug) return;
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
