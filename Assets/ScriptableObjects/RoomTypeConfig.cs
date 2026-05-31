using UnityEngine;
using System.Collections.Generic;

// Defines the exact number of each room type for a given teddy bear threshold.
// The system picks the highest tier whose minTeddyCount the player has reached.
[System.Serializable]
public class RoomTypeTier
{
    [Tooltip("This tier activates when the player has collected at least this many teddy bears.")]
    public int minTeddyCount;

    [Tooltip("Exact number of special rooms to place in the level.")]
    [Min(0)] public int specialRoomCount;

    [Tooltip("Exact number of loot rooms to place in the level.")]
    [Min(0)] public int lootRoomCount;
}

// Add tiers in ascending 'minTeddyCount' order.
// Any rooms left over after filling the defined counts will become CombatRegular.
[CreateAssetMenu(fileName = "RoomTypeConfig", menuName = "Scriptable Objects/Room Type Config")]
public class RoomTypeConfig : ScriptableObject
{
    [Tooltip("Tiers in ascending minTeddyCount order. The highest tier the player qualifies for is used.")]
    public List<RoomTypeTier> tiers = new List<RoomTypeTier>();

    /// <summary>
    /// Returns the highest tier whose 'minTeddyCount' is less than or equal to the given count.
    /// Falls back to the first tier if none match, or null if the list is empty.
    /// </summary>
    public RoomTypeTier GetTierForTeddyCount(int teddyCount)
    {
        RoomTypeTier result = null;

        foreach (RoomTypeTier tier in tiers)
        {
            if (teddyCount >= tier.minTeddyCount)
                result = tier;
        }

        return result;
    }
}