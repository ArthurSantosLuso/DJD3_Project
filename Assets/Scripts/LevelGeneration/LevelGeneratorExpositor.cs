using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelGeneratorExpositor : MonoBehaviour
{
    [SerializeField] private GameObject treeContainer;
    [SerializeField] private GameObject assetContainer;

    [Header("Generation Scaling")]
    [SerializeField] private int difficultyScale = 1;
    [SerializeField] private int baseRoomCount = 4;
    [SerializeField] private int roomsPerScale = 2;

    [Header("Room Size Constraints")]
    [SerializeField] private float baseMinWidth = 16f;
    [SerializeField] private float baseMinDepth = 16f;
    [SerializeField] private float sizeIncreasePerScale = 2f;
    [SerializeField] private float maxRandomVariance = 6f;

    [Header("Prefabs & Spacing")]
    [SerializeField] private GameObject roomPrefab;
    [SerializeField] private GameObject corridorPrefab;
    [SerializeField] private float minCorridorLength = 6f;

    [Tooltip("The actual length of the corridor prefab along the Z-axis in the editor before scaling")]
    [SerializeField] private float corridorPrefabLength = 10f;

    [Header("Assets")]
    [SerializeField] private RoomAssetConfig defaultAssetConfig;
    [SerializeField] private GameObject startRoomMarker;
    [SerializeField] private GameObject endRoomMarker;

    [Header("UI Components")]
    [SerializeField] private TMP_InputField difficultyInputField;
    [SerializeField] private TMP_InputField baseRoomCountInputField;
    [SerializeField] private TMP_InputField roomsPerScaleInputField;
    [SerializeField] private Button regenerateButton;

    private int defaultDifficultyScale;
    private int defaultBaseRoomCount;
    private int defaultRoomsPerScale;

    private Dictionary<Vector2Int, RoomBlueprint> levelLayout = new Dictionary<Vector2Int, RoomBlueprint>();
    private float maxGeneratedWidth = 0f;
    private float maxGeneratedDepth = 0f;
    private List<GameObject> spawnedLevelObjects = new List<GameObject>();

    void Start()
    {
        defaultDifficultyScale = difficultyScale;
        defaultBaseRoomCount = baseRoomCount;
        defaultRoomsPerScale = roomsPerScale;

        InitializeUIFields();

        if (regenerateButton != null)
            regenerateButton.onClick.AddListener(RegenerateLevel);

        GenerateLevel();
    }

    private void InitializeUIFields()
    {
        if (difficultyInputField != null) difficultyInputField.text = defaultDifficultyScale.ToString();
        if (baseRoomCountInputField != null) baseRoomCountInputField.text = defaultBaseRoomCount.ToString();
        if (roomsPerScaleInputField != null) roomsPerScaleInputField.text = defaultRoomsPerScale.ToString();
    }

    public void RegenerateLevel()
    {
        difficultyScale = ParseInputField(difficultyInputField, defaultDifficultyScale);
        baseRoomCount = ParseInputField(baseRoomCountInputField, defaultBaseRoomCount);
        roomsPerScale = ParseInputField(roomsPerScaleInputField, defaultRoomsPerScale);

        ClearLevel();
        GenerateLevel();
    }

    private int ParseInputField(TMP_InputField inputField, int defaultValue)
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text)) return defaultValue;
        return int.TryParse(inputField.text, out int result) ? result : defaultValue;
    }

    // -----------------------------------------------------------------------
    // Generation pipeline
    // -----------------------------------------------------------------------
    private void GenerateLevel()
    {
        int targetRoomCount = baseRoomCount + (difficultyScale * roomsPerScale);
        float minWidth = baseMinWidth + (difficultyScale * sizeIncreasePerScale);
        float minDepth = baseMinDepth + (difficultyScale * sizeIncreasePerScale);

        CreateLayoutBlueprint(targetRoomCount, minWidth, minDepth);
        DetermineEntrances();
        MarkStartAndEndRooms();   // sets IsStartRoom, IsEndRoom
        MarkCorrectPath();        // BFS → populates CorrectPathExits on every room
        SpawnPhysicalLevel();
    }

    private void ClearLevel()
    {
        foreach (GameObject obj in spawnedLevelObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedLevelObjects.Clear();

        ClearContainer(treeContainer);
        ClearContainer(assetContainer);
    }

    private void ClearContainer(GameObject container)
    {
        if (container == null) return;
        for (int i = container.transform.childCount - 1; i >= 0; i--)
            Destroy(container.transform.GetChild(i).gameObject);
    }

    // -----------------------------------------------------------------------
    // Blueprint construction
    // -----------------------------------------------------------------------
    private void CreateLayoutBlueprint(int targetCount, float minW, float minD)
    {
        levelLayout.Clear();
        maxGeneratedWidth = 0f;
        maxGeneratedDepth = 0f;

        List<Vector2Int> roomPositions = new List<Vector2Int>();
        Vector2Int startPos = Vector2Int.zero;

        RoomBlueprint startRoom = new RoomBlueprint(startPos);
        RandomizeRoomSize(startRoom, minW, minD);
        levelLayout[startPos] = startRoom;
        roomPositions.Add(startPos);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (roomPositions.Count < targetCount)
        {
            Vector2Int currentRoom = roomPositions[Random.Range(0, roomPositions.Count)];
            Vector2Int randomDir = directions[Random.Range(0, directions.Length)];
            Vector2Int potentialNeighbor = currentRoom + randomDir;

            if (!levelLayout.ContainsKey(potentialNeighbor))
            {
                RoomBlueprint newRoom = new RoomBlueprint(potentialNeighbor);
                RandomizeRoomSize(newRoom, minW, minD);
                levelLayout[potentialNeighbor] = newRoom;
                roomPositions.Add(potentialNeighbor);
            }
        }
    }

    private void RandomizeRoomSize(RoomBlueprint room, float minW, float minD)
    {
        room.Width = Mathf.Round(Random.Range(minW, minW + maxRandomVariance));
        room.Depth = Mathf.Round(Random.Range(minD, minD + maxRandomVariance));

        if (room.Width > maxGeneratedWidth) maxGeneratedWidth = room.Width;
        if (room.Depth > maxGeneratedDepth) maxGeneratedDepth = room.Depth;
    }

    private void DetermineEntrances()
    {
        foreach (KeyValuePair<Vector2Int, RoomBlueprint> pair in levelLayout)
        {
            Vector2Int pos = pair.Key;
            RoomBlueprint room = pair.Value;

            if (levelLayout.ContainsKey(pos + Vector2Int.up)) room.EntranceNorth = true;
            if (levelLayout.ContainsKey(pos + Vector2Int.down)) room.EntranceSouth = true;
            if (levelLayout.ContainsKey(pos + Vector2Int.right)) room.EntranceEast = true;
            if (levelLayout.ContainsKey(pos + Vector2Int.left)) room.EntranceWest = true;
        }
    }

    // -----------------------------------------------------------------------
    // Start / End room assignment
    // -----------------------------------------------------------------------

    // Start is always the generation origin.
    // End is the room furthest from it by Manhattan distance — guarantees the player
    // has to traverse most of the level to reach the goal.
    private void MarkStartAndEndRooms()
    {
        levelLayout[Vector2Int.zero].IsStartRoom = true;

        Vector2Int endPos = FindFarthestRoom();
        levelLayout[endPos].IsEndRoom = true;
    }

    private Vector2Int FindFarthestRoom()
    {
        Vector2Int farthest = Vector2Int.zero;
        int maxDist = 0;

        foreach (Vector2Int pos in levelLayout.Keys)
        {
            int dist = Mathf.Abs(pos.x) + Mathf.Abs(pos.y);
            if (dist > maxDist)
            {
                maxDist = dist;
                farthest = pos;
            }
        }

        return farthest;
    }

    // -----------------------------------------------------------------------
    // Correct-path marking via BFS
    // -----------------------------------------------------------------------

    // Runs BFS from the start room to the end room, then walks the reconstructed path
    // and records — on each room — which direction faces the next step toward the goal.
    // RoomAssetSpawner reads these exits to decide where to place guiding lights.
    private void MarkCorrectPath()
    {
        Vector2Int startPos = Vector2Int.zero;
        Vector2Int endPos = FindFarthestRoom();

        List<Vector2Int> path = BFSPath(startPos, endPos);
        if (path == null) return;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2Int current = path[i];
            Vector2Int next = path[i + 1];
            Vector2Int direction = next - current;
            levelLayout[current].CorrectPathExits.Add(direction);
        }
    }

    // Standard BFS returning the shortest grid path, or null if unreachable
    private List<Vector2Int> BFSPath(Vector2Int start, Vector2Int end)
    {
        if (start == end) return new List<Vector2Int> { start };

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == end)
            {
                // Reconstruct path from end back to start, then reverse
                List<Vector2Int> path = new List<Vector2Int>();
                Vector2Int step = end;
                while (step != start)
                {
                    path.Add(step);
                    step = cameFrom[step];
                }
                path.Add(start);
                path.Reverse();
                return path;
            }

            foreach (Vector2Int dir in dirs)
            {
                Vector2Int neighbor = current + dir;
                if (levelLayout.ContainsKey(neighbor) && !cameFrom.ContainsKey(neighbor))
                {
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return null; // disconnected graph — shouldn't happen with this generator
    }

    // -----------------------------------------------------------------------
    // Physical spawning
    // -----------------------------------------------------------------------
    private void SpawnPhysicalLevel()
    {
        float stepX = maxGeneratedWidth + minCorridorLength;
        float stepZ = maxGeneratedDepth + minCorridorLength;

        foreach (KeyValuePair<Vector2Int, RoomBlueprint> pair in levelLayout)
        {
            Vector2Int gridPos = pair.Key;
            RoomBlueprint blueprint = pair.Value;

            Vector3 worldPos = new Vector3(gridPos.x * stepX, 0f, gridPos.y * stepZ);

            GameObject roomObj = Instantiate(roomPrefab, worldPos, Quaternion.identity);
            spawnedLevelObjects.Add(roomObj);

            // Tree borders
            BorderTreeSpawner treeSpawner = roomObj.GetComponent<BorderTreeSpawner>();
            if (treeSpawner != null)
            {
                treeSpawner.InitializeRoom(
                    blueprint.Width, blueprint.Depth,
                    blueprint.EntranceNorth, blueprint.EntranceSouth,
                    blueprint.EntranceEast, blueprint.EntranceWest,
                    treeContainer);
            }

            // Asset decoration
            RoomAssetSpawner assetSpawner = roomObj.GetComponent<RoomAssetSpawner>();
            if (assetSpawner != null && defaultAssetConfig != null)
            {
                assetSpawner.InitializeAssets(
                    blueprint.Width, blueprint.Depth,
                    blueprint.EntranceNorth, blueprint.EntranceSouth,
                    blueprint.EntranceEast, blueprint.EntranceWest,
                    blueprint.CorrectPathExits,
                    assetContainer,
                    defaultAssetConfig);
            }

            // Start / end room markers
            if (blueprint.IsStartRoom && startRoomMarker != null)
            {
                GameObject marker = Instantiate(startRoomMarker, worldPos, Quaternion.identity);
                spawnedLevelObjects.Add(marker);
            }

            if (blueprint.IsEndRoom && endRoomMarker != null)
            {
                GameObject marker = Instantiate(endRoomMarker, worldPos, Quaternion.identity);
                spawnedLevelObjects.Add(marker);
            }

            // North corridor (spawned once per room — south-side room handles it)
            if (blueprint.EntranceNorth)
            {
                RoomBlueprint northNeighbor = levelLayout[gridPos + Vector2Int.up];
                Vector3 northNeighborWorldPos = new Vector3(gridPos.x * stepX, 0f, (gridPos.y + 1) * stepZ);

                float currentRoomTopEdge = worldPos.z + (blueprint.Depth / 2f);
                float neighborBottomEdge = northNeighborWorldPos.z - (northNeighbor.Depth / 2f);
                float distance = neighborBottomEdge - currentRoomTopEdge;

                Vector3 corridorPos = new Vector3(worldPos.x, 0f, currentRoomTopEdge + (distance / 2f));
                GameObject corridor = Instantiate(corridorPrefab, corridorPos, Quaternion.identity);
                spawnedLevelObjects.Add(corridor);

                float scaleZ = distance / corridorPrefabLength;
                corridor.transform.localScale = new Vector3(
                    corridor.transform.localScale.x,
                    corridor.transform.localScale.y,
                    scaleZ);
            }

            // East corridor
            if (blueprint.EntranceEast)
            {
                RoomBlueprint eastNeighbor = levelLayout[gridPos + Vector2Int.right];
                Vector3 eastNeighborWorldPos = new Vector3((gridPos.x + 1) * stepX, 0f, gridPos.y * stepZ);

                float currentRoomRightEdge = worldPos.x + (blueprint.Width / 2f);
                float neighborLeftEdge = eastNeighborWorldPos.x - (eastNeighbor.Width / 2f);
                float distance = neighborLeftEdge - currentRoomRightEdge;

                Vector3 corridorPos = new Vector3(currentRoomRightEdge + (distance / 2f), 0f, worldPos.z);
                GameObject corridor = Instantiate(corridorPrefab, corridorPos, Quaternion.Euler(0f, 90f, 0f));
                spawnedLevelObjects.Add(corridor);

                float scaleZ = distance / corridorPrefabLength;
                corridor.transform.localScale = new Vector3(
                    corridor.transform.localScale.x,
                    corridor.transform.localScale.y,
                    scaleZ);
            }
        }
    }
}