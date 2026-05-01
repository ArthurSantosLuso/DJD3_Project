using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    public static DamageFlash Instance { get; private set; }

    [Header("UI Image Component")]
    [SerializeField] private Image displayImage; // Image in Canvas
    [Header("UI References")]
    [SerializeField] private Sprite noncriticalHealthFlash; // Image for > 50% health
    [SerializeField] private Sprite criticalHealthFlash; // Image for < 50% health

    private float flashTimer;
    private float maxTime;

    private void Awake()
    {
        Instance = this;

        // Hides the image at the start by setting alpha to 0
        if (displayImage != null)
        {
            Color c = displayImage.color;
            c.a = 0;
            displayImage.color = c;
        }
    }

    public void CallFlash(float duration, bool isCritical)
    {
        if (displayImage == null) return;

        // swaps the texture based on the health
        displayImage.sprite = isCritical ? criticalHealthFlash : noncriticalHealthFlash;

        flashTimer = duration;
        maxTime = duration;

        // Reset alpha to full for the start of the flash
        Color c = displayImage.color;
        c.a = 0.8f;
        displayImage.color = c;
    }

    private void Update()
    {
        if (flashTimer > 0 && displayImage != null)
        {
            flashTimer -= Time.deltaTime;

            Color c = displayImage.color;
            c.a = Mathf.Clamp01(flashTimer / maxTime);
            displayImage.color = c;
        }
    }
    
}