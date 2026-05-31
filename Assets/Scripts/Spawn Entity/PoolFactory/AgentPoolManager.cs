using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

/*
This script handles:
Pre-warming a pool of enemies at startup and handing them out on request.
Enemies are never destroyed mid-game they return to the pool and are reused.
*/

[RequireComponent(typeof(AgentFactory))]
public class AgentPoolManager : MonoBehaviour
{
    [Header("Pool Settings")]
    [Tooltip("Total number of agents to pre-warm into the pool.")]
    [SerializeField] private int totalAgentsToSpawn = 100;

    [Tooltip("Fraction of total agents that will be Melee (0.0 to 1.0). The rest will be Ranged.")]
    [Range(0f, 1f)]
    [SerializeField] private float meleeRatio = 0.7f;

    private AgentFactory factory;
    private ObjectPool<EnemyBaseAI> meleePool;
    private ObjectPool<EnemyBaseAI> rangedPool;

    private List<EnemyBaseAI> activeAgents = new List<EnemyBaseAI>();

    private void Awake()
    {
        factory = GetComponent<AgentFactory>();
        InitializePools();
    }

    private void Start()
    {
        PreWarmPool();
    }

    /// <summary>
    /// Instantiates all agents at startup and parks them as inactive in the pool.
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