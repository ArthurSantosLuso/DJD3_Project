using UnityEngine;

public class BorderTreeSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] treePrefabs;
    [SerializeField]
    private float roomWidth = 20f, roomDepth = 20f;
    [SerializeField]
    private float spacing = 2f;
    [SerializeField] 
    private int extraLayers = 2;

    void Start() => SpawnBorderTrees();

    void SpawnBorderTrees()
    {
        for (int layer = 0; layer <= extraLayers; layer++)
        {
            float ex = roomWidth / 2f + layer * spacing;
            float ez = roomDepth / 2f + layer * spacing;

            for (float x = -ex; x <= ex; x += spacing)
            {
                SpawnTree(new Vector3(x, 0, ez));
                SpawnTree(new Vector3(x, 0, -ez));
            }
            for (float z = -ez + spacing; z < ez; z += spacing)
            {
                SpawnTree(new Vector3(ex, 0, z));
                SpawnTree(new Vector3(-ex, 0, z));
            }
        }
    }

    void SpawnTree(Vector3 pos)
    {
        pos += new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
        var prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
        var t = Instantiate(prefab, pos, Quaternion.identity);
        t.transform.localScale *= Random.Range(0.85f, 1.15f);
    }
}