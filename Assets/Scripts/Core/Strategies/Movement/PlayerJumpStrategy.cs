using UnityEngine;

[System.Serializable]
public class PlayerJumpStrategy : IJumpStrategy
{
    [Header("Jump Settings")]
    [SerializeField] private float minJumpHeight = 0.8f;
    [SerializeField] private float maxJumpHeight = 2.5f;
    [SerializeField] private float jumpHoldTime  = 0.4f;

    [Header("Charge Mode")]
    [SerializeField] private bool chargeOnRelease = true; 

    [Header("Jump Cut Settings")]
    [SerializeField] private float jumpCutMultiplier = 3f;
    [SerializeField] private float jumpCutThreshold  = 0.5f;

    [Header("Coyote Time & Jump Buffer")]
    [SerializeField] private float coyoteTime     = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.10f;
    [Header("Fall Tuning")]
    [SerializeField, Tooltip("Relación t_down / t_up. 0.5 = la bajada tarda la mitad que la subida")]
    private float fallTimeRatio = 0.5f; // => g_down = g_up / (ratio^2)
    private float fallMultiplier = 2f;  

    // State
    private bool isJumpHeld;
    private bool  isCharging;       
    private bool  jumpExecuted;
    private bool  canCutJump;
    private float jumpHoldTimer;
    private float lastGroundedTime;
    private float jumpBufferTimer;
    public PlayerJumpStrategy()
    {
        SetFallTimeRatio(fallTimeRatio);
    }

    public void SetFallTimeRatio(float ratio)
    {
        fallTimeRatio = Mathf.Clamp(ratio, 0.25f, 2f);
        fallMultiplier = 1f / (fallTimeRatio * fallTimeRatio);
    }
    public void SetJumpHeld(bool held)
    {
        isJumpHeld = held;

        if (held)
        {
            if (CanStartCharge())
            {
                isCharging     = true;
                jumpHoldTimer  = 0f;         
                jumpBufferTimer = jumpBufferTime; 
            }
        }
        else
        {
            if (isCharging && !jumpExecuted)
            {
                ExecuteChargedJump(_rbCached);
            }
            isCharging = false;
        }
    }

    private Rigidbody _rbCached;

    public void UpdateJump(Rigidbody rb)
    {
        _rbCached = rb;

        // timers
        if (jumpBufferTimer > 0f) jumpBufferTimer -= Time.fixedDeltaTime;

        if (IsGroundedApprox(rb))
            lastGroundedTime = Time.time;

        if (isCharging && rb != null)
        {
            jumpHoldTimer += Time.fixedDeltaTime;
            if (jumpHoldTimer > jumpHoldTime) jumpHoldTimer = jumpHoldTime;
        }
        if (canCutJump && rb.velocity.y > jumpCutThreshold && !isJumpHeld && jumpExecuted)
        {
            ApplyJumpCut(rb);
            canCutJump = false;
        }

        // Reset al caer
        if (rb.velocity.y <= 0f)
        {
            float g = Mathf.Abs(Physics.gravity.y);
            float extra = g * (fallMultiplier - 1f);
            rb.AddForce(Vector3.down * extra, ForceMode.Acceleration);
            
            jumpExecuted = false;
            canCutJump = false;
        }
         else
        {
            if (canCutJump && rb.velocity.y > jumpCutThreshold && !isJumpHeld && jumpExecuted)
            {
                ApplyJumpCut(rb);
                canCutJump = false;
            }
        }
    }

    public void ApplyJump(Rigidbody rb, float baseJumpHeight)
    {
        if (!chargeOnRelease)
            ExecuteImmediateJump(rb, baseJumpHeight);
        else
            _rbCached = rb; 
    }

    public bool CanJump(bool isGrounded)
    {
        if (isGrounded) { lastGroundedTime = Time.time; return true; }
        bool coyoteOK = Time.time - lastGroundedTime <= coyoteTime;
        bool bufferOK = jumpBufferTimer > 0f;
        return (coyoteOK || bufferOK) && !jumpExecuted;
    }

    // ===== Helpers =====
    private bool CanStartCharge()
    {
        if (jumpExecuted) return false;
        bool coyoteOK = Time.time - lastGroundedTime <= coyoteTime;
        bool bufferOK = jumpBufferTimer > 0f;
        return coyoteOK || bufferOK; 
    }

    private void ExecuteImmediateJump(Rigidbody rb, float height)
    {
        float v = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * height);
        rb.velocity = new Vector3(rb.velocity.x, v, rb.velocity.z);
        jumpExecuted = true;
        canCutJump   = true;
        jumpHoldTimer = 0f;
    }

    private void ExecuteChargedJump(Rigidbody rb)
    {
        float t = Mathf.Clamp01(jumpHoldTimer / jumpHoldTime);
        float height = Mathf.Lerp(minJumpHeight, maxJumpHeight, t);

        float v = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * height);
        rb.velocity = new Vector3(rb.velocity.x, v, rb.velocity.z);

        jumpExecuted  = true;
        canCutJump    = true;
        jumpHoldTimer = 0f;
        jumpBufferTimer = 0f;
    }

    private void ApplyJumpCut(Rigidbody rb)
    {
       
        float cutForce = Physics.gravity.y * (jumpCutMultiplier - 1f);
        rb.AddForce(Vector3.up * cutForce, ForceMode.Acceleration);
    }

    private bool IsGroundedApprox(Rigidbody rb)
    {
        return rb.velocity.y <= 0.01f; 
    }
}
