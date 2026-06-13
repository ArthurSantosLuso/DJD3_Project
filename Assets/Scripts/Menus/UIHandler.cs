using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [Tooltip("Slide bars for UI. Ex: Health and Stamina")]
    [SerializeField]
    private List<Image> bars;

    [Header("Glow Flash Overlay Settings")]
    [Tooltip("Element 0 = Health Glow Image, Element 1 = Stamina Glow Image")]
    [SerializeField] private List<Image> glowBars;
    [SerializeField] private float flashSpeed = 4f;
    [Range(0f, 1f)][SerializeField] private float lowThreshold = 0.25f; // flashes when below 25%

    private float healthPercentage = 1f;   
    private float staminaPercentage = 1f;  
    [Space]
    [Header("Screens")]
    [SerializeField]
    private GameObject pausePanel;
    [SerializeField]
    private GameObject interactionPanel;
    [SerializeField]
    private GameObject deathScreen;

    [Header("Weapons UI")]
    [SerializeField]
    private GameObject axeIcon;
    [SerializeField]
    private GameObject shotgunIcon;
    [SerializeField]
    private TextMeshProUGUI shotgunAmmoCount;

    public void SetBarValue(int barIdx, float currentValue, float maxValue)
    {

        if (maxValue <= 0) return;

        float percentage = currentValue / maxValue;

        if (barIdx == 0) // health bar
        {
            healthPercentage = percentage;
            // remap values
            float mappedValue = Mathf.Lerp(0.17f, 0.77f, percentage);
            bars[barIdx].fillAmount = mappedValue;
        }
        else if (barIdx == 1) // stamina bar
        {
            staminaPercentage = percentage;
            // remap values
            float mappedValue = Mathf.Lerp(0.13f, 0.63f, percentage);
            bars[barIdx].fillAmount = mappedValue;
        }
        else 
        {
            bars[barIdx].fillAmount = percentage;
        }
    }

    private void Update() //added for the glow tracking
    {
        HandleGlowFlashing();
    }

    private void HandleGlowFlashing()
    {
        if (glowBars == null) return;

        // smooth oscillation from 0 to 1 on the alpha
        float alphaOscillation = Mathf.PingPong(Time.time * flashSpeed, 1f);

        //health glow
        if (glowBars.Count > 0 && glowBars[0] != null)
        {
            if (healthPercentage <= lowThreshold)
                SetImageAlpha(glowBars[0], alphaOscillation); //pulsing
            else
                SetImageAlpha(glowBars[0], 0f); // invisible
        }

        // stamina glow
        if (glowBars.Count > 1 && glowBars[1] != null)
        {
            if (staminaPercentage <= lowThreshold)
                SetImageAlpha(glowBars[1], alphaOscillation); //pulsing
            else
                SetImageAlpha(glowBars[1], 0f); // invi
        }
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        Color updatedColor = image.color;
        updatedColor.a = alpha;
        image.color = updatedColor;
    }

    public void ShowDeathScreen()
    {
        deathScreen.SetActive(true);
    }

    public void ShowInteractionPanel()
    {
        interactionPanel.SetActive(true);
    }

    public void ChangeWeaponIcon()
    {
        axeIcon.SetActive(!axeIcon.activeSelf);
        shotgunIcon.SetActive(!shotgunIcon.activeSelf);
    }

    public void ToggleConfig()
    {
        pausePanel.SetActive(!pausePanel.activeSelf);
        if (pausePanel.activeSelf)
            GameManager.Instance.StopPlayerActions();
        else GameManager.Instance.ActivatePlayerActions();
    }

    public void UpdateAmmoText(int current, int max)
    {
        if (shotgunAmmoCount != null)
        {
            shotgunAmmoCount.text = $"{current}/{max}";
        }
    }
}
