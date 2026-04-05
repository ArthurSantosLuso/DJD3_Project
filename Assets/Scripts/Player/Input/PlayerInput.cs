using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private Character character;

    private void Start()
    {
        character = GetComponent<Character>();
    }

    public void OnWeaponChange(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            character.ChangeToNextWeapon();
        }
    }

    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            character.UseAbility(0);
        }
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            character.UseAbility(1);
        }
    }

    public void OnSpecialAbility(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            character.UseAbility(2);
        }
    }
}
