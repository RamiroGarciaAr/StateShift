using UnityEngine;

[System.Serializable]
public class PlayerJumpStrategy : IJumpStrategy
{
    [Header("Jump Heights")]
    [SerializeField] private float minJumpHeight = 1.2f;
    [SerializeField] private float maxJumpHeight = 2.8f;  
    
    [Header("Jump Timing")]
    [SerializeField] private float jumpRiseTime = 0.3f;   
    [SerializeField] private float minJumpTime = 0.08f;   
    
    [Header("Jump Feel")]
    [SerializeField] private float jumpCutMultiplier = 2.5f; 
    [SerializeField] private float fallGravityMultiplier = 2.2f;
    [SerializeField] private float lowJumpMultiplier = 3f;       
    
    [Header("Coyote & Buffer")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    
    // Estados internos
    private bool isJumpHeld;
    private bool isJumping;
    private bool canCutJump;
    private float jumpStartTime;
    private float lastGroundedTime;
    private float jumpBufferTimer;
    private float originalGravityY;
    private bool usingCustomGravity;
    
    // Cache para performance
    private Rigidbody cachedRb;

    public void SetJumpHeld(bool held)
    {
        bool wasHeld = isJumpHeld;
        isJumpHeld = held;
        
        if (wasHeld && !held && CanCutJump())
        {
            ApplyJumpCut();
        }
    }

    public void ApplyJump(Rigidbody rb, float baseJumpHeight)
    {
        if (!CanExecuteJump(rb)) return;
        
        ExecuteJump(rb);
    }

    public void UpdateJump(Rigidbody rb)
    {
        cachedRb = rb;
        UpdateTimers();
        UpdateGroundedState(rb);
        HandleJumpPhysics(rb);
        ResetJumpStateOnLanding(rb);
    }

    public bool CanJump(bool isGrounded)
    {
        if (isGrounded) 
        {
            lastGroundedTime = Time.time;
            return true;
        }
        bool coyoteTimeValid = Time.time - lastGroundedTime <= coyoteTime;
        bool jumpBuffered = jumpBufferTimer > 0f;
        
        return (coyoteTimeValid || jumpBuffered) && !isJumping;
    }
    
    private bool CanExecuteJump(Rigidbody rb)
    {
        bool groundedNow = IsGroundedApprox(rb);
        if (!CanJump(groundedNow)) return false;
        
        if (!groundedNow)
        {
            jumpBufferTimer = jumpBufferTime;
            return false;
        }
        
        return true;
    }
    
    private void ExecuteJump(Rigidbody rb)
    {
        float initialVelocity = CalculateJumpVelocity(minJumpHeight);
        rb.velocity = new Vector3(rb.velocity.x, initialVelocity, rb.velocity.z);
        
        isJumping = true;
        canCutJump = false; 
        jumpStartTime = Time.time;
        jumpBufferTimer = 0f; 

        if (!usingCustomGravity)
        {
            originalGravityY = Physics.gravity.y;
            usingCustomGravity = true;
        }
    }
    
    private void HandleJumpPhysics(Rigidbody rb)
    {
        if (!isJumping) return;
        
        float jumpTime = Time.time - jumpStartTime;
        

        if (!canCutJump && jumpTime >= minJumpTime)
        {
            canCutJump = true;
        }
        
        if (isJumpHeld && rb.velocity.y > 0f && jumpTime < jumpRiseTime)
        {
            float progress = jumpTime / jumpRiseTime;
            float targetHeight = Mathf.Lerp(minJumpHeight, maxJumpHeight, progress);
            float targetVelocity = CalculateJumpVelocity(targetHeight);
            

            float currentVelocity = rb.velocity.y;
            float adjustedVelocity = Mathf.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * 8f);
            
            rb.velocity = new Vector3(rb.velocity.x, adjustedVelocity, rb.velocity.z);
        }
        

        if (rb.velocity.y < 0f)
        {
            ApplyFallGravity(rb);
        }
    }
    
    private bool CanCutJump()
    {
        return isJumping && canCutJump && cachedRb != null && cachedRb.velocity.y > 0.1f;
    }
    
    private void ApplyJumpCut()
    {
        if (cachedRb == null) return;
        

        cachedRb.velocity = new Vector3(
            cachedRb.velocity.x, 
            cachedRb.velocity.y * (1f / jumpCutMultiplier), 
            cachedRb.velocity.z
        );
        
        float extraGravity = originalGravityY * (lowJumpMultiplier - 1f);
        cachedRb.AddForce(Vector3.up * extraGravity, ForceMode.Acceleration);
        
        canCutJump = false;
    }
    
    private void UpdateTimers()
    {
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.fixedDeltaTime;
    }
    
    private void UpdateGroundedState(Rigidbody rb)
    {
        if (IsGroundedApprox(rb))
            lastGroundedTime = Time.time;
    }
    
    private void ResetJumpStateOnLanding(Rigidbody rb)
    {
        if (IsGroundedApprox(rb) || (isJumping && rb.velocity.y < -1f))
        {
            isJumping = false;
            canCutJump = false;
            usingCustomGravity = false;
        }
    }
    
    
    private void ApplyFallGravity(Rigidbody rb)
    {
        float extraGravity = originalGravityY * (fallGravityMultiplier - 1f);
        rb.AddForce(Vector3.up * extraGravity, ForceMode.Acceleration);
    }
    
    private float CalculateJumpVelocity(float height)
    {
        return Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * height);
    }
    
    private bool IsGroundedApprox(Rigidbody rb)
    {
        return Mathf.Abs(rb.velocity.y) <= 0.1f;
    }
}
