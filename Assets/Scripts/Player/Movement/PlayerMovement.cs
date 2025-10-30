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
    [Header("Momentum")]
    [SerializeField] private float postDashCarryDuration = 0.25f;
    [SerializeField] private float groundCarryDamping = 3f;
    [SerializeField] private float maxMomentum = 0.4f; // +40% speed cap
    [SerializeField] private float momentumDecayHalfLife = 1.5f; // seconds
    [SerializeField] private float sprintGainPerSec = 0.05f;
    [SerializeField] private float slideGainPerSec = 0.10f;
    [SerializeField] private float downhillGainPerSec = 0.12f;
    [SerializeField] private float postDashGain = 0.10f;

    private Rigidbody _rb;
    private GroundChecker _groundChecker;
    private PlayerJumper _playerJumper;
    private Vector2 _rawMoveDir, _smoothMoveDir, _smoothMoveDirVelocity;

    private MovementState _currentMovementState = MovementState.Walking;
    private MovementState _lastMovementState = MovementState.Walking;
    private float _postDashCarryTimer = 0f;
    private float _momentum = 0f;
    #endregion

    #region Properties
    // ===== Movement =====
    public float Speed { get => baseSpeed; set => baseSpeed = value; }
    public float MovementSmoothing { get => movementSmoothing; set => movementSmoothing = value; }
    private float CurrentSpeed
    {
        get
        {
            float stateSpeed = _currentMovementState switch
            {
                MovementState.Sprinting => baseSpeed * sprintSpeedMultiplier,
                MovementState.Walking => baseSpeed * walkSpeedMultiplier,
                MovementState.Crouching => baseSpeed * crouchSpeedMultiplier,
                MovementState.Sliding => baseSpeed * slideSpeedMultiplier,
                MovementState.WallRunning => baseSpeed * wallRunSpeedMultiplier,
                MovementState.Dashing => 0f, // Dash handles its own speed
                _ => baseSpeed
            };
            return stateSpeed * (1f + _momentum);
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
    public float Momentum01 => maxMomentum > 0f ? (_momentum / maxMomentum) : 0f;
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

    protected virtual void FixedUpdate()
    {
        _groundChecker.CheckGround();
        if (_postDashCarryTimer > 0f) _postDashCarryTimer -= Time.fixedDeltaTime;
        AccumulateAndDecayMomentum();
        SmoothInput();
        UpdateSlopeDrag();
        UpdateMovement();
        ClearInput();

    }
    #endregion

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
        _lastMovementState = _currentMovementState;
        _currentMovementState = state;
        if (_lastMovementState == MovementState.Dashing && _currentMovementState != MovementState.Dashing)
        {
            _postDashCarryTimer = postDashCarryDuration;
        }
    }

    public void AddMomentum(float amount)
    {
        if (amount <= 0f) return;
        _momentum = Mathf.Clamp(_momentum + amount, 0f, maxMomentum);
    }

    public void AddPostDashMomentum()
    {
        AddMomentum(postDashGain);
    }

    public void AddSlideMomentumTick(float weight = 1f)
    {
        if (!_groundChecker.IsGrounded) return;
        _momentum = Mathf.Clamp(_momentum + slideGainPerSec * Mathf.Max(0f, weight) * Time.fixedDeltaTime, 0f, maxMomentum);
    }

    private void AccumulateAndDecayMomentum()
    {
        bool fueled = false;

        if (_groundChecker.IsGrounded)
        {
            // Sprinting fuels momentum
            if (_currentMovementState == MovementState.Sprinting && _rawMoveDir.sqrMagnitude > 0.0001f)
            {
                _momentum = Mathf.Clamp(_momentum + sprintGainPerSec * Time.fixedDeltaTime, 0f, maxMomentum);
                fueled = true;
            }

            // Sliding fuels momentum (also handled in PlayerSlide, but keep a base gain here)
            if (_currentMovementState == MovementState.Sliding)
            {
                _momentum = Mathf.Clamp(_momentum + slideGainPerSec * 0.25f * Time.fixedDeltaTime, 0f, maxMomentum);
                fueled = true;
            }

            // Downhill fuels momentum
            if (_groundChecker.IsOnSlope && _groundChecker.IsOnWalkableSlope)
            {
                Vector3 horizontalVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
                float downhillDot = Vector3.Dot(horizontalVel.normalized, _groundChecker.SlopeDir);
                if (downhillDot > 0.1f)
                {
                    float slopeFactor = Mathf.Clamp01(_groundChecker.SlopeAngle / 45f);
                    float gain = downhillGainPerSec * slopeFactor * downhillDot * Time.fixedDeltaTime;
                    _momentum = Mathf.Clamp(_momentum + gain, 0f, maxMomentum);
                    fueled = true;
                }
            }
        }

        // Exponential decay when not fueled
        if (!fueled && _momentum > 0f)
        {
            float lambda = Mathf.Log(2f) / Mathf.Max(0.0001f, momentumDecayHalfLife);
            float factor = Mathf.Exp(-lambda * Time.fixedDeltaTime);
            _momentum *= factor;
            if (_momentum < 0.0001f) _momentum = 0f;
        }
    }

    private void UpdateMovement()
    {
        if (_currentMovementState == MovementState.Dashing) return; 
        if (_groundChecker.IsGrounded && _rb.drag < 1f) 
        {
            _rb.MovePosition(Vector3.MoveTowards(_rb.position, _groundChecker.GroundPoint, Time.fixedDeltaTime * 1f));
        }

        var currentVelocity = _rb.velocity;
        Vector3 velocityY = CalculateVerticalMovement(currentVelocity.y);

        if (_currentMovementState == MovementState.Sliding)
        {
            _rb.velocity = new Vector3(currentVelocity.x, velocityY.y, currentVelocity.z);
            return;
        }
        Vector3 moveVec = CalculateHorizontalMovement(currentVelocity);
        ApplyFinalVelocity(moveVec, velocityY);
    }
    private void UpdateSlopeDrag()
    {
        // No aplicar drag si estamos en el aire
        if (!_groundChecker.IsGrounded)
        {
            _rb.drag = 0f;
            return;
        }

        // No aplicar drag si estamos en medio de un slide
        if (_currentMovementState == MovementState.Sliding) return; 


        if (_rawMoveDir.sqrMagnitude > 0.01f)
        {
            _rb.drag = 0f;
            return;
        }

        if (_postDashCarryTimer > 0f)
        {
            _rb.drag = 0f; // evitar freno fuerte justo después del dash
            return;
        }

        // Mantener fluidez mientras existe momentum acumulado
        if (_momentum > 0f)
        {
            _rb.drag = 0f;
            return;
        }

        if (_groundChecker.IsOnWalkableSlope)
        {
            _rb.drag = 10f; // Freno de mano
        }
        else
        {
            _rb.drag = 0f; // Sin freno en suelo plano
        }
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

            var curVelXZ = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            if (!_groundChecker.IsGrounded)
            {
                moveVec = Vector3.MoveTowards(curVelXZ, moveVec, airMovementAcceleration * Time.fixedDeltaTime);
            }
            else if (_smoothMoveDir.sqrMagnitude < 0.0001f && _postDashCarryTimer > 0f)
            {
                // sin input en suelo justo tras dash: amortiguar en lugar de cortar a 0
                moveVec = Vector3.MoveTowards(curVelXZ, Vector3.zero, groundCarryDamping * Time.fixedDeltaTime);
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
        if (_groundChecker.IsGrounded)
        {
            return Vector3.zero;
        }
        // Si no saltamos, aplicar gravedad
        return _playerJumper.ApplyGravity(currentVerticalVelocity);
    }

    private void ApplyFinalVelocity(Vector3 horizontalVelocity, Vector3 verticalVelocity)
    {

        Vector3 groundVel = (_groundChecker.GroundRigidbody != null) ? _groundChecker.GroundVelocity : Vector3.zero;

        _rb.velocity = horizontalVelocity + groundVel;
        _rb.velocity += verticalVelocity;
    }

    private void ClearInput()
    {
        _rawMoveDir = Vector2.zero;
    }
}