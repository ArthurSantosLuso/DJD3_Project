using UnityEditor;
using UnityEngine;

public class AgentFactory : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField]
    private EnemyBaseAI meleePrefab;
    [SerializeField]
    private EnemyBaseAI rangedAgent;

    [Header("Containers")]
    [SerializeField]
    private Transform meleeContainer;
    [SerializeField]
    private Transform rangedContainer;
    
    /// <summary>
    /// Creates and returns a specific type of agent
    /// </summary>
    public EnemyBaseAI CreateAgent(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Melee:
                return Instantiate(meleePrefab, meleeContainer);

            case EnemyType.Ranged:
                return Instantiate(rangedAgent, rangedContainer);

            default:
                Debug.LogError($"Error trying to instantiate enemy type: {type}");
                return null;
        }
    }
}
