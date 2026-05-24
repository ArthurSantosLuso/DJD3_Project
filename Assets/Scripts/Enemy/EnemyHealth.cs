using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class EnemyHealth : ValueBase, IDamageable
{
    private HitFlash    hitFlash;
    private EnemyBaseAI enemyAI;

    [SerializeField] private List<GameObject> drops;
    [SerializeField, Range(0.0f, 1f)] private float dropRate;


    private void Start()
    {
        // Get the flash script component
        hitFlash = GetComponent<HitFlash>();
        enemyAI = GetComponent<EnemyBaseAI>();
    }

    public void Damage(float damageValue)
    {
        base.ReduceValue(damageValue); 
        //OnValueChanged?.Invoke(0, currentValue, maxValue);
        // Trigger the flash effect
        hitFlash?.Flash();

        enemyAI.TriggerStagger();

        VerifyLife();
    }



    // Check if died
    private void VerifyLife()
    {
        if (currentValue <= 0)
        {
            Kill();
        }
    }

    private void Kill()
    {
        if (dropRate >= Random.value)
        {
            float drop = Random.value;

            if (drop > 0.5f)
                Instantiate(drops[1], transform.position, Quaternion.identity);
            else Instantiate(drops[0], transform.position, Quaternion.identity);
        }

        GameManager.Instance.EnemyDeadCount++;
        Destroy(gameObject);
    }

    public bool CanDamage()
    {
        return currentValue > 0;
    }
}
