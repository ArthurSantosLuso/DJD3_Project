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
    [SerializeField] private int agentsPerTypeToSpawn = 100;

    private AgentFactory factory;
    private Dictionary<EnemyType, ObjectPool<EnemyBaseAI>> pools = new Dictionary<EnemyType, ObjectPool<EnemyBaseAI>>();

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
    /// Builds one Object Pool per enemy type registered in Agent Factory.
    /// </summary>
    private void InitializePools()
    {
        foreach (EnemyType type in factory.GetRegisteredTypes())
        {
            EnemyType capturedType = type;

            pools[capturedType] = new ObjectPool<EnemyBaseAI>(
                createFunc: () => factory.CreateAgent(capturedType),
                actionOnGet: (agent) => { },
                actionOnRelease: (agent) => agent.gameObject.SetActive(false),
                actionOnDestroy: (agent) => Destroy(agent.gameObject),
                collectionCheck: false,
                defaultCapacity: agentsPerTypeToSpawn,
                maxSize: agentsPerTypeToSpawn * 2);
        }
    }

    /// <summary>
    /// Instantiates all agents at startup and parks them as inactive in the pool.
    /// </summary>
    private void PreWarmPool()
    {
        foreach (ObjectPool<EnemyBaseAI> pool in pools.Values)
        {
            for (int i = 0; i < agentsPerTypeToSpawn; i++)
                MakeAgentInactive(pool.Get());
        }
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

        if (pools.TryGetValue(type, out ObjectPool<EnemyBaseAI> pool))
        {
            pool.Release(agent);
        }
        else
        {
            Destroy(agent.gameObject);
        }
    }

    /// <summary>
    /// Pulls an agent from the pool, places it at the given position, and marks it active.
    /// </summary>
    public EnemyBaseAI RequestAgent(EnemyType type, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(type, out ObjectPool<EnemyBaseAI> pool))
        {
            return null;
        }

        EnemyBaseAI agent = pool.Get();

        if (agent == null)
        {
            return null;
        }

        agent.transform.SetPositionAndRotation(position, rotation);
        agent.gameObject.SetActive(true);

        activeAgents.Add(agent);
        return agent;
    }
}