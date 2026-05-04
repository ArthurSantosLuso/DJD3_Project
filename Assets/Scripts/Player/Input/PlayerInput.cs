using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [SerializeField]
    private PlayerStamina stamina;
    [SerializeField]
    private PlayerHealth health;
    [SerializeField]
    private UIHandler uiHandler;

    private Character character;
    private PlayerMovement playerMovement;
    public Vector2 moveInput;
    public bool sprintHeld;

    public Action OnWeaponChange;

    private void Start()
    {
        PlayerStamina stamina = GetComponent<PlayerStamina>();
        character = GetComponent<Character>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    public void OnWeaponChangeInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            character.ChangeToNextWeapon();
            OnWeaponChange?.Invoke();
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
    public void OnDash(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            playerMovement.UseDash();
        }
    }

    public void OnInfiniteStamina(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            stamina.InfiniteStamina();
            health.InfiniteHealth();
        }
    }

    public void OnToggleConfigCanva(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            uiHandler.ToggleConfig();
        }
    }
}
