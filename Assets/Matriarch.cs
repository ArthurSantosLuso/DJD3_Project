using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Matriarch : MonoBehaviour, IDamageable
{
    [Header("Slam Attack")]
    [SerializeField] private GameObject tentaclePrefab;
    [SerializeField] private GameObject tentacleShield;
    [SerializeField] private float timeBetweenAttacks;
    [SerializeField] private float timeBetweenTentaclesInstantiation;

    [Header("Enemy Spawning")]
    [SerializeField] private AgentPoolManager agentPoolManager;
    [SerializeField] private Transform[] enemySpawnPoints;
    [SerializeField] private List<EnemyType> spawnableEnemyTypes;
    [SerializeField] private Vector2Int enemiesPerWaveRange = new Vector2Int(2, 4);

    [Header("Health")]
    [SerializeField] private float health;

    private int slamAttacksToHappen;
    private int tentaclesRemaining;
    private float timer;
    private Transform player;
    private bool hasAttackedBefore = false;
    private bool isShieldUp = false;

    private TentacleSlam[] shieldTentacles;
    private int shieldTentaclesRemaining;
    private bool enemiesSpawned = false;
    private List<EnemyBaseAI> spawnedEnemies = new List<EnemyBaseAI>();

    private void Update()
    {
        if (player == null)
        {
            player = LevelManager.Instance.Player.transform;
        }
        else
        {
            if (slamAttacksToHappen == 0)
            {
                if (!hasAttackedBefore)
                {
                    timer += Time.deltaTime;
                    if (timer > timeBetweenAttacks)
                    {
                        timer = 0;
                        // Choose between 1 and 3 attacks
                        slamAttacksToHappen = Random.Range(1, 4);
                        TriggerSlamAttack();
                    }
                }
                else
                {
                    if (!isShieldUp)
                    {
                        if (tentaclesRemaining == 0)
                        {
                            RaiseShield();
                        }
                    }
                    else
                    {
                        if (!enemiesSpawned)
                        {
                            // Wait until every shield tentacle has finished slamming down
                            if (shieldTentaclesRemaining == 0)
                            {
                                SpawnEnemyWave();
                                enemiesSpawned = true;
                            }
                        }
                        else
                        {
                            CheckEnemiesDefeated();
                        }
                    }
                }
            }
            else
            {
                timer += Time.deltaTime;
                if (timer > timeBetweenTentaclesInstantiation)
                {
                    timer = 0;
                    TriggerSlamAttack();
                }
            }
        }

        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
    }

    private void TriggerSlamAttack()
    {
        slamAttacksToHappen--;
        if (slamAttacksToHappen == 0) hasAttackedBefore = true;

        Vector3 offset = Random.insideUnitSphere * 5f;
        offset.y = 0f;

        Vector3 spawnPos = player.position + offset;

        GameObject tentacle = Instantiate(tentaclePrefab, spawnPos, Quaternion.identity);

        TentacleSlam slam = tentacle.GetComponent<TentacleSlam>();
        if (slam != null)
        {
            tentaclesRemaining++;
            slam.OnSlamComplete += () => tentaclesRemaining--;
        }
    }

    /// <summary>
    /// Activates the shield and waits for all of its TentacleSlam pieces
    /// to finish their slam animation before enemies are allowed to spawn.
    /// </summary>
    private void RaiseShield()
    {
        isShieldUp = true;
        enemiesSpawned = false;
        tentacleShield.SetActive(true);

        shieldTentacles = tentacleShield.GetComponentsInChildren<TentacleSlam>(true);
        shieldTentaclesRemaining = shieldTentacles.Length;

        foreach (TentacleSlam slam in shieldTentacles)
        {
            slam.OnSlamComplete += HandleShieldTentacleSlamComplete;
        }
    }

    private void HandleShieldTentacleSlamComplete()
    {
        shieldTentaclesRemaining--;
    }

    /// <summary>
    /// Picks a random number/type of enemies and spawns them at random
    /// pre-placed spawn points in the boss arena.
    /// </summary>
    private void SpawnEnemyWave()
    {
        spawnedEnemies.Clear();

        if (agentPoolManager == null)
        {
            Debug.LogWarning("Matriarch: no AgentPoolManager assigned, skipping enemy spawn.");
            return;
        }

        if (enemySpawnPoints == null || enemySpawnPoints.Length == 0)
        {
            Debug.LogWarning("Matriarch: no enemy spawn points assigned, skipping enemy spawn.");
            return;
        }

        if (spawnableEnemyTypes == null || spawnableEnemyTypes.Count == 0)
        {
            Debug.LogWarning("Matriarch: no spawnable enemy types assigned, skipping enemy spawn.");
            return;
        }

        int enemyCount = Random.Range(enemiesPerWaveRange.x, enemiesPerWaveRange.y + 1);

        for (int i = 0; i < enemyCount; i++)
        {
            EnemyType type = spawnableEnemyTypes[Random.Range(0, spawnableEnemyTypes.Count)];
            Transform spawnPoint = enemySpawnPoints[Random.Range(0, enemySpawnPoints.Length)];

            EnemyBaseAI enemy = agentPoolManager.RequestAgent(type, spawnPoint.position, spawnPoint.rotation);
            if (enemy != null)
            {
                spawnedEnemies.Add(enemy);
            }
        }
    }

    /// <summary>
    /// Checks if every enemy from the wave is gone (returned to the pool /
    /// deactivated). If so, lowers the shield and resumes the slam-attack phase.
    /// </summary>
    private void CheckEnemiesDefeated()
    {
        foreach (EnemyBaseAI enemy in spawnedEnemies)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                return; // at least one is still alive
            }
        }

        LowerShield();
    }

    /// <summary>
    /// Deactivates the shield, unsubscribes from its tentacles, and resets
    /// state so Matriarch goes back into the slam-attack phase.
    /// </summary>
    private void LowerShield()
    {
        tentacleShield.SetActive(false);

        if (shieldTentacles != null)
        {
            foreach (TentacleSlam slam in shieldTentacles)
            {
                if (slam != null)
                {
                    slam.OnSlamComplete -= HandleShieldTentacleSlamComplete;
                }
            }
        }

        isShieldUp = false;
        enemiesSpawned = false;
        hasAttackedBefore = false;
        spawnedEnemies.Clear();
        timer = 0f;
    }

    public bool HasBlood()
    {
        return true;
    }

    public bool CanDamage()
    {
        return !isShieldUp;
    }

    public void Damage(float damageValue)
    {
        health -= damageValue;
    }

    public void DamageNoStagger(float damageValue)
    {
        throw new System.NotImplementedException();
    }
}