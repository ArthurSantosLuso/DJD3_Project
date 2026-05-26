using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Tooltip("The fill image of the health bar")]
    [SerializeField] private Image fillImage;

    [Tooltip("Offset above the enemy's pivot in world units")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2.2f, 0f);

    private Transform targetTransform;
    private RectTransform rectTransform;
    private RectTransform canvasRect;
    private Camera mainCamera;


    private RawImage renderImage;

    // ── Initialisation ──────────────────────────────────────────────────────

    /// <summary>
    /// Call this right after Instantiate to bind the bar to its enemy.
    /// </summary>
    /// <param name="target">The enemy Transform to follow.</param>
    /// <param name="parentCanvas">The overlay canvas this bar lives in.</param>
    /// <param name="renderTexImage">
    ///     The RawImage that displays the render texture. 
    ///     Pass null if the image fills the entire screen.
    /// </param>
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
    /// Updates the fill amount of the bar.
    /// </summary>
    public void UpdateBar(float currentValue, float maxValue)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = Mathf.Clamp01(currentValue / maxValue);
    }

    /// <summary>
    /// Hides or shows the bar 
    /// </summary>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}