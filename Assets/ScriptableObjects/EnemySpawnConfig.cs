using System.Linq;
using UnityEngine;

[System.Serializable]
public class EnemySpawnEntry
{
    public EnemyType type;
    [Min(0)] public int minCount;
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
    /// Returns the enemies of current teddy bear tier.
    /// </summary>
    public EnemySpawnTier GetTierForTeddyCount(int teddyCount)
    {
        return tiers.Where(s => s.teddyBearThreshold == teddyCount).FirstOrDefault();
    }
}