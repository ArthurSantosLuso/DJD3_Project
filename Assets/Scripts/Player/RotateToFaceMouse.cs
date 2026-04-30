using UnityEngine;
using UnityEngine.UI;
public class RotateToFaceMouse : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float rotationSpeed = 10f;


    public Camera mainCamera;
    public LayerMask groundLayer;
    public RawImage renderImage;
    void Update()
    {
        Vector2 localPoint;
        RectTransform rectTransform = renderImage.rectTransform;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint))
        {
            Vector2 uv = new Vector2(
                (localPoint.x + rectTransform.rect.width * 0.5f) / rectTransform.rect.width,
                (localPoint.y + rectTransform.rect.height * 0.5f) / rectTransform.rect.height
            );
            Ray ray = mainCamera.ViewportPointToRay(uv);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
            {
                Vector3 direction = hit.point - transform.position;
                direction.y = 0;
                if (direction.magnitude < minDistance) return;
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
    }
}