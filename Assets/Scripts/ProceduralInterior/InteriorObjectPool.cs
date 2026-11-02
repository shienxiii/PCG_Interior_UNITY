using ProceduralInterior;
using UnityEngine;

/// <summary>
/// Centralized point to manage all objects spawned by
/// the ProceduralInterior system including:
/// 1. Interior GameObject
/// 2. Interior Meshes - Walls, Floors etc
/// 3. Furniture
/// </summary>
public class InteriorObjectPool : MonoBehaviour
{
    private static InteriorObjectPool _instance = null;
    
    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }
}
