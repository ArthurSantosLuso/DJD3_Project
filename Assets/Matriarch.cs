using System.Collections;
using UnityEngine;

public class Matriarch : MonoBehaviour
{
    [SerializeField] private GameObject tentaclePrefab;
    [SerializeField] private float timeBetweenAttacks;
    [SerializeField] private float timeBetweenTentaclesInstantiation;

    private int         slamAttacksToHappen;
    private float       timer;
    private Transform   player;
    private bool        hasAttackedBefore = false;

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
                    // hasAttackedBefore = false;
                    // Spawn enemies imortality logic.
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
    }

    private void TriggerSlamAttack()
    {
        slamAttacksToHappen--;

        Vector3 offset = Random.insideUnitSphere * 5f;
        offset.y = 0f;

        Vector3 spawnPos = player.position + offset;

        Instantiate(tentaclePrefab, spawnPos, Quaternion.identity);
        //Debug.Log("Slam Attack!");
    }

}
