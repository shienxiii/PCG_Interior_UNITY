using System;
using Unity.VisualScripting;
using UnityEngine;

namespace ProceduralInterior
{
    [Serializable]
    public struct RoomPrefab
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Vector3 _dimension;

        // Leave null if not overriding the material of the prefab
        [SerializeField] private Material _material;

        public GameObject Prefab => _prefab;
        public Vector3 Dimension => _dimension;
        public Material Material => _material;
    }
}