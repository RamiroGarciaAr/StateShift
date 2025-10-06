using UnityEngine.InputSystem;
using UnityEngine;
using Strategies;
namespace Entities.Controllers
{
    public class PlayerController : Controller
    {
        [SerializeField] private InputActionAsset _inputActions;
        private PlayerInput _playerInput;
        private InputAction _moveAction, _jumpAction;
        private Vector2 _movementInput = Vector2.zero;

        private void Awake()
        {
            // Asignar el Controllable (asumiendo que PlayerMovement está en el mismo GameObject)
            Controllable = GetComponent<IControllable>();
            _playerInput = GetComponent<PlayerInput>();
            
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

            _moveAction.Enable();
            _jumpAction.Enable();
            
            // Suscribirse a los eventos de salto
            _jumpAction.performed += OnJumpPerformed;
            _jumpAction.canceled += OnJumpCanceled;
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _jumpAction?.Disable();
            
            // Desuscribirse de los eventos
            if (_jumpAction != null)
            {
                _jumpAction.performed -= OnJumpPerformed;
                _jumpAction.canceled -= OnJumpCanceled;
            }
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
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            Controllable?.Jump();
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            if (Controllable is PlayerMovement playerMovement)
            {
                playerMovement.SetHoldingJump(false);
            }
        }
    }
}