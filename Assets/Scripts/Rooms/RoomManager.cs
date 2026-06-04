using UnityEngine;
using System.Collections.Generic;

/*
This script handles:
Room activation, gate control, enemy spawning, and portal setup.
When the player enters a room trigger, the room activates based on its type — 
combat rooms lock the gates and spawn enemies, special rooms spawn a portal to a linked arena.
Gates reopen (or rewards appear) once all enemies are defeated.
*/

public class RoomManager : MonoBehaviour
{
    [Header("Gate")]
    [SerializeField] private GameObject gatePrefab;

    [Header("Enemy Spawning")]
    [SerializeField] private float delayBetweenSpawns = 0.4f;

    [Tooltip("How far from the room edges enemies are kept when spawning.")]
    [SerializeField] private float spawnMargin = 3f;

    [Header("Entrance Clearance")]
    [SerializeField] private float entranceClearance = 3f;

    [Header("Portal")]
    [SerializeField] private Transform portalSpawnPoint;
    /*[SerializeField] */
    private Transform returnPoint;
    [SerializeField] private GameObject portalPrefab;

    [Header("Teddy Bear")]
    [SerializeField] private GameObject teddyBearPrefab;

    private RoomType roomType;
    private float roomWidth;
    private float roomDepth;
    private bool entranceNorth, entranceSouth, entranceEast, entranceWest;

    private AgentPoolManager agentPool;
    private EnemySpawnConfig spawnConfig;
    private RoomManager linkedSpecialRoom;
    private GameObject returnPortalInstance;

    private List<GameObject> spawnedGates = new List<GameObject>();
    private List<EnemyBaseAI> spawnedEnemies = new List<EnemyBaseAI>();
    private bool isActivated = false;
    private int remainingEnemies = 0;

    /// <summary>
    /// Sets up all room data. Called by LevelGenerator after the room is placed in the world.
    /// </summary>
    public void InitializeRoom(
        RoomType type,
        float width,
        float depth,
        bool north, bool south, bool east, bool west,
        AgentPoolManager pool,
        EnemySpawnConfig config,
        RoomManager linkedSpecialRoom = null)
    {
        roomType = type;
        roomWidth = width;
        roomDepth = depth;
        entranceNorth = north;
        entranceSouth = south;
        entranceEast = east;
        entranceWest = west;
        agentPool = pool;
        spawnConfig = config;
        this.linkedSpecialRoom = linkedSpecialRoom;

        // Trigger sized to room but with a margin so it fires once inside, not at the door
        BoxCollider trigger = GetComponent<BoxCollider>();
        if (trigger != null)
            trigger.size = new Vector3(roomWidth - entranceClearance, 10f, roomDepth - entranceClearance);

        // Special rooms need a return point so the return portal knows where to teleport the player back
        if (roomType == RoomType.Special && returnPoint == null)
        {
            GameObject rp = new GameObject("ReturnPoint");
            rp.transform.SetParent(transform);
            rp.transform.localPosition = Vector3.zero;
            returnPoint = rp.transform;

            
        }

        GenerateGates();
        SetGatesOpen(true);

        // The teddy bear sits dormant until the final room is cleared
        if (teddyBearPrefab != null)
        {
            if (roomType == RoomType.Final)
                teddyBearPrefab.SetActive(false);
            else
                Destroy(teddyBearPrefab);
        }
        else
        {
            if (roomType == RoomType.Final)
                Debug.LogWarning("No teddy bear prefab in final room!");
        }
    }

