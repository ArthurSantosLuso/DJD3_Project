using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask groundLayer;

    private CharacterController controller;
    private Character character;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        character = GetComponent<Character>();
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (!CanMove())
            return;

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
    }

    private bool CanMove()
    {
        if (character.CharacterState == Character.State.Attacking)
            return false;

        return true;
    }
}