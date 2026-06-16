using UnityEngine;
using System.Collections.Generic;

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
            if (enemy.Prefab == null) continue;
            if (enemyLookup.ContainsKey(enemy.Type)) continue;

            enemyLookup.Add(enemy.Type, enemy);
        }
    }

    /// <summary>
    /// Instantiates and returns a new enemy of the given type, ready to use.
    /// </summary>
    public EnemyBaseAI CreateAgent(EnemyType type, Vector3 position, Quaternion rotation)
    {
        if (enemyLookup == null) BuildLookup();

        if (!enemyLookup.TryGetValue(type, out EnemyDefinition def))
        {
            Debug.LogWarning($"AgentFactory: no prefab registered for type {type}.");
            return null;
        }

        EnemyBaseAI agent = Instantiate(def.Prefab, position, rotation, enemiesContainer);
        return agent;
    }
}