using UnityEngine;
using System.Collections.Generic;

public class BorderTreeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private float roomWidth = 20f, roomDepth = 20f;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private int extraLayers = 2;
    [SerializeField] private bool shoudStartInitialize = false;

    [Header("Entrances")]
    [SerializeField] private bool entranceNorth = false;
    [SerializeField] private bool entranceSouth = false;
    [SerializeField] private bool entranceEast = false;
    [SerializeField] private bool entranceWest = false;
    [SerializeField] private float entranceWidth = 4f;

    private GameObject customTreeContainer;

    private void Start()
    {
        // Debug setup allowing standalone testing without a level generator script running
        if (shoudStartInitialize)
        {
            customTreeContainer = GameObject.FindGameObjectWithTag("TreeContainer");
            SpawnBorderTrees();
        }
    }

    public void InitializeRoom(float width, float depth, bool north, bool south, bool east, bool west, GameObject container)
    {
        roomWidth = width;
        roomDepth = depth;
        entranceNorth = north;
        entranceSouth = south;
        entranceEast = east;
        entranceWest = west;
        customTreeContainer = container;

        SpawnBorderTrees();
    }

    private void SpawnBorderTrees()
    {
        // Loop tracks outwards across concentric layout rings for dense wall look
        for (int layer = 0; layer <= extraLayers; layer++)
        {
            float ex = roomWidth / 2f + layer * spacing;
            float ez = roomDepth / 2f + layer * spacing;

            // Horizontal walls (north & south borders)
            for (float x = -ex; x <= ex; x += spacing)
            {
                // transform.TransformPoint converts local grid offsets relative to the parent transform's local matrix
                // This guarantees the layout tree coordinates spin, scale, and displace automatically alongside the room object
                if (!IsInEntrance(x, entranceNorth)) SpawnTree(transform.TransformPoint(new Vector3(x, 0, ez)));
                if (!IsInEntrance(x, entranceSouth)) SpawnTree(transform.TransformPoint(new Vector3(x, 0, -ez)));
            }

            // Vertical walls (east & west borders)
            for (float z = -ez + spacing; z < ez; z += spacing)
            {
                if (!IsInEntrance(z, entranceEast)) SpawnTree(transform.TransformPoint(new Vector3(ex, 0, z)));
                if (!IsInEntrance(z, entranceWest)) SpawnTree(transform.TransformPoint(new Vector3(-ex, 0, z)));
            }
        }
    }

    // Helper checking whether a coordinate is within the doorway opening thresholds
    private bool IsInEntrance(float pos, bool hasEntrance)
    {
        if (!hasEntrance) return false;
        float half = entranceWidth / 2f;
        return pos >= -half && pos <= half;
    }

    private void SpawnTree(Vector3 pos)
    {
        // Offset variance to make the grid positioning look organic rather than unnatural
        pos += new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
        var prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];

        // Matches tree object rotation back to the parent room rotation setting
        var t = Instantiate(prefab, pos, transform.rotation);
        t.transform.localScale *= Random.Range(0.85f, 1.15f);

        if (customTreeContainer != null)
        {
            t.transform.SetParent(customTreeContainer.transform);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Binds scene view preview rendering matrix directly into the local coordinates of this transform
        // This ensures wire gizmos track in the scene window when manually move or rotate rooms
        Matrix4x4 originalMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        float halfW = roomWidth / 2f;
        float halfD = roomDepth / 2f;
        float halfE = entranceWidth / 2f;

        Gizmos.color = Color.green;
        if (entranceNorth) Gizmos.DrawLine(new Vector3(-halfE, 0, halfD), new Vector3(halfE, 0, halfD));
        if (entranceSouth) Gizmos.DrawLine(new Vector3(-halfE, 0, -halfD), new Vector3(halfE, 0, -halfD));
        if (entranceEast) Gizmos.DrawLine(new Vector3(halfW, 0, -halfE), new Vector3(halfW, 0, halfE));
        if (entranceWest) Gizmos.DrawLine(new Vector3(-halfW, 0, -halfE), new Vector3(-halfW, 0, halfE));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(roomWidth, 1f, roomDepth));

        // Reinstate old system matrix settings so it doesn't leave editor scaling altered globally
        Gizmos.matrix = originalMatrix;
    }
}