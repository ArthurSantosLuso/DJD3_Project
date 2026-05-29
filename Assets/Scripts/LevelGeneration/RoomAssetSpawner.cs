using UnityEngine;
using System.Collections.Generic;

public class RoomAssetSpawner : MonoBehaviour
{
    // How deep into the room the forbidden rectangle extends from each entrance wall
    [SerializeField] private float entranceForbiddenDepth = 5f;

    [SerializeField] private float entranceClearance = 3f;

    // Grid step used when sampling candidate positions across the full room interior
    [SerializeField] private float roomSamplingStep = 2f;

    private float roomWidth;
    private float roomDepth;
    private bool entranceNorth, entranceSouth, entranceEast, entranceWest;
    private HashSet<Vector2Int> correctPathExits;
    private GameObject assetContainer;
    private RoomAssetConfig assetConfig;

    /// <summary>
    /// Initialize the configuration for asset spawning
    /// </summary>
    public void InitializeAssets(
        float width, float depth,
        bool north, bool south, bool east, bool west,
        HashSet<Vector2Int> pathExits,
        GameObject container,
        RoomAssetConfig config)
    {
        roomWidth = width;
        roomDepth = depth;
        entranceNorth = north;
        entranceSouth = south;
        entranceEast = east;
        entranceWest = west;
        correctPathExits = pathExits;
        assetContainer = container;
        assetConfig = config;

        SpawnAssets();
    }

    /// <summary>
    /// Dispatch assets around the room.
    /// </summary>
    private void SpawnAssets()
    {
        // Do not spawn assets if no assetsConfig found
        if (assetConfig == null) return;

        foreach (AssetCategory category in assetConfig.categories)
        {
            if (category.prefabs == null || category.prefabs.Length == 0) continue;

            // Randomly, within the min and max value indicated, pick an amount of assets to be spawnned 
            int count = Random.Range(category.minCount, category.maxCount + 1);

            switch (category.type)
            {
                case AssetPlacementType.Collision:
                    SpawnCollisionAssets(category, count);
                    break;
                case AssetPlacementType.NoCollision:
                    SpawnNoCollisionAssets(category, count);
                    break;
                case AssetPlacementType.Light:
                    SpawnLightAssets(category, count);
                    break;
            }
        }
    }

    /// <summary>
    /// Prepare collision assets for spawning.
    /// </summary>
    /// <param name="category">Asset category</param>
    /// <param name="count">Amout to spawn</param>
    private void SpawnCollisionAssets(AssetCategory category, int count)
    {
        List<Vector3> candidates = GenerateRoomCandidates();
        Shuffle(candidates);

        int spawned = 0;
        foreach (Vector3 localPos in candidates)
        {
            // Spawn asset until the generated limit
            if (spawned >= count) break;
            // Verify if the spawn position is inside the forbidden area
            if (!IsInsideEntranceForbiddenZone(localPos))
            {
                // Call method to handle asset instantiation
                InstantiateAsset(category, transform.TransformPoint(localPos));
                // Increment support variable
                spawned++;
            }
        }
    }

    /// <summary>
    /// Prepare non collision assets for spawning.
    /// These assets can be spawned in any part of the room, since they don't collide with the player
    /// </summary>
    /// <param name="category">Assets category</param>
    /// <param name="count">Amount to spawn</param>
    private void SpawnNoCollisionAssets(AssetCategory category, int count)
    {
        float halfW = roomWidth / 2f;
        float halfD = roomDepth / 2f;

        for (int i = 0; i < count; i++)
        {
            Vector3 localPos = new Vector3(
                Random.Range(-halfW, halfW),
                0f,
                Random.Range(-halfD, halfD));

            InstantiateAsset(category, transform.TransformPoint(localPos));
        }
    }

    /// <summary>
    /// Prepate light assets for spawning. 
    /// </summary>
    /// <param name="category">Asset category</param>
    /// <param name="count">Amout to spawn</param>
    private void SpawnLightAssets(AssetCategory category, int count)
    {
        // Get the desired positions for the lights
        List<Vector3> positions = BuildLightPositions();

        int spawned = 0;
        foreach (Vector3 worldPos in positions)
        {
            if (spawned >= count) break;
            InstantiateAsset(category, worldPos);
            spawned++;
        }
    }

