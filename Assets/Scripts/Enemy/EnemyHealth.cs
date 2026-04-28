using UnityEngine;

public class EnemyHealth : ValueBase, IDamageable
{
    // Reduce hp
    public void Damage(float damageValue)
    {
        base.ReduceValue(damageValue);
        //OnValueChanged?.Invoke(0, currentValue, maxValue);
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
