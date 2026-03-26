using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStamina : ValueBase
{
    private void Start()
    {
        AddStamina(maxValue);    
    }

    public void AddStamina(float value)
    {
        base.IncreaseValue(value);
        OnValueChanged?.Invoke(1, currentValue, maxValue);
    }

    public void a(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            base.ReduceValue(10);
            OnValueChanged?.Invoke(1, currentValue, maxValue);
        }
    }
}
