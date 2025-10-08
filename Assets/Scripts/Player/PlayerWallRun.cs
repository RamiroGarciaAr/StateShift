using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerMovement))]
public class PlayerWallRun : MonoBehaviour
{
    [Header("WallRunning - Physics")]
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private float wallRunSpeed = 12f;
    [SerializeField] private float wallRunAcceleration = 10f;
    [SerializeField] private float maxWallRunTime = 2f;
    [SerializeField] private float wallStickForce = 15f;
    [SerializeField] private float gravityCounterForce = 15f;
    
    [Header("Wall Jump")]
    [SerializeField] private float wallJumpUpForce = 10f;
    [SerializeField] private float wallJumpSideForce = 15f;
    [SerializeField] private float wallJumpForwardForce = 5f;

    [Header("Detection")]
    [SerializeField] private float wallCheckDistance = 0.8f;
    [SerializeField] private float minJumpHeight = 1f;
    [SerializeField] private float minSpeedToWallRun = 3f;

    [Header("Cooldown")]
    [SerializeField] private float wallRunCooldown = 0.3f;

    // Propiedades públicas
    public bool HasWall => _isWallRight || _isWallLeft;
    public bool IsWallRunning { get; private set; }
    public bool IsWallLeft => _isWallLeft;
    public bool IsWallRight => _isWallRight;
    public float WallRunMomentum => _currentSpeed;

    // Referencias privadas
    private float _wallRunTimer;
    private float _cooldownTimer;
    private float _currentSpeed;
    private RaycastHit _leftWallHit;
    private RaycastHit _rightWallHit;
    private bool _isWallLeft;
    private bool _isWallRight;
    private Rigidbody _rb;
    private PlayerMovement _playerMovement;
    private Transform _cameraTransform;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _playerMovement = GetComponent<PlayerMovement>();
        _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        CheckForWall();

        if (_cooldownTimer > 0)
            _cooldownTimer -= Time.deltaTime;

        if (IsWallRunning)
        {
            _wallRunTimer -= Time.deltaTime;
            
            if (_wallRunTimer <= 0)
            {
                StopWallRun();
            }
        }
    }

    private void FixedUpdate()
    {
        if (IsWallRunning)
        {
            WallRunningMovement();
        }
    }

    private void CheckForWall()
    {
        _isWallRight = Physics.Raycast(
            transform.position, 
            _cameraTransform.right, 
            out _rightWallHit, 
            wallCheckDistance, 
            wallLayerMask
        );
        
        _isWallLeft = Physics.Raycast(
            transform.position, 
            -_cameraTransform.right, 
            out _leftWallHit, 
            wallCheckDistance, 
            wallLayerMask
        );

        // Debug rays
        Debug.DrawRay(transform.position, _cameraTransform.right * wallCheckDistance, 
            _isWallRight ? Color.green : Color.red);
        Debug.DrawRay(transform.position, -_cameraTransform.right * wallCheckDistance, 
            _isWallLeft ? Color.green : Color.red);
    }

    public bool CanWallRun()
    {
        if (_cooldownTimer > 0) return false;
        if (_playerMovement.IsGrounded) return false;

        Vector3 horizontalVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        if (horizontalVel.magnitude < minSpeedToWallRun) return false;

        if (!HasWall) return false;

        if (Physics.Raycast(transform.position, Vector3.down, minJumpHeight)) return false;

        return true;
    }

    public void StartWallRun()
    {
        if (!CanWallRun()) return;

        IsWallRunning = true;
        _wallRunTimer = maxWallRunTime;
        
        Vector3 horizontalVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        _currentSpeed = Mathf.Max(horizontalVel.magnitude, wallRunSpeed * 0.7f);

        _rb.velocity = new Vector3(_rb.velocity.x, Mathf.Max(_rb.velocity.y, 2f), _rb.velocity.z);
    }

    private void WallRunningMovement()
    {
        _rb.useGravity = false;

        Vector3 wallNormal = _isWallRight ? _rightWallHit.normal : _leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if (Vector3.Dot(wallForward, _cameraTransform.forward) < 0)
        {
            wallForward = -wallForward;
        }

        _currentSpeed = Mathf.MoveTowards(_currentSpeed, wallRunSpeed, 
            wallRunAcceleration * Time.fixedDeltaTime);

        Vector3 targetVelocity = wallForward * _currentSpeed;
        Vector3 currentHorizontal = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
        Vector3 newHorizontal = Vector3.Lerp(currentHorizontal, targetVelocity, 0.3f);

        float verticalVelocity = Mathf.MoveTowards(_rb.velocity.y, -1f, 
            gravityCounterForce * Time.fixedDeltaTime);

        _rb.AddForce(-wallNormal * wallStickForce, ForceMode.Force);

        _rb.velocity = new Vector3(newHorizontal.x, verticalVelocity, newHorizontal.z);
    }

    public void StopWallRun()
    {
        if (!IsWallRunning) return;

        IsWallRunning = false;
        _rb.useGravity = true;
        _cooldownTimer = wallRunCooldown;
    
    }

    public void WallJump()
    {
        if (!IsWallRunning) return;

        Vector3 wallNormal = _isWallRight ? _rightWallHit.normal : _leftWallHit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        
        if (Vector3.Dot(wallForward, _cameraTransform.forward) < 0)
            wallForward = -wallForward;

        Vector3 jumpDirection = (transform.up * wallJumpUpForce + 
                                 wallNormal * wallJumpSideForce + 
                                 wallForward * wallJumpForwardForce).normalized;

        float jumpMagnitude = Mathf.Max(wallJumpUpForce + wallJumpSideForce + wallJumpForwardForce, 
                                        _currentSpeed * 1.2f);

        _rb.velocity = Vector3.zero;
        _rb.AddForce(jumpDirection * jumpMagnitude, ForceMode.Impulse);       
        StopWallRun();
    }
}