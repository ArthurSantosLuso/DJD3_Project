using UnityEngine;

// Pure data container used during the layout planning phase. 
// It keeps generation abstract so we don't handle heavy Unity transforms until the layout is locked.
public class RoomBlueprint
{
    public Vector2Int GridPosition { get; private set; }
    public float Width { get; set; }
    public float Depth { get; set; }

    // Flags determining whether a room should leave openings in its tree borders
    public bool EntranceNorth { get; set; }
    public bool EntranceSouth { get; set; }
    public bool EntranceEast { get; set; }
    public bool EntranceWest { get; set; }

    public RoomBlueprint(Vector2Int gridPos)
    {
        GridPosition = gridPos;
    }
}