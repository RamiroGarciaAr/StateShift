using UnityEngine;

[System.Serializable]
public class VariableJumpStrategy : IJumpStrategy
{
    [Header("Jump Settings")]
    [SerializeField] private float minJumpHeight = 0.8f;
    [SerializeField] private float maxJumpHeight = 2.5f;
    [SerializeField] private float jumpHoldTime = 0.4f;
    
    [Header("Jump Cut Settings")]
    [Tooltip("Multiplicador de gravedad extra cuando se corta el salto")]
    [SerializeField] private float jumpCutMultiplier = 3f;
    [SerializeField] private float jumpCutThreshold = 0.5f; // Velocidad Y mínima para cortar
    
    [Header("Coyote Time & Jump Buffer")]
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    
    // State tracking
    private bool isJumpHeld;
    private float jumpHoldTimer;
    private float lastGroundedTime;
    private float jumpBufferTimer;
    private bool jumpExecuted;
    private bool canCutJump;

    public void ApplyJump(Rigidbody rb, float baseJumpHeight)
    {
        // Calcular altura actual basada en hold time
        float currentJumpHeight = CalculateJumpHeight();
        
        float jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * currentJumpHeight);
        
        // Resetear velocidad Y y aplicar salto
        rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
        
        // Marcar que el salto fue ejecutado
        jumpExecuted = true;
        canCutJump = true;
        jumpHoldTimer = 0f;
    }

    public void SetJumpHeld(bool held)
    {
        isJumpHeld = held;
        
        if (held)
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            // Reset del timer cuando se suelta
            jumpHoldTimer = 0f;
        }
    }

    public void UpdateJump(Rigidbody rb)
    {
        // Update timers
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.fixedDeltaTime;
            
        // Update hold timer solo si estamos subiendo
        if (isJumpHeld && rb.velocity.y > 0f && canCutJump)
        {
            jumpHoldTimer += Time.fixedDeltaTime;
        }  

        // Jump cut logic - aplicar solo si se suelta el botón durante la subida
        if (canCutJump && rb.velocity.y > jumpCutThreshold && !isJumpHeld && jumpExecuted)
        {
            ApplyJumpCut(rb);
            canCutJump = false; // Solo cortar una vez por salto
        }

        // Reset jump executed cuando tocamos suelo o empezamos a caer fuertemente
        if (rb.velocity.y <= 0f)
        {
            jumpExecuted = false;
            canCutJump = false;
        }
    }

    public bool CanJump(bool isGrounded)
    {
        // Coyote time: permitir salto por un breve período después de dejar el suelo
        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            return true;
        }
        
        bool coyoteTimeValid = Time.time - lastGroundedTime <= coyoteTime;
        bool jumpBufferValid = jumpBufferTimer > 0f;
        
        return (coyoteTimeValid || jumpBufferValid) && !jumpExecuted;
    }

    private float CalculateJumpHeight()
    {
        if (!isJumpHeld)
            return minJumpHeight;
            
        // Interpolar entre min y max basado en hold time
        float t = Mathf.Clamp01(jumpHoldTimer / jumpHoldTime);
        return Mathf.Lerp(minJumpHeight, maxJumpHeight, t);
    }

    private void ApplyJumpCut(Rigidbody rb)
    {
        // Aplicar fuerza hacia abajo para cortar el salto
        float cutForce = Physics.gravity.y * (jumpCutMultiplier - 1f);
        rb.AddForce(Vector3.up * cutForce, ForceMode.Acceleration);
    }

    // Debug/Inspector helpers
    public float GetCurrentJumpHeight() => CalculateJumpHeight();
    public float GetHoldTimer() => jumpHoldTimer;
    public bool IsJumpExecuted() => jumpExecuted;
}

