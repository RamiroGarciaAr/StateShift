using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction,sprintAction,jumpAction,crouchAction;

    private PlayerController controller;

    private Vector2 movementInput = Vector2.zero;
    private bool jumpPressedThisFrame;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        controller = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        
        if (playerInput.currentActionMap == null)
            playerInput.SwitchCurrentActionMap("Player");

        moveAction = playerInput.actions["Movement"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];

        moveAction.performed += OnMovePerformed;
        moveAction.canceled  += OnMoveCanceled;

        jumpAction.performed += OnJumpPerformed;

        moveAction.Enable();
        jumpAction.Enable();
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
            jumpAction.Disable();
        }
    }

    private void FixedUpdate()
    {
       
        if (controller != null)
        {
            Vector3 fwd   = controller.orientation ? Vector3.ProjectOnPlane(controller.orientation.forward, Vector3.up).normalized : Vector3.forward;
            Vector3 right = controller.orientation ? Vector3.ProjectOnPlane(controller.orientation.right,   Vector3.up).normalized : Vector3.right;


            controller.Move(movementInput,40f);

            if (jumpPressedThisFrame)
            {
                jumpPressedThisFrame = false;
            }
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        movementInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        Debug.Log("Yahooo!");
        controller.Jump();
    }
}