    /// <returns>Returns world space positions on both side of every correct path entrance</returns>
    private List<Vector3> BuildLightPositions()
    {
        // list to be returned
        List<Vector3> positions = new List<Vector3>();
        // Check if room has a corridor entrance that leads to correct the path
        // If not, return an empty list
        if (correctPathExits == null) return positions;

        float halfW = roomWidth / 2f;
        float halfD = roomDepth / 2f;
        float flankOffset = entranceClearance; // lateral distance from centre of corridor

        // North entrance
        if (correctPathExits.Contains(Vector2Int.up))
        {
            positions.Add(transform.TransformPoint(new Vector3(-flankOffset, 0f, halfD)));
            positions.Add(transform.TransformPoint(new Vector3(flankOffset, 0f, halfD)));
        }

        // South entrance
        if (correctPathExits.Contains(Vector2Int.down))
        {
            positions.Add(transform.TransformPoint(new Vector3(-flankOffset, 0f, -halfD)));
            positions.Add(transform.TransformPoint(new Vector3(flankOffset, 0f, -halfD)));
        }

        // East entrance
        if (correctPathExits.Contains(Vector2Int.right))
        {
            positions.Add(transform.TransformPoint(new Vector3(halfW, 0f, -flankOffset)));
            positions.Add(transform.TransformPoint(new Vector3(halfW, 0f, flankOffset)));
        }

        // West entrance
        if (correctPathExits.Contains(Vector2Int.left))
        {
            positions.Add(transform.TransformPoint(new Vector3(-halfW, 0f, -flankOffset)));
            positions.Add(transform.TransformPoint(new Vector3(-halfW, 0f, flankOffset)));
        }

        return positions;
    }

    /// <summary>
    /// Builds a grid of local space candidate possible positions for asset placement
    /// </summary>
    /// <returns></returns>
    private List<Vector3> GenerateRoomCandidates()
    {
        List<Vector3> positions = new List<Vector3>();
        float halfW = roomWidth / 2f;
        float halfD = roomDepth / 2f;

        for (float x = -halfW; x <= halfW; x += roomSamplingStep)
        {
            for (float z = -halfD; z <= halfD; z += roomSamplingStep)
            {
                positions.Add(new Vector3(x, 0f, z));
            }
        }

        return positions;
    }

    /// <summary>
    /// Check if the given position is inside of any entrance forbidden area.
    /// </summary>
    /// <param name="localPos">Position inside the room to be checked</param>
    /// <returns>Returns true if the position falls inside the forbidden area.</returns>
    private bool IsInsideEntranceForbiddenZone(Vector3 localPos)
    {
        float halfW = roomWidth / 2f;
        float halfD = roomDepth / 2f;

        if (entranceNorth && localPos.z > halfD - entranceForbiddenDepth && Mathf.Abs(localPos.x) < entranceClearance) return true;
        if (entranceSouth && localPos.z < -halfD + entranceForbiddenDepth && Mathf.Abs(localPos.x) < entranceClearance) return true;
        if (entranceEast && localPos.x > halfW - entranceForbiddenDepth && Mathf.Abs(localPos.z) < entranceClearance) return true;
        if (entranceWest && localPos.x < -halfW + entranceForbiddenDepth && Mathf.Abs(localPos.z) < entranceClearance) return true;

        return false;
    }

    /// <summary>
    /// Instantiate the asset inside the room.
    /// </summary>
    /// <param name="category">Asset category</param>
    /// <param name="worldPos">Position to be instantiated</param>
    private void InstantiateAsset(AssetCategory category, Vector3 worldPos)
    {
        // Randomly get one of the assets of the category
        GameObject prefab = category.prefabs[Random.Range(0, category.prefabs.Length)];
        // Randomly rotate the asset in Y to give an organic variety
        GameObject obj = Instantiate(prefab, worldPos, Quaternion.Euler(0f, Random.Range(0f, 360f), prefab.transform.rotation.z));

        if (assetContainer != null)
            obj.transform.SetParent(assetContainer.transform);
    }

    /// <summary>
    /// Shuffle a generic list
    /// </summary>
    /// <param name="list">List to be shuffled</param>
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void OnDrawGizmosSelected()
    {
        float halfW = roomWidth / 2f;
        float halfD = roomDepth / 2f;

        // Light placement markers
        if (correctPathExits != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Vector3 pos in BuildLightPositions())
                Gizmos.DrawSphere(pos, 0.3f);
        }

        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);

        if (entranceNorth)
            Gizmos.DrawCube(
                new Vector3(0f, 0f, halfD - entranceForbiddenDepth / 2f),
                new Vector3(entranceClearance * 2f, 1f, entranceForbiddenDepth));

        if (entranceSouth)
            Gizmos.DrawCube(
                new Vector3(0f, 0f, -halfD + entranceForbiddenDepth / 2f),
                new Vector3(entranceClearance * 2f, 1f, entranceForbiddenDepth));

        if (entranceEast)
            Gizmos.DrawCube(
                new Vector3(halfW - entranceForbiddenDepth / 2f, 0f, 0f),
                new Vector3(entranceForbiddenDepth, 1f, entranceClearance * 2f));

        if (entranceWest)
            Gizmos.DrawCube(
                new Vector3(-halfW + entranceForbiddenDepth / 2f, 0f, 0f),
                new Vector3(entranceForbiddenDepth, 1f, entranceClearance * 2f));

        Gizmos.matrix = prev;
    }
}