using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Character))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float sprintMultiplier;

    private CharacterController controller;
    private PlayerInput input;
    private Character character;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();
        character = GetComponent<Character>();
        Debug.Log(input.moveInput);
    }

    private void Update()
    {
        if (!CanMove())
            return;

        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = input.moveInput;

        Vector3 move = new Vector3(
            moveInput.x,
            0f,
            moveInput.y
        );
        move = new Vector3(
            move.x - move.z,
            0f,
            move.x + move.z
        );

        move = move.normalized;

        float speed = moveSpeed;

        if (input.sprintHeld && move != Vector3.zero)
        {
            speed = moveSpeed * sprintMultiplier;
        }

        controller.Move(move * speed * Time.deltaTime);
    }

    private bool CanMove()
    {
        if (character.CharacterState == Character.State.Attacking)
            return false;

        return true;
    }

}