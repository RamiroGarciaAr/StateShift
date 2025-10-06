using System;
using Strategies;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour, IControllable
{
    #region Fields
    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float speedMultiplier;
    [Range(0, 1)]
    [SerializeField] private float movementSmoothing = .1f;

    [Header("Jump and Gravity")]
    [SerializeField] private float jumpForce = 5;
    [SerializeField] private float gravityMultiplier = 2;
    [SerializeField] private float jumpingGravityMultiplier = 1;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.1f;
    [SerializeField] private float legHeight = 0.5f;
    [SerializeField] private LayerMask groundLayerMask;

    [Header("References")]
    [SerializeField] private new CapsuleCollider collider;


    private Rigidbody _rb;
    private Vector2 _rawMoveDir, _smoothMoveDir, _smoothMoveDirVelocity;
    private bool _jumpInputDown;
    private bool _jumpInputHeld;
    #endregion

    #region Properties
    // ===== Movement =====
    public float Speed { get => speed; set => speed = value; }
    public float SpeedMultiplier { get => speedMultiplier; set => speedMultiplier = value; }
    public float MovementSmoothing { get => movementSmoothing; set => movementSmoothing = value; }
    // ===== Jump =====
    public float JumpForce { get => jumpForce; set => jumpForce = value; }
    public bool IsHoldingJump { get; private set; }
    public bool IsJumping => (_jumpInputHeld && !IsGrounded) || _jumpInputDown;

    // ===== Ground Detection =====
    public bool IsGrounded { get; private set; }
    public bool WasGrounded { get; private set; }
    public Rigidbody GroundRigidbody { get; private set; }
    public Vector3 GroundNormal { get; private set; }
    public Vector3 GroundPoint { get; private set; }
    public Vector3 GroundVelocity
    {
        get
        {
            if (GroundRigidbody != null)
                return GroundRigidbody.GetPointVelocity(GroundPoint);
            return Vector3.zero;
        }
    }
    public float LegHeight
    {
        get => legHeight;
        set => legHeight = value;
    }
    public CapsuleCollider Collider
    {
        get => collider;
        set => collider = value;
    }
    #endregion
    #region Unity Methods
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }
    protected virtual void FixedUpdate()
    {
        CheckGround();
        SmoothInput();
        UpdateMovement();
        ClearInput();
    }


    protected virtual void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;

        var origin = collider.bounds.center;
        var maxDistance = Vector3.Distance(origin, transform.position + groundCheckDistance * Vector3.down);
        var extent = origin + Vector3.down * maxDistance;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origin, extent);
    }

    #endregion
    private void CheckGround()
    {
        var origin = collider.bounds.center;
        var maxDistance = Vector3.Distance(origin, _rb.position + groundCheckDistance * Vector3.down);
        var ray = new Ray(origin, Vector3.down);

        WasGrounded = IsGrounded;

        if (Physics.Raycast(ray, out var hit, maxDistance, groundLayerMask) && !hit.collider.isTrigger)
        {
                Debug.DrawLine(origin, origin + Vector3.down * maxDistance, Color.green);

                IsGrounded = true;
                GroundPoint = hit.point;
                GroundNormal = hit.normal;
        }
        else
        {
            Debug.DrawLine(origin, origin + Vector3.down * groundCheckDistance, Color.red);

            IsGrounded = false;
            GroundPoint = transform.position;
            GroundNormal = Vector3.up;
            GroundRigidbody = null;
        }
    }
    private void SmoothInput()
    {
        _smoothMoveDir = Vector2.SmoothDamp(
                        _smoothMoveDir,
                        _rawMoveDir,
                        ref _smoothMoveDirVelocity,
                        movementSmoothing,
                        Mathf.Infinity,
                        Time.fixedDeltaTime
                    );
    }
    public void Jump()
    {
        if (IsGrounded) _jumpInputDown = true;
    }
    public void SetHoldingJump(bool holding)
    {
        _jumpInputHeld = holding;
    }

    public void Move(Vector2 direction)
    {
        _rawMoveDir = direction.sqrMagnitude > 1 ? direction.normalized : direction;
    }
    private void UpdateMovement()
    {
        if (_rb.GetAccumulatedForce().sqrMagnitude > 0) IsGrounded = false;
            
        if (IsGrounded) _rb.MovePosition(Vector3.MoveTowards(_rb.position, GroundPoint, Time.fixedDeltaTime * 1f));

        var currentVelocity = _rb.velocity;

        var moveVec = Vector3.ProjectOnPlane(new Vector3(_smoothMoveDir.x, 0, _smoothMoveDir.y), GroundNormal) * (Speed * SpeedMultiplier);
        if (!IsGrounded)
        {
            var curVelXZ = new Vector3(currentVelocity.x, 0, currentVelocity.z);
           // moveVec = Vector3.MoveTowards(curVelXZ, moveVec, _airMovementAcceleration * Time.fixedDeltaTime);
        }

        var velocityY = Vector3.zero;
        if (!IsGrounded)
        {
            Vector3 appliedGravity;

            if (currentVelocity.y > 0 && _jumpInputHeld)
            {
                appliedGravity = Vector3.up * (Physics.gravity.y * jumpingGravityMultiplier * Time.fixedDeltaTime);
            }
            else
            {
                appliedGravity = Vector3.up * (Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime);
            }

                velocityY += Vector3.up * currentVelocity.y + appliedGravity;
            }
            else if (_jumpInputDown)
            {
                velocityY = Vector3.up * jumpForce;
                _jumpInputDown = false;

               // OnJump?.Invoke();
            }

            _rb.velocity = moveVec + velocityY + GroundVelocity;
        }


    private void ModifyColliderHeight()
    {
        if (LegHeight >= collider.height)
        {
            Debug.LogError("Leg height is greater than collider height", this);
            return;
        }

        var newHeight = collider.height - legHeight;
        var newCenter = collider.center + Vector3.up * (collider.height - newHeight) * .5f;
        collider.height = newHeight;
        collider.center = newCenter;
    }

    private void ClearInput()
    {
        _rawMoveDir = Vector2.zero;
    }

}
