using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelGeneratorExpositor : MonoBehaviour
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
        {
            regenerateButton.onClick.AddListener(RegenerateLevel);
        }

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
        if (inputField == null || string.IsNullOrEmpty(inputField.text))
        {
            return defaultValue;
        }

        if (int.TryParse(inputField.text, out int result))
        {
            return result;
        }

        return defaultValue;
    }

    private void GenerateLevel()
    {
        int targetRoomCount = baseRoomCount + (difficultyScale * roomsPerScale);
        float minWidth = baseMinWidth + (difficultyScale * sizeIncreasePerScale);
        float minDepth = baseMinDepth + (difficultyScale * sizeIncreasePerScale);

        CreateLayoutBlueprint(targetRoomCount, minWidth, minDepth);
        DetermineEntrances();
        SpawnPhysicalLevel();
    }

    private void ClearLevel()
    {
        foreach (GameObject obj in spawnedLevelObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedLevelObjects.Clear();

        if (treeContainer != null)
        {
            for (int i = treeContainer.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(treeContainer.transform.GetChild(i).gameObject);
            }
        }
    }

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

    private void SpawnPhysicalLevel()
    {
        float stepX = maxGeneratedWidth + minCorridorLength;
        float stepZ = maxGeneratedDepth + minCorridorLength;

        foreach (KeyValuePair<Vector2Int, RoomBlueprint> pair in levelLayout)
        {
            Vector2Int gridPos = pair.Key;
            RoomBlueprint blueprint = pair.Value;

            Vector3 worldPos = new Vector3(gridPos.x * stepX, 0, gridPos.y * stepZ);

            GameObject roomObj = Instantiate(roomPrefab, worldPos, Quaternion.identity);
            spawnedLevelObjects.Add(roomObj);

            BorderTreeSpawner spawner = roomObj.GetComponent<BorderTreeSpawner>();
            if (spawner != null)
            {
                spawner.InitializeRoom(blueprint.Width, blueprint.Depth,
                    blueprint.EntranceNorth, blueprint.EntranceSouth, blueprint.EntranceEast, blueprint.EntranceWest, treeContainer);
            }

            if (blueprint.EntranceNorth)
            {
                RoomBlueprint northNeighbor = levelLayout[gridPos + Vector2Int.up];
                Vector3 northNeighborWorldPos = new Vector3(gridPos.x * stepX, 0, (gridPos.y + 1) * stepZ);

                float currentRoomTopEdge = worldPos.z + (blueprint.Depth / 2f);
                float neighborBottomEdge = northNeighborWorldPos.z - (northNeighbor.Depth / 2f);
                float distance = neighborBottomEdge - currentRoomTopEdge;

                Vector3 corridorPos = new Vector3(worldPos.x, 0, currentRoomTopEdge + (distance / 2f));
                GameObject corridor = Instantiate(corridorPrefab, corridorPos, Quaternion.identity);
                spawnedLevelObjects.Add(corridor);

                float newScaleZ = distance / corridorPrefabLength;
                corridor.transform.localScale = new Vector3(corridor.transform.localScale.x, corridor.transform.localScale.y, newScaleZ);
            }

            if (blueprint.EntranceEast)
            {
                RoomBlueprint eastNeighbor = levelLayout[gridPos + Vector2Int.right];
                Vector3 eastNeighborWorldPos = new Vector3((gridPos.x + 1) * stepX, 0, gridPos.y * stepZ);

                float currentRoomRightEdge = worldPos.x + (blueprint.Width / 2f);
                float neighborLeftEdge = eastNeighborWorldPos.x - (eastNeighbor.Width / 2f);
                float distance = neighborLeftEdge - currentRoomRightEdge;

                Vector3 corridorPos = new Vector3(currentRoomRightEdge + (distance / 2f), 0, worldPos.z);
                GameObject corridor = Instantiate(corridorPrefab, corridorPos, Quaternion.Euler(0, 90f, 0));
                spawnedLevelObjects.Add(corridor);

                float newScaleZ = distance / corridorPrefabLength;
                corridor.transform.localScale = new Vector3(corridor.transform.localScale.x, corridor.transform.localScale.y, newScaleZ);
            }
        }
    }
}