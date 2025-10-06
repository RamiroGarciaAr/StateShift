using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction moveAction, sprintAction, jumpAction, pauseAction, interactAction;
    private PlayerInteractor interactor;
    private Vector2 movementInput = Vector2.zero;
    private bool jumpPressedThisFrame;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        interactor = GetComponent<PlayerInteractor>();
    }
    private void OnEnable()
    {
        if (playerInput.currentActionMap == null)
            playerInput.SwitchCurrentActionMap("Player");

        moveAction   = playerInput.actions["Movement"];
        jumpAction   = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        pauseAction  = playerInput.actions["Pause"];
        interactAction= playerInput.actions["Interact"];

        moveAction.performed += OnMovePerformed;
        moveAction.canceled  += OnMoveCanceled;

       
        jumpAction.performed += OnJumpPerformed;
       
       jumpAction.performed += ctx => {
          //  controller.Jump();            
          //  controller.SetJumpHeld(true);
        };
        //jumpAction.canceled  += ctx => controller.SetJumpHeld(false);

        //sprintAction.performed += ctx => controller.SetSprint(true);
        //sprintAction.canceled  += ctx => controller.SetSprint(false);

        pauseAction.performed += OnPausePerformed;

        interactAction.performed += ctx => {
            if (interactor != null) interactor.TryInteract(gameObject);
        };

        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        pauseAction.Enable();
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
            //jumpAction.performed -= ctx => controller.SetJumpHeld(true);
            //jumpAction.canceled  -= ctx => controller.SetJumpHeld(false);
            jumpAction.Disable();
        }
        if (sprintAction != null) sprintAction.Disable();
    }



    private void OnMovePerformed(InputAction.CallbackContext ctx) => movementInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled (InputAction.CallbackContext ctx) => movementInput = Vector2.zero;

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
      //  controller.Jump();
        jumpPressedThisFrame = true;
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        EventsManager.Instance.ActionGamePause(true);
    }
}
