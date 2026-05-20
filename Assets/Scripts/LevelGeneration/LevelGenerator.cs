using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject treeContainer;

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

    private Dictionary<Vector2Int, RoomBlueprint> levelLayout = new Dictionary<Vector2Int, RoomBlueprint>();

    // Tracks the absolute largest room generated for the level
    // Used to calculate grid spacing so rooms never collide
    private float maxGeneratedWidth = 0f;
    private float maxGeneratedDepth = 0f;

    void Start()
    {
        GenerateLevel();
    }

    private void GenerateLevel()
    {
        // Scale sizes and room amount based on difficulty
        int targetRoomCount = baseRoomCount + (difficultyScale * roomsPerScale);
        float minWidth = baseMinWidth + (difficultyScale * sizeIncreasePerScale);
        float minDepth = baseMinDepth + (difficultyScale * sizeIncreasePerScale);

        CreateLayoutBlueprint(targetRoomCount, minWidth, minDepth);
        DetermineEntrances();
        SpawnPhysicalLevel();
    }

    // Create a random layout on a 2D grid
    private void CreateLayoutBlueprint(int targetCount, float minW, float minD)
    {
        levelLayout.Clear();
        maxGeneratedWidth = 0f;
        maxGeneratedDepth = 0f;

        List<Vector2Int> roomPositions = new List<Vector2Int>();
        Vector2Int startPos = Vector2Int.zero;

        // Create initial room
        RoomBlueprint startRoom = new RoomBlueprint(startPos);
        RandomizeRoomSize(startRoom, minW, minD);
        levelLayout[startPos] = startRoom;
        roomPositions.Add(startPos);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // Algorithm to grow the level
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

        // Check if width/depth is bigger than current biggest width/depth 
        if (room.Width > maxGeneratedWidth) maxGeneratedWidth = room.Width;
        if (room.Depth > maxGeneratedDepth) maxGeneratedDepth = room.Depth;
    }

    // Look at adjacent cells to check where corridors need to cross link
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

    // Instantiate rooms and corridors
    private void SpawnPhysicalLevel()
    {
        // Grid stride relies on max dimensions to make sure small rooms get longer hallways instead of breaking the grid lines
        float stepX = maxGeneratedWidth + minCorridorLength;
        float stepZ = maxGeneratedDepth + minCorridorLength;

        foreach (KeyValuePair<Vector2Int, RoomBlueprint> pair in levelLayout)
        {
            Vector2Int gridPos = pair.Key;
            RoomBlueprint blueprint = pair.Value;

            Vector3 worldPos = new Vector3(gridPos.x * stepX, 0, gridPos.y * stepZ);

            // Spawn room pivot and pass rendering properties to its dedicated spawner
            GameObject roomObj = Instantiate(roomPrefab, worldPos, Quaternion.identity);
            BorderTreeSpawner spawner = roomObj.GetComponent<BorderTreeSpawner>();
            if (spawner != null)
            {
                spawner.InitializeRoom(blueprint.Width, blueprint.Depth,
                    blueprint.EntranceNorth, blueprint.EntranceSouth, blueprint.EntranceEast, blueprint.EntranceWest, treeContainer);
            }

            // North corridors
            if (blueprint.EntranceNorth)
            {
                RoomBlueprint northNeighbor = levelLayout[gridPos + Vector2Int.up];
                Vector3 northNeighborWorldPos = new Vector3(gridPos.x * stepX, 0, (gridPos.y + 1) * stepZ);

                // Calculate the gap size between where this room ends and the next begins
                float currentRoomTopEdge = worldPos.z + (blueprint.Depth / 2f);
                float neighborBottomEdge = northNeighborWorldPos.z - (northNeighbor.Depth / 2f);
                float distance = neighborBottomEdge - currentRoomTopEdge;

                Vector3 corridorPos = new Vector3(worldPos.x, 0, currentRoomTopEdge + (distance / 2f));
                GameObject corridor = Instantiate(corridorPrefab, corridorPos, Quaternion.identity);

                // Normalizes the stretch factor relative to the source prefab original length
                float newScaleZ = distance / corridorPrefabLength;
                corridor.transform.localScale = new Vector3(corridor.transform.localScale.x, corridor.transform.localScale.y, newScaleZ);
            }

            // East corridors
            if (blueprint.EntranceEast)
            {
                RoomBlueprint eastNeighbor = levelLayout[gridPos + Vector2Int.right];
                Vector3 eastNeighborWorldPos = new Vector3((gridPos.x + 1) * stepX, 0, gridPos.y * stepZ);

                // Calculate the gap size between where this room ends and the next begins
                float currentRoomRightEdge = worldPos.x + (blueprint.Width / 2f);
                float neighborLeftEdge = eastNeighborWorldPos.x - (eastNeighbor.Width / 2f);
                float distance = neighborLeftEdge - currentRoomRightEdge;

                Vector3 corridorPos = new Vector3(currentRoomRightEdge + (distance / 2f), 0, worldPos.z);
                GameObject corridor = Instantiate(corridorPrefab, corridorPos, Quaternion.Euler(0, 90f, 0));

                // Normalizes the stretch factor relative to the source prefab original length
                float newScaleZ = distance / corridorPrefabLength;
                corridor.transform.localScale = new Vector3(corridor.transform.localScale.x, corridor.transform.localScale.y, newScaleZ);
            }
        }
    }
}