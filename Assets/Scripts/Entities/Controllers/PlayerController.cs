using UnityEngine.InputSystem;
using UnityEngine;
using Strategies;
using Core;

namespace Entities.Controllers
{
    [RequireComponent(typeof(PlayerCrouch))]
    [RequireComponent(typeof(PlayerSlide))]
    [RequireComponent(typeof(PlayerWallRun))]  
    public class PlayerController : Controller
    {
        private PlayerInput _playerInput;
        private InputAction _moveAction, _jumpAction, _sprintAction, _crouchAction;
        
        // State Machine
        private StateMachine<MovementState> _stateMachine;
        private PlayerMovementContext _context;
        
        protected override void Awake()
        {
            Controllable = GetComponent<IControllable>();
            _playerInput = GetComponent<PlayerInput>();

            if (Controllable == null)
            {
                Debug.LogError("No se encontró un componente IControllable en " + gameObject.name, this);
            }

            InitializeStateMachine();
        }

        private void InitializeStateMachine()
        {
            var playerMovement = GetComponent<PlayerMovement>();
            
            // Crear contexto
            _context = new PlayerMovementContext
            {
                Controllable = Controllable,
                PlayerMovement = playerMovement,          
                PlayerCrouch = GetComponent<PlayerCrouch>(),
                PlayerSlide = GetComponent<PlayerSlide>(),
                PlayerWallRun = GetComponent<PlayerWallRun>(),  
                Rigidbody = GetComponent<Rigidbody>()
            };

            // Crear state machine
            _stateMachine = new StateMachine<MovementState>();
            _context.StateMachine = _stateMachine;

            // Registrar estados
            _stateMachine.RegisterState(MovementState.Walking, new WalkingState(_context));
            _stateMachine.RegisterState(MovementState.Sprinting, new SprintingState(_context));
            _stateMachine.RegisterState(MovementState.Crouching, new CrouchingState(_context));
            _stateMachine.RegisterState(MovementState.Sliding, new SlidingState(_context));
            _stateMachine.RegisterState(MovementState.WallRunning, new WallRunningState(_context)); 

            // Inicializar en Walking
            _stateMachine.Initialize(MovementState.Walking);
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

            Vector2 movementInput = _moveAction.ReadValue<Vector2>();
            Vector2 direction = CalculateCameraRelativeDirection(movementInput);

            UpdateContext(direction);
            _stateMachine.Update();

            Controllable.Move(direction);
            HandleJump();
        }

        private void FixedUpdate()
        {
            _stateMachine?.FixedUpdate();
        }

        private Vector2 CalculateCameraRelativeDirection(Vector2 input)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                var camTransform = cam.transform;
                var camForward = camTransform.forward;
                var camRight = camTransform.right;

                var forwardXZ = new Vector2(camForward.x, camForward.z).normalized;
                var rightXZ = new Vector2(camRight.x, camRight.z).normalized;

                return forwardXZ * input.y + rightXZ * input.x;
            }
            
            return input;
        }

        private void UpdateContext(Vector2 direction)
        {
            _context.MovementInput = direction;
            _context.WantsToCrouch = _crouchAction != null && _crouchAction.IsPressed();
            _context.WantsToSprint = _sprintAction != null && _sprintAction.IsPressed();
            _context.WantsToJump = _jumpAction != null && _jumpAction.WasPressedThisFrame();
        }

        private void HandleJump()
        {
            if (_jumpAction.WasPressedThisFrame())
            {
                Controllable.Jump();
            }
            
            Controllable.SetHoldingJump(_jumpAction.IsPressed());
        }

        // Debug helper - Ahora muestra más información
        private void OnGUI()
        {
            if (_stateMachine != null)
            {
                GUI.Label(new Rect(10, 10, 200, 20), $"Estado: {_stateMachine.CurrentStateType}");
                
            }
        }
    }
}