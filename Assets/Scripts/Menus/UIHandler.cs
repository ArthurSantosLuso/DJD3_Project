using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [Tooltip("Slide bars for UI. Ex: Health and Stamina")]
    [SerializeField]
    private List<Image> bars;

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
            // remap values
            float mappedValue = Mathf.Lerp(0.17f, 0.77f, percentage);
            bars[barIdx].fillAmount = mappedValue;
        }
        else if (barIdx == 1) // stamina bar
        {
            // remap values
            float mappedValue = Mathf.Lerp(0.13f, 0.63f, percentage);
            bars[barIdx].fillAmount = mappedValue;
        }
        else 
        {
            bars[barIdx].fillAmount = percentage;
        }
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
