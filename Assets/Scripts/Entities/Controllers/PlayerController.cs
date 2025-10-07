using UnityEngine.InputSystem;
using UnityEngine;
using Strategies;
using Core;
namespace Entities.Controllers
{
    [RequireComponent(typeof(PlayerCrouch))]
    public class PlayerController : Controller
    {
        private PlayerInput _playerInput;
        private InputAction _moveAction, _jumpAction, _sprintAction, _crouchAction;
        private PlayerCrouch _playerCrouch;
        private Vector2 _movementInput = Vector2.zero;
        private bool _wantToCrouch = false;
        protected override void Awake()
        {
            Controllable = GetComponent<IControllable>();
            _playerInput = GetComponent<PlayerInput>();
            _playerCrouch = GetComponent<PlayerCrouch>();

            if (Controllable == null)
            {
                Debug.LogError("No se encontró un componente IControllable en " + gameObject.name, this);
            }
        }

        private void OnEnable()
        {
            if (_playerInput.currentActionMap == null)
                _playerInput.SwitchCurrentActionMap("Player");

            _moveAction = _playerInput.actions["Movement"];
            _jumpAction = _playerInput.actions["Jump"];
            _sprintAction = _playerInput.actions["Sprint"];
            _crouchAction = _playerInput.actions["Crouch"];

            _moveAction.Enable();
            _jumpAction.Enable();
            _crouchAction.Enable();
            _sprintAction.Enable();

        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _jumpAction?.Disable();
            _crouchAction?.Disable();
            _sprintAction?.Disable();
        }

        private void Update()
        {
            if (Controllable == null) return;

            _movementInput = _moveAction.ReadValue<Vector2>();
            Vector2 direction;

            var cam = Camera.main;
            if (cam != null)
            {
                var camTransform = cam.transform;
                var camForward = camTransform.forward;
                var camRight = camTransform.right;

                var forwardXZ = new Vector2(camForward.x, camForward.z).normalized;
                var rightXZ = new Vector2(camRight.x, camRight.z).normalized;

                direction = forwardXZ * _movementInput.y + rightXZ * _movementInput.x;
            }
            else
            {
                direction = _movementInput;
            }

            Controllable.Move(direction);

            _wantToCrouch = _crouchAction != null && _crouchAction.IsPressed();
            _playerCrouch.SetCrouching(_wantToCrouch);

            if (_jumpAction.WasPressedThisFrame()) Controllable.Jump();
            Controllable.SetHoldingJump(_jumpAction.IsPressed());
            if (_wantToCrouch)
                Controllable.SetMovementState(MovementState.Crouching);
            else if (_sprintAction.IsPressed())
                Controllable.SetMovementState(MovementState.Sprinting);
            else
                Controllable.SetMovementState(MovementState.Walking);
        }

    }
}