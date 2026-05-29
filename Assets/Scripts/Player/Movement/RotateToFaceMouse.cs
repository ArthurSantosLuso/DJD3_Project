using UnityEngine;
using UnityEngine.UI;

public class RotateToFaceMouse : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float rotationSpeed = 10f;

    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private RawImage renderImage;

    public void Start()
    {
        mainCamera = Camera.main;
        renderImage = GameObject.FindGameObjectWithTag("Render Texture").GetComponent<RawImage>();
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.x /= Screen.width;
        mousePos.y /= Screen.height;

        Ray ray = mainCamera.ViewportPointToRay(mousePos);

        float deltaY = ((transform.position.y + 1.0f) - ray.origin.y);
        float t = deltaY / ray.direction.y;

        //Debug.Log($"MousePos = {mousePos}, Ray = {ray.origin}/{ray.direction}, DeltaY={deltaY}, T={t}");

        Vector3 targetPos = ray.origin + ray.direction * t;
        Vector3 toTarget = targetPos - (transform.position + Vector3.up * 1.0f);
        toTarget.Normalize();

        // Debug.Log($"targetPos = {targetPos}, toTarget = {toTarget}");

        Quaternion targetRotation = Quaternion.LookRotation(toTarget, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Debug.DrawLine(transform.position + Vector3.up * 1.0f, targetPos, Color.yellow, 1);
    }
}