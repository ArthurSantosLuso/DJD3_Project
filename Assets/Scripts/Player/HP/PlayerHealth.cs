using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class PlayerHealth : ValueBase
{
    private void Start()
    {
        AddHP(maxValue);
    }

    // Reduce player hp
    public void TakeDamage(float hpToReduce)
    {
        base.ReduceValue(hpToReduce);
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
    }

    public void AddHP(float value)
    {
        base.IncreaseValue(value);
        OnValueChanged?.Invoke(0, currentValue, maxValue);
    }

    public void a(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            base.ReduceValue(10);
            OnValueChanged?.Invoke(0, currentValue, maxValue);
            VerifyLife();
        }
    }
}