    /// <summary>
    /// Spawns a gate at every entrance of this room.
    /// </summary>
    private void GenerateGates()
    {
        if (gatePrefab == null) return;

        float halfW = roomWidth / 2f;
        float halfD = roomDepth / 2f;

        if (entranceNorth) SpawnGate(new Vector3(0f, 0f, halfD), Quaternion.identity);
        if (entranceSouth) SpawnGate(new Vector3(0f, 0f, -halfD), Quaternion.identity);
        if (entranceEast) SpawnGate(new Vector3(halfW, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
        if (entranceWest) SpawnGate(new Vector3(-halfW, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
    }

    private void SpawnGate(Vector3 localPos, Quaternion localRot)
    {
        GameObject gate = Instantiate(
            gatePrefab,
            transform.TransformPoint(localPos),
            transform.rotation * localRot,
            transform);
        spawnedGates.Add(gate);
    }

    // Gates are hidden when open, visible when closed
    private void SetGatesOpen(bool open)
    {
        foreach (GameObject gate in spawnedGates)
            if (gate != null) gate.SetActive(!open);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;
        if (!other.CompareTag("Player")) return;

        isActivated = true;
        ActivateRoom();
    }

    /// <summary>
    /// Runs the activation logic for this room based on its type.
    /// </summary>
    private void ActivateRoom()
    {
        switch (roomType)
        {
            case RoomType.CombatRegular:
            case RoomType.Final:
                SetGatesOpen(false);
                StartCoroutine(SpawnEnemiesRoutine());
                break;

            case RoomType.Special:
                SetGatesOpen(false);
                SpawnEntryPortal();
                break;

            case RoomType.Initial:
            case RoomType.Loot:
                break;
        }
    }

    /// <summary>
    /// Spawns each enemy from the config one at a time, with a short delay between each.
    /// The tier is resolved at activation time using the current teddy bear count.
    /// </summary>
    private System.Collections.IEnumerator SpawnEnemiesRoutine()
    {
        if (agentPool == null || spawnConfig == null)
        {
            Debug.LogWarning($"{gameObject.name}: missing agentPool or spawnConfig.");
            yield break;
        }

        EnemySpawnTier tier = spawnConfig.GetTierForTeddyCount(LevelManager.Instance.TeddyBearCount);
        if (tier == null)
        {
            Debug.LogWarning($"{gameObject.name}: no matching spawn tier for TeddyBearCount {LevelManager.Instance.TeddyBearCount}.");
            yield break;
        }

        foreach (EnemySpawnEntry entry in tier.entries)
        {
            int countToSpawn = Random.Range(entry.minCount, entry.maxCount + 1);
            for (int i = 0; i < countToSpawn; i++)
            {
                Vector3 spawnPos = GetRandomSpawnPosition();
                EnemyBaseAI enemy = agentPool.RequestAgent(entry.type, spawnPos, Quaternion.identity);
                if (enemy == null) continue;

                // Since enemies are recycled via pooling, their NavMeshAgents are already active.
                // Disabling and re-enabling forces an instant position update
                // instead of snapping back to where the enemy previously died.
                var navAgent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (navAgent != null)
                {
                    navAgent.enabled = false;
                    enemy.transform.position = spawnPos;
                    navAgent.enabled = true;
                }

                Debug.Log($"{gameObject.name}: spawned {entry.type} at {spawnPos}. Room centre: {transform.position}");

                enemy.GetComponent<EnemyHealth>().OnDeath += OnEnemyDied;
                spawnedEnemies.Add(enemy);
                remainingEnemies++;

                yield return new WaitForSeconds(delayBetweenSpawns);
            }
        }

        Debug.Log($"{gameObject.name}: all enemies spawned. Total to kill: {remainingEnemies}");
    }

    /// <summary>
    /// Returns a random world-space position guaranteed to be inside the walkable floor area of this room.
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        // If it's a pre-made special room (width/depth are 0), use a default radius
        float rangeX = (roomWidth > 0f) ? (roomWidth / 2f - spawnMargin) : 8f;
        float rangeZ = (roomDepth > 0f) ? (roomDepth / 2f - spawnMargin) : 8f;

        rangeX = Mathf.Max(rangeX, 1f);
        rangeZ = Mathf.Max(rangeZ, 1f);

        // Try up to 15 times to find a valid spot on the floor and NavMesh
        for (int i = 0; i < 15; i++)
        {
            float x = Random.Range(-rangeX, rangeX);
            float z = Random.Range(-rangeZ, rangeZ);
            Vector3 samplePos = transform.position + new Vector3(x, 0f, z);

            // Raycast downward to detect the actual floor height
            if (Physics.Raycast(new Vector3(samplePos.x, transform.position.y + 20f, samplePos.z), Vector3.down, out RaycastHit hit, 40f))
                samplePos.y = hit.point.y;

            // Sample the NavMesh to confirm it's a walkable area
            if (UnityEngine.AI.NavMesh.SamplePosition(samplePos, out UnityEngine.AI.NavMeshHit navHit, 3f, UnityEngine.AI.NavMesh.AllAreas))
            {
                if (roomWidth > 0f)
                {
                    // For procedural rooms, confirm the point didn't snap to an outside corridor
                    Vector3 localPos = transform.InverseTransformPoint(navHit.position);
                    if (Mathf.Abs(localPos.x) < roomWidth / 2f && Mathf.Abs(localPos.z) < roomDepth / 2f)
                        return navHit.position;
                }
                else
                {
                    // For pre-made rooms, any valid NavMesh spot near the centre works
                    return navHit.position;
                }
            }
        }

        // Fallback if no valid NavMesh position is found in time
        float fallbackX = Random.Range(-rangeX, rangeX);
        float fallbackZ = Random.Range(-rangeZ, rangeZ);
        return transform.position + new Vector3(fallbackX, 0f, fallbackZ);
    }

    private void OnEnemyDied(EnemyHealth enemy)
    {
        enemy.OnDeath -= OnEnemyDied;
        spawnedEnemies.Remove(enemy.GetComponent<EnemyBaseAI>());
        remainingEnemies--;

        Debug.Log($"{gameObject.name}: enemy died. Remaining: {remainingEnemies}");

        if (remainingEnemies <= 0)
            OnAllEnemiesDefeated();
    }

    /// <summary>
    /// Called when the last enemy in the room dies. Unlocks the room based on its type.
    /// </summary>
    private void OnAllEnemiesDefeated()
    {
        Debug.Log($"{gameObject.name}: all enemies defeated — room type: {roomType}");

        switch (roomType)
        {
            case RoomType.CombatRegular:
                SetGatesOpen(true);
                break;

            case RoomType.Final:
                if (teddyBearPrefab != null)
                {
                    teddyBearPrefab.SetActive(true);
                    SetGatesOpen(true);
                }
                break;

            case RoomType.Special:
                ActivateReturnPortal();
                break;
        }
    }

    /// <summary>
    /// Spawns the entry portal in this room and sets up the full portal pair with the arena.
    /// The return portal is created inside the arena and handed to it directly.
    /// </summary>
    private void SpawnEntryPortal()
    {
        if (portalPrefab == null || linkedSpecialRoom == null) return;

        Vector3 spawnPos = portalSpawnPoint != null ? portalSpawnPoint.position : transform.position;
        GameObject entryPortalObj = Instantiate(portalPrefab, spawnPos, Quaternion.identity);

        Portal entryPortal = entryPortalObj.GetComponent<Portal>();
        if (entryPortal != null)
        {
            entryPortal.SetDestination(linkedSpecialRoom.returnPoint);
            entryPortal.OnPlayerTeleport += SetGatesOpen;
        }

        // Build the return portal inside the arena and pass it straight to the arena's RoomManager
        // so it holds the reference and can activate it once the fight is over.
        GameObject returnPortal = linkedSpecialRoom.BuildReturnPortal(portalPrefab, returnPoint);
        linkedSpecialRoom.ActivateAsLinkedSpecialRoom(agentPool, spawnConfig, returnPortal);
    }

    /// <summary>
    /// Creates the return portal inside this room, inactive. Ownership stays with the caller.
    /// </summary>
    public GameObject BuildReturnPortal(GameObject prefab, Transform destination)
    {
        Vector3 spawnPos = portalSpawnPoint != null ? portalSpawnPoint.position : transform.position;
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
        obj.SetActive(false);

        Portal portal = obj.GetComponent<Portal>();
        if (portal != null)
            portal.SetDestination(destination);

        return obj;
    }

    private void ActivateReturnPortal()
    {
        if (returnPortalInstance != null)
            returnPortalInstance.SetActive(true);
    }

    /// <summary>
    /// Turns this arena into an active combat room for the linked special encounter.
    /// Receives the return portal reference so it can activate it once the fight is over.
    /// </summary>
    public void ActivateAsLinkedSpecialRoom(AgentPoolManager pool, EnemySpawnConfig config, GameObject returnPortal)
    {
        agentPool = pool;
        spawnConfig = config;
        roomType = RoomType.Special;
        returnPortalInstance = returnPortal;
        StartCoroutine(SpawnEnemiesRoutine());
    }

    private void OnDrawGizmosSelected()
    {
        if (roomWidth == 0f || roomDepth == 0f) return;

        // Room bounds
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(roomWidth, 1f, roomDepth));

        // Gate positions
        float halfW = roomWidth / 2f;
        float halfD = roomDepth / 2f;
        Gizmos.color = Color.blue;
        Matrix4x4 prev = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        if (entranceNorth) Gizmos.DrawCube(new Vector3(0f, 0f, halfD), new Vector3(entranceClearance * 2f, 2f, 0.2f));
        if (entranceSouth) Gizmos.DrawCube(new Vector3(0f, 0f, -halfD), new Vector3(entranceClearance * 2f, 2f, 0.2f));
        if (entranceEast) Gizmos.DrawCube(new Vector3(halfW, 0f, 0f), new Vector3(0.2f, 2f, entranceClearance * 2f));
        if (entranceWest) Gizmos.DrawCube(new Vector3(-halfW, 0f, 0f), new Vector3(0.2f, 2f, entranceClearance * 2f));
        Gizmos.matrix = prev;
    }
}