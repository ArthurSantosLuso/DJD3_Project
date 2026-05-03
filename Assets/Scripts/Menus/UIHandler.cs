using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [Tooltip("Slide bars for UI. Ex: Health and Stamina")]
    [SerializeField]
    private List<Image> bars;

    [Space]
    [SerializeField]
    private GameObject interactionPanel;
    [SerializeField]
    private GameObject deathScreen;

    public void SetBarValue(int barIdx, float currentValue, float maxValue)
    {
        // Get normal values 0.0 - 1-0
        float percentage = currentValue / maxValue;

        
        if (barIdx == 1) //  Stamina needs to be the second bar in the list
        {
            // change those value to 0.2 - 0.8 so it makes sense with the stamina bar sprite
            float mappedValue = Mathf.Lerp(0.2f, 0.8f, percentage);

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
}
