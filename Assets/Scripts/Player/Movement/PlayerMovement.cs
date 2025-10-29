using System;
using Core;
using Core.Events;
using Strategies;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GroundChecker))]
[RequireComponent(typeof(PlayerJumper))]
public class PlayerMovement : MonoBehaviour, IControllable
{
    #region Fields
    [Header("Movement")]
    [SerializeField] private float baseSpeed = 5f;

    // ==== Speed Multipliers =====
    [SerializeField] private float sprintSpeedMultiplier = 2;
    [SerializeField] private float walkSpeedMultiplier = 1;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;
    [SerializeField] private float slideSpeedMultiplier = 0.3f;
    [SerializeField] private float wallRunSpeedMultiplier = 1.2f;
    [Range(0, 1)]
    [SerializeField] private float movementSmoothing = .1f;
    [SerializeField] private float airMovementAcceleration = .5f;

    private Rigidbody _rb;
    private GroundChecker _groundChecker;
    private PlayerJumper _playerJumper;
    private Vector2 _rawMoveDir, _smoothMoveDir, _smoothMoveDirVelocity;

    private MovementState _currentMovementState = MovementState.Walking;
    #endregion

    #region Properties
    // ===== Movement =====
    public float Speed { get => baseSpeed; set => baseSpeed = value; }
    public float MovementSmoothing { get => movementSmoothing; set => movementSmoothing = value; }
    private float CurrentSpeed
    {
        get
        {
            return _currentMovementState switch
            {
                MovementState.Sprinting => baseSpeed * sprintSpeedMultiplier,
                MovementState.Walking => baseSpeed * walkSpeedMultiplier,
                MovementState.Crouching => baseSpeed * crouchSpeedMultiplier,
                MovementState.Sliding => baseSpeed * slideSpeedMultiplier,
                MovementState.WallRunning => baseSpeed * wallRunSpeedMultiplier,
                _ => baseSpeed
            };
        }
    }

    // ===== Jump  =====
    public float JumpForce { get => _playerJumper.JumpForce; set => _playerJumper.JumpForce = value; }
    public bool IsHoldingJump => _playerJumper.IsHoldingJump;
    public bool IsJumping => _playerJumper.IsJumping;

    // ===== Ground Detection  =====
    public bool IsGrounded => _groundChecker.IsGrounded;
    public bool WasGrounded => _groundChecker.WasGrounded;
    public Rigidbody GroundRigidbody => _groundChecker.GroundRigidbody;
    public Vector3 GroundNormal => _groundChecker.GroundNormal;
    public Vector3 GroundPoint => _groundChecker.GroundPoint;
    public Vector3 GroundVelocity => _groundChecker.GroundVelocity;
    #endregion

    #region Unity Methods
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _groundChecker = GetComponent<GroundChecker>();
        _playerJumper = GetComponent<PlayerJumper>();

        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        // Suscribirse a eventos
        _groundChecker.OnLanded += HandleLanding;
        _playerJumper.OnJumped += HandleJump;
    }

    private void OnDestroy()
    {
        if (_groundChecker != null)
        {
            _groundChecker.OnLanded -= HandleLanding;
        }
        
        if (_playerJumper != null)
        {
            _playerJumper.OnJumped -= HandleJump;
        }
    }

    private void Update()
    {
        Debug.Log(_groundChecker.GroundNormal);
    }

    protected virtual void FixedUpdate()
    {
        _groundChecker.CheckGround();
        SmoothInput();
        UpdateMovement();
        ClearInput();
    }
    #endregion

    private void HandleJump(PlayerJumpingEventArgs jumpData)
    {
        //@TODO: SOUNDS
    }

    private void HandleLanding()
    {        
        var landingData = new PlayerLandingEventArgs(
            fallVelocity: Mathf.Abs(_groundChecker.LastAirVelocityY),
            landingPoint: _groundChecker.GroundPoint,
            landingNormal: _groundChecker.GroundNormal
        );
        //@TODO: SOUNDS
       
    }

    private void SmoothInput()
    {
        float smoothingValue = _currentMovementState == MovementState.Sliding ? 
            movementSmoothing * 3f : movementSmoothing;
            
        _smoothMoveDir = Vector2.SmoothDamp(
            _smoothMoveDir,
            _rawMoveDir,
            ref _smoothMoveDirVelocity,
            smoothingValue,
            Mathf.Infinity,
            Time.fixedDeltaTime
        );
    }

    public void Jump()
    {
        _playerJumper.RequestJump();
    }

    public void SetHoldingJump(bool holding)
    {
        _playerJumper.SetHoldingJump(holding);
    }

    public void Move(Vector2 direction)
    {
        _rawMoveDir = direction.sqrMagnitude > 1 ? direction.normalized : direction;
    }

    public void SetMovementState(MovementState state)
    {
       _currentMovementState = state;
    }

    private void UpdateMovement()
    {
        if (_groundChecker.IsGrounded) 
        {
            _rb.MovePosition(Vector3.MoveTowards(_rb.position, _groundChecker.GroundPoint, Time.fixedDeltaTime * 1f));
        }

        var currentVelocity = _rb.velocity;
        Vector3 moveVec = CalculateHorizontalMovement(currentVelocity);
        Vector3 velocityY = CalculateVerticalMovement(currentVelocity.y);

        ApplyFinalVelocity(moveVec, velocityY);
    }

    private Vector3 CalculateHorizontalMovement(Vector3 currentVelocity)
    {
        Vector3 moveVec;
        
        if (_currentMovementState == MovementState.Sliding && _groundChecker.IsGrounded)
        {
            moveVec = Vector3.ProjectOnPlane(
                new Vector3(_smoothMoveDir.x, 0, _smoothMoveDir.y),
                _groundChecker.GroundNormal
            ) * CurrentSpeed;

            Vector3 currentHorizontal = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            moveVec = Vector3.Lerp(currentHorizontal, moveVec, 0.1f);
        }
        else
        {
            moveVec = Vector3.ProjectOnPlane(
                new Vector3(_smoothMoveDir.x, 0, _smoothMoveDir.y),
                _groundChecker.GroundNormal
            ) * CurrentSpeed;

            if (!_groundChecker.IsGrounded)
            {
                var curVelXZ = new Vector3(currentVelocity.x, 0, currentVelocity.z);
                moveVec = Vector3.MoveTowards(curVelXZ, moveVec, airMovementAcceleration * Time.fixedDeltaTime);
            }
        }

        return moveVec;
    }

    private Vector3 CalculateVerticalMovement(float currentVerticalVelocity)
    {
        // Intentar saltar primero
        Vector3 jumpVelocity = _playerJumper.ProcessJump();
        if (jumpVelocity != Vector3.zero)
        {
            return jumpVelocity;
        }

        // Si no saltamos, aplicar gravedad
        return _playerJumper.ApplyGravity(currentVerticalVelocity);
    }

    private void ApplyFinalVelocity(Vector3 horizontalVelocity, Vector3 verticalVelocity)
    {
        if (_currentMovementState != MovementState.Sliding)
        {
            Vector3 groundVel = (_groundChecker.GroundRigidbody != null) 
                ? _groundChecker.GroundVelocity 
                : Vector3.zero;
            
            _rb.velocity = horizontalVelocity + verticalVelocity + groundVel;
        }
        else
        {
            // Durante el slide, solo actualizar la velocidad vertical
            _rb.velocity = new Vector3(_rb.velocity.x, verticalVelocity.y, _rb.velocity.z);
        }
    }

    private void ClearInput()
    {
        _rawMoveDir = Vector2.zero;
        _playerJumper.ClearJumpInput();
    }
}