using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    //[SerializeField]
    //private PlayerStamina stamina;
    //[SerializeField]
    //private PlayerHealth health;
    [SerializeField]
    private UIHandler uiHandler;
    [SerializeField] 
    private float shakeIntensity = 0.5f;
    [SerializeField] 
    private float shakeDuration = 0.2f;
    [SerializeField]
    private float shakeDelay = 0.2f;

    private Character character;
    private PlayerMovement playerMovement;
    private Interactor interactor;
    
    
    public Vector2 MoveInput { get; private set; }
    public bool SprintHeld { get; private set; }


    public Action OnWeaponChange;

    private void Start()
    {
        uiHandler = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIHandler>();
        // stamina = GetComponent<PlayerStamina>();
        character = GetComponent<Character>();
        playerMovement = GetComponent<PlayerMovement>();
        interactor = GetComponent<Interactor>();
    }
    public void OnWeaponChangeInput(InputAction.CallbackContext context)
    {
        if (!VerifyIfPlayerCanAct()) return;

        if (context.phase == InputActionPhase.Performed)
        {
            bool changed = character.ChangeToNextWeapon();
            if (changed) OnWeaponChange?.Invoke();
        }
    }

    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (!VerifyIfPlayerCanAct()) return;

        if (context.phase == InputActionPhase.Performed)
        {
            character.UseAbility(0);
            if (ScreenShake.Instance != null) //trigger camera shake
            {
                //not sure if its okay to put this here :C 
                ScreenShake.Instance.ShakeWithDelay(shakeIntensity, shakeDuration, shakeDelay);
            }
        }
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (!VerifyIfPlayerCanAct()) return;

        if (context.phase == InputActionPhase.Performed)
        {
            character.UseAbility(1);

            if (ScreenShake.Instance != null) //trigger camera shake
            {
                //not sure if its okay to put this here :C 
                ScreenShake.Instance.ShakeWithDelay(shakeIntensity * 2, shakeDuration, shakeDelay);
            }
        }
    }

    public void OnSpecialAbility(InputAction.CallbackContext context)
    {
        if (!VerifyIfPlayerCanAct()) return;

        if (context.phase == InputActionPhase.Performed)
        {
            character.UseAbility(2);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!VerifyIfPlayerCanAct()) return;

        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (!VerifyIfPlayerCanAct()) return;

        if (context.phase == InputActionPhase.Performed)
            SprintHeld = true;
        else if (context.phase == InputActionPhase.Canceled)
            SprintHeld = false;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!VerifyIfPlayerCanAct()) return;

        if (context.phase == InputActionPhase.Performed)
        {
            playerMovement.UseDash();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            if (DialogueManager.Instance.IsDialoguePlaying)
            {
                DialogueManager.Instance.RequestNextLine();
                return;
            }

            interactor.ExecuteInteraction();
        }
    }

    //// >>>>>>>>>>>>>>> Temporary <<<<<<<<<<<<<<<<<<
    //public void OnInfiniteStamina(InputAction.CallbackContext context)
    //{
    //    if (context.phase == InputActionPhase.Performed)
    //    {
    //        stamina.InfiniteStamina();
    //        health.InfiniteHealth();
    //    }
    //}

    public void OnToggleConfigCanva(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            uiHandler.ToggleConfig();
        }
    }


    private bool VerifyIfPlayerCanAct() => GameManager.Instance.CanPlayerAct;
}