using System.Collections.Generic;
using ProceduralInterior;
using UnityEngine;


/// <summary>
/// Holds information of how a room should look and how is should be furnished
/// </summary>
[CreateAssetMenu(fileName = "RoomTheme", menuName = "Scriptable Objects/RoomTheme")]
public class RoomTheme : ScriptableObject
{
    // Furnitures that must be spawned in the room being given this theme
    private List<Furniture> _main;
    private List<Furniture> _sub;
}
