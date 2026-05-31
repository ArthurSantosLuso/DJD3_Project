using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

/*
This script handles:
Pre-warming a pool of enemies at startup and handing them out on request.
Enemies are never destroyed in runtime, they return to the pool and are reused.

Two workflows coexist here that should probably be separated:
  - The timed simulation mode (ShouldSpawn, Update) activates pre-warmed agents over time at fixed spawn points.
  - The room-based mode (RequestAgent) is called by RoomManager to place enemies in specific positions.

These two paths don't interfere, but they share agentsActivatedCount and activeAgents,
which can cause the counter to drift if both modes run at the same time.
Consider whether the simulation mode is still needed, or whether it can be removed entirely.
*/

[RequireComponent(typeof(AgentFactory))]
public class AgentPoolManager : MonoBehaviour
{
    [Header("Pool Settings")]
    [Tooltip("Total number of agents to pre-warm into the pool.")]
    [SerializeField] private int totalAgentsToSpawn = 100;

    [Tooltip("How long the timed simulation spawn takes to activate all agents.")]
    [SerializeField] private float spawnDuration = 5f;

    [Tooltip("Fraction of total agents that will be Melee (0.0 to 1.0). The rest will be Ranged.")]
    [Range(0f, 1f)]
    [SerializeField] private float meleeRatio = 0.7f;

    // NOTE: spawnPoints is only used by the timed simulation mode.
    // If that mode is removed, this list can go with it.
    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    private AgentFactory factory;
    private ObjectPool<EnemyBaseAI> meleePool;
    private ObjectPool<EnemyBaseAI> rangedPool;

    private Queue<EnemyBaseAI> inactiveAgents = new Queue<EnemyBaseAI>();
    private List<EnemyBaseAI> activeAgents = new List<EnemyBaseAI>();

    private float elapsedTime = 0f;
    private int agentsActivatedCount = 0;

    public bool ShouldSpawn { get; set; }

    private void Awake()
    {
        factory = GetComponent<AgentFactory>();
        ShouldSpawn = false;
        InitializePools();
    }

    private void Start()
    {
        PreWarmPool();
    }

    /// <summary>
    /// Instantiates all agents at startup and parks them in the inactive queue.
    /// </summary>
    private void PreWarmPool()
    {
        int meleeCount = Mathf.RoundToInt(totalAgentsToSpawn * meleeRatio);
        int rangedCount = totalAgentsToSpawn - meleeCount;

        for (int i = 0; i < meleeCount; i++)
            MakeAgentInactive(meleePool.Get());

        for (int i = 0; i < rangedCount; i++)
            MakeAgentInactive(rangedPool.Get());
    }

    private void MakeAgentInactive(EnemyBaseAI agent)
    {
        agent.Initialize();
        agent.gameObject.SetActive(false);
        inactiveAgents.Enqueue(agent);
    }

    // Timed simulation mode: gradually activates pre-warmed agents at the configured spawn points.
    // Not used by the room system — see the note at the top of this file.
    private void Update()
    {
        if (!ShouldSpawn) return;
        if (agentsActivatedCount >= totalAgentsToSpawn) return;

        elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsedTime / spawnDuration);
        int targetCount = Mathf.FloorToInt(progress * totalAgentsToSpawn);

        while (agentsActivatedCount < targetCount && inactiveAgents.Count > 0)
            ActivateNextAgent();
    }

    private void ActivateNextAgent()
    {
        EnemyBaseAI agent = inactiveAgents.Dequeue();

        if (spawnPoints.Count > 0)
        {
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
            agent.transform.SetPositionAndRotation(point.position, point.rotation);
        }

        agent.gameObject.SetActive(true);
        activeAgents.Add(agent);
        agentsActivatedCount++;
    }

    /// <summary>
    /// Returns an agent to its pool and removes it from the active list.
    /// </summary>
    public void ReturnAgentToPool(EnemyBaseAI agent, EnemyType type)
    {
        activeAgents.Remove(agent);

        if (type == EnemyType.Melee)
            meleePool.Release(agent);
        else
            rangedPool.Release(agent);
    }

    /// <summary>
    /// Pulls an agent from the pool, places it at the given position, and marks it active.
    /// </summary>
    public EnemyBaseAI RequestAgent(EnemyType type, Vector3 position, Quaternion rotation)
    {
        EnemyBaseAI agent = type == EnemyType.Melee ? meleePool.Get() : rangedPool.Get();

        if (agent == null)
        {
            Debug.LogWarning($"AgentPoolManager: no available agent of type {type}.");
            return null;
        }

        agent.transform.SetPositionAndRotation(position, rotation);
        agent.gameObject.SetActive(true);

        activeAgents.Add(agent);
        agentsActivatedCount++;
        return agent;
    }

    private void InitializePools()
    {
        meleePool = new ObjectPool<EnemyBaseAI>(
            createFunc: () => factory.CreateAgent(EnemyType.Melee),
            actionOnGet: (agent) => { },
            actionOnRelease: (agent) => agent.gameObject.SetActive(false),
            actionOnDestroy: (agent) => Destroy(agent.gameObject),
            collectionCheck: false,
            defaultCapacity: totalAgentsToSpawn,
            maxSize: totalAgentsToSpawn * 2);

        rangedPool = new ObjectPool<EnemyBaseAI>(
            createFunc: () => factory.CreateAgent(EnemyType.Ranged),
            actionOnGet: (agent) => { },
            actionOnRelease: (agent) => agent.gameObject.SetActive(false),
            actionOnDestroy: (agent) => Destroy(agent.gameObject),
            collectionCheck: false,
            defaultCapacity: totalAgentsToSpawn,
            maxSize: totalAgentsToSpawn * 2);
    }
}