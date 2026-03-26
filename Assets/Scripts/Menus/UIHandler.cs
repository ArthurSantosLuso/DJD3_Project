using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    [Tooltip("Slide bars for UI. Ex: Health and Stamina")]
    [SerializeField]
    private List<Slider> bars;

    public void SetBarValue(int barIdx, float currentValue, float maxValue)
    {
        bars[barIdx].value = currentValue / maxValue;
    }
}
