using UnityEngine.InputSystem;
using UnityEngine;
using Strategies;
using Core;
namespace Entities.Controllers
{
    [RequireComponent(typeof(PlayerSlide))]
    [RequireComponent(typeof(PlayerCrouch))]
    public class PlayerController : Controller
    {
        private PlayerInput _playerInput;
        private InputAction _moveAction, _jumpAction, _sprintAction, _crouchAction;
        private PlayerCrouch _playerCrouch;
        private PlayerSlide _playerSlide;
        private Vector2 _movementInput = Vector2.zero;
        private bool _wantToCrouch = false;
        private bool _wasSprinting = false;
        protected override void Awake()
        {
            Controllable = GetComponent<IControllable>();
            _playerInput = GetComponent<PlayerInput>();
            _playerCrouch = GetComponent<PlayerCrouch>();
            _playerSlide = GetComponent<PlayerSlide>();

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
            bool isSprinting = _sprintAction.IsPressed();
            
            // Detectar transición de sprint a crouch para hacer slide
            if (_wantToCrouch && _wasSprinting && !_playerSlide.IsSliding)
            {
                bool slideStarted = _playerSlide.TryStartSlide();
                
                if (slideStarted)
                {
                    _playerCrouch.SetCrouching(true);
                    Controllable.SetMovementState(MovementState.Sliding);
                }
                else
                {
                    // Si no tiene suficiente velocidad, solo agacharse
                    _playerCrouch.SetCrouching(true);
                    Controllable.SetMovementState(MovementState.Crouching);
                }
            }
            else if (_playerSlide.IsSliding)
            {
                // Mantener estado de slide hasta que termine
                Controllable.SetMovementState(MovementState.Sliding);
                
                // Si el jugador deja de presionar crouch durante el slide, cancelarlo
                if (!_wantToCrouch)
                {
                    _playerSlide.CancelSlide();
                    _playerCrouch.SetCrouching(false);
                }
            }
            else if (_wantToCrouch)
            {
                _playerCrouch.SetCrouching(true);
                Controllable.SetMovementState(MovementState.Crouching);
            }
            else
            {
                _playerCrouch.SetCrouching(false);
                
                if (isSprinting)
                    Controllable.SetMovementState(MovementState.Sprinting);
                else
                    Controllable.SetMovementState(MovementState.Walking);
            }
            
            // Actualizar estado de sprint previo
            _wasSprinting = isSprinting && !_wantToCrouch;

            if (_jumpAction.WasPressedThisFrame())
            {
                // Cancelar slide si está activo y el jugador salta
                if (_playerSlide.IsSliding)
                {
                    _playerSlide.CancelSlide();
                }
                Controllable.Jump();
            }
            
            Controllable.SetHoldingJump(_jumpAction.IsPressed());
        }

    }
}