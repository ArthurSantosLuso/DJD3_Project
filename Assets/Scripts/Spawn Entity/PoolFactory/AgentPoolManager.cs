using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AgentFactory))]
public class AgentPoolManager : MonoBehaviour
{
    public static AgentPoolManager Instance { get; private set; }

    private AgentFactory factory;
    private List<EnemyBaseAI> activeAgents = new List<EnemyBaseAI>();

    private void Awake()
    {
        Instance = this;
        factory = GetComponent<AgentFactory>();
    }

    /// <summary>
    /// Instantiates an enemy of the given type at the given position/rotation,
    /// initializes its FSM, and tracks it as active.
    /// </summary>
    public EnemyBaseAI RequestAgent(EnemyType type, Vector3 position, Quaternion rotation)
    {
        EnemyBaseAI agent = factory.CreateAgent(type, position, rotation);

        if (agent == null) return null;

        agent.Initialize();
        agent.gameObject.SetActive(true);

        activeAgents.Add(agent);

        EnemyHealth health = agent.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDeath += HandleAgentDeath;
        }

        return agent;
    }

    /// <summary>
    /// Removes the agent from the active list and destroys it.
    /// </summary>
    public void ReturnAgentToPool(EnemyBaseAI agent, EnemyType type)
    {
        activeAgents.Remove(agent);

        if (agent != null)
        {
            Destroy(agent.gameObject);
        }
    }

    /// <summary>
    /// Called when a spawned agent's death sequence finishes.
    /// Destroys it instead of returning it to a pool.
    /// </summary>
    private void HandleAgentDeath(EnemyHealth health)
    {
        health.OnDeath -= HandleAgentDeath;

        EnemyBaseAI agent = health.GetComponent<EnemyBaseAI>();
        ReturnAgentToPool(agent, default);
    }
}