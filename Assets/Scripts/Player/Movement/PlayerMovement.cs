using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
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
    private PlayerInput input;


    void Start()
    {
        animator = GetComponent<Animator>();
        character = GetComponent<Character>();
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        if (!CanMove())
        {
            animator.SetFloat("VelocityX", 0f, 0.1f, Time.deltaTime);
            animator.SetFloat("VelocityZ", 0f, 0.1f, Time.deltaTime);
            animator.SetFloat("MoveMagnitude", 0f);
            return;
        }

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
        Vector2 moveInput = input.moveInput;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        float speed = moveSpeed;

        if (input.sprintHeld && moveInput != Vector2.zero)
        {
            speed = moveSpeed * sprintMultiplier;
        }

        if (moveDirection.magnitude >= 0.1f)
        {
            controller.Move(moveDirection * speed * Time.deltaTime);
        }

        Vector3 localMove = transform.InverseTransformDirection(moveDirection);

        animator.SetFloat("VelocityX", localMove.x, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityZ", localMove.z, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveMagnitude", moveDirection.magnitude);
    }

    private bool CanMove()
    {
        if (character.CharacterState == Character.State.Attacking ||
            character.CharacterState == Character.State.Lunging)
            return false;

        return true;
    }
}