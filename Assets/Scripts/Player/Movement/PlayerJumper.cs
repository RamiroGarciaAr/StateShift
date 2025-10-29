using Core.Events;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(GroundChecker))]
public class PlayerJumper : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravityMultiplier = 2f;
    [SerializeField] private float jumpingGravityMultiplier = 1f;

    private Rigidbody _rb;
    private GroundChecker _groundChecker;
    private bool _jumpInputDown;
    private bool _jumpInputHeld;

    #region Properties
    public float JumpForce 
    { 
        get => jumpForce; 
        set => jumpForce = value; 
    }

    public bool IsHoldingJump => _jumpInputHeld;
    public bool IsJumping => (_jumpInputHeld && !_groundChecker.IsGrounded) || _jumpInputDown;
    #endregion

    #region Events
    public event System.Action<PlayerJumpingEventArgs> OnJumped;
    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _groundChecker = GetComponent<GroundChecker>();
    }

    /// <summary>
    /// Request a jump. Will only execute if grounded.
    /// </summary>
    public void RequestJump()
    {
        if (_groundChecker.IsGrounded)
        {
            _jumpInputDown = true;
        }
    }

    /// <summary>
    /// Set whether the jump button is being held down.
    /// </summary>
    public void SetHoldingJump(bool holding)
    {
        _jumpInputHeld = holding;
    }

    /// <summary>
    /// Apply jump force if a jump was requested.
    /// Returns the vertical velocity to apply.
    /// </summary>
    public Vector3 ProcessJump()
    {
        if (_jumpInputDown && _groundChecker.IsGrounded)
        {
            _jumpInputDown = false;
            
            var jumpData = new PlayerJumpingEventArgs(jumpForce: jumpForce);
            OnJumped?.Invoke(jumpData);
            
            return Vector3.up * jumpForce;
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Calculate and apply gravity based on current state.
    /// Returns the vertical velocity with gravity applied.
    /// </summary>
    public Vector3 ApplyGravity(float currentVerticalVelocity)
    {
        if (_groundChecker.IsGrounded)
        {
            return Vector3.zero;
        }

        Vector3 appliedGravity;

        // Variable jump height: less gravity when holding jump and moving up
        if (currentVerticalVelocity > 0 && _jumpInputHeld)
        {
            appliedGravity = Vector3.up * (Physics.gravity.y * jumpingGravityMultiplier * Time.fixedDeltaTime);
        }
        else
        {
            appliedGravity = Vector3.up * (Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime);
        }

        return Vector3.up * currentVerticalVelocity + appliedGravity;
    }

    /// <summary>
    /// Clear jump input. Call this at the end of FixedUpdate.
    /// </summary>
    public void ClearJumpInput()
    {
        // El input down se limpia cuando se ejecuta el salto
        // Aquí podrías agregar lógica adicional si es necesario
    }
}