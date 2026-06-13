using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StaminaVisualFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerStamina playerStamina;
    [SerializeField] private Volume globalVolume;

    [Header("Settings")]
    [Tooltip("The raw value of stamina where desaturation begins")]
    [SerializeField] private float lowStaminaValueThreshold = 0.45f;

    [Range(-100f, 0f)]
    [SerializeField] private float maxDesaturation = -80f;

    [Tooltip("How fast the screen turns gray when out of stamina.")]
    [SerializeField] private float drainSpeed = 25f;

    [Tooltip("How fast the color returns when regenerating stamina.")]
    [SerializeField] private float shiftSpeed = 5f;

    private ColorAdjustments colorAdjustments;
    private float targetSaturation = 0f;
    private bool playerFound = false;

    private void Start()
    {
        if (globalVolume == null)
            globalVolume = FindFirstObjectByType<Volume>();

        if (globalVolume != null && globalVolume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.saturation.overrideState = true;
        }

        TryFindPlayer();
    }

    private void Update()
    {
        if (colorAdjustments == null) return;

        if (!playerFound)
        {
            TryFindPlayer();
            colorAdjustments.saturation.value = Mathf.MoveTowards(colorAdjustments.saturation.value, 0f, shiftSpeed * Time.deltaTime);
            return;
        }

        if (playerStamina == null)
        {
            playerFound = false;
            return;
        }

        float currentStamina = playerStamina.CurrentValue;
        float currentSat = colorAdjustments.saturation.value;

        if (currentStamina <= lowStaminaValueThreshold)
        {
            targetSaturation = maxDesaturation;

            colorAdjustments.saturation.value = Mathf.MoveTowards(currentSat, targetSaturation, drainSpeed * Time.deltaTime);
        }
        else
        {
            targetSaturation = 0f;
            colorAdjustments.saturation.value = Mathf.MoveTowards(currentSat, targetSaturation, shiftSpeed * Time.deltaTime);
        }
    }

    private void TryFindPlayer()
    {
        playerStamina = FindFirstObjectByType<PlayerStamina>();
        if (playerStamina != null)
        {
            playerFound = true;
        }
    }
}