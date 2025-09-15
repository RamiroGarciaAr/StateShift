using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputHandler : MonoBehaviour
{
    private PlayerInput inputActions;

    private void Awake()
    {
        inputActions = GetComponent<PlayerInput>();
        inputActions.actions.FindActionMap("Player").Enable();
        inputActions.actions.FindAction("Movement").performed += HandleMove;
        inputActions.actions.FindAction("Jump").performed += HandleJump;

    }
    private void HandleMove(InputAction.CallbackContext context)
    {
        if (context.action.name != "Movement") return;

        Vector2 movementInput = context.ReadValue<Vector2>();
        Debug.Log($"Movement Input: {movementInput}");
    }
    private void HandleJump(InputAction.CallbackContext context)
    {
        if (context.action.name != "Jump") return;

        if (context.phase == InputActionPhase.Performed)
        {
            Debug.Log("Jump started");
        }
    }
}
