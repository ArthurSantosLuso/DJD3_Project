using UnityEngine;
using System.Collections.Generic;

/*
This script handles:
Spawning decorative and gameplay assets inside a room after it's placed in the world.
Assets are split into three categories: collision objects (blocked near entrances),
non-collision objects (placed anywhere), and lights (placed on the side of the correct-path entrances).
*/

public class RoomAssetSpawner : MonoBehaviour
{
    // How deep into the room the forbidden rectangle extends from each entrance wall
    [SerializeField] private float entranceForbiddenDepth = 5f;
    [SerializeField] private float entranceClearance = 3f;

    // Grid step used when sampling candidate positions across the room interior
    [SerializeField] private float roomSamplingStep = 2f;

    private float roomWidth;
    private float roomDepth;
    private bool entranceNorth, entranceSouth, entranceEast, entranceWest;
    private HashSet<Vector2Int> correctPathExits;
    private GameObject assetContainer;
    private RoomAssetConfig assetConfig;

    /// <summary>
    /// Sets up room data and triggers asset placement. Called by LevelGenerator after the room is spawned.
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
    /// Dispatches asset placement for each category in the config.
    /// </summary>
    private void SpawnAssets()
    {
        if (assetConfig == null) return;

        foreach (AssetCategory category in assetConfig.categories)
        {
            if (category.prefabs == null || category.prefabs.Length == 0) continue;

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
    /// Places assets that have collision. They don't spawn in forbidden zones around entrances.
    /// </summary>
    private void SpawnCollisionAssets(AssetCategory category, int count)
    {
        List<Vector3> candidates = GenerateRoomCandidates();
        Shuffle(candidates);

        int spawned = 0;
        foreach (Vector3 localPos in candidates)
        {
            if (spawned >= count) break;
            if (!IsInsideEntranceForbiddenZone(localPos))
            {
                InstantiateAsset(category, transform.TransformPoint(localPos));
                spawned++;
            }
        }
    }

    /// <summary>
    /// Places assets with no collision. These can go anywhere in the room since they don't block the player.
    /// </summary>
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
    /// Places lights at fixed positions on the side of each entrance that leads toward the correct path.
    /// </summary>
    private void SpawnLightAssets(AssetCategory category, int count)
    {
        List<Vector3> positions = BuildLightPositions();

        int spawned = 0;
        foreach (Vector3 worldPos in positions)
        {
            if (spawned >= count) break;
            InstantiateAsset(category, worldPos);
            spawned++;
        }
    }

    /// <summary>
    /// Builds world space light positions near every correct path entrance of this room.
    /// </summary>
    private List<Vector3> BuildLightPositions()
    {
        List<Vector3> positions = new List<Vector3>();
        if (correctPathExits == null) return positions;

        float halfW = roomWidth / 2f;
        float halfD = roomDepth / 2f;
        float flankOffset = entranceClearance;

        if (correctPathExits.Contains(Vector2Int.up))
        {
            positions.Add(transform.TransformPoint(new Vector3(-flankOffset, 0f, halfD)));
            positions.Add(transform.TransformPoint(new Vector3(flankOffset, 0f, halfD)));
        }

        if (correctPathExits.Contains(Vector2Int.down))
        {
            positions.Add(transform.TransformPoint(new Vector3(-flankOffset, 0f, -halfD)));
            positions.Add(transform.TransformPoint(new Vector3(flankOffset, 0f, -halfD)));
        }

        if (correctPathExits.Contains(Vector2Int.right))
        {
            positions.Add(transform.TransformPoint(new Vector3(halfW, 0f, -flankOffset)));
            positions.Add(transform.TransformPoint(new Vector3(halfW, 0f, flankOffset)));
        }

        if (correctPathExits.Contains(Vector2Int.left))
        {
            positions.Add(transform.TransformPoint(new Vector3(-halfW, 0f, -flankOffset)));
            positions.Add(transform.TransformPoint(new Vector3(-halfW, 0f, flankOffset)));
        }

        return positions;
    }

    /// <summary>
    /// Builds a grid of local space candidate positions covering the room floor.
    /// </summary>
    private List<Vector3> GenerateRoomCandidates()
    {
        List<Vector3> positions = new List<Vector3>();
        float halfW = roomWidth / 2f;
        float halfD = roomDepth / 2f;

        for (float x = -halfW; x <= halfW; x += roomSamplingStep)
            for (float z = -halfD; z <= halfD; z += roomSamplingStep)
                positions.Add(new Vector3(x, 0f, z));

        return positions;
    }

    /// <summary>
    /// Returns true if the given local position falls inside the forbidden zone of any entrance.
    /// </summary>
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
    /// Picks a random prefab from the category and instantiates it, applying a random Y rotation.
    /// Skips the spawn if the prefab's chance roll fails.
    /// </summary>
    private void InstantiateAsset(AssetCategory category, Vector3 worldPos)
    {
        AssetPrefab randAsset = category.prefabs[Random.Range(0, category.prefabs.Length)];

        if (Random.value > randAsset.chanceToSpawn) return;

        GameObject obj = Instantiate(
            randAsset.prefab,
            worldPos,
            Quaternion.Euler(0f, Random.Range(0f, 360f), randAsset.prefab.transform.rotation.z));

        if (assetContainer != null)
            obj.transform.SetParent(assetContainer.transform);
    }

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

        // Correct path light positions
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