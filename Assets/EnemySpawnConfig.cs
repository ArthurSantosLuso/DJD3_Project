using UnityEngine;

// Defines what enemies should spawn in a room and how many of each.
[System.Serializable]
public class EnemySpawnEntry
{
    public EnemyType type;
    [Min(1)] public int count;
}

[CreateAssetMenu(fileName = "EnemySpawnConfig", menuName = "Scriptable Objects/Enemy Spawn Config")]
public class EnemySpawnConfig : ScriptableObject
{
    public EnemySpawnEntry[] entries;
}