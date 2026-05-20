using UnityEngine;

public class BorderSpawnerManual : MonoBehaviour
{
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private float roomWidth = 20f, roomDepth = 20f;
    [SerializeField] private float spacing = 2f;
    [SerializeField] private int extraLayers = 2;
    [SerializeField] private bool shouldStart = true;

    [Header("Entrances")]
    [SerializeField] private bool entranceNorth = false;
    [SerializeField] private bool entranceSouth = false;
    [SerializeField] private bool entranceEast = false;
    [SerializeField] private bool entranceWest = false;
    [SerializeField] private float entranceWidth = 4f;

    private GameObject spawnParent;

    public void InitializeRoom(float width, float depth, bool north, bool south, bool east, bool west, GameObject treesParent = null)
    {
        spawnParent = treesParent;
        roomWidth = width;
        roomDepth = depth;

        entranceNorth = north;
        entranceSouth = south;
        entranceEast = east;
        entranceWest = west;

        SpawnBorderTrees();
    }

    private void Start()
    {
        if (shouldStart)
        {
            spawnParent = GameObject.FindWithTag("TreeContainer");
            SpawnBorderTrees();
        }
    }

    void SpawnBorderTrees()
    {
        Vector3 center = transform.position;

        for (int layer = 0; layer <= extraLayers; layer++)
        {
            float ex = roomWidth / 2f + layer * spacing;
            float ez = roomDepth / 2f + layer * spacing;

            for (float x = -ex; x <= ex; x += spacing)
            {
                if (!IsInEntrance(x, entranceNorth)) SpawnTree(center + new Vector3(x, 0, ez));
                if (!IsInEntrance(x, entranceSouth)) SpawnTree(center + new Vector3(x, 0, -ez));
            }

            for (float z = -ez + spacing; z < ez; z += spacing)
            {
                if (!IsInEntrance(z, entranceEast)) SpawnTree(center + new Vector3(ex, 0, z));
                if (!IsInEntrance(z, entranceWest)) SpawnTree(center + new Vector3(-ex, 0, z));
            }
        }
    }

    bool IsInEntrance(float pos, bool hasEntrance)
    {
        if (!hasEntrance) return false;
        float half = entranceWidth / 2f;
        return pos >= -half && pos <= half;
    }

    void SpawnTree(Vector3 pos)
    {
        pos += new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
        var prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
        var t = Instantiate(prefab, pos, Quaternion.identity, spawnParent.transform);
        t.transform.rotation = Quaternion.identity;
        t.transform.localScale *= Random.Range(0.85f, 1.15f);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;
        float halfW = roomWidth / 2f;
        float halfD = roomDepth / 2f;
        float halfE = entranceWidth / 2f;

        Gizmos.color = Color.green;
        if (entranceNorth) Gizmos.DrawLine(center + new Vector3(-halfE, 0, halfD), center + new Vector3(halfE, 0, halfD));
        if (entranceSouth) Gizmos.DrawLine(center + new Vector3(-halfE, 0, -halfD), center + new Vector3(halfE, 0, -halfD));
        if (entranceEast) Gizmos.DrawLine(center + new Vector3(halfW, 0, -halfE), center + new Vector3(halfW, 0, halfE));
        if (entranceWest) Gizmos.DrawLine(center + new Vector3(-halfW, 0, -halfE), center + new Vector3(-halfW, 0, halfE));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, new Vector3(roomWidth, 1f, roomDepth));
    }
}
