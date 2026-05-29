using UnityEngine;
using System.Collections.Generic;

/*
This script handles:
Room generation, room amout, its positions, notufy to spawn assets and border to each room.
First it creates a 2D layout of the room, then translate it to the 3D level itself.

* 'Breadth-First Search'
It's a graph traversal algorithm that explores nodes level by level, always visiting the closest ones first before going further out.
In the case of our game "graph" is our room grid. 
Starting from the start room, BFS visits all rooms 1 step away, then all rooms 2 steps away, and so on, guaranteeing that when it reaches the end room, the path it took is the shortest possible one through the layout.
The reason it's used here instead of something simpler is that Manhattan distance alone can't give us a path, it just tells us how far two rooms are. BFS actually walks the connections that exist in our levelLayout, so it respects the real topology of the generated level.
*/

public class LevelGenerator : MonoBehaviour
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

    // 2d layout of the level
    private Dictionary<Vector2Int, RoomBlueprint> levelLayout = new Dictionary<Vector2Int, RoomBlueprint>();

    // Tracks the absolute largest room generated for the level
    // Used to calculate grid spacing so rooms never collide
    private float maxGeneratedWidth = 0f;
    private float maxGeneratedDepth = 0f;
    private List<GameObject> spawnedLevelObjects = new List<GameObject>();

    private void Start()
    {
        // Generate level
        GenerateLevel();
    }

    /// <summary>
    /// Call all methods to handle the level generation.
    /// </summary>
    private void GenerateLevel()
    {
        // Scale sizes and room amount based on difficulty
        int targetRoomCount = baseRoomCount + (difficultyScale * roomsPerScale);
        float minWidth = baseMinWidth + (difficultyScale * sizeIncreasePerScale);
        float minDepth = baseMinDepth + (difficultyScale * sizeIncreasePerScale);

        // Determine the 2D layout of the level
        CreateLayoutBlueprint(targetRoomCount, minWidth, minDepth);
        DetermineEntrances();       // Set all entrances to all rooms
        MarkStartAndEndRooms();     // sets IsStartRoom, IsEndRoom
        MarkCorrectPath();          // BFS* populates CorrectPathExits on every room
        SpawnPhysicalLevel();       // Spawn the level itself to 3D world
    }


    /// <summary>
    /// Create a random room layout on a 2D grid.
    /// </summary>
    /// <param name="targetCount">Amout of rooms to spawn</param>
    /// <param name="minW">Minumun widht for each room</param>
    /// <param name="minD">Minimun depth for each room</param>
    private void CreateLayoutBlueprint(int targetCount, float minW, float minD)
    {
        maxGeneratedWidth = 0f;
        maxGeneratedDepth = 0f;

        List<Vector2Int> roomPositions = new List<Vector2Int>();
        Vector2Int startPos = Vector2Int.zero;

        // Create initial room
        RoomBlueprint startRoom = new RoomBlueprint(startPos);
        // Generate a random size for the room
        RandomizeRoomSize(startRoom, minW, minD);
        // Add the room in the layout
        levelLayout[startPos] = startRoom;
        // Store room position
        roomPositions.Add(startPos);

        // Create all potential corridor directions for rooms
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        // Algorithm to grow the level
        while (roomPositions.Count < targetCount)
        {
            // Get one room random room position
            Vector2Int currentRoom = roomPositions[Random.Range(0, roomPositions.Count)];
            // Get one random direction
            Vector2Int randomDir = directions[Random.Range(0, directions.Length)];
            // Create a new potetial neighbor
            Vector2Int potentialNeighbor = currentRoom + randomDir;

            // If the layout do not already have the potential neighbor, add it
            if (!levelLayout.ContainsKey(potentialNeighbor))
            {
                // Create the new room
                RoomBlueprint newRoom = new RoomBlueprint(potentialNeighbor);
                // Generate a random size for the room
                RandomizeRoomSize(newRoom, minW, minD);
                // Add the room in the layout
                levelLayout[potentialNeighbor] = newRoom;
                // Add room position to list
                roomPositions.Add(potentialNeighbor);
            }
        }
    }

    /// <summary>
    /// Randommize the size of a room.
    /// </summary>
    /// <param name="room">Room to be randomized</param>
    /// <param name="minW">Minimum width for the room</param>
    /// <param name="minD">Minimum depth for the room</param>
    private void RandomizeRoomSize(RoomBlueprint room, float minW, float minD)
    {
        // Randomize size of the room with integer values
        room.Width = Mathf.Round(Random.Range(minW, minW + maxRandomVariance));
        room.Depth = Mathf.Round(Random.Range(minD, minD + maxRandomVariance));

        // Check if width/depth is bigger than current biggest width/depth 
        if (room.Width > maxGeneratedWidth) maxGeneratedWidth = room.Width;
        if (room.Depth > maxGeneratedDepth) maxGeneratedDepth = room.Depth;
    }

    /// <summary>
    /// Add entrances flags to all room.
    /// Look at adjacent cells to check where corridors need to cross link.
    /// </summary>
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

    /// <summary>
    /// Flag initial and final room.
    /// Start is always the generation origin (point 0, 0, 0).
    /// End is the room furthest from initial room by Manhattan distance.
    /// </summary>
    private void MarkStartAndEndRooms()
    {
        levelLayout[Vector2Int.zero].IsStartRoom = true;

        Vector2Int endPos = FindFarthestRoom();
        levelLayout[endPos].IsEndRoom = true;
    }

    /// <summary>
    /// Look for the farthest room from point zero (intial room)
    /// </summary>
    /// <returns>Return farthest room from initial room.</returns>
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

    /// <summary>
    /// Runs BFS from the start room to the end room.
    /// Store, on each room, which direction faces the next step toward the final room.
    /// </summary>
    private void MarkCorrectPath()
    {
        // Initial room
        Vector2Int startPos = Vector2Int.zero;
        // Final room
        Vector2Int endPos = FindFarthestRoom();

        // Creates the "correct" room path
        List<Vector2Int> path = BFSPath(startPos, endPos);
        if (path == null) return;

        // Cycle through all room paths
        for (int i = 0; i < path.Count - 1; i++)
        {
            // Get current room
            Vector2Int current = path[i];
            // Get next room
            Vector2Int next = path[i + 1];
            // Get direction of the current from the next
            Vector2Int direction = next - current;
            // Add the correct path to room
            levelLayout[current].CorrectPathExits.Add(direction);
        }
    }

    /// <summary>
    /// Runs BFS algorithm to get the shortest path from initial to final room.
    /// </summary>
    /// <param name="start">Initial room</param>
    /// <param name="end">Final room</param>
    /// <returns>Return the shortest grid path, or null if unreachable</returns>
    private List<Vector2Int> BFSPath(Vector2Int start, Vector2Int end)
    {
        // If initial room IS final room, do nothing
        if (start == end) return new List<Vector2Int> { start };

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        // Maps each visited room back to the room it was reached from, used to reconstruct the path
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();

        queue.Enqueue(start);
        // Start room points to itself to mark it as visited
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
                // Only visit neighbors that exist in the layout and haven't been seen yet
                if (levelLayout.ContainsKey(neighbor) && !cameFrom.ContainsKey(neighbor))
                {
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        return null; // Disconnected graph, this shouldn't happen with this system, but just to make sure
    }

    /// <summary>
    /// Translates the 2D blueprint into actual GameObjects in the 3D world.
    /// Spawns rooms, corridors, tree borders, assets, and role markers.
    /// </summary>
    private void SpawnPhysicalLevel()
    {
        // Grid step is based on the largest room so no two rooms ever overlap
        float stepX = maxGeneratedWidth + minCorridorLength;
        float stepZ = maxGeneratedDepth + minCorridorLength;

        foreach (KeyValuePair<Vector2Int, RoomBlueprint> pair in levelLayout)
        {
            Vector2Int gridPos = pair.Key;
            RoomBlueprint blueprint = pair.Value;

            // Convert 2D grid coordinates to a world position
            Vector3 worldPos = new Vector3(gridPos.x * stepX, 0f, gridPos.y * stepZ);

            // Spawn the room prefab at the calculated world position
            GameObject roomObj = Instantiate(roomPrefab, worldPos, Quaternion.identity);
            spawnedLevelObjects.Add(roomObj);

            // Hand off room dimensions and entrance flags to the tree border spawner
            BorderTreeSpawner treeSpawner = roomObj.GetComponent<BorderTreeSpawner>();
            if (treeSpawner != null)
            {
                treeSpawner.InitializeRoom(
                    blueprint.Width, blueprint.Depth,
                    blueprint.EntranceNorth, blueprint.EntranceSouth,
                    blueprint.EntranceEast, blueprint.EntranceWest,
                    treeContainer);
            }

            // Hand off placement data to the asset spawner
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

            // Spawn the start room marker at the room's centre
            if (blueprint.IsStartRoom && startRoomMarker != null)
            {
                GameObject marker = Instantiate(startRoomMarker, worldPos, Quaternion.identity);
                spawnedLevelObjects.Add(marker);
            }

            // Spawn the end room marker at the room's centre
            if (blueprint.IsEndRoom && endRoomMarker != null)
            {
                GameObject marker = Instantiate(endRoomMarker, worldPos, Quaternion.identity);
                spawnedLevelObjects.Add(marker);
            }

            // North corridor
            if (blueprint.EntranceNorth)
            {
                RoomBlueprint northNeighbor = levelLayout[gridPos + Vector2Int.up];
                // World position of the room directly above this one
                Vector3 northNeighborWorldPos = new Vector3(gridPos.x * stepX, 0f, (gridPos.y + 1) * stepZ);

                // Calculate the gap between the top edge of this room and the bottom edge of the neighbor
                float currentRoomTopEdge = worldPos.z + (blueprint.Depth / 2f);
                float neighborBottomEdge = northNeighborWorldPos.z - (northNeighbor.Depth / 2f);
                float distance = neighborBottomEdge - currentRoomTopEdge;

                // Place the corridor prefab centred in the gap
                Vector3 corridorPos = new Vector3(worldPos.x, 0f, currentRoomTopEdge + (distance / 2f));
                GameObject corridor = Instantiate(corridorPrefab, corridorPos, Quaternion.identity);
                spawnedLevelObjects.Add(corridor);

                // Stretch the corridor prefab to exactly fill the gap
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
                // World position of the room directly to the right of this one
                Vector3 eastNeighborWorldPos = new Vector3((gridPos.x + 1) * stepX, 0f, gridPos.y * stepZ);

                // Calculate the gap between the right edge of this room and the left edge of the neighbor
                float currentRoomRightEdge = worldPos.x + (blueprint.Width / 2f);
                float neighborLeftEdge = eastNeighborWorldPos.x - (eastNeighbor.Width / 2f);
                float distance = neighborLeftEdge - currentRoomRightEdge;

                // Place the corridor prefab centred in the gap, rotated 90° to run east-west
                Vector3 corridorPos = new Vector3(currentRoomRightEdge + (distance / 2f), 0f, worldPos.z);
                GameObject corridor = Instantiate(corridorPrefab, corridorPos, Quaternion.Euler(0f, 90f, 0f));
                spawnedLevelObjects.Add(corridor);

                // Stretch the corridor prefab to exactly fill the gap
                float scaleZ = distance / corridorPrefabLength;
                corridor.transform.localScale = new Vector3(
                    corridor.transform.localScale.x,
                    corridor.transform.localScale.y,
                    scaleZ);
            }
        }
    }
}