using UnityEngine;

/// <summary>
/// Estado cuando el player está corriendo por una pared
/// </summary>
public class PlayerWallRunningState : PlayerStateBase
{
    private Vector3 wallNormal;
    private Vector3 wallForward;
    private bool isWallRight;
    private float wallRunTimer;
    
    public PlayerWallRunningState(PlayerController context) : base(context) { }

    public override void OnEnter()
    {
        if (Context.IsDebugModeOn)
            Log("Entering Wall Running State");

        // Detectar la pared
        WallRunHit wallHit = Context.DetectWall();
        if (wallHit.hit)
        {
            wallNormal = wallHit.normal;
            isWallRight = wallHit.isRight;
            
            // Calcular dirección forward a lo largo de la pared
            wallForward = Vector3.Cross(wallNormal, Vector3.up).normalized;
            
            // Ajustar dirección según el lado de la pared
            if (!isWallRight)
                wallForward = -wallForward;

            wallRunTimer = 0f;

            // Reducir velocidad vertical al entrar
            if (rb.velocity.y < 0)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            }
        }
        else
        {
            // Si no hay pared, salir inmediatamente
            Context.ChangeState(PlayerStateType.Falling);
        }
    }

    public override void OnUpdate()
    {
        CheckTransitions();
    }

    public override void OnFixedUpdate()
    {
        // Aplicar drag reducido en wall run
        rb.drag = Context.WallRunDrag;

        wallRunTimer += Time.fixedDeltaTime;

        // Actualizar detección de pared
        WallRunHit wallHit = Context.DetectWall();
        if (wallHit.hit)
        {
            wallNormal = wallHit.normal;
            wallForward = Vector3.Cross(wallNormal, Vector3.up).normalized;
            if (!isWallRight)
                wallForward = -wallForward;
        }

        // Aplicar movimiento a lo largo de la pared
        ApplyWallRunMovement();

        // Aplicar fuerza hacia la pared para mantener contacto
        ApplyWallStickForce();

        // Reducir gravedad durante wall run
        ApplyReducedGravity();
    }

    private void ApplyWallRunMovement()
    {
        // Movimiento forward a lo largo de la pared
        Vector3 forwardDirection = wallForward;
        
        // Permitir input para acelerar/frenar
        float inputForward = Vector3.Dot(CalculateMovementDirection(MoveInput), forwardDirection);
        
        Vector3 targetVelocity = forwardDirection * Context.WallRunSpeed;
        
        // Si hay input hacia adelante, usar velocidad completa
        if (inputForward > 0.1f)
        {
            targetVelocity = forwardDirection * Context.WallRunSpeed;
        }
        else
        {
            // Mantener una velocidad mínima
            targetVelocity = forwardDirection * Context.WallRunSpeed * 0.7f;
        }

        Vector3 currentVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 velocityDifference = targetVelocity - currentVelocity;
        
        float maxVelocityChange = Context.WallRunAcceleration * Time.fixedDeltaTime;
        Vector3 velocityChange = Vector3.ClampMagnitude(velocityDifference, maxVelocityChange);
        
        Vector3 newVelocity = currentVelocity + velocityChange;
        rb.velocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);
    }

    private void ApplyWallStickForce()
    {
        // Fuerza hacia la pared para mantener contacto
        rb.AddForce(-wallNormal * Context.WallStickForce, ForceMode.Force);
    }

    private void ApplyReducedGravity()
    {
        // Aplicar gravedad reducida para simular "correr" en la pared
        float reducedGravity = Physics.gravity.y * Context.WallRunGravityMultiplier;
        float gravityOffset = Physics.gravity.y - reducedGravity;
        rb.AddForce(Vector3.up * -gravityOffset, ForceMode.Acceleration);
    }

    public override void CheckTransitions()
    {
        // Si toca el suelo → Walking/Sprinting/Idle
        if (IsGrounded)
        {
            if (MoveInput.sqrMagnitude > 0.01f)
            {
                if (IsSprinting)
                    Context.ChangeState(PlayerStateType.Sprinting);
                else
                    Context.ChangeState(PlayerStateType.Walking);
            }
            else
            {
                Context.ChangeState(PlayerStateType.Idle);
            }
            return;
        }

        // Si ya no está tocando la pared → Falling
        WallRunHit wallHit = Context.DetectWall();
        if (!wallHit.hit)
        {
            Context.ChangeState(PlayerStateType.Falling);
            return;
        }

        // Si supera el tiempo máximo de wall run → Falling
        if (wallRunTimer >= Context.MaxWallRunTime)
        {
            Context.ChangeState(PlayerStateType.Falling);
            return;
        }

        // Si la velocidad es muy baja → Falling
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        if (horizontalVelocity.magnitude < Context.MinWallRunSpeed)
        {
            Context.ChangeState(PlayerStateType.Falling);
            return;
        }

        // Si el player está cayendo muy rápido → Falling
        if (rb.velocity.y < -Context.MaxWallRunFallSpeed)
        {
            Context.ChangeState(PlayerStateType.Falling);
            return;
        }
    }

    public override void OnExit()
    {
        if (Context.IsDebugModeOn)
            Log("Exiting Wall Running State");

        // Al salir del wall run, aplicar un pequeño impulso alejándose de la pared
        if (Context.WallRunExitBoost > 0)
        {
            rb.AddForce(wallNormal * Context.WallRunExitBoost, ForceMode.VelocityChange);
        }
    }
}