using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Pool;

[RequireComponent(typeof(AgentFactory))]
public class AgentPoolManager : MonoBehaviour
{
    [Header("Generation Settings")]
    [Tooltip("Total number of agents to simulate")]
    [SerializeField] private int totalAgentsToSpawn = 100;
    [Tooltip("Time that will take to spawn every entity")]
    [SerializeField] private float spawnDuration = 5f;
    [Tooltip("Percentage of the total agents that will be Crew (0.0 to 1.0). The rest will be Robots")]
    [Range(0f, 1f)]
    [SerializeField] private float crewRatio = 0.7f;

    [Header("Spawn Locations")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();

    private AgentFactory factory;
    private ObjectPool<EnemyBaseAI> meleePool;
    private ObjectPool<EnemyBaseAI> rangedPool;

    private Queue<EnemyBaseAI> inactiveAgents = new Queue<EnemyBaseAI>();
    private List<EnemyBaseAI> activeAgents = new List<EnemyBaseAI>();

    private float elapsedTime = 0f;
    private int agentsActivatedCount = 0;


    public bool ShouldSpawn {  get; set; }

    private void Awake()
    {
        factory = GetComponent<AgentFactory>();
        ShouldSpawn = false;
        InitializePools();
    }

    private void Start()
    {
        PreWarmSimulation();
    }

    /// <summary>
    /// Instantialize all agents at the start of the simulation
    /// </summary>
    private void PreWarmSimulation()
    {
        int crewCount = Mathf.RoundToInt(totalAgentsToSpawn * crewRatio);
        int robotCount = totalAgentsToSpawn - crewCount;

        for (int i = 0; i < crewCount; i++)
        {
            MakeAgentInactive(meleePool.Get());
        }

        for (int i = 0; i < robotCount; i++)
        {
            MakeAgentInactive(rangedPool.Get());
        }
    }

    private void MakeAgentInactive(EnemyBaseAI agent)
    {
        agent.Initialize();
        agent.gameObject.SetActive(false);
        inactiveAgents.Enqueue(agent);
    }

    private void Update()
    {
        if (ShouldSpawn)
        {
            if (agentsActivatedCount < totalAgentsToSpawn)
            {
                elapsedTime += Time.deltaTime;

                float progress = Mathf.Clamp01(elapsedTime / spawnDuration);
                int targetCount = Mathf.FloorToInt(progress * totalAgentsToSpawn);

                while (agentsActivatedCount < targetCount && inactiveAgents.Count > 0)
                {
                    ActivateNextAgent();
                }
            }
        }

    }

    private void ActivateNextAgent()
    {
        // Store the next inactive agent
        EnemyBaseAI agent = inactiveAgents.Dequeue();

        // position the agent
        if (spawnPoints.Count > 0)
        {
            Transform point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)];
            agent.transform.SetPositionAndRotation(point.position, point.rotation);
        }

        agent.gameObject.SetActive(true);
        activeAgents.Add(agent);
        agentsActivatedCount++;
    }

    /// <summary>
    /// Reclaims an agent into the pool
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
    /// Configures the behavior of the pool
    /// </summary>
    private void InitializePools()
    {
        meleePool = new ObjectPool<EnemyBaseAI>(
            createFunc: () => factory.CreateAgent(EnemyType.Melee),
            actionOnGet: (agent) => agent.gameObject.SetActive(true),
            actionOnRelease: (agent) => agent.gameObject.SetActive(false),
            actionOnDestroy: (agent) => Destroy(agent.gameObject),
            collectionCheck: false,
            defaultCapacity: totalAgentsToSpawn,
            maxSize: totalAgentsToSpawn * 2);

        rangedPool = new ObjectPool<EnemyBaseAI>(
                createFunc: () => factory.CreateAgent(EnemyType.Ranged),
                actionOnGet: (agent) => agent.gameObject.SetActive(true),
                actionOnRelease: (agent) => agent.gameObject.SetActive(false),
                actionOnDestroy: (agent) => Destroy(agent.gameObject),
                collectionCheck: false,
                defaultCapacity: totalAgentsToSpawn,
                maxSize: totalAgentsToSpawn * 2);
    }

}
