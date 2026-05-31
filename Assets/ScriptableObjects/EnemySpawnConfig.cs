using UnityEngine;

[System.Serializable]
public class EnemySpawnEntry
{
    public EnemyType type;
    [Min(1)] public int minCount;
    [Min(1)] public int maxCount;
}
[System.Serializable]
public class EnemySpawnTier
{
    [Tooltip("Minimum teddy bear count needed to use this tier.")]
    public int teddyBearThreshold;
    public EnemySpawnEntry[] entries;
}

[CreateAssetMenu(fileName = "EnemySpawnConfig", menuName = "Scriptable Objects/Enemy Spawn Config")]
public class EnemySpawnConfig : ScriptableObject
{
    [SerializeField] private EnemySpawnTier[] tiers;

    /// <summary>
    /// Returns the first tier whose threshold is <= teddyCount.
    /// Tiers should be ordered highest threshold first in the Inspector.
    /// Returns null if no tier matches.
    /// </summary>
    public EnemySpawnTier GetTierForTeddyCount(int teddyCount)
    {
        foreach (EnemySpawnTier tier in tiers)
        {
            if (teddyCount >= tier.teddyBearThreshold)
                return tier;
        }

        return null;
    }
}