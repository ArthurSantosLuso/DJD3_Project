using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerHealth : ValueBase, IDamageable, IHealable
{
    [Header("Screen Shake Settings")]
    [SerializeField] private float shakeIntensityMultiplier = 0.5f;
    [SerializeField] private float shakeDuration = 0.2f;

    public override void ApplyUpgrades()
    {
        List<UpgradeData> upgrades = GameManager.Instance.PlayerUpgrades
            .Where(s => s.UpgradeType == UpgradeData.UpgradeTypes.Life)
            .ToList();

        foreach (UpgradeData upgrade in upgrades)
        {
            maxValue += upgrade.AmountToChange;
        }
    }

    // Reduce player hp
    public void Damage(float damageValue)
    {
        base.ReduceValue(damageValue);
        OnValueChanged?.Invoke(0, currentValue, maxValue);

        if (ScreenShake.Instance != null) //trigger camera shake
        {
            // Multiplies by the value of the damage
            // Big damage = big shake, Small damage = small shake
            ScreenShake.Instance.Shake(damageValue * shakeIntensityMultiplier, shakeDuration);
        }

        if (DamageFlash.Instance != null) //trigger image damage flash
        {
            // if health is above or equal to 50
            bool isLowHealth = (currentValue / (float)maxValue) <= 0.5f;

            // if health is bellow 50
            DamageFlash.Instance.CallFlash(0.2f, isLowHealth);
        }

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
        LevelManager.Instance.DisplayDeathScreen();
        GameManager.Instance.StopPlayerActions();
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
    public void DamageNoStagger(float damageValue)
    {
        Damage(damageValue);
    }

    public void Heal(float amount)
    {
        AddHP(amount);
    }

    public bool NeedsHealing() => currentValue < maxValue;

    public void InfiniteHealth()
    {
        IncreaseMaximumValue(100000);
        IncreaseValue(100000);
    }
}
