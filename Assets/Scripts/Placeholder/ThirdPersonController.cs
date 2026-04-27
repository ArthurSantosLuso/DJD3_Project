using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 15f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("References")]
    public Transform cameraTransform;
    public LayerMask groundLayer;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        HandleRotationToMouse();
        HandleMovement();
    }

    void HandleRotationToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 pointToLook = ray.GetPoint(rayDistance);

            Vector3 lookDirection = new Vector3(pointToLook.x, transform.position.y, pointToLook.z) - transform.position;

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * vertical + camRight * horizontal).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        Vector3 localMove = transform.InverseTransformDirection(moveDirection);

        animator.SetFloat("VelocityX", localMove.x, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityZ", localMove.z, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveMagnitude", moveDirection.magnitude);

        // Pulo e Gravidade
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}