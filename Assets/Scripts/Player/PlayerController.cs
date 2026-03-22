using UnityEngine.InputSystem;
using UnityEngine;
using Strategies;
using Core;
using System;
namespace Entities.Controllers
{
    [RequireComponent(typeof(PlayerCrouch))]
    [RequireComponent(typeof(PlayerSlide))]
    [RequireComponent(typeof(PlayerWallRun))]
    [RequireComponent(typeof(PlayerDash))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(PlayerGrapple))]
    public class PlayerController : Controller
    {
        private PlayerInput _playerInput;
        private InputAction _moveAction, _jumpAction, _sprintAction, _crouchAction, _grappleAction, _dashAction,_shootAction;

        //Events
        public static event Action OnShoot;

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
                PlayerDash = GetComponent<PlayerDash>(),
                PlayerGrapple = GetComponent<PlayerGrapple>(),
                Rigidbody = GetComponent<Rigidbody>()
            };

            // Crear state machine
            _stateMachine = new StateMachine<MovementState>();
            _context.StateMachine = _stateMachine;

            // Registrar estados
            _stateMachine.RegisterState(MovementState.Grounded, new GroundedState(_context));
            _stateMachine.RegisterState(MovementState.WallRunning, new WallRunningState(_context));
            _stateMachine.RegisterState(MovementState.Dashing, new DashingState(_context));
            _stateMachine.RegisterState(MovementState.Grappling, new GrapplingState(_context));
            _stateMachine.RegisterState(MovementState.InAir, new InAirState(_context));

            // Inicializar en Grounded (entra en Walking por defecto)
            _stateMachine.Initialize(MovementState.Grounded);
        }

        private void OnEnable()
        {
            if (_playerInput.currentActionMap == null)
                _playerInput.SwitchCurrentActionMap("Player");

            _moveAction = _playerInput.actions["Movement"];
            _jumpAction = _playerInput.actions["Jump"];
            _sprintAction = _playerInput.actions["Sprint"];
            _crouchAction = _playerInput.actions["Crouch"];
            _dashAction = _playerInput.actions["Dash"];
            _grappleAction = _playerInput.actions["Grapple"];
            _shootAction = _playerInput.actions["Shoot"];

            _moveAction.Enable();
            _jumpAction.Enable();
            _crouchAction.Enable();
            _sprintAction.Enable();
            _dashAction.Enable();
            _grappleAction.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _jumpAction?.Disable();
            _crouchAction?.Disable();
            _sprintAction?.Disable();
            _dashAction?.Disable();
            _grappleAction?.Disable();
            _shootAction?.Disable();
        }

        private void Update()
        {
            if (Controllable == null) return;

            if (_shootAction.WasPressedThisFrame())
            {
                OnShoot?.Invoke();
            }

            Vector2 movementInput = _moveAction.ReadValue<Vector2>();
            Vector2 direction = CalculateCameraRelativeDirection(movementInput);

            UpdateContext(direction, movementInput);
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

        private void UpdateContext(Vector2 direction, Vector2 inputRaw)
        {
            _context.MovementInput = direction;
            _context.DashInputDirection = inputRaw;
            _context.WantsToCrouch = _crouchAction != null && _crouchAction.IsPressed();
            _context.WantsToSprint = _sprintAction != null && _sprintAction.IsPressed();
            _context.WantsToJump = _jumpAction != null && _jumpAction.WasPressedThisFrame();
            _context.WantsToDash = _dashAction != null && _dashAction.WasPressedThisFrame();
            _context.WantsToGrapple = _grappleAction != null && _grappleAction.IsPressed();
        }

        private void HandleJump()
        {
            if (_jumpAction.WasPressedThisFrame())
            {
                Controllable.Jump();
            }

            Controllable.SetHoldingJump(_jumpAction.IsPressed());
        }
    }
}