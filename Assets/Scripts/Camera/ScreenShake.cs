using System.Threading;
using Unity.Cinemachine;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public static ScreenShake Instance { get; private set; }
    private CinemachineCamera cinemachineCamera;
    private CinemachineBasicMultiChannelPerlin noiseComponent;
    private float shakeTimer;

    private void Awake()
    {
        Instance = this;

        cinemachineCamera = GetComponent<CinemachineCamera>();
        noiseComponent = GetComponent<CinemachineBasicMultiChannelPerlin>();

        if (noiseComponent != null)
        {
            noiseComponent.AmplitudeGain = 0f;
        }
    }

    public void Shake(float intensity, float duration)
    {
       if (noiseComponent != null)
        {
            noiseComponent.AmplitudeGain = intensity;
            shakeTimer = duration;
        }
        
    }

    private void Update()
    {
        if (shakeTimer > 0) //if timer is still running
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer < 0) //timer ended
            {
                StopShake();
            }
        }
    }
    private void StopShake()
    {
        if (noiseComponent != null)
        {
            noiseComponent.AmplitudeGain = 0f;
        }
    }
}