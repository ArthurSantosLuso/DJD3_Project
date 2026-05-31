// 
/// <summary>
/// Defines the functional role of a room.
/// </summary>
public enum RoomType
{
    Initial,        // Starting room — no combat, no gates
    CombatRegular,  // Standard fight room — gates close, enemies spawn
    Special,        // Portal to an out of bounds arena fight
    Loot,           // Reward room — no combat, just loot
    Final           // Last room — combat ends with the teddy bear 
}
