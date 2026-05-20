using UnityEngine;

public class SceneViewCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float shiftMultiplier = 2.5f;
    public float panSpeed = 15f;

    [Header("Look Settings")]
    public float lookSpeed = 2f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 10f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        // Initialize rotations based on the current camera orientation
        Vector3 euler = transform.localRotation.eulerAngles;
        rotationX = euler.y;
        rotationY = euler.x;
    }

    void Update()
    {
        // 1. RIGHT-CLICK: LOOK & FLY
        if (Input.GetMouseButton(1))
        {
            // Camera Rotation (Look around)
            rotationX += Input.GetAxis("Mouse X") * lookSpeed;
            rotationY -= Input.GetAxis("Mouse Y") * lookSpeed;
            rotationY = Mathf.Clamp(rotationY, -90f, 90f); // Prevent flipping upside down

            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0f);

            // Keyboard Movement (WASDQE)
            float currentSpeed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                currentSpeed *= shiftMultiplier; // Sprinting
            }

            Vector3 moveDirection = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
            if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
            if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
            if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;
            if (Input.GetKey(KeyCode.E)) moveDirection += transform.up;   // Fly Up
            if (Input.GetKey(KeyCode.Q)) moveDirection -= transform.up;   // Fly Down

            transform.position += moveDirection * currentSpeed * Time.deltaTime;
        }

        // 2. MIDDLE-CLICK: PANNING
        else if (Input.GetMouseButton(2))
        {
            float panX = -Input.GetAxis("Mouse X") * panSpeed * Time.deltaTime;
            float panY = -Input.GetAxis("Mouse Y") * panSpeed * Time.deltaTime;

            // Move relative to camera's current local orientation
            transform.Translate(new Vector3(panX, panY, 0f), Space.Self);
        }

        // 3. SCROLL WHEEL: ZOOM
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            transform.position += transform.forward * scroll * zoomSpeed;
        }
    }
}