using UnityEngine;

/*
This script handles:
Instantiating enemies by type and placing them in their designated containers.
Acts as the creation layer for the pool — AgentPoolManager asks for agents,
AgentFactory builds them.
*/

public class AgentFactory : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private EnemyBaseAI meleePrefab;
    [SerializeField] private EnemyBaseAI rangedPrefab;
    [SerializeField] private EnemyBaseAI buffedPrefab;

    [Header("Containers")]
    [SerializeField] private Transform meleeContainer;
    [SerializeField] private Transform rangedContainer;
    [SerializeField] private Transform buffedContainer;

    /// <summary>
    /// Instantiates and returns a new enemy of the given type.
    /// </summary>
    public EnemyBaseAI CreateAgent(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.Melee:
                return Instantiate(meleePrefab, meleeContainer);

            case EnemyType.Ranged:
                return Instantiate(rangedPrefab, rangedContainer);

            case EnemyType.Buffed:
                return Instantiate(buffedPrefab, buffedContainer);

            default:
                Debug.LogError($"AgentFactory: unknown enemy type: {type}");
                return null;
        }
    }
}