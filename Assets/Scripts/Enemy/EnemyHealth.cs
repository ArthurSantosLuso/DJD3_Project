using UnityEngine;

public class EnemyHealth : ValueBase, IDamageable
{
    private HitFlash hitFlash;

    private void Start()
    {
        // Get the flash script component
        hitFlash = GetComponent<HitFlash>();
    }

    public void Damage(float damageValue)
    {
        base.ReduceValue(damageValue); // Reduce hp
        //OnValueChanged?.Invoke(0, currentValue, maxValue);
        // Trigger the flash effect
        if (hitFlash != null)
        {
            hitFlash.Flash();
        }

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
        Destroy(gameObject);
    }

    public bool CanDamage()
    {
        return currentValue > 0;
    }
}
