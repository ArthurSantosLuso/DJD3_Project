using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Sprint Settings")]
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float sprintStaminaCostPerSecond;

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashStaminaCost;
    [SerializeField] private float dashCooldown;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask groundLayer;

    private CharacterController controller;
    private Character character;
    private Animator animator;
    private PlayerInput input;
    private PlayerStamina stamina;
    private bool wasSprinting = false;
    private float lastDashTime = -999f;
    private bool isDashing = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        character = GetComponent<Character>();
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        stamina = GetComponent<PlayerStamina>();

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
        //Base Movement
        Vector2 moveInput = input.moveInput;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        //Sprint Section
        bool isSprinting = input.sprintHeld && moveInput != Vector2.zero && stamina.HasStamina(0.01f);

        if (isSprinting && !wasSprinting)
            stamina.StartConsuming(sprintStaminaCostPerSecond);
        else if (!isSprinting && wasSprinting)
            stamina.StopConsuming();

        wasSprinting = isSprinting;

        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        //Movement Direction
        if (moveDirection.magnitude >= 0.1f)
        {
            controller.Move(moveDirection * speed * Time.deltaTime);
        }

        Vector3 localMove = transform.InverseTransformDirection(moveDirection);

        animator.SetFloat("VelocityX", localMove.x, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityZ", localMove.z, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveMagnitude", moveDirection.magnitude);
    }

    public bool UseDash()
    {
        if (isDashing) return false;
        if (Time.time < lastDashTime + dashCooldown) return false;
        if (!stamina.HasStamina(dashStaminaCost)) return false;

        Vector2 moveInput = input.moveInput;
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 dashDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;
        if (dashDirection == Vector3.zero)
            dashDirection = transform.forward;

        stamina.UseStamina(dashStaminaCost);
        animator.SetTrigger("Dash");
        StartCoroutine(DashCoroutine(dashDirection));
        return true;
    }
    private IEnumerator DashCoroutine(Vector3 direction)
    {
        isDashing = true;
        character.ChangeState(Character.State.Dodging);
        lastDashTime = Time.time;

        float dashForce = dashDistance / dashDuration;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            controller.Move(direction * dashForce * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        character.ChangeState(Character.State.Normal);
        isDashing = false;
    }

    private bool CanMove()
    {
        if (character.CharacterState == Character.State.Attacking ||
            character.CharacterState == Character.State.Lunging)
            return false;

        return true;
    } 
}