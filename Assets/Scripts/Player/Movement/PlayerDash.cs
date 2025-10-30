using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 25f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private int maxDashCharges = 2;
    [SerializeField] private float chargeRecoveryTime = 2f;
    [Range(0, 1)]
    [SerializeField] private float momentumGain = 0.5f;

    [Header("Dash Behavior")]
    [SerializeField] private float gravityScale = 0f;
    [SerializeField] private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);


    private Rigidbody _rb;
    private GroundChecker _groundChecker;
    private Camera _mainCamera;

    private bool _isDashing = false;
    private float _dashTimer = 0f;
    private float _cooldownTimer = 0f;
    private Vector3 _dashDirection = Vector3.zero;
    private int _currentCharges;
    private Vector3 _lastDashDirection = Vector3.zero;
    private float _chargeRecoveryTimer = 0f;
    private bool _originalGravitySetting;

    #region Properties
    public bool IsDashing => _isDashing;
    public bool CanDash => _currentCharges > 0 && _cooldownTimer <= 0 && !_isDashing;
    public int CurrentCharges => _currentCharges;
    public int MaxCharges => maxDashCharges;
    public float CooldownProgress => 1f - (_cooldownTimer / dashCooldown);
    public float ChargeRecoveryProgress => 1f - (_chargeRecoveryTimer / chargeRecoveryTime);
    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _groundChecker = GetComponent<GroundChecker>();
        _mainCamera = Camera.main;
        _currentCharges = maxDashCharges;
    }

    private void Update()
    {
        UpdateTimers();
    }

    private void FixedUpdate()
    {
        if (_isDashing)
        {
            UpdateDash();
        }
    }

    private void UpdateTimers()
    {
        // Cooldown timer
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }

        // Charge recovery timer
        if (_currentCharges < maxDashCharges)
        {
            _chargeRecoveryTimer -= Time.deltaTime;

            if (_chargeRecoveryTimer <= 0)
            {
                _currentCharges++;
                _chargeRecoveryTimer = chargeRecoveryTime;
            }
        }
    }
    public bool TryStartDash(Vector2 inputDirection)
    {
        if (!CanDash) return false;

        // Determine dash direction
        Vector3 dashDir = CalculateDashDirection(inputDirection);

        if (dashDir == Vector3.zero) return false;

        StartDash(dashDir);
        return true;
    }

    private Vector3 CalculateDashDirection(Vector2 inputDirection)
    {
        float yaw = _mainCamera.transform.eulerAngles.y;
        Quaternion cameraYawRotation = Quaternion.Euler(0f, yaw, 0f);

        if (inputDirection.sqrMagnitude < 0.01f)
            return (cameraYawRotation * Vector3.forward).normalized;

        Vector3 localInput = new Vector3(inputDirection.x, 0f, inputDirection.y);
        Vector3 worldDirection = (cameraYawRotation * localInput).normalized;
        _lastDashDirection = worldDirection;
        return worldDirection;
    }

    private void StartDash(Vector3 direction)
    {
        _isDashing = true;
        _dashTimer = 0f;
        _dashDirection = direction;

        // Use a charge
        _currentCharges--;
        _cooldownTimer = dashCooldown;

        // Start charge recovery if not at max
        if (_currentCharges < maxDashCharges && _chargeRecoveryTimer <= 0)
        {
            _chargeRecoveryTimer = chargeRecoveryTime;
        }

        // Store and disable gravity
        _originalGravitySetting = _rb.useGravity;
        _rb.useGravity = gravityScale > 0;

        // Apply initial dash force
        _rb.AddForce(_dashDirection * dashForce, ForceMode.VelocityChange);
    }

    private void UpdateDash()
    {
        _dashTimer += Time.fixedDeltaTime;

        // Apply dash curve modifier
        float curveValue = dashCurve.Evaluate(_dashTimer / dashDuration);
        Vector3 dashVelocity = _dashDirection * (dashForce * curveValue);

        // Apply minimal gravity during dash if configured
        if (gravityScale > 0)
        {
            dashVelocity.y += Physics.gravity.y * gravityScale * Time.fixedDeltaTime;
        }

        _rb.velocity = dashVelocity;

        // End dash after duration
        if (_dashTimer >= dashDuration)
        {
            EndDash();
        }
    }

    private void EndDash()
    {
        _isDashing = false;
        _dashTimer = 0f;

        // Restore gravity
        _rb.useGravity = _originalGravitySetting;


        Vector3 currentVel = _rb.velocity;
        currentVel *= momentumGain;
        _rb.velocity = currentVel;
    }

    public void CancelDash()
    {
        if (_isDashing)
        {
            EndDash();
        }
    }

    public void RefillCharges()
    {
        _currentCharges = maxDashCharges;
        _chargeRecoveryTimer = 0f;
    }
    public void ResetCooldown()
    {
        _cooldownTimer = 0f;
    }
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.cyan;
        Vector3 start = transform.position;
        Vector3 end = start + _lastDashDirection * 2f;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(end, 0.1f);
    }
}