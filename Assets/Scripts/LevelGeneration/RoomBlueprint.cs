using UnityEngine;
using System.Collections.Generic;

/*
This script handles:
Storing all layout data for a single room during the generation planning phase.
No Unity objects are created here, it's pure data that LevelGenerator reads
before translating the layout into actual GameObjects.
*/

public class RoomBlueprint
{
    public Vector2Int GridPosition { get; private set; }
    public float Width { get; set; }
    public float Depth { get; set; }

    // Flags marking which sides of this room connect to a neighbour
    public bool EntranceNorth { get; set; }
    public bool EntranceSouth { get; set; }
    public bool EntranceEast { get; set; }
    public bool EntranceWest { get; set; }

    // Role of this room in the level layout
    public bool IsStartRoom { get; set; }
    public bool IsEndRoom { get; set; }

    // Drives which systems activate when the player enters
    public RoomType RoomType { get; set; } = RoomType.Initial;

    // Entrances that face the next step on the shortest path to the final room.
    // Used to place guiding lights so the player knows which way to go.
    public HashSet<Vector2Int> CorrectPathExits { get; private set; } = new HashSet<Vector2Int>();

    public RoomBlueprint(Vector2Int gridPos)
    {
        GridPosition = gridPos;
    }
}