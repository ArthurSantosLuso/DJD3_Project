using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AbilityCooldownUI : MonoBehaviour
{
    [SerializeField] private Image axeFill;
    [SerializeField] private Image shotgunFill;


    [SerializeField] private Animator axeIconAnimator;
    [SerializeField] private Animator axeFillAnimator;
    [SerializeField] private Animator shotgunIconAnimator;
    [SerializeField] private Animator shotgunFillAnimator;

    private AxeBoomerangAbility axeAbility;
    private ShotgunExplosiveAbility shotgunAbility;
    private PlayerInput playerInput;

    private void Start()
    {
        Character player = GameObject.FindWithTag("Player").GetComponent<Character>();
        axeAbility = player.GetComponentInChildren<AxeBoomerangAbility>();
        shotgunAbility = player.GetComponentInChildren<ShotgunExplosiveAbility>(true);
        playerInput = FindFirstObjectByType<global::PlayerInput>();
        playerInput.OnWeaponChange += UpdateActiveIcon;
        UpdateActiveIcon();
    }

    private void Update()
    {
        if (axeAbility != null)
        {
            axeFill.fillAmount = axeAbility.CooldownProgress;
        }    
           
        if (shotgunAbility != null)
        {
            shotgunFill.fillAmount = shotgunAbility.CooldownProgress;
        }

    }
    private void UpdateActiveIcon()
    {
        bool axeActive = axeIconAnimator.GetBool("IsActive?");
        axeIconAnimator.SetBool("IsActive?", !axeActive);
        axeFillAnimator.SetBool("IsActive?", !axeActive);
        shotgunIconAnimator.SetBool("IsActive?", axeActive);
        shotgunFillAnimator.SetBool("IsActive?", axeActive);
    }
}