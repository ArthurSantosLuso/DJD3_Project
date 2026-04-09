using UnityEngine;
using UnityEngine.UI;

public class RotateToFaceMouse : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask groundLayer;
    public RawImage renderImage;

    // Update is called once per frame
    void Update()
    {
        Vector2 localPoint;

        RectTransform rectTransform = renderImage.rectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint))
        {
            // Convert to UV (0 to 1)
            Vector2 uv = new Vector2(
                (localPoint.x + rectTransform.rect.width * 0.5f) / rectTransform.rect.width,
                (localPoint.y + rectTransform.rect.height * 0.5f) / rectTransform.rect.height
            );

            Ray ray = mainCamera.ViewportPointToRay(uv);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                Vector3 direction = hit.point - transform.position;
                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);
                }
            }
            Debug.Log(uv);
        }
    }
}
