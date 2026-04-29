using System.Collections.Generic;
using System.Security;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WormSpawn : Ability
{
    [SerializeField] private float timerBetweenSpawn = 2f;
    [SerializeField] private int spawnPerTick = 2;
    [SerializeField] private GameObject worm;
    [SerializeField] private Collider floor;
    [SerializeField] private GameObject wormWarning;

    private float timer;
    private Bounds bounds;
    private List<Vector3> nextWormsPos = new List<Vector3>();

    private void Start()
    {
        bounds = floor.bounds;
        DefineNextPosition();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= timerBetweenSpawn)
        {
            Perform();
            timer = 0;
        }
    }

    public override float AbilityRange => throw new System.NotImplementedException();

    public override void Perform()
    {
        for (int i = 0; i < spawnPerTick; i++)
        {
            Vector3 spawnPos = new(nextWormsPos[i].x, floor.gameObject.transform.position.y, nextWormsPos[i].z);
            Instantiate(worm, spawnPos, Quaternion.identity);
        }
        DefineNextPosition();

    }

    private void DefineNextPosition()
    {
        nextWormsPos.Clear();
        for (int i = 0; i < spawnPerTick; i++)
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float z = Random.Range(bounds.min.z, bounds.max.z);
            nextWormsPos.Add(new Vector3(x, 0, z));
        }

        for (int i = 0; i < spawnPerTick; i++)
        {
            Vector3 spawnPos = new(nextWormsPos[i].x, floor.gameObject.transform.position.y + 0.5f, nextWormsPos[i].z);
            Instantiate(wormWarning, spawnPos, Quaternion.Euler(90, 0, 0));
        }
    }



    protected override bool CanAttack()
    {
        throw new System.NotImplementedException();
    }

    protected override void IdentifyEnemyInRange(List<IDamageable> entitiesHit)
    {
        throw new System.NotImplementedException();
    }
}
