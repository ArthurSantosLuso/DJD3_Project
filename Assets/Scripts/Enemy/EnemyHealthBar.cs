using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private List<Image> healthSegments = new List<Image>();

    [Tooltip("How many health each segment of life will have.")]
    [SerializeField] private float segmentValue = 100f;

    [Tooltip("Offset above the enemy's pivot in world units")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);

    private Transform targetTransform;
    private RectTransform rectTransform;
    private RectTransform canvasRect;
    private Camera mainCamera;


    private RawImage renderImage;


    public void Initialize(Transform target, Canvas parentCanvas, RawImage renderTexImage = null)
    {
        targetTransform = target;
        rectTransform = GetComponent<RectTransform>();
        canvasRect = parentCanvas.GetComponent<RectTransform>();
        mainCamera = Camera.main;
        renderImage = renderTexImage;
    }

    private void LateUpdate()
    {
        // Enemy died
        // Remove the health bar 
        if (targetTransform == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 worldPos = targetTransform.position + worldOffset;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(worldPos);

        if (viewportPos.z < 0f)
        {
            rectTransform.localPosition = new Vector3(-99999f, -99999f, 0f);
            return;
        }

        Vector2 screenPos;

        if (renderImage != null)
        {
            // Get the raw image rect in screen pixels
            Rect imageScreenRect = GetWorldRect(renderImage.rectTransform);

            screenPos = new Vector2(
                imageScreenRect.x + viewportPos.x * imageScreenRect.width,
                imageScreenRect.y + viewportPos.y * imageScreenRect.height
            );
        }
        else
        {
            screenPos = new Vector2(
                viewportPos.x * Screen.width,
                viewportPos.y * Screen.height
            );
        }

        // Convert window pixels to canva position
        // Pass null as camera because the canvas is overlay
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out Vector2 localPos
        );

        rectTransform.localPosition = localPos;
    }

    private static Rect GetWorldRect(RectTransform rt)
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        return new Rect(
            corners[0].x,
            corners[0].y,
            corners[2].x - corners[0].x,
            corners[2].y - corners[0].y
        );
    }


    /// <summary>
    /// Updates a segmented health bar. Each entry in healthSegments represents up to
    /// 'segmentValue' HP (index 0 = lowest segment, last index = highest segment).
    /// </summary>
    public void UpdateHealthSegments(float currentValue, float maxValue)
    {
        if (healthSegments == null || healthSegments.Count == 0 || segmentValue <= 0f) return;

        int segmentsNeeded = Mathf.CeilToInt(maxValue / segmentValue);

        for (int i = 0; i < healthSegments.Count; i++)
        {
            Image segment = healthSegments[i];
            if (segment == null) continue;

            if (i >= segmentsNeeded)
            {
                // This enemy's maxValue doesn't reach this segment, hide it
                segment.gameObject.SetActive(false);
                continue;
            }

            segment.gameObject.SetActive(true);

            float segmentMin = i * segmentValue;
            float segmentMax = segmentMin + segmentValue;

            if (currentValue >= segmentMax)
                segment.fillAmount = 1f;
            else if (currentValue <= segmentMin)
                segment.fillAmount = 0f;
            else
                segment.fillAmount = (currentValue - segmentMin) / segmentValue;
        }
    }

    /// <summary>
    /// Hides or shows the bar 
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}