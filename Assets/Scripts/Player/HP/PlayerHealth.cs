using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class PlayerHealth : ValueBase
{
    private void Start()
    {
        UIManager.Instance.RegisterHealth(this);
    }

    // Reduce player hp
    public void TakeDamage(float hpToReduce)
    {
        base.ReduceValue(hpToReduce);
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
    }

    public void a(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            base.ReduceValue(10);
            VerifyLife();
        }
    }
}
