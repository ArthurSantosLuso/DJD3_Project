using UnityEngine;

public class PlayerHealth : ValueBase, IDamageable, IHealable
{
    // Reduce player hp
    public void Damage(float damageValue)
    {
        base.ReduceValue(damageValue);
        OnValueChanged?.Invoke(0, currentValue, maxValue);
        VerifyLife();
    }

    // Check if player died
    private void VerifyLife()
    {
        if (currentValue <= 0)
        {
            KillPlayer();
        }
    }

    // Trigger player death 
    private void KillPlayer()
    {
        // Death logic
        GameManager.Instance.DisplayDeathScreen();
    }

    public void AddHP(float value)
    {
        base.IncreaseValue(value);
        OnValueChanged?.Invoke(0, currentValue, maxValue);
    }

    public bool CanDamage()
    {
        return currentValue > 0;
    }

    public void Heal(float amount)
    {
        AddHP(amount);
    }

    public bool NeedsHealing() => currentValue < maxValue;
}
