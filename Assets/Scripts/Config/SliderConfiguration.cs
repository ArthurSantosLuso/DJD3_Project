using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Security;

public class SliderConfiguration : MonoBehaviour
{
    public enum ConfigurationSliderType { Brightness, MasterVolume }

    [SerializeField] private ConfigurationSliderType sliderType;
    [SerializeField] private Slider slider;
    [SerializeField] private Volume globalVolume;

    private delegate void VolumeEvent(float volume);
    private static event VolumeEvent OnChangeVolume;

    private ColorAdjustments colorAdjustments;

    void Start()
    {
        if (sliderType == ConfigurationSliderType.Brightness)
        {
            if (globalVolume.profile.TryGet(out colorAdjustments))
            {
                slider.onValueChanged.AddListener(SetBrightness);
            }
            else
            {
                Debug.LogError("Color adjustments not found in Volume");
            }
        }
        else if (sliderType == ConfigurationSliderType.MasterVolume)
        {
            slider.onValueChanged.AddListener(SetMasterVolume);
        }
        
    }

    private void SetBrightness(float value)
    {
        colorAdjustments.postExposure.value = value;
    }

    private void SetMasterVolume(float value)
    {
        AudioListener.volume = value;
    }


}
