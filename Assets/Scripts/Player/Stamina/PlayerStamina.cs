using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStamina : ValueBase
{
    private enum StaminaUsage { InUse, NotUsing, Recovering}

    [Header("Stamina Settings")]
    [Tooltip("Stamina amout to restore per second")]
    [SerializeField] private float regenRate = 15f;

    [Tooltip("Time before start to regenerate stamina")]
    [SerializeField] private float regenDelay = .5f;

    private float lastUsageTime;
    private StaminaUsage staminaUsage;
    private float currentCostPerSecond;

    private void Start()
    {
        staminaUsage = StaminaUsage.NotUsing;
        /// The way the system is now, the stamina UI value is just shown when the UI is notified that something changed.
        /// So in order to initialize the UI, I call this method to use 0.01 stamina.
        UseStamina(0.01f);
    }

    private void Update()
    {
        HandleConsumption();
        HandleRegen();
    }

    private void HandleConsumption()
    {
        if (staminaUsage == StaminaUsage.NotUsing)
            return;

        base.ReduceValue(currentCostPerSecond * Time.deltaTime);
        OnValueChanged?.Invoke(1, currentValue, maxValue);

        lastUsageTime = Time.time;

        // If stamina is over, cease stamina consumption
        if (currentValue <= 0)
        {
            staminaUsage = StaminaUsage.Recovering;
        }
    }

    private void HandleRegen()
    {
        if (staminaUsage == StaminaUsage.InUse)
            return;

        //if (Time.time < lastUsageTime + regenDelay)
        //    return;

        if (currentValue < maxValue)
        {
            base.IncreaseValue(regenRate * Time.deltaTime);
            OnValueChanged?.Invoke(1, currentValue, maxValue);
        }
    }

    
    public void StartConsuming(int staminaToUsePerSecond)
    {
        currentCostPerSecond = staminaToUsePerSecond;
        staminaUsage = StaminaUsage.InUse;
    }

    public void StopConsuming()
    {
        currentCostPerSecond = 0;
        staminaUsage = StaminaUsage.NotUsing;
        lastUsageTime = Time.time;
    }

    // Instant use of stamina (Attack, Dodge, etc)
    public void UseStamina(float amount)
    {
        if (!HasStamina(amount))
            return;

        base.ReduceValue(amount);
        OnValueChanged?.Invoke(1, currentValue, maxValue);

        lastUsageTime = Time.time;
    }

    public bool HasStamina(float amount)
    {
        return currentValue >= amount;
    }
}
