using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Security;

public class BrightnessSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Volume volume;

    private ColorAdjustments colorAdjustments;

    void Start()
    {
        if (volume.profile.TryGet(out colorAdjustments))
        {
            slider.onValueChanged.AddListener(SetBrightness);
        }
        else
        {
            Debug.LogError("Color adjustments not found in Volume");
        }
    }

    void SetBrightness(float value)
    {
        colorAdjustments.postExposure.value = value;
    }
}
