using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : PausableMonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Sprint Settings")]
    [SerializeField] private float sprintMultiplier;
    [SerializeField] private float sprintStaminaCostPerSecond;

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance;
    [SerializeField] private float dashDuration;
    [SerializeField] private float dashStaminaCost;
    [SerializeField] private float dashCooldown;

    [Header("Gravity Settings")]
    [SerializeField] private float gravityMultiplier = 2f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask groundLayer;

    [Header("Footsteps")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private Vector2 footstepPitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] private float footstepInterval = 0.4f;
    [SerializeField] private float sprintFootstepInterval = 0.25f;

    private CharacterController controller;
    private Character character;
    private Animator animator;
    private PlayerInput input;
    private PlayerStamina stamina;
    private bool wasSprinting = false;
    private float lastDashTime = -999f;
    private bool isDashing = false;
    private float verticalVelocity = 0f;
    private float footstepTimer = 0f;


    private void Start()
    {
        animator = GetComponent<Animator>();
        character = GetComponent<Character>();
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        stamina = GetComponent<PlayerStamina>();

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        ApplyGravity();

        if (!CanMove())
        {
            animator.SetFloat("VelocityX", 0f, 0.1f, Time.deltaTime);
            animator.SetFloat("VelocityZ", 0f, 0.1f, Time.deltaTime);
            animator.SetFloat("MoveMagnitude", 0f);
            return;
        }
        HandleMovement();
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // Small constant keeps the controller grounded
        }
        else
        {
            verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }

        controller.Move(new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime);
    }

    private void HandleMovement()
    {
        Vector2 moveInput = input.MoveInput;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        bool isSprinting;
        if (character.ShouldUseStamina)
        {
            isSprinting = input.SprintHeld && moveInput != Vector2.zero && stamina.HasStamina(0.01f);
        }
        else
        {
            isSprinting = input.SprintHeld && moveInput != Vector2.zero;
        }

        if (character.ShouldUseStamina)
        {
            if (isSprinting && !wasSprinting)
                stamina.StartConsuming(sprintStaminaCostPerSecond);
            else if (!isSprinting && wasSprinting)
                stamina.StopConsuming();
        }

        wasSprinting = isSprinting;

        float speed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        if (moveDirection.magnitude >= 0.1f)
        {
            controller.Move(moveDirection * speed * Time.deltaTime);
        }

        Vector3 localMove = transform.InverseTransformDirection(moveDirection);

        animator.SetFloat("VelocityX", localMove.x, 0.1f, Time.deltaTime);
        animator.SetFloat("VelocityZ", localMove.z, 0.1f, Time.deltaTime);
        animator.SetFloat("MoveMagnitude", moveDirection.magnitude);

        if (moveDirection.magnitude >= 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                float interval = isSprinting ? sprintFootstepInterval : footstepInterval;
                footstepTimer = interval;
                if (footstepSounds.Length > 0 && AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(
                        footstepSounds[UnityEngine.Random.Range(0, footstepSounds.Length)],
                        UnityEngine.Random.Range(footstepPitchRange.x, footstepPitchRange.y)
                    );
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    public bool UseDash()
    {
        if (isDashing) return false;
        if (Time.time < lastDashTime + dashCooldown) return false;
        if (character.ShouldUseStamina && !stamina.HasStamina(dashStaminaCost)) return false;

        Vector2 moveInput = input.MoveInput;
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 dashDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;
        if (dashDirection == Vector3.zero)
            dashDirection = transform.forward;

        if (character.ShouldUseStamina) 
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
        if (character.ShouldAttack)
        {
            if (character.CharacterState == Character.State.Attacking)
                return false;
        }

        return true;
    }
}