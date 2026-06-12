using UnityEngine;
using System.Collections.Generic;

/*
This script handles:
Instantiating enemies by type and placing them in their designated containers.
Acts as the creation layer for the pool — AgentPoolManager asks for agents,
AgentFactory builds them.
*/

public class AgentFactory : MonoBehaviour
{
    [Header("Enemy Definitions")]
    [SerializeField] private List<EnemyDefinition> enemies = new List<EnemyDefinition>();

    [Header("Container")]
    [SerializeField] private Transform enemiesContainer;

    private Dictionary<EnemyType, EnemyDefinition> enemyLookup;

    private void Awake()
    {
        BuildLookup();
    }

    private void BuildLookup()
    {
        enemyLookup = new Dictionary<EnemyType, EnemyDefinition>();

        foreach (EnemyDefinition enemy in enemies)
        {
            // Enemy does not have a prefab
            if (enemy.Prefab == null)
            {
                continue;
            }

            // Avoid duplications
            if (enemyLookup.ContainsKey(enemy.Type))
            {
                continue;
            }

            enemyLookup.Add(enemy.Type, enemy);
        }
    }

    /// <summary>
    /// Instantiates and returns a new enemy of the given type.
    /// </summary>
    public EnemyBaseAI CreateAgent(EnemyType type)
    {
        if (enemyLookup == null) BuildLookup();

        if (enemyLookup.TryGetValue(type, out EnemyDefinition def))
            return Instantiate(def.Prefab, enemiesContainer);

        return null;
    }

    /// <summary>
    /// All enemy types currently registered with this factory.
    /// </summary>
    public IEnumerable<EnemyType> GetRegisteredTypes()
    {
        if (enemyLookup == null) BuildLookup();

        return enemyLookup.Keys;
    }

    /// <summary>
    /// Instantiates and returns a new enemy of the given type.
    /// </summary>
    //public EnemyBaseAI CreateAgent(EnemyType type)
    //{
    //    switch (type)
    //    {
    //        case EnemyType.Melee:
    //            return Instantiate(meleePrefab, meleeContainer);

    //        case EnemyType.Ranged:
    //            return Instantiate(rangedPrefab, rangedContainer);

    //        case EnemyType.Buffed:
    //            return Instantiate(buffedPrefab, buffedContainer);

    //        default:
    //            Debug.LogError($"AgentFactory: unknown enemy type: {type}");
    //            return null;
    //    }
    //}
}