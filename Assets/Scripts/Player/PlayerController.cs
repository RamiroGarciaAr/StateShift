using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour, IMovable, IJump
{
    [Header("References")]
    public Transform orientation;
    [Tooltip("Punto desde el que se chequea el suelo. Si está vacío, se calcula en runtime.")]
    public Transform feet;

    [Header("Drag")]
    public float groundDrag = 4f;
    public float airDrag = 0.1f;

    [Header("Movement Settings")]
    public float walkSpeed = 30f;
    public float sprintSpeed = 100f;

    [Header("Movement Physics")]
    [SerializeField] private float groundAcceleration = 60f;
    [SerializeField] private float airAcceleration = 24f;
    [SerializeField] private float airControlFactor = 0.4f;

    [Header("Jump Settings")]
    [SerializeField] private float minJumpHeight = 1.2f;
    [SerializeField] private float maxJumpHeight = 2.8f;
    [SerializeField] private float jumpRiseTime = 0.3f;
    [SerializeField] private float minJumpTime = 0.08f;

    [Header("Jump Feel")]
    [SerializeField] private float jumpCutMultiplier = 2.5f;
    [SerializeField] private float fallGravityMultiplier = 2.2f;
    [SerializeField] private float lowJumpMultiplier = 3f;

    [Header("Coyote & Buffer")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.15f;

    [Header("Ground Check")]
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundMask;

    [Header("Debug")]
    public bool isDebugModeOn = false;

    // State Machine
    private StateMachine<PlayerStateType> stateMachine;

    // Componentes
    private Rigidbody rb;


    // Estado interno
    private bool isGrounded;
    private bool isSprinting;
    private bool isJumpHeld;
    private Vector2 moveInput;
    private bool jumpRequested;
    
    // Timers
    private float lastGroundedTime;
    private float jumpBufferTimer;

    // Properties públicas para los estados
    public Rigidbody Rb => rb;
    public Transform Orientation => orientation;
    public float GroundDrag => groundDrag;
    public float AirDrag => airDrag;
    public float WalkSpeed => walkSpeed;
    public float SprintSpeed => sprintSpeed;
    public float GroundAcceleration => groundAcceleration;
    public float AirAcceleration => airAcceleration;
    public float AirControlFactor => airControlFactor;
    public float MinJumpHeight => minJumpHeight;
    public float MaxJumpHeight => maxJumpHeight;
    public float JumpRiseTime => jumpRiseTime;
    public float MinJumpTime => minJumpTime;
    public float JumpCutMultiplier => jumpCutMultiplier;
    public float FallGravityMultiplier => fallGravityMultiplier;
    public float LowJumpMultiplier => lowJumpMultiplier;
    public bool IsGrounded => isGrounded;
    public bool IsSprinting => isSprinting;
    public bool IsJumpHeld => isJumpHeld;
    public Vector2 MoveInput => moveInput;
    public bool IsDebugModeOn => isDebugModeOn;

    void OnValidate()
    {
        if (groundMask == 0)
            groundMask = LayerMask.GetMask("Ground");

        if (groundCheckRadius < 0.05f) groundCheckRadius = 0.05f;
        if (groundCheckDistance < groundCheckRadius) groundCheckDistance = groundCheckRadius + 0.05f;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        if (groundMask == 0)
            groundMask = LayerMask.GetMask("Ground");

        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        stateMachine = new StateMachine<PlayerStateType>();

        // Registrar todos los estados
        stateMachine.RegisterState(PlayerStateType.Idle, new PlayerIdleState(this));
        stateMachine.RegisterState(PlayerStateType.Walking, new PlayerWalkingState(this));
        stateMachine.RegisterState(PlayerStateType.Jumping, new PlayerJumpingState(this));
        stateMachine.RegisterState(PlayerStateType.Falling, new PlayerFallingState(this));

        // Inicializar en Idle
        stateMachine.Initialize(PlayerStateType.Idle);
    }

    void Update()
    {
        DoGroundCheck();
        UpdateTimers();
        stateMachine.Update();

        // Debug
        if (isDebugModeOn)
        {
            Debug.Log($"Current State: {stateMachine.CurrentStateType}");
        }
    }

    void FixedUpdate()
    {
        HandleJumpRequest();
        stateMachine.FixedUpdate();
    }

    // ===== Ground Check =====
    private void DoGroundCheck()
    {
        Vector3 center = (feet != null) ? feet.position : (rb.position + Vector3.down * groundCheckDistance);
        isGrounded = Physics.CheckSphere(center, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        if (isGrounded)
            lastGroundedTime = Time.time;

        if (isDebugModeOn)
        {
            Color c = isGrounded ? Color.green : Color.red;
            Debug.DrawLine(center, center + Vector3.up * 0.1f, c, 0f, false);
        }
    }

    // ===== Timers =====
    private void UpdateTimers()
    {
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;
    }

    // ===== Jump Handling =====
    private void HandleJumpRequest()
    {
        if (!jumpRequested) return;

        jumpRequested = false;

        // Coyote time check
        bool canJumpFromCoyote = Time.time - lastGroundedTime <= coyoteTime;

        if (isGrounded || canJumpFromCoyote)
        {
            // Cambiar al estado de salto
            ChangeState(PlayerStateType.Jumping);
        }
        else
        {
            // Buffer el salto
            jumpBufferTimer = jumpBufferTime;
        }
    }

    // Check del buffer en los estados que tocan suelo
    public bool HasJumpBuffered()
    {
        return jumpBufferTimer > 0f;
    }

    public void ConsumeJumpBuffer()
    {
        jumpBufferTimer = 0f;
    }

    // ===== IMovable / IJump =====
    public void SetSprint(bool value) => isSprinting = value;

    public void MovePlayer(Vector2 input)
    {
        moveInput = input;
    }

    public void Move(Vector3 inputAxis, float maxSpeed)
    {
        moveInput = new Vector2(inputAxis.x, inputAxis.z);
    }

    public void Jump()
    {
        jumpRequested = true;
    }

    public void SetJumpHeld(bool held)
    {
        isJumpHeld = held;
    }

    // ===== State Machine Control =====
    public void ChangeState(PlayerStateType newState)
    {
        stateMachine.ChangeState(newState);
    }

    public PlayerStateType GetCurrentState()
    {
        return stateMachine.CurrentStateType;
    }

    // ===== Gizmos =====
    void OnDrawGizmosSelected()
    {
        if (!isDebugModeOn) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Vector3 center = (feet != null) ? feet.position : transform.position + Vector3.down * groundCheckDistance;
        Gizmos.DrawWireSphere(center, groundCheckRadius);
    }
}