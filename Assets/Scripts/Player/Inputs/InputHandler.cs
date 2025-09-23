using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction, sprintAction, jumpAction;

    private PlayerController controller;

    private Vector2 movementInput = Vector2.zero;
    private bool jumpPressedThisFrame;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        controller  = GetComponent<PlayerController>();
    }
    private void OnEnable()
    {
        if (playerInput.currentActionMap == null)
            playerInput.SwitchCurrentActionMap("Player");

        moveAction   = playerInput.actions["Movement"];
        jumpAction   = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];

        moveAction.performed += OnMovePerformed;
        moveAction.canceled  += OnMoveCanceled;

       
        jumpAction.performed += OnJumpPerformed;
       
       jumpAction.performed += ctx => {
            controller.Jump();            
            controller.SetJumpHeld(true);
        };
        jumpAction.canceled  += ctx => controller.SetJumpHeld(false);

        sprintAction.performed += ctx => controller.SetSprint(true);
        sprintAction.canceled  += ctx => controller.SetSprint(false);

        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled  -= OnMoveCanceled;
            moveAction.Disable();
        }
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
            // Quitar también los handlers anónimos
            jumpAction.performed -= ctx => controller.SetJumpHeld(true);
            jumpAction.canceled  -= ctx => controller.SetJumpHeld(false);
            jumpAction.Disable();
        }
        if (sprintAction != null) sprintAction.Disable();
    }

    private void FixedUpdate()
    {
        if (controller != null)
        {
            controller.MovePlayer(movementInput);

            if (jumpPressedThisFrame)
            {
                jumpPressedThisFrame = false;
            }
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx) => movementInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled (InputAction.CallbackContext ctx) => movementInput = Vector2.zero;

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        controller.Jump();
        jumpPressedThisFrame = true;
    }
}
