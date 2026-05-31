using UnityEngine;

public enum AssetPlacementType
{
    Collision,
    NoCollision,
    Light
}

[System.Serializable]
public class AssetCategory
{
    public AssetPlacementType type;
    public AssetPrefab[] prefabs;

    [Min(0)] public int minCount = 1;
    [Min(0)] public int maxCount = 5;
}

[CreateAssetMenu(fileName = "RoomAssetConfig", menuName = "Scriptable Objects/Room Asset Config")]
public class RoomAssetConfig : ScriptableObject
{
    public AssetCategory[] categories;
}

[System.Serializable]
public struct AssetPrefab
{
    public GameObject prefab;
    [Range(0f, 1f),] public float chanceToSpawn;
}
