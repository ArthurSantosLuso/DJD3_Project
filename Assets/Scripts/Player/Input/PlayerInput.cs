using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private Character character;
    public Vector2 moveInput;
    public bool sprintHeld;

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

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            sprintHeld = true;
        else if (context.phase == InputActionPhase.Canceled)
            sprintHeld = false;
    }
}
